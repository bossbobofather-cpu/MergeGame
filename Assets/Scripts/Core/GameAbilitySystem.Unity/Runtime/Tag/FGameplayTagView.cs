using System;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// Unity?먯꽌 ?몄쭛 媛?ν븳 ?쒓렇 ?쒗쁽?낅땲??
    /// </summary>
    [Serializable]
    public struct FGameplayTagView : IEquatable<FGameplayTagView>
    {
        [SerializeField] private string _value;
        [NonSerialized] private int _hash;

        /// <summary>
        /// 臾몄옄??媛믪쓣 湲곕컲?쇰줈 ?쒓렇瑜??앹꽦?⑸땲??
        /// </summary>
        /// <param name="value">?쒓렇 臾몄옄??/param>
        public FGameplayTagView(string value)
        {
            _value = value;
            _hash = 0;
        }

        /// <summary>
        /// ?쒓렇 臾몄옄?댁엯?덈떎.
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// ?쒓렇 ?댁떆 媛믪엯?덈떎.
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
        /// ?쒓렇 臾몄옄?댁씠 ?좏슚?쒖? ?щ??낅땲??
        /// </summary>
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        /// <summary>
        /// Domain ?쒓렇濡?蹂?섑빀?덈떎.
        /// </summary>
        public FGameplayTag ToDomain()
        {
            return new FGameplayTag(_value);
        }

        /// <summary>
        /// ?숈씪 ?щ?瑜?鍮꾧탳?⑸땲??
        /// </summary>
        public bool Equals(FGameplayTagView other)
        {
            return Hash == other.Hash;
        }

        /// <summary>
        /// ?숈씪 ?щ?瑜?鍮꾧탳?⑸땲??
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is FGameplayTagView other && Equals(other);
        }

        /// <summary>
        /// ?댁떆 肄붾뱶瑜?諛섑솚?⑸땲??
        /// </summary>
        public override int GetHashCode()
        {
            return Hash;
        }

        /// <summary>
        /// 臾몄옄???쒗쁽??諛섑솚?⑸땲??
        /// </summary>
        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}

