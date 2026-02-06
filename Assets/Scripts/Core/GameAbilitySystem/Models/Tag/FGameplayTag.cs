using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// 주석 정리
    /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        private string _value;
        private int _hash;

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
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
        /// 주석 정리
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public int Hash
        {
            get
            {
                if (_hash == 0 && !string.IsNullOrEmpty(_value))
                {
                    // 주석 정리
                    _hash = GameplayTagUtility.Fnv1a32(_value);
                }
                return _hash;
            }
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
        /// 주석 정리
        public bool Equals(FGameplayTag other)
        {
            return Hash == other.Hash;
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
        /// 주석 정리
        public override bool Equals(object obj)
        {
            return obj is FGameplayTag other && Equals(other);
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
        public override int GetHashCode()
        {
            return Hash;
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        /// 주석 정리
        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}

