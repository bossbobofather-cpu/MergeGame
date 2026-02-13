namespace Noname.GameAbilitySystem
{
    public static class AttributeCalculatorFactory
    {
        /// <summary>
        /// Create 함수를 처리합니다.
        /// </summary>
        public static IAttributeCalculator Create(AttributeCalculatorType type)
        {
            // 핵심 로직을 처리합니다.
            return type switch
            {
                AttributeCalculatorType.HealByTargetMaxHealthPercent => HealByTargetMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.HealBySourceMaxHealthPercent => HealBySourceMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.FullHeal => FullHealCalculator.Instance,
                AttributeCalculatorType.DamageBySourceAttackDamage => DamageBySourceAttackDamageCalculator.Instance,
                _ => null
            };
        }
        /// <summary>
        /// Create 함수를 처리합니다.
        /// </summary>

        public static IAttributeCalculator Create(string typeName)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(typeName)) return null;

            return typeName switch
            {
                "HealByTargetMaxHealthPercent" => HealByTargetMaxHealthPercentCalculator.Instance,
                "HealBySourceMaxHealthPercent" => HealBySourceMaxHealthPercentCalculator.Instance,
                "FullHeal" => FullHealCalculator.Instance,
                "DamageBySourceAttackDamage" => DamageBySourceAttackDamageCalculator.Instance,
                _ => null
            };
        }
        /// <summary>
        /// GetTypeName 함수를 처리합니다.
        /// </summary>

        public static string GetTypeName(IAttributeCalculator calculator)
        {
            // 핵심 로직을 처리합니다.
            return calculator switch
            {
                HealByTargetMaxHealthPercentCalculator => "HealByTargetMaxHealthPercent",
                HealBySourceMaxHealthPercentCalculator => "HealBySourceMaxHealthPercent",
                FullHealCalculator => "FullHeal",
                DamageBySourceAttackDamageCalculator => "DamageBySourceAttackDamage",
                _ => null
            };
        }
    }
}
