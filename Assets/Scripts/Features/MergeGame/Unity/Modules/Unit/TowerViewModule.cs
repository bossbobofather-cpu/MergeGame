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
    /// 드래그 & 드롭으로 로컬 플레이어 타워 머지를 지원합니다.
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

        private readonly Dictionary<int, Dictionary<long, GameObject>> _towerObjectsByPlayer = new();
        private readonly Dictionary<int, IReadOnlyList<SlotPositionData>> _cachedSlotPositionsByPlayer = new();
        private readonly HashSet<long> _seen = new();
        private readonly List<long> _removeBuffer = new();

        private MergeHostSnapshot _localSnapshot;

        // 등급 단계가 확실히 구분되도록 8단계 고정 팔레트를 사용합니다.
        private static readonly Color[] GradePalette =
        {
            new(0.62f, 0.72f, 0.96f, 1f), // 1
            new(0.42f, 0.82f, 0.95f, 1f), // 2
            new(0.36f, 0.90f, 0.62f, 1f), // 3
            new(0.58f, 0.94f, 0.40f, 1f), // 4
            new(0.95f, 0.90f, 0.30f, 1f), // 5
            new(1.00f, 0.72f, 0.26f, 1f), // 6
            new(1.00f, 0.48f, 0.28f, 1f), // 7
            new(0.96f, 0.28f, 0.42f, 1f)  // 8
        };

        // 드래그 상태
        private Camera _mainCamera;
        private bool _isDragging;
        private long _dragTowerUid;
        private int _dragFromSlotIndex;
        private Transform _dragVisualTransform;
        private Vector3 _dragVisualOriginalLocal;
        private bool _waitingForMergeResult;
        /// <summary>
        /// OnInit 메서드입니다.
        /// </summary>

        protected override void OnInit()
        {
            base.OnInit();
            _mainCamera = Camera.main;
        }
        /// <summary>
        /// OnEventMsg 메서드입니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
            switch (evt)
            {
                case MapInitializedEvent mapEvt:
                    _cachedSlotPositionsByPlayer[mapEvt.PlayerIndex] = mapEvt.SlotPositions;
                    break;

                case TowerSpawnedEvent spawned:
                    HandleSpawnTowerEvent(spawned);
                    break;

                case TowerAttackedEvent attacked:
                    HandleTowerAttackEvent(attacked);
                    break;

                case TowerRemovedEvent removed:
                    HandleRemoveTowerEvent(removed.PlayerIndex, removed.TowerUid);
                    break;

                case TowerMergedEvent merged:
                    HandleTowerMergedEvent(merged);
                    break;
            }
        }
        /// <summary>
        /// OnSnapshotMsg 메서드입니다.
        /// </summary>

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            if (GameView != null && GameView.AssignedPlayerIndex >= 0 && snapshot.PlayerIndex == GameView.AssignedPlayerIndex)
            {
                _localSnapshot = snapshot;
            }

            SyncTowers(snapshot);
        }
        /// <summary>
        /// OnCommandResultMsg 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// Update 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// TryBeginDrag 메서드입니다.
        /// </summary>

        private void TryBeginDrag(Mouse mouse)
        {
            var ray = _mainCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit)) return;

            if (!TryFindTowerByObject(hit.collider.gameObject, out var uid, out var slotIndex)) return;

            // 내 타워만 드래그 가능
            if (!IsLocalTower(uid)) return;

            if (!TryGetLocalTowerMap(out var localTowerMap)) return;
            if (!localTowerMap.TryGetValue(uid, out var root) || root == null || root.transform.childCount == 0) return;

            _dragTowerUid = uid;
            _dragFromSlotIndex = slotIndex;
            _dragVisualTransform = root.transform.GetChild(0);  // 지금은 visual child 하나
            _dragVisualOriginalLocal = _dragVisualTransform.localPosition;
            _isDragging = true;
        }
        /// <summary>
        /// UpdateDrag 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// EndDrag 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// ResetDragVisual 메서드입니다.
        /// </summary>

        private void ResetDragVisual()
        {
            if (_dragVisualTransform != null)
                _dragVisualTransform.localPosition = _dragVisualOriginalLocal;
            _dragVisualTransform = null;
        }
        /// <summary>
        /// TryFindTowerByObject 메서드입니다.
        /// </summary>

        private bool TryFindTowerByObject(GameObject obj, out long uid, out int slotIndex)
        {
            uid = 0;
            slotIndex = -1;

            if (!TryGetLocalTowerMap(out var localTowerMap))
            {
                return false;
            }

            foreach (var kv in localTowerMap)
            {
                if (kv.Value == null) continue;
                if (kv.Value == obj || obj.transform.IsChildOf(kv.Value.transform))
                {
                    uid = kv.Key;
                    slotIndex = GetSlotIndexByUid(uid);
                    return slotIndex >= 0;
                }
            }

            return false;
        }
        /// <summary>
        /// GetSlotIndexByUid 메서드입니다.
        /// </summary>

        private int GetSlotIndexByUid(long uid)
        {
            if (_localSnapshot == null) return -1;
            var towers = _localSnapshot.Towers;
            for (var i = 0; i < towers.Count; i++)
            {
                if (towers[i].Uid == uid) return towers[i].SlotIndex;
            }
            return -1;
        }
        /// <summary>
        /// IsLocalTower 메서드입니다.
        /// </summary>

        private bool IsLocalTower(long uid)
        {
            if (_localSnapshot == null) return false;
            if (GameView == null || _localSnapshot.PlayerIndex != GameView.AssignedPlayerIndex) return false;

            var towers = _localSnapshot.Towers;
            for (var i = 0; i < towers.Count; i++)
            {
                if (towers[i].Uid == uid) return true;
            }
            return false;
        }

        #endregion

        #region Snapshot Sync
        /// <summary>
        /// SyncTowers 메서드입니다.
        /// </summary>

        private void SyncTowers(MergeHostSnapshot snapshot)
        {
            var towerMap = GetOrCreateTowerMap(snapshot.PlayerIndex);
            _seen.Clear();

            var towers = snapshot.Towers;
            for (var i = 0; i < towers.Count; i++)
            {
                var t = towers[i];
                _seen.Add(t.Uid);

                if (!towerMap.TryGetValue(t.Uid, out var obj) || obj == null)
                {
                    obj = CreateTowerObject(t.TowerId);
                    if (obj == null)
                    {
                        continue;
                    }

                    towerMap[t.Uid] = obj;
                }

                if (TryGetCachedSlotPosition(snapshot.PlayerIndex, t.SlotIndex, out var pos))
                {
                    obj.transform.position = pos;
                }
                else
                {
                    obj.transform.position = ApplyOffset(snapshot.PlayerIndex, t.PositionX, t.PositionY, t.PositionZ);
                }

                obj.name = $"Tower_P{snapshot.PlayerIndex}_{t.Uid}_G{t.Grade}";
                ApplyGradeColor(obj, t.Grade);
            }

            RemoveNotSeen(towerMap);
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// HandleSpawnTowerEvent 메서드입니다.
        /// </summary>

        private void HandleSpawnTowerEvent(TowerSpawnedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            var towerMap = GetOrCreateTowerMap(evt.PlayerIndex);
            if (towerMap.TryGetValue(evt.TowerUid, out var existing) && existing != null)
            {
                return;
            }

            var obj = CreateTowerObject(evt.TowerId);
            if (obj == null)
            {
                return;
            }

            towerMap[evt.TowerUid] = obj;

            if (TryGetCachedSlotPosition(evt.PlayerIndex, evt.SlotIndex, out var pos))
            {
                obj.transform.position = pos;
            }
            else
            {
                obj.transform.position = ApplyOffset(evt.PlayerIndex, evt.PositionX, evt.PositionY, evt.PositionZ);
            }

            obj.name = $"Tower_P{evt.PlayerIndex}_{evt.TowerUid}_G{evt.Grade}";
            ApplyGradeColor(obj, evt.Grade);
        }
        /// <summary>
        /// HandleTowerAttackEvent 메서드입니다.
        /// </summary>

        private void HandleTowerAttackEvent(TowerAttackedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (TryGetTowerMap(evt.PlayerIndex, out var towerMap)
                && towerMap.TryGetValue(evt.AttackerUid, out var obj)
                && obj != null)
            {
                var state = obj.GetComponent<TowerViewState>() ?? obj.AddComponent<TowerViewState>();
                state.TriggerAttack();
            }

            if (evt.AttackType == TowerAttackType.HitScan && _enableHitScanLine)
            {
                var start = ApplyOffset(evt.PlayerIndex, evt.AttackerX, evt.AttackerY, evt.AttackerZ);
                var target = ApplyOffset(evt.PlayerIndex, evt.TargetX, evt.TargetY, evt.TargetZ);
                SpawnHitScanLine(start, target);
            }
        }
        /// <summary>
        /// HandleTowerMergedEvent 메서드입니다.
        /// </summary>

        private void HandleTowerMergedEvent(TowerMergedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            HandleRemoveTowerEvent(evt.PlayerIndex, evt.SourceTowerUid);
            HandleRemoveTowerEvent(evt.PlayerIndex, evt.TargetTowerUid);

            var towerMap = GetOrCreateTowerMap(evt.PlayerIndex);
            var obj = CreateTowerObject(evt.ResultTowerId);
            if (obj != null)
            {
                towerMap[evt.ResultTowerUid] = obj;

                if (TryGetCachedSlotPosition(evt.PlayerIndex, evt.SlotIndex, out var pos))
                {
                    obj.transform.position = pos;
                }
                else
                {
                    obj.transform.position = GetPlayerOffset(evt.PlayerIndex);
                }

                obj.name = $"Tower_P{evt.PlayerIndex}_{evt.ResultTowerUid}_G{evt.ResultGrade}";
                ApplyGradeColor(obj, evt.ResultGrade);
            }

            if (GameView != null && evt.PlayerIndex == GameView.AssignedPlayerIndex)
            {
                _dragVisualTransform = null;
                _waitingForMergeResult = false;
            }
        }
        /// <summary>
        /// HandleRemoveTowerEvent 메서드입니다.
        /// </summary>

        private void HandleRemoveTowerEvent(int playerIndex, long uid)
        {
            var localPlayerIndex = GameView != null ? GameView.AssignedPlayerIndex : -1;
            if (_isDragging && playerIndex == localPlayerIndex && _dragTowerUid == uid)
            {
                _isDragging = false;
                _dragVisualTransform = null;
                _waitingForMergeResult = false;
            }

            if (!TryGetTowerMap(playerIndex, out var towerMap))
            {
                return;
            }

            if (towerMap.TryGetValue(uid, out var obj) && obj != null)
            {
                Destroy(obj);
            }

            towerMap.Remove(uid);
        }

        #endregion

        #region Helpers
        /// <summary>
        /// SpawnHitScanLine 메서드입니다.
        /// </summary>

        private void SpawnHitScanLine(Vector3 start, Vector3 target)
        {
            var line = _hitScanLinePrefab != null
                ? Instantiate(_hitScanLinePrefab, transform)
                : CreateLineRendererFallback();

            if (line == null)
            {
                return;
            }

            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, target);
            line.startWidth = _hitScanLineWidth;
            line.endWidth = _hitScanLineWidth;
            line.startColor = _hitScanLineColor;
            line.endColor = _hitScanLineColor;

            Destroy(line.gameObject, _hitScanLineDuration);
        }
        /// <summary>
        /// CreateLineRendererFallback 메서드입니다.
        /// </summary>

        private LineRenderer CreateLineRendererFallback()
        {
            var obj = new GameObject("HitScanLine");
            obj.transform.SetParent(transform, false);

            var line = obj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = true;
            return line;
        }
        /// <summary>
        /// RemoveNotSeen 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// CreateTowerObject 메서드입니다.
        /// </summary>

        private GameObject CreateTowerObject(long towerId)
        {
            GameObject selectionPrefab = _defaultTowerPrefab;

            if (_towerPrefabs != null && _towerPrefabs.Count > 0)
            {
                foreach (var towerPrefab in _towerPrefabs)
                {
                    if (towerPrefab.Id == towerId && towerPrefab.Prefab != null)
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
            _ = instance.GetComponent<TowerViewState>() ?? instance.AddComponent<TowerViewState>();
            return instance;
        }
        /// <summary>
        /// ApplyGradeColor 메서드입니다.
        /// </summary>

        private static void ApplyGradeColor(GameObject obj, int grade)
        {
            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var maxGrade = Mathf.Max(1, DevHelperSet.DevRuleHelper.DEV_TOWER_MAX_GRADE);
            var normalized = maxGrade > 1 ? Mathf.Clamp01((grade - 1f) / (maxGrade - 1f)) : 0f;
            var paletteIndex = Mathf.Clamp(Mathf.RoundToInt(normalized * (GradePalette.Length - 1)), 0, GradePalette.Length - 1);
            var color = GradePalette[paletteIndex];

            try
            {
                renderer.material.color = color;
            }
            catch
            {
                // 일부 렌더러/머티리얼은 color 프로퍼티가 없을 수 있습니다.
            }
        }
        /// <summary>
        /// TryGetCachedSlotPosition 메서드입니다.
        /// </summary>

        private bool TryGetCachedSlotPosition(int playerIndex, int slotIndex, out Vector3 outPos)
        {
            outPos = Vector3.zero;

            if (!_cachedSlotPositionsByPlayer.TryGetValue(playerIndex, out var slotPositions)
                || slotPositions == null
                || slotPositions.Count == 0)
            {
                return false;
            }

            for (var i = 0; i < slotPositions.Count; i++)
            {
                var pos = slotPositions[i];
                if (pos.Index != slotIndex)
                {
                    continue;
                }

                var towerSizeYHalf = 0.5f;
                outPos = new Vector3(pos.X, pos.Y + towerSizeYHalf, pos.Z) + GetPlayerOffset(playerIndex);
                return true;
            }

            return false;
        }
        /// <summary>
        /// GetOrCreateTowerMap 메서드입니다.
        /// </summary>

        private Dictionary<long, GameObject> GetOrCreateTowerMap(int playerIndex)
        {
            if (_towerObjectsByPlayer.TryGetValue(playerIndex, out var towerMap))
            {
                return towerMap;
            }

            towerMap = new Dictionary<long, GameObject>();
            _towerObjectsByPlayer[playerIndex] = towerMap;
            return towerMap;
        }
        /// <summary>
        /// TryGetTowerMap 메서드입니다.
        /// </summary>

        private bool TryGetTowerMap(int playerIndex, out Dictionary<long, GameObject> towerMap)
        {
            return _towerObjectsByPlayer.TryGetValue(playerIndex, out towerMap);
        }
        /// <summary>
        /// TryGetLocalTowerMap 메서드입니다.
        /// </summary>

        private bool TryGetLocalTowerMap(out Dictionary<long, GameObject> towerMap)
        {
            towerMap = null;
            if (GameView == null || GameView.AssignedPlayerIndex < 0)
            {
                return false;
            }

            return _towerObjectsByPlayer.TryGetValue(GameView.AssignedPlayerIndex, out towerMap);
        }
        /// <summary>
        /// GetPlayerOffset 메서드입니다.
        /// </summary>

        private Vector3 GetPlayerOffset(int playerIndex)
        {
            return GameView != null
                ? GameView.GetPlayerOffsetPosition(playerIndex)
                : Vector3.zero;
        }
        /// <summary>
        /// ApplyOffset 메서드입니다.
        /// </summary>

        private Vector3 ApplyOffset(int playerIndex, float x, float y, float z)
        {
            return new Vector3(x, y, z) + GetPlayerOffset(playerIndex);
        }

        #endregion
        /// <summary>
        /// OnShutdown 메서드입니다.
        /// </summary>

        protected override void OnShutdown()
        {
            base.OnShutdown();

            foreach (var playerMap in _towerObjectsByPlayer.Values)
            {
                foreach (var kv in playerMap)
                {
                    if (kv.Value != null)
                    {
                        Destroy(kv.Value);
                    }
                }
            }

            _towerObjectsByPlayer.Clear();
            _cachedSlotPositionsByPlayer.Clear();
            _localSnapshot = null;
        }
    }
}
