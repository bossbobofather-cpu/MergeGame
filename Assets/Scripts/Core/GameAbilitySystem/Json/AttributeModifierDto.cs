using System;

namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    [Serializable]
    public class AttributeModifierDto
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string ValueMode;
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string AttributeId;

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string Operation;

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

