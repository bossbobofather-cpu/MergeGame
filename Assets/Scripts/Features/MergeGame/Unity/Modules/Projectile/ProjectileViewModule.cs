using System.Collections.Generic;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;
using UnityEngine;

namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 투사체(Projectile) 뷰 모듈입니다.
    /// 발사 이벤트를 받아 투사체를 생성하고 이동시킵니다.
    /// Throw 타입은 포물선 궤적 + 착지 후 트랩 비주얼을 표현합니다.
    /// 스냅샷 동기화로 Host에서 이미 제거된 투사체를 정리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileViewModule : MergeViewModuleBase
    {
        [Header("Prefab (Optional)")]
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Fallback Visual")]
        [SerializeField] private bool _usePrimitiveFallback = true;
        [SerializeField] private Vector3 _projectileScale = new Vector3(0.25f, 0.25f, 0.25f);

        [Header("Settings")]
        [SerializeField] private float _defaultProjectileSpeed = 8f;

        [Header("Throw Settings")]
        [SerializeField] private float _throwArcHeight = 3f;
        [SerializeField] private Color _trapActiveColor = new Color(1f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color _trapRangeColor = new Color(1f, 0.2f, 0.2f, 0.25f);
        [SerializeField] private float _trapPulseSpeed = 6f;

        private readonly Dictionary<int, Dictionary<long, ProjectileInstance>> _projectileMapByPlayer = new();
        private readonly List<long> _removeBuffer = new();
        private readonly HashSet<long> _snapshotUids = new();
        /// <summary>
        /// OnEventMsg 함수를 처리합니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
            // 핵심 로직을 처리합니다.
            if (evt is not TowerAttackedEvent attackedEvent)
            {
                return;
            }

            if (attackedEvent.AttackType != TowerAttackType.Projectile)
            {
                return;
            }

            var start = ApplyOffset(attackedEvent.PlayerIndex, attackedEvent.AttackerX, attackedEvent.AttackerY, attackedEvent.AttackerZ);
            var target = ApplyOffset(attackedEvent.PlayerIndex, attackedEvent.TargetX, attackedEvent.TargetY, attackedEvent.TargetZ);
            var speed = attackedEvent.ProjectileSpeed > 0f ? attackedEvent.ProjectileSpeed : _defaultProjectileSpeed;

            SpawnProjectile(
                attackedEvent.PlayerIndex,
                attackedEvent.ProjectileUid,
                start,
                target,
                speed,
                attackedEvent.ProjectileType,
                attackedEvent.ThrowRadius);
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

            var projectileMap = GetOrCreateProjectileMap(snapshot.PlayerIndex);
            _snapshotUids.Clear();

            var projectiles = snapshot.Projectiles;
            for (var i = 0; i < projectiles.Count; i++)
            {
                _snapshotUids.Add(projectiles[i].Uid);
            }

            // Host에서 이미 제거된 투사체 정리
            _removeBuffer.Clear();
            foreach (var kv in projectileMap)
            {
                if (!_snapshotUids.Contains(kv.Key))
                {
                    _removeBuffer.Add(kv.Key);
                }
            }

            for (var i = 0; i < _removeBuffer.Count; i++)
            {
                var uid = _removeBuffer[i];
                if (projectileMap.TryGetValue(uid, out var proj))
                {
                    DestroyProjectile(proj);
                }

                projectileMap.Remove(uid);
            }
        }
        /// <summary>
        /// Update 함수를 처리합니다.
        /// </summary>

        private void Update()
        {
            // 핵심 로직을 처리합니다.
            if (_projectileMapByPlayer.Count == 0)
            {
                return;
            }

            var deltaTime = Time.deltaTime;

            foreach (var playerMapPair in _projectileMapByPlayer)
            {
                var projectileMap = playerMapPair.Value;
                if (projectileMap == null || projectileMap.Count == 0)
                {
                    continue;
                }

                _removeBuffer.Clear();
                foreach (var kv in projectileMap)
                {
                    var projectile = kv.Value;
                    if (projectile.Type == ProjectileType.Throw)
                    {
                        UpdateThrowProjectile(projectile, deltaTime);
                    }
                    else
                    {
                        UpdateDirectProjectile(projectile, deltaTime);
                    }

                    if (projectile.ShouldRemove)
                    {
                        _removeBuffer.Add(kv.Key);
                    }
                }

                for (var i = 0; i < _removeBuffer.Count; i++)
                {
                    var uid = _removeBuffer[i];
                    if (projectileMap.TryGetValue(uid, out var projectile))
                    {
                        DestroyProjectile(projectile);
                    }

                    projectileMap.Remove(uid);
                }
            }
        }
        /// <summary>
        /// UpdateDirectProjectile 함수를 처리합니다.
        /// </summary>

        private void UpdateDirectProjectile(ProjectileInstance projectile, float deltaTime)
        {
            // 핵심 로직을 처리합니다.
            projectile.Elapsed += deltaTime;
            var t = projectile.TravelTime <= 0f ? 1f : Mathf.Clamp01(projectile.Elapsed / projectile.TravelTime);
            if (projectile.GameObject != null)
            {
                projectile.GameObject.transform.position = Vector3.Lerp(projectile.Start, projectile.Target, t);
            }
            if (t >= 1f)
            {
                projectile.ShouldRemove = true;
            }
        }
        /// <summary>
        /// UpdateThrowProjectile 함수를 처리합니다.
        /// </summary>

        private void UpdateThrowProjectile(ProjectileInstance projectile, float deltaTime)
        {
            // 핵심 로직을 처리합니다.
            if (!projectile.IsLanded)
            {
                // 비행 단계: 포물선
                projectile.Elapsed += deltaTime;
                var t = projectile.TravelTime <= 0f ? 1f : Mathf.Clamp01(projectile.Elapsed / projectile.TravelTime);

                if (projectile.GameObject != null)
                {
                    var horizontalPos = Vector3.Lerp(projectile.Start, projectile.Target, t);
                    var arcY = projectile.ArcHeight * 4f * t * (1f - t);
                    projectile.GameObject.transform.position =
                        new Vector3(horizontalPos.x, horizontalPos.y + arcY, horizontalPos.z);
                }

                if (t >= 1f)
                {
                    // 착지
                    projectile.IsLanded = true;
                    projectile.LandedTimer = 0f;

                    if (projectile.GameObject != null)
                    {
                        projectile.GameObject.transform.position = projectile.Target;
                        ApplyColor(projectile.GameObject, _trapActiveColor);
                    }

                    if (projectile.ThrowRadius > 0f)
                    {
                        projectile.RangeIndicator = CreateRangeIndicator(projectile.Target, projectile.ThrowRadius);
                    }
                }
            }
            else
            {
                // 트랩 대기 단계: Host가 스냅샷에서 제거할 때까지 비주얼 유지
                projectile.LandedTimer += deltaTime;

                if (projectile.GameObject != null)
                {
                    var pulse = 1f + 0.2f * Mathf.Sin(projectile.LandedTimer * _trapPulseSpeed * Mathf.PI);
                    projectile.GameObject.transform.localScale = projectile.BaseScale * pulse;
                }
            }
        }
        /// <summary>
        /// SpawnProjectile 함수를 처리합니다.
        /// </summary>

        private void SpawnProjectile(int playerIndex, long uid, Vector3 start, Vector3 target, float speed, ProjectileType projectileType, float throwRadius = 0f)
        {
            // 핵심 로직을 처리합니다.
            var projectileMap = GetOrCreateProjectileMap(playerIndex);

            // 이미 같은 UID의 투사체가 있으면 무시
            if (uid != 0 && projectileMap.ContainsKey(uid))
            {
                return;
            }

            var obj = CreateProjectileObject();
            if (obj == null)
            {
                return;
            }

            obj.transform.SetParent(transform, false);
            obj.transform.position = start;
            obj.transform.localScale = _projectileScale;
            obj.name = projectileType == ProjectileType.Throw
                ? $"Projectile_P{playerIndex}_Throw_{uid}"
                : $"Projectile_P{playerIndex}_Direct_{uid}";

            ApplyProjectileTint(obj, projectileType);
            var distance = Vector3.Distance(start, target);
            var travelTime = speed <= 0f ? 0f : distance / speed;

            var instance = new ProjectileInstance
            {
                GameObject = obj,
                Start = start,
                Target = target,
                TravelTime = travelTime,
                Elapsed = 0f,
                Type = projectileType,
                ArcHeight = projectileType == ProjectileType.Throw ? _throwArcHeight : 0f,
                ThrowRadius = throwRadius,
                IsLanded = false,
                LandedTimer = 0f,
                BaseScale = _projectileScale,
                ShouldRemove = false
            };

            projectileMap[uid] = instance;
        }
        /// <summary>
        /// CreateProjectileObject 함수를 처리합니다.
        /// </summary>

        private GameObject CreateProjectileObject()
        {
            // 핵심 로직을 처리합니다.
            if (_projectilePrefab != null)
            {
                return Instantiate(_projectilePrefab);
            }

            if (_usePrimitiveFallback)
            {
                return GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }

            return new GameObject("Projectile");
        }
        /// <summary>
        /// ApplyProjectileTint 함수를 처리합니다.
        /// </summary>

        private static void ApplyProjectileTint(GameObject obj, ProjectileType projectileType)
        {
            // 핵심 로직을 처리합니다.
            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var color = projectileType == ProjectileType.Throw
                ? new Color(1f, 0.45f, 0.2f, 1f)
                : new Color(0.4f, 0.8f, 1f, 1f);

            try
            {
                renderer.material.color = color;
            }
            catch
            {
            }
        }
        /// <summary>
        /// CreateRangeIndicator 함수를 처리합니다.
        /// </summary>

        private GameObject CreateRangeIndicator(Vector3 position, float radius)
        {
            // 핵심 로직을 처리합니다.
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(indicator.GetComponent<Collider>());
            indicator.transform.SetParent(transform, false);

            var diameter = radius * 2f;
            indicator.transform.position = position;
            indicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);
            indicator.name = "TrapRange";

            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = renderer.material;
                mat.color = _trapRangeColor;
            }

            return indicator;
        }
        /// <summary>
        /// ApplyColor 함수를 처리합니다.
        /// </summary>

        private static void ApplyColor(GameObject obj, Color color)
        {
            // 핵심 로직을 처리합니다.
            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null) return;
            try
            {
                renderer.material.color = color;
            }
            catch
            {
            }
        }
        /// <summary>
        /// DestroyProjectile 함수를 처리합니다.
        /// </summary>

        private void DestroyProjectile(ProjectileInstance proj)
        {
            // 핵심 로직을 처리합니다.
            if (proj.RangeIndicator != null)
            {
                Destroy(proj.RangeIndicator);
            }

            if (proj.GameObject != null)
            {
                Destroy(proj.GameObject);
            }
        }
        /// <summary>
        /// GetOrCreateProjectileMap 함수를 처리합니다.
        /// </summary>

        private Dictionary<long, ProjectileInstance> GetOrCreateProjectileMap(int playerIndex)
        {
            // 핵심 로직을 처리합니다.
            if (_projectileMapByPlayer.TryGetValue(playerIndex, out var projectileMap))
            {
                return projectileMap;
            }

            projectileMap = new Dictionary<long, ProjectileInstance>();
            _projectileMapByPlayer[playerIndex] = projectileMap;
            return projectileMap;
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

            foreach (var playerMap in _projectileMapByPlayer.Values)
            {
                foreach (var kv in playerMap)
                {
                    DestroyProjectile(kv.Value);
                }
            }

            _projectileMapByPlayer.Clear();
        }

        private sealed class ProjectileInstance
        {
            public GameObject GameObject;
            public Vector3 Start;
            public Vector3 Target;
            public float TravelTime;
            public float Elapsed;
            public ProjectileType Type;
            public float ArcHeight;
            public float ThrowRadius;
            public GameObject RangeIndicator;
            public bool IsLanded;
            public float LandedTimer;
            public Vector3 BaseScale;
            public bool ShouldRemove;
        }
    }
}
