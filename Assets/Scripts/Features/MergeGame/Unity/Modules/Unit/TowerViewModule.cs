using System;
using System.Collections.Generic;
using MyProject.MergeGame.Commands;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;
using MyProject.MergeGame.Unity.Network;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 타워 뷰 모듈입니다.
    /// 스냅샷을 Source of Truth로 삼아 오브젝트를 생성/갱신/제거합니다.
    /// 드래그 & 드롭으로 타워 머지를 지원합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TowerViewModule : MergeViewModuleBase
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _defaultTowerPrefab;

        [Serializable]
        public class TowerPrefab
        {
            public long Id;
            public GameObject Prefab;
        }

        [SerializeField] private List<TowerPrefab> _towerPrefabs;

        [Header("HitScan Visual")]
        [SerializeField] private bool _enableHitScanLine = true;
        [SerializeField] private LineRenderer _hitScanLinePrefab;
        [SerializeField] private float _hitScanLineDuration = 0.08f;
        [SerializeField] private float _hitScanLineWidth = 0.05f;
        [SerializeField] private Color _hitScanLineColor = new Color(1f, 0.8f, 0.2f, 1f);

        private readonly Dictionary<long, GameObject> _towerObjects = new();
        private readonly HashSet<long> _seen = new();
        private readonly List<long> _removeBuffer = new();
        private IReadOnlyList<SlotPositionData> _cachedSlotPositions;
        private MergeHostSnapshot _lastSnapshot;

        // 드래그 상태
        private Camera _mainCamera;
        private bool _isDragging;
        private long _dragTowerUid;
        private int _dragFromSlotIndex;
        private Transform _dragVisualTransform;
        private Vector3 _dragVisualOriginalLocal;
        private bool _waitingForMergeResult;

        protected override void OnInit()
        {
            base.OnInit();
            _mainCamera = Camera.main;
        }

        public override void OnEventMsg(MergeGameEvent evt)
        {
            if (!IsMyEvent(evt))
            {
                return;
            }
            switch (evt)
            {
                case MapInitializedEvent mapEvt:
                    _cachedSlotPositions = mapEvt.SlotPositions;
                    break;

                case TowerSpawnedEvent spawned:
                    HandleSpawnTowerEvent(spawned);
                    break;

                case TowerAttackedEvent attacked:
                    HandleTowerAttackEvent(attacked);
                    break;

                case TowerRemovedEvent removed:
                    HandleRemoveTowerEvent(removed.TowerUid);
                    break;

                case TowerMergedEvent merged:
                    HandleTowerMergedEvent(merged);
                    break;
            }
        }

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            if (snapshot == null || !IsMySnapshot(snapshot))
            {
                return;
            }

            _lastSnapshot = snapshot;
            SyncTowers(snapshot);
        }

        public override void OnCommandResultMsg(MergeCommandResult result)
        {
            if (result is not MergeTowerResult mergeResult) return;
            if (!_waitingForMergeResult) return;

            if (!mergeResult.Success)
            {
                _waitingForMergeResult = false;
                ResetDragVisual();
            }
            // 성공 시 TowerMergedEvent가 처리
        }

        #region Drag & Merge

        private void Update()
        {
            if (_waitingForMergeResult) return;
            if (_mainCamera == null) return;
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
                TryBeginDrag(mouse);
            else if (mouse.leftButton.isPressed && _isDragging)
                UpdateDrag(mouse);
            else if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
                EndDrag(mouse);
        }

        private void TryBeginDrag(Mouse mouse)
        {
            var ray = _mainCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit)) return;

            if (!TryFindTowerByObject(hit.collider.gameObject, out var uid, out var slotIndex)) return;

            // 내 타워만 드래그 가능
            if (!IsLocalTower(uid)) return;

            var root = _towerObjects[uid];
            if (root == null || root.transform.childCount == 0) return;

            _dragTowerUid = uid;
            _dragFromSlotIndex = slotIndex;
            _dragVisualTransform = root.transform.GetChild(0);  //지금은 visual child 하나
            _dragVisualOriginalLocal = _dragVisualTransform.localPosition;
            _isDragging = true;
        }

        private void UpdateDrag(Mouse mouse)
        {
            if (_dragVisualTransform == null)
            {
                _isDragging = false;
                return;
            }

            var ray = _mainCamera.ScreenPointToRay(mouse.position.ReadValue());
            var plane = new Plane(Vector3.up, _dragVisualTransform.parent.position);
            if (plane.Raycast(ray, out var dist))
            {
                _dragVisualTransform.position = ray.GetPoint(dist);
            }
        }

        private void EndDrag(Mouse mouse)
        {
            _isDragging = false;

            var ray = _mainCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out var hit))
            {
                if (TryFindTowerByObject(hit.collider.gameObject, out var targetUid, out var targetSlotIndex))
                {
                    if (targetUid != _dragTowerUid)
                    {
                        _waitingForMergeResult = true;
                        var cmd = new MergeTowerCommand(
                            GameView.LocalUserId, _dragFromSlotIndex, targetSlotIndex);
                        GameView.SendCommand(cmd, MergeNetCommandType.MergeTower);
                        return;
                    }
                }
            }

            ResetDragVisual();
        }

        private void ResetDragVisual()
        {
            if (_dragVisualTransform != null)
                _dragVisualTransform.localPosition = _dragVisualOriginalLocal;
            _dragVisualTransform = null;
        }

        private bool TryFindTowerByObject(GameObject obj, out long uid, out int slotIndex)
        {
            foreach (var kv in _towerObjects)
            {
                if (kv.Value == null) continue;
                if (kv.Value == obj || obj.transform.IsChildOf(kv.Value.transform))
                {
                    uid = kv.Key;
                    slotIndex = GetSlotIndexByUid(uid);
                    return slotIndex >= 0;
                }
            }
            uid = 0;
            slotIndex = -1;
            return false;
        }

        private int GetSlotIndexByUid(long uid)
        {
            if (_lastSnapshot == null) return -1;
            var towers = _lastSnapshot.Towers;
            for (var i = 0; i < towers.Count; i++)
            {
                if (towers[i].Uid == uid) return towers[i].SlotIndex;
            }
            return -1;
        }

        private bool IsLocalTower(long uid)
        {
            if (_lastSnapshot == null) return false;
            if (_lastSnapshot.PlayerIndex != GameView.AssignedPlayerIndex) return false;
            var towers = _lastSnapshot.Towers;
            for (var i = 0; i < towers.Count; i++)
            {
                if (towers[i].Uid == uid) return true;
            }
            return false;
        }

        #endregion

        #region Snapshot Sync

        private void SyncTowers(MergeHostSnapshot snapshot)
        {
            _seen.Clear();

            var towers = snapshot.Towers;
            for (var i = 0; i < towers.Count; i++)
            {
                var t = towers[i];
                _seen.Add(t.Uid);

                if (!_towerObjects.TryGetValue(t.Uid, out var obj) || obj == null)
                {
                    obj = CreateTowerObject(t.TowerId);
                    _towerObjects[t.Uid] = obj;
                }

                if (TryGetCachedSlotPosition(t.SlotIndex, out var pos))
                {
                    obj.transform.position = pos;
                }
                else
                {
                    obj.transform.position = new Vector3(t.PositionX, t.PositionY, t.PositionZ);
                }

                obj.name = $"Tower_{t.Uid}_G{t.Grade}";

                ApplyGradeColor(obj, t.Grade);
            }

            RemoveNotSeen(_towerObjects);
        }

        #endregion

        #region Event Handlers

        private void HandleSpawnTowerEvent(TowerSpawnedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (_towerObjects.TryGetValue(evt.TowerUid, out var obj) && obj != null)
            {
                return;
            }

            obj = CreateTowerObject(evt.TowerId);
            _towerObjects[evt.TowerUid] = obj;


            if (TryGetCachedSlotPosition(evt.SlotIndex, out var pos))
            {
                obj.transform.position = pos;
            }
            else
            {
                obj.transform.position = new Vector3(evt.PositionX, evt.PositionY, evt.PositionZ);
            }

            obj.name = $"Tower_{evt.TowerUid}_G{evt.Grade}";
            ApplyGradeColor(obj, evt.Grade);
        }

        private void HandleTowerAttackEvent(TowerAttackedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (_towerObjects.TryGetValue(evt.AttackerUid, out var obj) && obj != null)
            {
                var state = obj.GetComponent<TowerViewState>() ?? obj.AddComponent<TowerViewState>();
                state.TriggerAttack();
            }

            if (evt.AttackType == TowerAttackType.HitScan && _enableHitScanLine)
            {
                var start = new Vector3(evt.AttackerX, evt.AttackerY, evt.AttackerZ);
                var target = new Vector3(evt.TargetX, evt.TargetY, evt.TargetZ);
                SpawnHitScanLine(start, target);
            }
        }

        private void HandleTowerMergedEvent(TowerMergedEvent evt)
        {
            // 1. source/target 타워 제거
            HandleRemoveTowerEvent(evt.SourceTowerUid);
            HandleRemoveTowerEvent(evt.TargetTowerUid);

            // 2. result 타워 생성 + 슬롯 위치 배치
            var obj = CreateTowerObject(evt.ResultTowerId);
            if (obj != null)
            {
                _towerObjects[evt.ResultTowerUid] = obj;

                if (TryGetCachedSlotPosition(evt.SlotIndex, out var pos))
                    obj.transform.position = pos;

                obj.name = $"Tower_{evt.ResultTowerUid}_G{evt.ResultGrade}";
                ApplyGradeColor(obj, evt.ResultGrade);
            }

            // 3. 드래그 비주얼 정리
            _dragVisualTransform = null;
            _waitingForMergeResult = false;
        }

        private void HandleRemoveTowerEvent(long uid)
        {
            // 드래그 중인 타워가 제거되면 드래그 취소
            if (_isDragging && _dragTowerUid == uid)
            {
                _isDragging = false;
                _dragVisualTransform = null;
                _waitingForMergeResult = false;
            }

            if (_towerObjects.TryGetValue(uid, out var obj) && obj != null)
            {
                Destroy(obj);
            }

            _towerObjects.Remove(uid);
        }

        #endregion

        #region Helpers

        private void SpawnHitScanLine(Vector3 start, Vector3 target)
        {
            var line = _hitScanLinePrefab != null
                ? Instantiate(_hitScanLinePrefab, transform)
                : CreateLineRendererFallback();

            if (line == null)
            {
                return;
            }

            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, target);
            line.startWidth = _hitScanLineWidth;
            line.endWidth = _hitScanLineWidth;
            line.startColor = _hitScanLineColor;
            line.endColor = _hitScanLineColor;

            Destroy(line.gameObject, _hitScanLineDuration);
        }

        private LineRenderer CreateLineRendererFallback()
        {
            var obj = new GameObject("HitScanLine");
            obj.transform.SetParent(transform, false);

            var line = obj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = false;
            return line;
        }

        private void RemoveNotSeen(Dictionary<long, GameObject> dict)
        {
            _removeBuffer.Clear();

            foreach (var kv in dict)
            {
                if (!_seen.Contains(kv.Key))
                {
                    _removeBuffer.Add(kv.Key);
                }
            }

            for (var i = 0; i < _removeBuffer.Count; i++)
            {
                var uid = _removeBuffer[i];
                if (dict.TryGetValue(uid, out var obj) && obj != null)
                {
                    Destroy(obj);
                }

                dict.Remove(uid);
            }
        }

        private GameObject CreateTowerObject(long towerId)
        {
            GameObject selectionPrefab = null;
            selectionPrefab = _defaultTowerPrefab;

            if(_towerPrefabs != null && _towerPrefabs.Count > 0)
            {
                foreach(var towerPrefab in _towerPrefabs)
                {
                    if(towerPrefab.Id == towerId && towerPrefab.Prefab != null)
                    {
                        selectionPrefab = towerPrefab.Prefab;
                        break;
                    }
                }
            }

            if (selectionPrefab == null)
            {
                Debug.LogWarning("Tower Prefab Is Null");
                return null;
            }

            var instance = Instantiate(selectionPrefab, transform);
            var tower = instance.GetComponent<TowerViewState>() ?? instance.AddComponent<TowerViewState>();
            return instance;
        }

        private static void ApplyGradeColor(GameObject obj, int grade)
        {
            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            // 프로토타입용: 등급이 올라갈수록 밝고 따뜻한 색으로.
            var t = Mathf.Clamp01((grade - 1) / 8f);
            var color = Color.Lerp(new Color(0.8f, 0.8f, 0.9f, 1f), new Color(1f, 0.8f, 0.2f, 1f), t);

            try
            {
                renderer.material.color = color;
            }
            catch
            {
                // 일부 렌더러/머티리얼은 color 프로퍼티가 없을 수 있습니다.
            }
        }

        private bool TryGetCachedSlotPosition(int slotIndex, out Vector3 out_Pos)
        {
            out_Pos = Vector3.zero;

            if (_cachedSlotPositions == null) return false;
            if (_cachedSlotPositions.Count == 0) return false;

            for (var i = 0; i < _cachedSlotPositions.Count; i++)
            {
                var pos = _cachedSlotPositions[i];
                if (pos.Index == slotIndex)
                {
                    //타워 피벗이 지금은 정중앙이라서
                    var towerSizeYHalf = 0.5f;
                    out_Pos = new Vector3(pos.X, pos.Y + towerSizeYHalf, pos.Z);
                    return true;
                }
            }

            return false;
        }

        #endregion

        protected override void OnShutdown()
        {
            base.OnShutdown();

            foreach (var kv in _towerObjects)
            {
                if (kv.Value != null)
                {
                    Destroy(kv.Value);
                }
            }

            _towerObjects.Clear();
        }
    }
}


