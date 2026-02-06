using System;
using System.Collections.Generic;
using MyProject.MergeGame.Models;
using Noname.GameAbilitySystem;

namespace MyProject.MergeGame.Systems
{
    /// <summary>
    /// 타워 공격 및 투사체 처리를 담당하는 전투 시스템입니다.
    /// 공격 판정은 Host에서만 수행합니다.
    /// </summary>
    public sealed class MergeCombatSystem
    {
        private sealed class PendingProjectile
        {
            public long AttackerUid;
            public long TargetUid;
            public AbilitySystemComponent OwnerAsc;
            public ITargetingStrategy TargetingStrategy;
            public Point3D Start;
            public Point3D Impact;
            public float RemainingTime;
            public float Damage;
            public ProjectileType ProjectileType;
            public float ThrowRadius;
        }

        private readonly MergeHostState _state;
        private readonly TargetContext _targetContext;
        private readonly List<AbilitySystemComponent> _monsterAscList = new();
        private readonly List<PendingProjectile> _projectiles = new();
        private readonly List<PendingProjectile> _projectileRemoveBuffer = new();

        public TargetContext TargetContext => _targetContext;

        public MergeCombatSystem(MergeHostState state, float defaultAttackRange = 10f)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _targetContext = new TargetContext(GetEnemiesForTower, GetAlliesForTower, GetPositionForAsc);
        }

        /// <summary>
        /// 타워의 능력 활성화 결과(TargetData)를 바탕으로 공격을 실행합니다.
        /// </summary>
        public void ExecuteTowerAttack(
            long tick,
            MergeTower tower,
            GameplayAbility ability,
            TargetData targetData,
            List<MergeHostEvent> events)
        {
            if (tower == null || targetData == null || events == null)
            {
                return;
            }

            var attackDamage = tower.ASC.Get(AttributeId.AttackDamage);
            if (attackDamage <= 0f)
            {
                return;
            }

            if (tower.AttackType == TowerAttackType.HitScan)
            {
                ProcessHitScanAttack(tick, tower, attackDamage, targetData, events);
                return;
            }

            ProcessProjectileAttack(tick, tower, ability, attackDamage, targetData, events);
        }

        /// <summary>
        /// 투사체 이동 및 충돌 처리를 업데이트합니다.
        /// </summary>
        public void TickProjectiles(long tick, float deltaTime, List<MergeHostEvent> events)
        {
            if (events == null || _projectiles.Count == 0)
            {
                return;
            }

            _projectileRemoveBuffer.Clear();

            for (var i = 0; i < _projectiles.Count; i++)
            {
                var projectile = _projectiles[i];
                projectile.RemainingTime -= deltaTime;
                if (projectile.RemainingTime > 0f)
                {
                    continue;
                }

                if (projectile.ProjectileType == ProjectileType.Throw)
                {
                    ResolveThrowHit(tick, projectile, events);
                }
                else
                {
                    ResolveDirectHit(tick, projectile, events);
                }

                _projectileRemoveBuffer.Add(projectile);
            }

            for (var i = 0; i < _projectileRemoveBuffer.Count; i++)
            {
                _projectiles.Remove(_projectileRemoveBuffer[i]);
            }
        }

        private void ProcessHitScanAttack(
            long tick,
            MergeTower tower,
            float attackDamage,
            TargetData targetData,
            List<MergeHostEvent> events)
        {
            foreach (var targetAsc in targetData.Targets)
            {
                var monster = FindMonsterByAsc(targetAsc);
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                monster.TakeDamage(attackDamage);

                events.Add(new TowerAttackedEvent(
                    tick,
                    tower.Uid,
                    monster.Uid,
                    attackDamage,
                    tower.Position.X,
                    tower.Position.Y,
                    tower.Position.Z,
                    monster.Position.X,
                    monster.Position.Y,
                    monster.Position.Z,
                    TowerAttackType.HitScan,
                    tower.ProjectileType,
                    tower.ProjectileSpeed,
                    tower.ThrowRadius
                ));

                events.Add(new MonsterDamagedEvent(
                    tick,
                    monster.Uid,
                    attackDamage,
                    monster.ASC.Get(AttributeId.Health),
                    tower.Uid
                ));
            }
        }

        private void ProcessProjectileAttack(
            long tick,
            MergeTower tower,
            GameplayAbility ability,
            float attackDamage,
            TargetData targetData,
            List<MergeHostEvent> events)
        {
            if (tower.ProjectileType == ProjectileType.Throw)
            {
                var throwPoint = ResolveThrowPoint(tower, targetData);

                EnqueueProjectile(
                    tower,
                    ability,
                    targetUid: 0,
                    impact: throwPoint,
                    damage: attackDamage,
                    projectileType: ProjectileType.Throw,
                    throwRadius: tower.ThrowRadius);

                events.Add(new TowerAttackedEvent(
                    tick,
                    tower.Uid,
                    0,
                    attackDamage,
                    tower.Position.X,
                    tower.Position.Y,
                    tower.Position.Z,
                    throwPoint.X,
                    throwPoint.Y,
                    throwPoint.Z,
                    TowerAttackType.Projectile,
                    ProjectileType.Throw,
                    tower.ProjectileSpeed,
                    tower.ThrowRadius
                ));

                return;
            }

            foreach (var targetAsc in targetData.Targets)
            {
                var monster = FindMonsterByAsc(targetAsc);
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                var impact = monster.Position;

                EnqueueProjectile(
                    tower,
                    ability,
                    monster.Uid,
                    impact,
                    attackDamage,
                    ProjectileType.Direct,
                    0f);

                events.Add(new TowerAttackedEvent(
                    tick,
                    tower.Uid,
                    monster.Uid,
                    attackDamage,
                    tower.Position.X,
                    tower.Position.Y,
                    tower.Position.Z,
                    impact.X,
                    impact.Y,
                    impact.Z,
                    TowerAttackType.Projectile,
                    ProjectileType.Direct,
                    tower.ProjectileSpeed,
                    tower.ThrowRadius
                ));
            }
        }

