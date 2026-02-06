using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    public sealed class BaseAttackCooldownDurationPlicy : IEffectDurationPolicy
    {
          public static readonly BaseAttackCooldownDurationPlicy Instance = new();
        public float CalculateDuration(AbilitySystemComponent asc, ref float duration)
        {
            if(asc == null) return duration;

            // 주석 정리
            duration = Clamp(duration / asc.Get(AttributeId.AttackSpeed), 0.1f, 10f);
            return duration;
        }

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

