using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 타워 뷰 모듈입니다.
    /// 스냅샷을 Source of Truth로 삼아 오브젝트를 생성/갱신/제거합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TowerViewModule : MergeViewModuleBase
    {
        [Header("Prefabs (Optional)")]
        [SerializeField] private GameObject _towerPrefab;

        [Header("Fallback Visual")]
        [SerializeField] private bool _usePrimitiveFallback = true;
        [SerializeField] private Vector3 _towerScale = new Vector3(0.8f, 0.8f, 0.8f);

        [Header("HitScan Visual")]
        [SerializeField] private bool _enableHitScanLine = true;
        [SerializeField] private LineRenderer _hitScanLinePrefab;
        [SerializeField] private float _hitScanLineDuration = 0.08f;
        [SerializeField] private float _hitScanLineWidth = 0.05f;
        [SerializeField] private Color _hitScanLineColor = new Color(1f, 0.8f, 0.2f, 1f);

        private readonly Dictionary<long, GameObject> _towerObjects = new();
        private readonly HashSet<long> _seen = new();
        private readonly List<long> _removeBuffer = new();

        public override void OnHostEvent(MergeHostEvent evt)
        {
            switch (evt)
            {
                case TowerSpawnedEvent spawned:
                    EnsureTowerFromEvent(spawned);
                    break;

                case TowerAttackedEvent attacked:
                    HandleTowerAttack(attacked);
                    break;

                case TowerRemovedEvent removed:
                    RemoveTower(removed.TowerUid);
                    break;
            }
        }

        public override void OnSnapshotUpdated(MergeHostSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            SyncTowers(snapshot);
        }

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
                    obj = CreateTowerObject();
                    _towerObjects[t.Uid] = obj;
                    InitializeTowerObject(obj);
                }

                // 좌표계는 Host/Map과 동일한 로컬 좌표계를 가정합니다.
                obj.transform.localPosition = new Vector3(t.PositionX, t.PositionY, t.PositionZ);
                obj.name = $"Tower_{t.Uid}_G{t.Grade}";

                ApplyGradeColor(obj, t.Grade);
            }

            RemoveNotSeen(_towerObjects);
        }

        private void EnsureTowerFromEvent(TowerSpawnedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (_towerObjects.TryGetValue(evt.TowerUid, out var obj) && obj != null)
            {
                return;
            }

            obj = CreateTowerObject();
            _towerObjects[evt.TowerUid] = obj;
            InitializeTowerObject(obj);

            obj.transform.localPosition = new Vector3(evt.PositionX, evt.PositionY, evt.PositionZ);
            obj.name = $"Tower_{evt.TowerUid}_G{evt.Grade}";
            ApplyGradeColor(obj, evt.Grade);
        }

        private void HandleTowerAttack(TowerAttackedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (_towerObjects.TryGetValue(evt.AttackerUid, out var obj) && obj != null)
            {
                var state = obj.GetComponent<TowerViewState>() ?? obj.AddComponent<TowerViewState>();
                state.SetBaseScale(_towerScale);
                state.TriggerAttack();
            }

            if (evt.AttackType == TowerAttackType.HitScan && _enableHitScanLine)
            {
                var start = new Vector3(evt.AttackerX, evt.AttackerY, evt.AttackerZ);
                var target = new Vector3(evt.TargetX, evt.TargetY, evt.TargetZ);
                SpawnHitScanLine(start, target);
            }
        }

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

        private GameObject CreateTowerObject()
        {
            if (_towerPrefab != null)
            {
                return Instantiate(_towerPrefab, transform);
            }

            if (_usePrimitiveFallback)
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                primitive.transform.SetParent(transform, false);
                return primitive;
            }

            var obj = new GameObject("Tower");
            obj.transform.SetParent(transform, false);
            return obj;
        }

        private void InitializeTowerObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.transform.localScale = _towerScale;

            var state = obj.GetComponent<TowerViewState>() ?? obj.AddComponent<TowerViewState>();
            state.SetBaseScale(_towerScale);
        }

        private void RemoveTower(long uid)
        {
            if (_towerObjects.TryGetValue(uid, out var obj) && obj != null)
            {
                Destroy(obj);
            }

            _towerObjects.Remove(uid);
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
