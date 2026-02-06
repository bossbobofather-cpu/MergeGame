namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    public static class AttributeCalculatorFactory
    {
        /// <summary>
        /// 주석 정리
        /// </summary>
        public static IAttributeCalculator Create(AttributeCalculatorType type)
        {
            return type switch
            {
                AttributeCalculatorType.HealByTargetMaxHealthPercent => HealByTargetMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.HealBySourceMaxHealthPercent => HealBySourceMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.FullHeal => FullHealCalculator.Instance,
                _ => null
            };
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public static IAttributeCalculator Create(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            return typeName switch
            {
                "HealByTargetMaxHealthPercent" => HealByTargetMaxHealthPercentCalculator.Instance,
                "HealBySourceMaxHealthPercent" => HealBySourceMaxHealthPercentCalculator.Instance,
                "FullHeal" => FullHealCalculator.Instance,
                _ => null
            };
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public static string GetTypeName(IAttributeCalculator calculator)
        {
            return calculator switch
            {
                HealByTargetMaxHealthPercentCalculator => "HealByTargetMaxHealthPercent",
                HealBySourceMaxHealthPercentCalculator => "HealBySourceMaxHealthPercent",
                FullHealCalculator => "FullHeal",
                _ => null
            };
        }
    }
}

