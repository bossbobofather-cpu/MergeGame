using System.Collections.Generic;
using MyProject.MergeGame.Events;
using MyProject.MergeGame.Snapshots;
using UnityEngine;
using UnityEngine.Rendering;

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
        [SerializeField] private float _trapRangeHeightOffset = 0.05f;
        [SerializeField] private float _trapRangeThickness = 0.02f;

        private readonly Dictionary<int, Dictionary<long, ProjectileInstance>> _projectileMapByPlayer = new();
        private readonly List<long> _removeBuffer = new();
        private readonly HashSet<long> _snapshotUids = new();
        private Material _trapRangeMaterial;
        /// <summary>
        /// OnEventMsg 메서드입니다.
        /// </summary>

        public override void OnEventMsg(MergeGameEvent evt)
        {
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
        /// OnSnapshotMsg 메서드입니다.
        /// </summary>

        public override void OnSnapshotMsg(MergeHostSnapshot snapshot)
        {
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
        /// Update 메서드입니다.
        /// </summary>

        private void Update()
        {
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
        /// UpdateDirectProjectile 메서드입니다.
        /// </summary>

        private void UpdateDirectProjectile(ProjectileInstance projectile, float deltaTime)
        {
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
        /// UpdateThrowProjectile 메서드입니다.
        /// </summary>

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
        /// SpawnProjectile 메서드입니다.
        /// </summary>

        private void SpawnProjectile(int playerIndex, long uid, Vector3 start, Vector3 target, float speed, ProjectileType projectileType, float throwRadius = 0f)
        {
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
        /// CreateProjectileObject 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// ApplyProjectileTint 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// CreateRangeIndicator 메서드입니다.
        /// </summary>

        private GameObject CreateRangeIndicator(Vector3 position, float radius)
        {
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(indicator.GetComponent<Collider>());
            indicator.transform.SetParent(transform, false);

            var diameter = radius * 2f;
            indicator.transform.position = position + new Vector3(0f, Mathf.Max(0f, _trapRangeHeightOffset), 0f);
            indicator.transform.localScale = new Vector3(diameter, Mathf.Max(0.001f, _trapRangeThickness), diameter);
            indicator.name = "TrapRange";

            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sharedMaterial = GetOrCreateTrapRangeMaterial();
            }

            return indicator;
        }
        /// <summary>
        /// GetOrCreateTrapRangeMaterial 메서드입니다.
        /// </summary>

        private Material GetOrCreateTrapRangeMaterial()
        {
            if (_trapRangeMaterial != null)
            {
                _trapRangeMaterial.color = _trapRangeColor;
                return _trapRangeMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Standard");

            _trapRangeMaterial = shader != null
                ? new Material(shader)
                : new Material(Shader.Find("Sprites/Default"));

            _trapRangeMaterial.name = "TrapRangeMaterial(Runtime)";
            _trapRangeMaterial.color = _trapRangeColor;
            _trapRangeMaterial.renderQueue = (int)RenderQueue.Transparent;

            if (_trapRangeMaterial.HasProperty("_Surface"))
            {
                _trapRangeMaterial.SetFloat("_Surface", 1f);
            }
            if (_trapRangeMaterial.HasProperty("_ZWrite"))
            {
                _trapRangeMaterial.SetFloat("_ZWrite", 0f);
            }
            if (_trapRangeMaterial.HasProperty("_SrcBlend"))
            {
                _trapRangeMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            }
            if (_trapRangeMaterial.HasProperty("_DstBlend"))
            {
                _trapRangeMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            }

            return _trapRangeMaterial;
        }
        /// <summary>
        /// ApplyColor 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// DestroyProjectile 메서드입니다.
        /// </summary>

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
        /// <summary>
        /// GetOrCreateProjectileMap 메서드입니다.
        /// </summary>

        private Dictionary<long, ProjectileInstance> GetOrCreateProjectileMap(int playerIndex)
        {
            if (_projectileMapByPlayer.TryGetValue(playerIndex, out var projectileMap))
            {
                return projectileMap;
            }

            projectileMap = new Dictionary<long, ProjectileInstance>();
            _projectileMapByPlayer[playerIndex] = projectileMap;
            return projectileMap;
        }
        /// <summary>
        /// ApplyOffset 메서드입니다.
        /// </summary>

        private Vector3 ApplyOffset(int playerIndex, float x, float y, float z)
        {
            var offset = GameView != null ? GameView.GetPlayerOffsetPosition(playerIndex) : Vector3.zero;
            return new Vector3(x, y, z) + offset;
        }
        /// <summary>
        /// OnShutdown 메서드입니다.
        /// </summary>

        protected override void OnShutdown()
        {
            base.OnShutdown();

            foreach (var playerMap in _projectileMapByPlayer.Values)
            {
                foreach (var kv in playerMap)
                {
                    DestroyProjectile(kv.Value);
                }
            }

            _projectileMapByPlayer.Clear();

            if (_trapRangeMaterial != null)
            {
                Destroy(_trapRangeMaterial);
                _trapRangeMaterial = null;
            }
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
