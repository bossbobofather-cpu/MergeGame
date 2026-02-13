using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 자기 자신을 타겟으로 선택합니다.
    /// </summary>
    public sealed class SelfTargetingStrategy : ITargetingStrategy
    {
        /// <summary>
        /// FindTargets 함수를 처리합니다.
        /// </summary>
        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            // 핵심 로직을 처리합니다.
            var data = new TargetData(owner);
            data.AddTarget(owner);
            return data;
        }
    }

    /// <summary>
    /// 적 중에서 랜덤 1명을 선택합니다.
    /// </summary>
    public sealed class RandomTargetingStrategy : ITargetingStrategy
    {
        /// <summary>
        /// FindTargets 함수를 처리합니다.
        /// </summary>
        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            // 핵심 로직을 처리합니다.
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var index = context.Random.Next(0, enemies.Count);
            data.AddTarget(enemies[index]);
            return data;
        }
    }

    /// <summary>
    /// 가장 가까운 적 1명을 선택합니다.
    /// </summary>
    public sealed class NearestEnemyTargetingStrategy : ITargetingStrategy
    {
        private readonly float _maxRange;

        public NearestEnemyTargetingStrategy(float maxRange = float.PositiveInfinity)
        {
            _maxRange = maxRange;
        }
        /// <summary>
        /// FindTargets 함수를 처리합니다.
        /// </summary>

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            // 핵심 로직을 처리합니다.
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var origin = context.ResolvePosition(owner);
            var maxRangeSq = _maxRange * _maxRange;
            var bestDistance = float.PositiveInfinity;
            AbilitySystemComponent best = null;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var pos = context.ResolvePosition(enemy);
                var distance = Point3D.DistanceSquared(origin, pos);
                if (distance > maxRangeSq)
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            data.AddTarget(best);
            return data;
        }
    }

    /// <summary>
    /// 체력이 가장 낮은 적을 선택합니다.
    /// </summary>
    public sealed class LowestHpTargetingStrategy : ITargetingStrategy
    {
        private readonly AttributeId _healthId;

        public LowestHpTargetingStrategy(AttributeId healthId)
        {
            _healthId = healthId;
        }

        public LowestHpTargetingStrategy() : this(AttributeId.Health) { }
        /// <summary>
        /// FindTargets 함수를 처리합니다.
        /// </summary>

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            // 핵심 로직을 처리합니다.
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            AbilitySystemComponent best = null;
            var lowestHp = float.PositiveInfinity;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var hp = enemy.Get(_healthId);
                if (hp < lowestHp)
                {
                    lowestHp = hp;
                    best = enemy;
                }
            }

            data.AddTarget(best);
            return data;
        }
    }

    /// <summary>
    /// 가까운 적 N명을 선택합니다.
    /// </summary>
    public sealed class NearestNEnemiesTargetingStrategy : ITargetingStrategy
    {
        private readonly int _maxTargets;
        private readonly float _maxRange;

        public NearestNEnemiesTargetingStrategy(int maxTargets, float maxRange = float.PositiveInfinity)
        {
            _maxTargets = Math.Max(1, maxTargets);
            _maxRange = maxRange;
        }
        /// <summary>
        /// FindTargets 함수를 처리합니다.
        /// </summary>

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            // 핵심 로직을 처리합니다.
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var origin = context.ResolvePosition(owner);
            var maxRangeSq = _maxRange * _maxRange;

            // 거리 배열 구성
            var distances = new (AbilitySystemComponent enemy, float distSq)[enemies.Count];
            var validCount = 0;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var pos = context.ResolvePosition(enemy);
                var distSq = Point3D.DistanceSquared(origin, pos);
                if (distSq <= maxRangeSq)
                {
                    distances[validCount++] = (enemy, distSq);
                }
            }

            var addExtraTargetCount = Math.Min(10, _maxTargets + owner.Get(AttributeId.ExtraTargetCount));

            // 가까운 순서로 정렬하면서 추출
            for (var i = 0; i < Math.Min(addExtraTargetCount, validCount); i++)
            {
                var minIdx = i;
                for (var j = i + 1; j < validCount; j++)
                {
                    if (distances[j].distSq < distances[minIdx].distSq)
                    {
                        minIdx = j;
                    }
                }
                if (minIdx != i)
                {
                    (distances[i], distances[minIdx]) = (distances[minIdx], distances[i]);
                }
                data.AddTarget(distances[i].enemy);
            }

            return data;
        }
    }

    /// <summary>
    /// 반경 내 모든 적을 선택합니다.
    /// </summary>
    public sealed class AreaTargetingStrategy : ITargetingStrategy
    {
        private readonly float _radius;

        public AreaTargetingStrategy(float radius)
        {
            _radius = Math.Max(0f, radius);
        }
        /// <summary>
        /// FindTargets 함수를 처리합니다.
        /// </summary>

        public TargetData FindTargets(AbilitySystemComponent owner, TargetContext context)
        {
            // 핵심 로직을 처리합니다.
            var data = new TargetData(owner);
            if (owner == null || context == null)
            {
                return data;
            }

            var enemies = context.ResolveEnemies(owner);
            if (enemies.Count == 0)
            {
                return data;
            }

            var origin = context.ResolvePosition(owner);
            var rangeSq = _radius * _radius;

            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var pos = context.ResolvePosition(enemy);
                var distance = Point3D.DistanceSquared(origin, pos);
                if (distance <= rangeSq)
                {
                    data.AddTarget(enemy);
                }
            }

            return data;
        }
    }
}
