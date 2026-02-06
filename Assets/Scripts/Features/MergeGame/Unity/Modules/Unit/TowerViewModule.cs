using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 타워(플레이어 유닛) 뷰 모듈입니다.
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

        private readonly Dictionary<long, GameObject> _towerObjects = new();
        private readonly HashSet<long> _seen = new();
        private readonly List<long> _removeBuffer = new();

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
                    obj = CreateTowerObject(t);
                    _towerObjects[t.Uid] = obj;
                }

                // 좌표계는 Host/Map과 동일한 로컬 좌표계를 가정합니다.
                obj.transform.localPosition = new Vector3(t.PositionX, t.PositionY, 0f);
                obj.transform.localScale = _towerScale;
                obj.name = $"Tower_{t.Uid}_G{t.Grade}";

                ApplyGradeColor(obj, t.Grade);
            }

            RemoveNotSeen(_towerObjects);
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

        private GameObject CreateTowerObject(TowerSnapshot snapshot)
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
