using System.Collections.Generic;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 몬스터 뷰 모듈입니다.
    /// 스냅샷을 Source of Truth로 삼아 오브젝트를 생성/갱신/제거합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewModule : MergeViewModuleBase
    {
        [Header("Prefabs (Optional)")]
        [SerializeField] private GameObject _monsterPrefab;

        [Header("Fallback Visual")]
        [SerializeField] private bool _usePrimitiveFallback = true;
        [SerializeField] private Vector3 _monsterScale = new Vector3(0.8f, 0.8f, 0.8f);

        private readonly Dictionary<long, GameObject> _monsterObjects = new();
        private readonly HashSet<long> _seen = new();
        private readonly List<long> _removeBuffer = new();

        public override void OnHostEvent(MergeHostEvent evt)
        {
            switch (evt)
            {
                case MonsterSpawnedEvent spawned:
                    EnsureMonsterFromEvent(spawned);
                    break;

                case MonsterMovedEvent moved:
                    HandleMonsterMoved(moved);
                    break;

                case MonsterDiedEvent died:
                    RemoveMonster(died.MonsterUid);
                    break;
            }
        }

        public override void OnSnapshotUpdated(MergeHostSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            SyncMonsters(snapshot);
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
                    obj = CreateMonsterObject();
                    _monsterObjects[m.Uid] = obj;
                    InitializeMonsterObject(obj);
                }

                obj.transform.localPosition = new Vector3(m.PositionX, m.PositionY, m.PositionZ);
                obj.name = $"Monster_{m.Uid}";

                // 몬스터는 HP 비율로 색을 살짝 바꿉니다.
                ApplyMonsterHpTint(obj, m.HealthRatio);
            }

            RemoveNotSeen(_monsterObjects);
        }

        private void EnsureMonsterFromEvent(MonsterSpawnedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (_monsterObjects.TryGetValue(evt.MonsterUid, out var obj) && obj != null)
            {
                return;
            }

            obj = CreateMonsterObject();
            _monsterObjects[evt.MonsterUid] = obj;
            InitializeMonsterObject(obj);

            obj.transform.localPosition = new Vector3(evt.PositionX, evt.PositionY, evt.PositionZ);
            obj.name = $"Monster_{evt.MonsterUid}";
        }

        private void HandleMonsterMoved(MonsterMovedEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            if (_monsterObjects.TryGetValue(evt.MonsterUid, out var obj) && obj != null)
            {
                var state = obj.GetComponent<MonsterViewState>() ?? obj.AddComponent<MonsterViewState>();
                state.SetBaseScale(_monsterScale);
                state.MarkMoving();
            }
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

        private GameObject CreateMonsterObject()
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

        private void InitializeMonsterObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.transform.localScale = _monsterScale;

            var state = obj.GetComponent<MonsterViewState>() ?? obj.AddComponent<MonsterViewState>();
            state.SetBaseScale(_monsterScale);
        }

        private void RemoveMonster(long uid)
        {
            if (_monsterObjects.TryGetValue(uid, out var obj) && obj != null)
            {
                Destroy(obj);
            }

            _monsterObjects.Remove(uid);
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
