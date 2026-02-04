using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ê²Œì„?Œë ˆ???œê·¸ êµ¬ì¡°ì²´ì…?ˆë‹¤ (?œìˆ˜ C# ëª¨ë¸).
    /// Unity???˜ì¡´?˜ì? ?Šìœ¼ë©?Host ?˜ê²½?ì„œ ?¬ìš© ê°€?¥í•©?ˆë‹¤.
    /// </summary>
    [Serializable]
    public struct FGameplayTag : IEquatable<FGameplayTag>
    {
        private string _value;
        private int _hash;

        /// <summary>
        /// ë¬¸ì??ê°’ì„ ê¸°ë°˜?¼ë¡œ ?œê·¸ë¥??ì„±?©ë‹ˆ??
        /// </summary>
        /// <param name="value">?œê·¸ ë¬¸ì??/param>
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
        /// ?œê·¸ ë¬¸ì?´ì…?ˆë‹¤.
        /// </summary>
        public string Value => _value;

        /// <summary>
        /// ?œê·¸ ?´ì‹œ ê°’ì…?ˆë‹¤.
        /// </summary>
        public int Hash
        {
            get
            {
                if (_hash == 0 && !string.IsNullOrEmpty(_value))
                {
                    // ?„ìš”?????´ì‹œë¥?ê³„ì‚°?œë‹¤.
                    _hash = GameplayTagUtility.Fnv1a32(_value);
                }
                return _hash;
            }
        }

        /// <summary>
        /// ?œê·¸ ë¬¸ì?´ì´ ? íš¨?œì? ?¬ë??…ë‹ˆ??
        /// </summary>
        public bool IsValid => GameplayTagUtility.IsValidTagString(_value);

        /// <summary>
        /// ?¤ë¥¸ ?œê·¸?€ ?™ì¼?œì? ë¹„êµ?©ë‹ˆ??
        /// </summary>
        /// <param name="other">ë¹„êµ ?€??/param>
        /// <returns>?™ì¼ ?¬ë?</returns>
        public bool Equals(FGameplayTag other)
        {
            return Hash == other.Hash;
        }

        /// <summary>
        /// ê°ì²´ ?™ì¼ ?¬ë?ë¥?ë¹„êµ?©ë‹ˆ??
        /// </summary>
        /// <param name="obj">ë¹„êµ ?€??/param>
        /// <returns>?™ì¼ ?¬ë?</returns>
        public override bool Equals(object obj)
        {
            return obj is FGameplayTag other && Equals(other);
        }

        /// <summary>
        /// ?´ì‹œ ì½”ë“œë¥?ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        /// <returns>?´ì‹œ ê°?/returns>
        public override int GetHashCode()
        {
            return Hash;
        }

        /// <summary>
        /// ë¬¸ì???œí˜„??ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        /// <returns>?œê·¸ ë¬¸ì??/returns>
        public override string ToString()
        {
            return _value ?? string.Empty;
        }
    }
}

