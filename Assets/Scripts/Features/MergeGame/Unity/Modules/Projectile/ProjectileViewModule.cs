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

        private readonly Dictionary<long, ProjectileInstance> _projectileMap = new();
        private readonly List<long> _removeBuffer = new();
        private readonly HashSet<long> _snapshotUids = new();

        public override void OnEventMsg(MergeGameEvent evt)
        {
            if (!IsMyEvent(evt))
            {
                return;
            }
            if (evt is not TowerAttackedEvent attackedEvent)
            {
                return;
            }
            if (attackedEvent.AttackType != TowerAttackType.Projectile)
            {
                return;
            }
            var start = new Vector3(attackedEvent.AttackerX, attackedEvent.AttackerY, attackedEvent.AttackerZ);
            var target = new Vector3(attackedEvent.TargetX, attackedEvent.TargetY, attackedEvent.TargetZ);
            var speed = attackedEvent.ProjectileSpeed > 0f ? attackedEvent.ProjectileSpeed : _defaultProjectileSpeed;
            SpawnProjectile(attackedEvent.ProjectileUid, start, target, speed, attackedEvent.ProjectileType, attackedEvent.ThrowRadius);
        }

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
            if (snapshot == null || !IsMySnapshot(snapshot)) return;

            _snapshotUids.Clear();

            var projectiles = snapshot.Projectiles;
            for (var i = 0; i < projectiles.Count; i++)
            {
                _snapshotUids.Add(projectiles[i].Uid);
            }

            // Host에서 이미 제거된 투사체 정리
            _removeBuffer.Clear();
            foreach (var kv in _projectileMap)
            {
                if (!_snapshotUids.Contains(kv.Key))
                {
                    _removeBuffer.Add(kv.Key);
                }
            }

            for (var i = 0; i < _removeBuffer.Count; i++)
            {
                var uid = _removeBuffer[i];
                if (_projectileMap.TryGetValue(uid, out var proj))
                {
                    DestroyProjectile(proj);
                }
                _projectileMap.Remove(uid);
            }
        }

        private void Update()
        {
            if (_projectileMap.Count == 0)
            {
                return;
            }
            var deltaTime = Time.deltaTime;
            _removeBuffer.Clear();
            foreach (var kv in _projectileMap)
            {
                var projectile = kv.Value;
                if (projectile.Type == ProjectileType.Throw)
                    UpdateThrowProjectile(projectile, deltaTime);
                else
                    UpdateDirectProjectile(projectile, deltaTime);

                if (projectile.ShouldRemove)
                {
                    _removeBuffer.Add(kv.Key);
                }
            }
            for (var i = 0; i < _removeBuffer.Count; i++)
            {
                var uid = _removeBuffer[i];
                if (_projectileMap.TryGetValue(uid, out var projectile))
                {
                    DestroyProjectile(projectile);
                }
                _projectileMap.Remove(uid);
            }
        }

        private void UpdateDirectProjectile(ProjectileInstance projectile, float deltaTime)
        {
            projectile.Elapsed += deltaTime;
            var t = projectile.TravelTime <= 0f ? 1f : Mathf.Clamp01(projectile.Elapsed / projectile.TravelTime);
            if (projectile.GameObject != null)
            {
                projectile.GameObject.transform.localPosition = Vector3.Lerp(projectile.Start, projectile.Target, t);
            }
            if (t >= 1f)
            {
                projectile.ShouldRemove = true;
            }
        }

        private void UpdateThrowProjectile(ProjectileInstance projectile, float deltaTime)
        {
            if (!projectile.IsLanded)
            {
                // 비행 단계: 포물선
                projectile.Elapsed += deltaTime;
                var t = projectile.TravelTime <= 0f ? 1f : Mathf.Clamp01(projectile.Elapsed / projectile.TravelTime);

                if (projectile.GameObject != null)
                {
                    var horizontalPos = Vector3.Lerp(projectile.Start, projectile.Target, t);
                    var arcY = projectile.ArcHeight * 4f * t * (1f - t);
                    projectile.GameObject.transform.localPosition =
                        new Vector3(horizontalPos.x, horizontalPos.y + arcY, horizontalPos.z);
                }

                if (t >= 1f)
                {
                    // 착지
                    projectile.IsLanded = true;
                    projectile.LandedTimer = 0f;

                    if (projectile.GameObject != null)
                    {
                        projectile.GameObject.transform.localPosition = projectile.Target;
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

        private void SpawnProjectile(long uid, Vector3 start, Vector3 target, float speed, ProjectileType projectileType, float throwRadius = 0f)
        {
            // 이미 같은 UID의 투사체가 있으면 무시
            if (uid != 0 && _projectileMap.ContainsKey(uid))
            {
                return;
            }

            var obj = CreateProjectileObject();
            if (obj == null)
            {
                return;
            }
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = start;
            obj.transform.localScale = _projectileScale;
            obj.name = projectileType == ProjectileType.Throw ? "Projectile_Throw" : "Projectile_Direct";
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

            _projectileMap[uid] = instance;
        }
        private GameObject CreateProjectileObject()
        {
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
        private static void ApplyProjectileTint(GameObject obj, ProjectileType projectileType)
        {
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

        private GameObject CreateRangeIndicator(Vector3 position, float radius)
        {
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(indicator.GetComponent<Collider>());
            indicator.transform.SetParent(transform, false);
            var diameter = radius * 2f;
            indicator.transform.localPosition = position;
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

        private static void ApplyColor(GameObject obj, Color color)
        {
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

        private void DestroyProjectile(ProjectileInstance proj)
        {
            if (proj.RangeIndicator != null)
            {
                Destroy(proj.RangeIndicator);
            }
            if (proj.GameObject != null)
            {
                Destroy(proj.GameObject);
            }
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            foreach (var kv in _projectileMap)
            {
                DestroyProjectile(kv.Value);
            }
            _projectileMap.Clear();
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


