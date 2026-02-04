namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?�과 지?�시�??�책 ?�토리입?�다.
    /// </summary>
    public static class EffectDurationPolicyFactory
    {
        /// <summary>
        /// enum ?�?�으�??�책 ?�스?�스�??�성?�니??
        /// </summary>
        public static IEffectDurationPolicy Create(EffectDurationPolicyType type)
        {
            return type switch
            {
                EffectDurationPolicyType.BaseAttackCooldown => BaseAttackCooldownDurationPlicy.Instance,
                _ => null
            };
        }

        /// <summary>
        /// 문자???�?�명?�로 ?�책 ?�스?�스�??�성?�니?? (JSON ??��?�화??
        /// </summary>
        public static IEffectDurationPolicy Create(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            return typeName switch
            {
                "BaseAttackCooldownDurationPlicy" => BaseAttackCooldownDurationPlicy.Instance,
                _ => null
            };
        }

        /// <summary>
        /// ?�책 ?�스?�스?�서 ?�??enum??반환?�니??
        /// </summary>
        public static EffectDurationPolicyType GetPolicyType(IEffectDurationPolicy policy)
        {
            return policy switch
            {
                BaseAttackCooldownDurationPlicy => EffectDurationPolicyType.BaseAttackCooldown,
                _ => EffectDurationPolicyType.None
            };
        }

        /// <summary>
        /// ?�책 ?�스?�스?�서 ?�?�명 문자?�을 반환?�니?? (JSON 직렬?�용)
        /// </summary>
        public static string GetTypeName(IEffectDurationPolicy policy)
        {
            return policy switch
            {
                BaseAttackCooldownDurationPlicy => "BaseAttackCooldown",
                _ => null
            };
        }
    }
}

