namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public static class EffectDurationPolicyFactory
    {
        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static IEffectDurationPolicy Create(EffectDurationPolicyType type)
        {
            // 핵심 로직을 처리합니다.
            return type switch
            {
                EffectDurationPolicyType.BaseAttackCooldown => BaseAttackCooldownDurationPlicy.Instance,
                _ => null
            };
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static IEffectDurationPolicy Create(string typeName)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(typeName)) return null;

            return typeName switch
            {
                "BaseAttackCooldownDurationPlicy" => BaseAttackCooldownDurationPlicy.Instance,
                _ => null
            };
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static EffectDurationPolicyType GetPolicyType(IEffectDurationPolicy policy)
        {
            // 핵심 로직을 처리합니다.
            return policy switch
            {
                BaseAttackCooldownDurationPlicy => EffectDurationPolicyType.BaseAttackCooldown,
                _ => EffectDurationPolicyType.None
            };
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static string GetTypeName(IEffectDurationPolicy policy)
        {
            // 핵심 로직을 처리합니다.
            return policy switch
            {
                BaseAttackCooldownDurationPlicy => "BaseAttackCooldown",
                _ => null
            };
        }
    }
}

