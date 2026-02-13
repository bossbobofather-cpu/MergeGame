namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public sealed class AttributeValue
    {
        private float _currentValue;
        /// <summary>
        /// AttributeValue 함수를 처리합니다.
        /// </summary>

        public AttributeValue(AttributeId attributeId, float baseValue = 0f, float minValue = 0f, float maxValue = 0f)
        {
            // 핵심 로직을 처리합니다.
            AttributeId = attributeId;
            BaseValue = baseValue;
            MinValue = minValue;
            MaxValue = maxValue;
            _currentValue = baseValue;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public AttributeId AttributeId { get; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public float BaseValue { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                // 二쇱꽍 ?뺣━
                if (MaxValue > 0f && value > MaxValue)
                {
                    _currentValue = MaxValue;
                }
                else if (value < MinValue)
                {
                    _currentValue = MinValue;
                }
                else
                {
                    _currentValue = value;
                }
            }
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public float MinValue { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public float MaxValue { get; set; }
    }
}

