using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    [Serializable]
    public struct AttributeModifier
    {
        /// <summary>
        /// 주석 정리
        /// </summary>
        public AttributeModifierValueMode ValueMode;

        // 주석 정리

        /// <summary>
        /// 주석 정리
        /// </summary>
        public AttributeId AttributeId;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public AttributeModifierOperationType Operation;

        /// <summary>
        /// 주석 정리
        /// </summary>
        public float Magnitude;

        // 주석 정리

        /// <summary>
        /// 주석 정리
        /// </summary>
        public AttributeCalculatorType CalculatorType;
    }
}

