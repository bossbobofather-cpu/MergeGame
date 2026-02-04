namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?€κ²ν… ?„λµ ?©ν† λ¦¬μ…?λ‹¤.
    /// </summary>
    public static class TargetingStrategyFactory
    {
        /// <summary>
        /// enum ?€?…κ³Ό ?λΌλ―Έν„°λ΅??„λµ ?Έμ¤?΄μ¤λ¥??μ„±?©λ‹??
        /// </summary>
        public static ITargetingStrategy Create(
            TargetingStrategyType type,
            float maxRange = float.PositiveInfinity,
            int maxTargets = 1,
            float radius = 0f)
        {
            return type switch
            {
                TargetingStrategyType.Self => new SelfTargetingStrategy(),
                TargetingStrategyType.Random => new RandomTargetingStrategy(),
                TargetingStrategyType.NearestEnemy => new NearestEnemyTargetingStrategy(maxRange),
                TargetingStrategyType.NearestN => new NearestNEnemiesTargetingStrategy(maxTargets, maxRange),
                TargetingStrategyType.LowestHp => new LowestHpTargetingStrategy(),
                TargetingStrategyType.Area => new AreaTargetingStrategy(radius),
                _ => null
            };
        }

        /// <summary>
        /// λ¬Έμ???€?…λª…κ³??λΌλ―Έν„°λ΅??„λµ ?Έμ¤?΄μ¤λ¥??μ„±?©λ‹?? (JSON ??§?¬ν™”??
        /// </summary>
        public static ITargetingStrategy Create(
            string typeName,
            float maxRange = float.PositiveInfinity,
            int maxTargets = 1,
            float radius = 0f)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            return typeName.ToLowerInvariant() switch
            {
                "self" => new SelfTargetingStrategy(),
                "random" => new RandomTargetingStrategy(),
                "nearestenemy" => new NearestEnemyTargetingStrategy(maxRange),
                "nearestn" => new NearestNEnemiesTargetingStrategy(maxTargets, maxRange),
                "lowesthp" => new LowestHpTargetingStrategy(),
                "area" => new AreaTargetingStrategy(radius),
                "none" => null,
                _ => null
            };
        }

        /// <summary>
        /// ?„λµ ?Έμ¤?΄μ¤?μ„ ?€??enum??λ°ν™?©λ‹??
        /// </summary>
        public static TargetingStrategyType GetStrategyType(ITargetingStrategy strategy)
        {
            return strategy switch
            {
                SelfTargetingStrategy => TargetingStrategyType.Self,
                RandomTargetingStrategy => TargetingStrategyType.Random,
                NearestEnemyTargetingStrategy => TargetingStrategyType.NearestEnemy,
                NearestNEnemiesTargetingStrategy => TargetingStrategyType.NearestN,
                LowestHpTargetingStrategy => TargetingStrategyType.LowestHp,
                AreaTargetingStrategy => TargetingStrategyType.Area,
                _ => TargetingStrategyType.None
            };
        }

        /// <summary>
        /// ?„λµ ?Έμ¤?΄μ¤?μ„ ?€?…λª… λ¬Έμ?΄μ„ λ°ν™?©λ‹?? (JSON μ§λ ¬?”μ©)
        /// </summary>
        public static string GetStrategyTypeName(ITargetingStrategy strategy)
        {
            return strategy switch
            {
                SelfTargetingStrategy => "Self",
                RandomTargetingStrategy => "Random",
                NearestEnemyTargetingStrategy => "NearestEnemy",
                NearestNEnemiesTargetingStrategy => "NearestN",
                LowestHpTargetingStrategy => "LowestHp",
                AreaTargetingStrategy => "Area",
                _ => null
            };
        }
    }
}

