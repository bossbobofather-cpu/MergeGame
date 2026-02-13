using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        private string _value;
        private int _hash;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
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
        /// 요약 설명입니다.
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public int Hash
        {
            get
            {
                if (_hash == 0 && !string.IsNullOrEmpty(_value))
                {
                    _hash = GameplayTagUtility.Fnv1a32(_value);
                }
                return _hash;
            }
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public bool Equals(FGameplayTag other)
        {
            return Hash == other.Hash;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is FGameplayTag other && Equals(other);
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public override int GetHashCode()
        {
            return Hash;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}

