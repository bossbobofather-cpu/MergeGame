namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?�?�의 최�? 체력 비율만큼 ?�복?�는 계산기입?�다.
    /// </summary>
    public sealed class HealByTargetMaxHealthPercentCalculator : IAttributeCalculator
    {
        public static readonly HealByTargetMaxHealthPercentCalculator Instance = new();

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            if (target == null) return;

            var maxHealth = target.Get(AttributeId.MaxHealth);
            var healAmount = maxHealth;

            target.Add(AttributeId.Health, healAmount);
        }
    }

    /// <summary>
    /// ?�전?�의 최�? 체력 비율만큼 ?�복?�는 계산기입?�다.
    /// </summary>
    public sealed class HealBySourceMaxHealthPercentCalculator : IAttributeCalculator
    {
        public static readonly HealBySourceMaxHealthPercentCalculator Instance = new();

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            if (source == null || target == null) return;

            var maxHealth = source.Get(AttributeId.MaxHealth);
            var healAmount = maxHealth;

            target.Add(AttributeId.Health, healAmount);
        }
    }

    /// <summary>
    /// ?�?�의 체력??최�? 체력?�로 ?�정?�는 계산기입?�다. (?�전 ?�복)
    /// </summary>
    public sealed class FullHealCalculator : IAttributeCalculator
    {
        public static readonly FullHealCalculator Instance = new();

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            if (target == null) return;

            var maxHealth = target.Get(AttributeId.MaxHealth);
            target.Set(AttributeId.Health, maxHealth);
        }
    }
}

