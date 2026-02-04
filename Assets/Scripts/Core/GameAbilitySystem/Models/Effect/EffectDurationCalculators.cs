using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 기본 공격 쿨다???�펙??지???�간 ?�책
    /// </summary>
    public sealed class BaseAttackCooldownDurationPlicy : IEffectDurationPolicy
    {
          public static readonly BaseAttackCooldownDurationPlicy Instance = new();
        public float CalculateDuration(AbilitySystemComponent asc, ref float duration)
        {
            if(asc == null) return duration;

            //?�무�?공격?�도가 빨라??쿨다??최소�?0.1 최�?�?10 부??
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

