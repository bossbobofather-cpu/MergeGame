using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public struct FGameplayAbilitySpecHandle : IEquatable<FGameplayAbilitySpecHandle>
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static readonly FGameplayAbilitySpecHandle Invalid = new FGameplayAbilitySpecHandle { Id = 0 };

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int Id;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool Equals(FGameplayAbilitySpecHandle other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is FGameplayAbilitySpecHandle other && Equals(other);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static bool operator ==(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            return a.Id == b.Id;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public static bool operator !=(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            return a.Id != b.Id;
        }
    }
}
