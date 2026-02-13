using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// 二쇱꽍 ?뺣━
    /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        private string _value;
        private int _hash;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        public FGameplayTag(string value)
        {
            _value = value;
            _hash = 0;
            if (!string.IsNullOrEmpty(value))
            {
                _hash = GameplayTagUtility.Fnv1a32(value);
            }
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public int Hash
        {
            get
            {
                if (_hash == 0 && !string.IsNullOrEmpty(_value))
                {
                    // 二쇱꽍 ?뺣━
                    _hash = GameplayTagUtility.Fnv1a32(_value);
                }
                return _hash;
            }
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        /// 二쇱꽍 ?뺣━
        public bool Equals(FGameplayTag other)
        {
            // 핵심 로직을 처리합니다.
            return Hash == other.Hash;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        /// 二쇱꽍 ?뺣━
        public override bool Equals(object obj)
        {
            // 핵심 로직을 처리합니다.
            return obj is FGameplayTag other && Equals(other);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        public override int GetHashCode()
        {
            // 핵심 로직을 처리합니다.
            return Hash;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        /// 二쇱꽍 ?뺣━
        public override string ToString()
        {
            // 핵심 로직을 처리합니다.
            return _value ?? string.Empty;
        }
    }
}

