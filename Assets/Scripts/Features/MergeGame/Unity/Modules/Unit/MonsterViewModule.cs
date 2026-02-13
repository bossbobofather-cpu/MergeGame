using System.Collections.Generic;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;
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
        [SerializeField] private Color _injectedMonsterColor = new Color(0.02f, 0.02f, 0.02f, 1f);

        private readonly Dictionary<int, Dictionary<long, GameObject>> _monsterObjectsByPlayer = new();
        private readonly HashSet<long> _seen = new();
        private readonly List<long> _removeBuffer = new();
        /// <summary>
        /// OnEventMsg 함수를 처리합니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
            // 핵심 로직을 처리합니다.
            switch (evt)
            {
                case MonsterSpawnedEvent spawned:
                    EnsureMonsterFromEvent(spawned);
                    break;

                case MonsterMovedEvent moved:
                    HandleMonsterMoved(moved);
                    break;

                case MonsterDiedEvent died:
                    RemoveMonster(died.PlayerIndex, died.MonsterUid);
                    break;
            }
        }
        /// <summary>
        /// OnSnapshotMsg 함수를 처리합니다.
        /// </summary>

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            // 핵심 로직을 처리합니다.
            if (snapshot == null)
            {
                return;
            }

            SyncMonsters(snapshot);
        }
        /// <summary>
        /// SyncMonsters 함수를 처리합니다.
        /// </summary>

        private void SyncMonsters(MergeHostSnapshot snapshot)
        {
            // 핵심 로직을 처리합니다.
            var monsterMap = GetOrCreateMonsterMap(snapshot.PlayerIndex);
            _seen.Clear();

            var monsters = snapshot.Monsters;
            for (var i = 0; i < monsters.Count; i++)
            {
                var m = monsters[i];
                _seen.Add(m.Uid);

                if (!monsterMap.TryGetValue(m.Uid, out var obj) || obj == null)
                {
                    obj = CreateMonsterObject();
                    if (obj == null)
                    {
                        continue;
                    }

                    monsterMap[m.Uid] = obj;
                    InitializeMonsterObject(obj);
                }

                obj.transform.position = ApplyOffset(snapshot.PlayerIndex, m.PositionX, m.PositionY, m.PositionZ);
                obj.name = $"Monster_P{snapshot.PlayerIndex}_{m.Uid}";

                if (m.IsInjectedByOpponent)
                {
                    ApplyInjectedMonsterTint(obj, _injectedMonsterColor);
                }
                else
                {
                    // 기본 몬스터는 HP 비율로 색을 바꿉니다.
                    ApplyMonsterHpTint(obj, m.HealthRatio);
                }
            }

            RemoveNotSeen(monsterMap);
        }
        /// <summary>
        /// EnsureMonsterFromEvent 함수를 처리합니다.
        /// </summary>

        private void EnsureMonsterFromEvent(MonsterSpawnedEvent evt)
        {
            // 핵심 로직을 처리합니다.
            if (evt == null)
            {
                return;
            }

            var monsterMap = GetOrCreateMonsterMap(evt.PlayerIndex);
            if (monsterMap.TryGetValue(evt.MonsterUid, out var existing) && existing != null)
            {
                return;
            }

            var obj = CreateMonsterObject();
            if (obj == null)
            {
                return;
            }

            monsterMap[evt.MonsterUid] = obj;
            InitializeMonsterObject(obj);

            obj.transform.position = ApplyOffset(evt.PlayerIndex, evt.PositionX, evt.PositionY, evt.PositionZ);
            obj.name = $"Monster_P{evt.PlayerIndex}_{evt.MonsterUid}";
        }
        /// <summary>
        /// HandleMonsterMoved 함수를 처리합니다.
        /// </summary>

        private void HandleMonsterMoved(MonsterMovedEvent evt)
        {
            // 핵심 로직을 처리합니다.
            if (evt == null)
            {
                return;
            }

            if (TryGetMonsterMap(evt.PlayerIndex, out var monsterMap)
                && monsterMap.TryGetValue(evt.MonsterUid, out var obj)
                && obj != null)
            {
                var state = obj.GetComponent<MonsterViewState>() ?? obj.AddComponent<MonsterViewState>();
                state.SetBaseScale(_monsterScale);
                state.MarkMoving();
            }
        }
        /// <summary>
        /// RemoveNotSeen 함수를 처리합니다.
        /// </summary>

        private void RemoveNotSeen(Dictionary<long, GameObject> dict)
        {
            // 핵심 로직을 처리합니다.
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
        /// CreateMonsterObject 함수를 처리합니다.
        /// </summary>

        private GameObject CreateMonsterObject()
        {
            // 핵심 로직을 처리합니다.
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
        /// <summary>
        /// InitializeMonsterObject 함수를 처리합니다.
        /// </summary>

        private void InitializeMonsterObject(GameObject obj)
        {
            // 핵심 로직을 처리합니다.
            if (obj == null)
            {
                return;
            }

            obj.transform.localScale = _monsterScale;

            var state = obj.GetComponent<MonsterViewState>() ?? obj.AddComponent<MonsterViewState>();
            state.SetBaseScale(_monsterScale);
        }
        /// <summary>
        /// RemoveMonster 함수를 처리합니다.
        /// </summary>

        private void RemoveMonster(int playerIndex, long uid)
        {
            // 핵심 로직을 처리합니다.
            if (!TryGetMonsterMap(playerIndex, out var monsterMap))
            {
                return;
            }

            if (monsterMap.TryGetValue(uid, out var obj) && obj != null)
            {
                Destroy(obj);
            }

            monsterMap.Remove(uid);
        }
        /// <summary>
        /// ApplyInjectedMonsterTint 함수를 처리합니다.
        /// </summary>

        private static void ApplyInjectedMonsterTint(GameObject obj, Color color)
        {
            // 핵심 로직을 처리합니다.
            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            try
            {
                renderer.material.color = color;
            }
            catch
            {
            }
        }
        /// <summary>
        /// ApplyMonsterHpTint 함수를 처리합니다.
        /// </summary>

        private static void ApplyMonsterHpTint(GameObject obj, float hpRatio)
        {
            // 핵심 로직을 처리합니다.
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
        /// <summary>
        /// GetOrCreateMonsterMap 함수를 처리합니다.
        /// </summary>

        private Dictionary<long, GameObject> GetOrCreateMonsterMap(int playerIndex)
        {
            // 핵심 로직을 처리합니다.
            if (_monsterObjectsByPlayer.TryGetValue(playerIndex, out var monsterMap))
            {
                return monsterMap;
            }

            monsterMap = new Dictionary<long, GameObject>();
            _monsterObjectsByPlayer[playerIndex] = monsterMap;
            return monsterMap;
        }
        /// <summary>
        /// TryGetMonsterMap 함수를 처리합니다.
        /// </summary>

        private bool TryGetMonsterMap(int playerIndex, out Dictionary<long, GameObject> monsterMap)
        {
            // 핵심 로직을 처리합니다.
            return _monsterObjectsByPlayer.TryGetValue(playerIndex, out monsterMap);
        }
        /// <summary>
        /// TryGetMonsterWorldPosition 함수를 처리합니다.
        /// </summary>

        public bool TryGetMonsterWorldPosition(int playerIndex, long monsterUid, out Vector3 worldPosition)
        {
            // 핵심 로직을 처리합니다.
            worldPosition = Vector3.zero;
            if (!TryGetMonsterMap(playerIndex, out var monsterMap))
            {
                return false;
            }

            if (!monsterMap.TryGetValue(monsterUid, out var obj) || obj == null)
            {
                return false;
            }

            worldPosition = obj.transform.position;
            return true;
        }
        /// <summary>
        /// TryGetAnyMonsterWorldPosition 함수를 처리합니다.
        /// </summary>

        public bool TryGetAnyMonsterWorldPosition(int playerIndex, out Vector3 worldPosition)
        {
            // 핵심 로직을 처리합니다.
            worldPosition = Vector3.zero;
            if (!TryGetMonsterMap(playerIndex, out var monsterMap))
            {
                return false;
            }

            foreach (var pair in monsterMap)
            {
                var obj = pair.Value;
                if (obj == null)
                {
                    continue;
                }

                worldPosition = obj.transform.position;
                return true;
            }

            return false;
        }
        /// <summary>
        /// ApplyOffset 함수를 처리합니다.
        /// </summary>

        private Vector3 ApplyOffset(int playerIndex, float x, float y, float z)
        {
            // 핵심 로직을 처리합니다.
            var offset = GameView != null ? GameView.GetPlayerOffsetPosition(playerIndex) : Vector3.zero;
            return new Vector3(x, y, z) + offset;
        }
        /// <summary>
        /// OnShutdown 함수를 처리합니다.
        /// </summary>

        protected override void OnShutdown()
        {
            // 핵심 로직을 처리합니다.
            base.OnShutdown();

            foreach (var playerMap in _monsterObjectsByPlayer.Values)
            {
                foreach (var kv in playerMap)
                {
                    if (kv.Value != null)
                    {
                        Destroy(kv.Value);
                    }
                }
            }

            _monsterObjectsByPlayer.Clear();
        }
    }
}


