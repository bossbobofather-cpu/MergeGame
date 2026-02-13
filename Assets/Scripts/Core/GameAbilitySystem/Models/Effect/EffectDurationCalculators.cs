using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public sealed class BaseAttackCooldownDurationPlicy : IEffectDurationPolicy
    {
          public static readonly BaseAttackCooldownDurationPlicy Instance = new();
        /// <summary>
        /// CalculateDuration 함수를 처리합니다.
        /// </summary>
        public float CalculateDuration(AbilitySystemComponent asc, ref float duration)
        {
            // 핵심 로직을 처리합니다.
            if(asc == null) return duration;

            // 二쇱꽍 ?뺣━
            duration = Clamp(duration / asc.Get(AttributeId.AttackSpeed), 0.1f, 10f);
            return duration;
        }
        /// <summary>
        /// Clamp 함수를 처리합니다.
        /// </summary>

        private static float Clamp(float value, float min, float max)
        {
            // 핵심 로직을 처리합니다.
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}

