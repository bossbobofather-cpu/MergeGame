using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?¥ë ¥ ?¬ì–‘???ë³„?˜ê¸° ?„í•œ ?¸ë“¤?…ë‹ˆ??
    /// </summary>
    public struct FGameplayAbilitySpecHandle : IEquatable<FGameplayAbilitySpecHandle>
    {
        /// <summary>
        /// ? íš¨?˜ì? ?Šì? ?¸ë“¤ ê°’ì…?ˆë‹¤.
        /// </summary>
        public static readonly FGameplayAbilitySpecHandle Invalid = new FGameplayAbilitySpecHandle { Id = 0 };

        /// <summary>
        /// ?ë³„??ê°’ì…?ˆë‹¤.
        /// </summary>
        public int Id;

        /// <summary>
        /// ?¤ë¥¸ ?¸ë“¤ê³??™ì¼?œì? ë¹„êµ?©ë‹ˆ??
        /// </summary>
        /// <param name="other">ë¹„êµ ?€??/param>
        /// <returns>?™ì¼ ?¬ë?</returns>
        public bool Equals(FGameplayAbilitySpecHandle other)
        {
            // ?ë³„??ê°’ìœ¼ë¡?ë¹„êµ?œë‹¤.
            return Id == other.Id;
        }

        /// <summary>
        /// ê°ì²´ ?™ì¼ ?¬ë?ë¥?ë¹„êµ?©ë‹ˆ??
        /// </summary>
        /// <param name="obj">ë¹„êµ ?€??/param>
        /// <returns>?™ì¼ ?¬ë?</returns>
        public override bool Equals(object obj)
        {
            // ê°™ì? ?€?…ì¸ì§€ ?•ì¸????ë¹„êµ?œë‹¤.
            return obj is FGameplayAbilitySpecHandle other && Equals(other);
        }

        /// <summary>
        /// ?´ì‹œ ì½”ë“œë¥?ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        /// <returns>?´ì‹œ ê°?/returns>
        public override int GetHashCode()
        {
            // ?ë³„??ê°’ì„ ê·¸ë?ë¡??¬ìš©?œë‹¤.
            return Id;
        }

        /// <summary>
        /// ?™ì¼ ?°ì‚°?ì…?ˆë‹¤.
        /// </summary>
        public static bool operator ==(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // ?ë³„??ê°’ìœ¼ë¡?ë¹„êµ?œë‹¤.
            return a.Id == b.Id;
        }

        /// <summary>
        /// ?¤ë¦„ ?°ì‚°?ì…?ˆë‹¤.
        /// </summary>
        public static bool operator !=(FGameplayAbilitySpecHandle a, FGameplayAbilitySpecHandle b)
        {
            // ?ë³„??ê°’ìœ¼ë¡?ë¹„êµ?œë‹¤.
            return a.Id != b.Id;
        }
    }
}
