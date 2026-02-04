using System;

namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// ?ì„± ?˜ì •??JSON DTO
    /// </summary>
    [Serializable]
    public class AttributeModifierDto
    {
        /// <summary>
        /// ê°?ê³„ì‚° ë°©ì‹ ("Static", "Calculated")
        /// </summary>
        public string ValueMode;

        // ===== Static ëª¨ë“œ ?„ë“œ =====

        /// <summary>
        /// ?€???ì„± ID (Static ëª¨ë“œ)
        /// </summary>
        public string AttributeId;

        /// <summary>
        /// ?ìš© ?°ì‚° ("Add", "Multiply", "Override") (Static ëª¨ë“œ)
        /// </summary>
        public string Operation;

        /// <summary>
        /// ?•ì  ê°?(Static ëª¨ë“œ)
        /// </summary>
        public float Magnitude;

        // ===== Calculated ëª¨ë“œ ?„ë“œ =====

        /// <summary>
        /// ê³„ì‚°ê¸??€??("HealByTargetMaxHealthPercent", "HealBySourceMaxHealthPercent", "FullHeal")
        /// </summary>
        public AttributeCalculatorType CalculatorType;
    }
}

