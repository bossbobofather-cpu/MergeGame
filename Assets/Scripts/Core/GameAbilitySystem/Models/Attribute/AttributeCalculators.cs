namespace Noname.GameAbilitySystem
{
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

    public sealed class DamageBySourceAttackDamageCalculator : IAttributeCalculator
    {
        public static readonly DamageBySourceAttackDamageCalculator Instance = new();

        public void Apply(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            if (source == null || target == null) return;

            var damage = source.Get(AttributeId.AttackDamage);
            if (damage <= 0f) return;

            target.Add(AttributeId.Health, -damage);
        }
    }
}