        private void EnqueueProjectile(
            MergeTower tower,
            GameplayAbility ability,
            long targetUid,
            Point3D impact,
            float damage,
            ProjectileType projectileType,
            float throwRadius)
        {
            var speed = tower.ProjectileSpeed > 0f ? tower.ProjectileSpeed : 1f;
            var distance = MathF.Sqrt(Point3D.DistanceSquared(tower.Position, impact));
            var travelTime = speed <= 0f ? 0f : distance / speed;

            _projectiles.Add(new PendingProjectile
            {
                AttackerUid = tower.Uid,
                TargetUid = targetUid,
                OwnerAsc = tower.ASC,
                TargetingStrategy = ability?.TargetingStrategy,
                Start = tower.Position,
                Impact = impact,
                RemainingTime = travelTime,
                Damage = damage,
                ProjectileType = projectileType,
                ThrowRadius = throwRadius
            });
        }

        private void ResolveDirectHit(long tick, PendingProjectile projectile, List<MergeHostEvent> events)
        {
            var monster = _state.GetMonster(projectile.TargetUid);
            if (monster == null || !monster.IsAlive)
            {
                return;
            }

            monster.TakeDamage(projectile.Damage);

            events.Add(new MonsterDamagedEvent(
                tick,
                monster.Uid,
                projectile.Damage,
                monster.ASC.Get(AttributeId.Health),
                projectile.AttackerUid
            ));
        }

        private void ResolveThrowHit(long tick, PendingProjectile projectile, List<MergeHostEvent> events)
        {
            var targets = ResolveThrowTargets(projectile);
            if (targets.Count == 0)
            {
                return;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                var targetAsc = targets[i];
                var monster = FindMonsterByAsc(targetAsc);
                if (monster == null || !monster.IsAlive)
                {
                    continue;
                }

                monster.TakeDamage(projectile.Damage);

                events.Add(new MonsterDamagedEvent(
                    tick,
                    monster.Uid,
                    projectile.Damage,
                    monster.ASC.Get(AttributeId.Health),
                    projectile.AttackerUid
                ));
            }
        }

        private IReadOnlyList<AbilitySystemComponent> ResolveThrowTargets(PendingProjectile projectile)
        {
            if (projectile.OwnerAsc != null && projectile.TargetingStrategy != null)
            {
                var impactContext = CreateImpactContext(projectile.OwnerAsc, projectile.Impact);
                var targetData = projectile.TargetingStrategy.FindTargets(projectile.OwnerAsc, impactContext);
                if (targetData != null && targetData.Targets.Count > 0)
                {
                    return targetData.Targets;
                }
            }

            // 기본 fallback: Throw 반경 내 모든 몬스터
            _monsterAscList.Clear();
            var radiusSq = projectile.ThrowRadius * projectile.ThrowRadius;
            foreach (var monster in _state.Monsters.Values)
            {
                if (!monster.IsAlive)
                {
                    continue;
                }

                var distSq = Point3D.DistanceSquared(projectile.Impact, monster.Position);
                if (distSq <= radiusSq)
                {
                    _monsterAscList.Add(monster.ASC);
                }
            }

            return _monsterAscList;
        }

        private TargetContext CreateImpactContext(AbilitySystemComponent owner, Point3D impact)
        {
            return new TargetContext(
                getEnemies: GetEnemiesForTower,
                getAllies: GetAlliesForTower,
                getPosition: asc => asc == owner ? impact : GetPositionForAsc(asc),
                random: _targetContext.Random);
        }

        private Point3D ResolveThrowPoint(MergeTower tower, TargetData targetData)
        {
            if (targetData != null && targetData.Targets.Count > 0)
            {
                var first = targetData.Targets[0];
                var monster = FindMonsterByAsc(first);
                if (monster != null)
                {
                    return monster.Position;
                }
            }

            return tower.Position;
        }

        private IReadOnlyList<AbilitySystemComponent> GetEnemiesForTower(AbilitySystemComponent owner)
        {
            _monsterAscList.Clear();

            foreach (var monster in _state.Monsters.Values)
            {
                if (monster.IsAlive)
                {
                    _monsterAscList.Add(monster.ASC);
                }
            }

            return _monsterAscList;
        }

        private IReadOnlyList<AbilitySystemComponent> GetAlliesForTower(AbilitySystemComponent owner)
        {
            return Array.Empty<AbilitySystemComponent>();
        }

        private Point3D GetPositionForAsc(AbilitySystemComponent asc)
        {
            if (asc == null)
            {
                return Point3D.zero;
            }

            if (asc.Owner is MergeTower tower)
            {
                return tower.Position;
            }

            if (asc.Owner is MergeMonster monster)
            {
                return monster.Position;
            }

            return Point3D.zero;
        }

        private MergeMonster FindMonsterByAsc(AbilitySystemComponent asc)
        {
            if (asc?.Owner is MergeMonster monster)
            {
                return monster;
            }

            return null;
        }
    }
}
