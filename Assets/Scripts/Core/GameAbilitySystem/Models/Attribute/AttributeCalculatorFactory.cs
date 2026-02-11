namespace Noname.GameAbilitySystem
{
    public static class AttributeCalculatorFactory
    {
        public static IAttributeCalculator Create(AttributeCalculatorType type)
        {
            return type switch
            {
                AttributeCalculatorType.HealByTargetMaxHealthPercent => HealByTargetMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.HealBySourceMaxHealthPercent => HealBySourceMaxHealthPercentCalculator.Instance,
                AttributeCalculatorType.FullHeal => FullHealCalculator.Instance,
                AttributeCalculatorType.DamageBySourceAttackDamage => DamageBySourceAttackDamageCalculator.Instance,
                _ => null
            };
        }

        public static IAttributeCalculator Create(string typeName)
        {
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

        public static string GetTypeName(IAttributeCalculator calculator)
        {
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
