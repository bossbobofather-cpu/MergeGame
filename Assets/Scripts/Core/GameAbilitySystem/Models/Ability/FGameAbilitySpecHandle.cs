using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public struct FGameplayAbilitySpecHandle : IEquatable<FGameplayAbilitySpecHandle>
    {
        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static readonly FGameplayAbilitySpecHandle Invalid = new FGameplayAbilitySpecHandle { Id = 0 };

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public int Id;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        /// 二쇱꽍 ?뺣━
        public bool Equals(FGameplayAbilitySpecHandle other)
        {
            // 二쇱꽍 ?뺣━
            return Id == other.Id;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        /// 二쇱꽍 ?뺣━
        public override bool Equals(object obj)
        {
            // 二쇱꽍 ?뺣━
            return obj is FGameplayAbilitySpecHandle other && Equals(other);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        public override int GetHashCode()
        {
            // 二쇱꽍 ?뺣━
            return Id;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static bool operator ==(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // 二쇱꽍 ?뺣━
            return a.Id == b.Id;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static bool operator !=(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // 二쇱꽍 ?뺣━
            return a.Id != b.Id;
        }
    }
}
