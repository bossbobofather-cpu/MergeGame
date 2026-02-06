using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    public struct FGameplayAbilitySpecHandle : IEquatable<FGameplayAbilitySpecHandle>
    {
        /// <summary>
        /// 주석 정리
        /// </summary>
        public static readonly FGameplayAbilitySpecHandle Invalid = new FGameplayAbilitySpecHandle { Id = 0 };

        /// <summary>
        /// 주석 정리
        /// </summary>
        public int Id;

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
        /// 주석 정리
        public bool Equals(FGameplayAbilitySpecHandle other)
        {
            // 주석 정리
            return Id == other.Id;
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
        /// 주석 정리
        public override bool Equals(object obj)
        {
            // 주석 정리
            return obj is FGameplayAbilitySpecHandle other && Equals(other);
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
        public override int GetHashCode()
        {
            // 주석 정리
            return Id;
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public static bool operator ==(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // 주석 정리
            return a.Id == b.Id;
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public static bool operator !=(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // 주석 정리
            return a.Id != b.Id;
        }
    }
}
