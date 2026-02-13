using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class BaseAttackCooldownDurationPlicy : IEffectDurationPolicy
    {
          public static readonly BaseAttackCooldownDurationPlicy Instance = new();
        /// <summary>
        /// CalculateDuration 메서드입니다.
        /// </summary>
        public float CalculateDuration(AbilitySystemComponent asc, ref float duration)
        {
            if(asc == null) return duration;
            duration = Clamp(duration / asc.Get(AttributeId.AttackSpeed), 0.1f, 10f);
            return duration;
        }
        /// <summary>
        /// Clamp 메서드입니다.
        /// </summary>

        private static float Clamp(float value, float min, float max)
        {
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

