namespace Noname.GameAbilitySystem
{
    public sealed class HealByTargetMaxHealthPercentCalculator : IAttributeCalculator
    {
        public static readonly HealByTargetMaxHealthPercentCalculator Instance = new();
        /// <summary>
        /// Apply 함수를 처리합니다.
        /// </summary>

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            // 핵심 로직을 처리합니다.
            if (target == null) return;

            var maxHealth = target.Get(AttributeId.MaxHealth);
            var healAmount = maxHealth;
            target.Add(AttributeId.Health, healAmount);
        }
    }

    public sealed class HealBySourceMaxHealthPercentCalculator : IAttributeCalculator
    {
        public static readonly HealBySourceMaxHealthPercentCalculator Instance = new();
        /// <summary>
        /// Apply 함수를 처리합니다.
        /// </summary>

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            // 핵심 로직을 처리합니다.
            if (source == null || target == null) return;

            var maxHealth = source.Get(AttributeId.MaxHealth);
            var healAmount = maxHealth;
            target.Add(AttributeId.Health, healAmount);
        }
    }

    public sealed class FullHealCalculator : IAttributeCalculator
    {
        public static readonly FullHealCalculator Instance = new();
        /// <summary>
        /// Apply 함수를 처리합니다.
        /// </summary>

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            // 핵심 로직을 처리합니다.
            if (target == null) return;

            var maxHealth = target.Get(AttributeId.MaxHealth);
            target.Set(AttributeId.Health, maxHealth);
        }
    }

    public sealed class DamageBySourceAttackDamageCalculator : IAttributeCalculator
    {
        public static readonly DamageBySourceAttackDamageCalculator Instance = new();
        /// <summary>
        /// Apply 함수를 처리합니다.
        /// </summary>

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            // 핵심 로직을 처리합니다.
            if (source == null || target == null) return;

            var damage = source.Get(AttributeId.AttackDamage);
            if (damage <= 0f) return;

            target.Add(AttributeId.Health, -damage);
        }
    }
}
