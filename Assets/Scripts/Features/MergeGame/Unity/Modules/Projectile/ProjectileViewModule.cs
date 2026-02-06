using System.Collections.Generic;
using UnityEngine;
namespace MyProject.MergeGame.Unity
{
    /// <summary>
    /// 투사체(Projectile) 뷰 모듈입니다.
    /// 발사 이벤트를 받아 투사체를 생성하고 이동시킵니다.
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
        private readonly List<ProjectileInstance> _projectiles = new();
        private readonly List<ProjectileInstance> _removeBuffer = new();
        public override void OnHostEvent(MergeHostEvent evt)
        {
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
            SpawnProjectile(start, target, speed, attackedEvent.ProjectileType);
        }
        private void Update()
        {
            if (_projectiles.Count == 0)
            {
                return;
            }
            var deltaTime = Time.deltaTime;
            _removeBuffer.Clear();
            for (var i = 0; i < _projectiles.Count; i++)
            {
                var projectile = _projectiles[i];
                projectile.Elapsed += deltaTime;
                var t = projectile.TravelTime <= 0f ? 1f : Mathf.Clamp01(projectile.Elapsed / projectile.TravelTime);
                if (projectile.GameObject != null)
                {
                    projectile.GameObject.transform.localPosition = Vector3.Lerp(projectile.Start, projectile.Target, t);
                }
                if (t >= 1f)
                {
                    _removeBuffer.Add(projectile);
                }
            }
            for (var i = 0; i < _removeBuffer.Count; i++)
            {
                var projectile = _removeBuffer[i];
                if (projectile.GameObject != null)
                {
                    Destroy(projectile.GameObject);
                }
                _projectiles.Remove(projectile);
            }
        }
        private void SpawnProjectile(Vector3 start, Vector3 target, float speed, ProjectileType projectileType)
        {
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
            _projectiles.Add(new ProjectileInstance
            {
                GameObject = obj,
                Start = start,
                Target = target,
                TravelTime = travelTime,
                Elapsed = 0f
            });
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
        protected override void OnShutdown()
        {
            base.OnShutdown();
            for (var i = 0; i < _projectiles.Count; i++)
            {
                if (_projectiles[i].GameObject != null)
                {
                    Destroy(_projectiles[i].GameObject);
                }
            }
            _projectiles.Clear();
        }
        private sealed class ProjectileInstance
        {
            public GameObject GameObject;
            public Vector3 Start;
            public Vector3 Target;
            public float TravelTime;
            public float Elapsed;
        }
    }
}

