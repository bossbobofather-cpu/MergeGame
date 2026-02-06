using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 유닛(캐릭터/몬스터) 뷰 모듈입니다.
    /// 스냅샷을 Source of Truth로 삼아 오브젝트를 생성/갱신/제거합니다.
    /// </summary>
    public sealed class UnitViewModule : MergeViewModuleBase
    {
        [Header("Prefabs (Optional)")]
        [SerializeField] private GameObject _towerPrefab;
        [SerializeField] private GameObject _monsterPrefab;

        [Header("Fallback Visual")]
        [SerializeField] private bool _usePrimitiveFallback = true;
        [SerializeField] private Vector3 _towerScale = new Vector3(0.8f, 0.8f, 0.8f);
        [SerializeField] private Vector3 _monsterScale = new Vector3(0.8f, 0.8f, 0.8f);

        private readonly Dictionary<long, GameObject> _towerObjects = new();
        private readonly Dictionary<long, GameObject> _monsterObjects = new();

        private readonly HashSet<long> _seen = new();
        private readonly List<long> _removeBuffer = new();

        public override void OnSnapshotUpdated(MergeHostSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            SyncTowers(snapshot);
            SyncMonsters(snapshot);
        }

        private void SyncTowers(MergeHostSnapshot snapshot)
        {
            _seen.Clear();

            var towers = snapshot.Towers;
            for (var i = 0; i < towers.Count; i++)
            {
                var ch = towers[i];
                _seen.Add(ch.Uid);

                if (!_towerObjects.TryGetValue(ch.Uid, out var obj) || obj == null)
                {
                    obj = CreateTowerObject(ch);
                    _towerObjects[ch.Uid] = obj;
                }

                // 좌표계는 Host/Map과 동일한 로컬 좌표계를 가정합니다.
                obj.transform.localPosition = new Vector3(ch.PositionX, ch.PositionY, ch.PositionZ);
                obj.transform.localScale = _towerScale;
                obj.name = $"Tower_{ch.Uid}_G{ch.Grade}";

                ApplyGradeColor(obj, ch.Grade);
            }

            RemoveNotSeen(_towerObjects);
        }

        private void SyncMonsters(MergeHostSnapshot snapshot)
        {
            _seen.Clear();

            var monsters = snapshot.Monsters;
            for (var i = 0; i < monsters.Count; i++)
            {
                var m = monsters[i];
                _seen.Add(m.Uid);

                if (!_monsterObjects.TryGetValue(m.Uid, out var obj) || obj == null)
                {
                    obj = CreateMonsterObject(m);
                    _monsterObjects[m.Uid] = obj;
                }

                obj.transform.localPosition = new Vector3(m.PositionX, m.PositionY, m.PositionZ);
                obj.transform.localScale = _monsterScale;
                obj.name = $"Monster_{m.Uid}";

                // 몬스터는 HP 비율로 색을 살짝 바꿉니다.
                ApplyMonsterHpTint(obj, m.HealthRatio);
            }

            RemoveNotSeen(_monsterObjects);
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

        private GameObject CreateMonsterObject(MonsterSnapshot snapshot)
        {
            if (_monsterPrefab != null)
            {
                return Instantiate(_monsterPrefab, transform);
            }

            if (_usePrimitiveFallback)
            {
                var primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                primitive.transform.SetParent(transform, false);
                return primitive;
            }

            var obj = new GameObject("Monster");
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

        private static void ApplyMonsterHpTint(GameObject obj, float hpRatio)
        {
            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var t = Mathf.Clamp01(hpRatio);
            var color = Color.Lerp(new Color(1f, 0.25f, 0.25f, 1f), new Color(0.25f, 1f, 0.25f, 1f), t);

            try
            {
                renderer.material.color = color;
            }
            catch
            {
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

            foreach (var kv in _monsterObjects)
            {
                if (kv.Value != null)
                {
                    Destroy(kv.Value);
                }
            }
            _monsterObjects.Clear();
        }
    }
}


