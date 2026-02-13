using System;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    [Serializable]
    public struct AttributeModifier
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public AttributeModifierValueMode ValueMode;
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public AttributeId AttributeId;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public AttributeModifierOperationType Operation;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float Magnitude;
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public AttributeCalculatorType CalculatorType;
    }
}

