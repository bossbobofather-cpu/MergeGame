namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public static class EffectDurationPolicyFactory
    {
        /// <summary>
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
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

