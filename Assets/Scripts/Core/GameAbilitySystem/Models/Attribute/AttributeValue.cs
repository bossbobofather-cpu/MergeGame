namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class AttributeValue
    {
        private float _currentValue;
        /// <summary>
        /// AttributeValue 메서드입니다.
        /// </summary>

        public AttributeValue(AttributeId attributeId, float baseValue = 0f, float minValue = 0f, float maxValue = 0f)
        {
            AttributeId = attributeId;
            BaseValue = baseValue;
            MinValue = minValue;
            MaxValue = maxValue;
            _currentValue = baseValue;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public AttributeId AttributeId { get; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float BaseValue { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
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
        /// 요약 설명입니다.
        /// </summary>
        public float MinValue { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public float MaxValue { get; set; }
    }
}

