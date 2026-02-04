using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?ì„± ?˜ì •???•ì˜?…ë‹ˆ??
    /// </summary>
    [Serializable]
    public struct AttributeModifier
    {
        /// <summary>
        /// ê°?ê³„ì‚° ë°©ì‹?…ë‹ˆ??
        /// </summary>
        public AttributeModifierValueMode ValueMode;

        // ===== Static ëª¨ë“œ ?„ë“œ =====

        /// <summary>
        /// ?˜ì •???€???ì„± Id?…ë‹ˆ?? (Static ëª¨ë“œ?ì„œ ?¬ìš©)
        /// </summary>
        public AttributeId AttributeId;

        /// <summary>
        /// ?ìš© ?°ì‚°?…ë‹ˆ?? (Static ëª¨ë“œ?ì„œ ?¬ìš©)
        /// </summary>
        public AttributeModifierOperationType Operation;

        /// <summary>
        /// ?•ì  ?¬ê¸° ê°’ì…?ˆë‹¤. (Static ëª¨ë“œ?ì„œ ?¬ìš©)
        /// </summary>
        public float Magnitude;

        // ===== Calculated ëª¨ë“œ ?„ë“œ =====

        /// <summary>
        /// ?ì„± ê³„ì‚°ê¸??€?…ì…?ˆë‹¤. (Calculated ëª¨ë“œ?ì„œ ?¬ìš©)
        /// </summary>
        public AttributeCalculatorType CalculatorType;
    }
}

