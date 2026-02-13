using System;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// Unity에서 사용 가능한 게임플레이 태그 뷰입니다.
    /// </summary>
    [Serializable]
    public struct FGameplayTagView : IEquatable<FGameplayTagView>
    {
        [SerializeField] private string _value;
        [NonSerialized] private int _hash;

        /// <summary>
        /// 문자열 값을 기반으로 태그를 생성합니다.
        /// </summary>
        /// <param name="value">태그 문자열</param>
        public FGameplayTagView(string value)
        {
            _value = value;
            _hash = 0;
        }

        /// <summary>
        /// 태그 문자열입니다.
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// 태그 해시 값입니다.
        /// </summary>
        public int Hash
        {
            get
            {
                var computed = GameplayTagUtility.Fnv1a32(_value);
                if (_hash != computed)
                {
                    _hash = computed;
                }

                return _hash;
            }
        }

        /// <summary>
        /// 태그 문자열이 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        /// <summary>
        /// Domain 태그로 변환합니다.
        /// </summary>
        public FGameplayTag ToDomain()
        {
            return new FGameplayTag(_value);
        }

        /// <summary>
        /// 동일한 태그인지 비교합니다.
        /// </summary>
        public bool Equals(FGameplayTagView other)
        {
            return Hash == other.Hash;
        }

        /// <summary>
        /// 동일한 태그인지 비교합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is FGameplayTagView other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            return Hash;
        }

        /// <summary>
        /// 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}
