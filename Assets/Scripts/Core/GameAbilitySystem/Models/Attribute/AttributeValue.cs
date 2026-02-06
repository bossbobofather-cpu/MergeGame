namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// 주석 정리
    /// </summary>
    public sealed class AttributeValue
    {
        private float _currentValue;

        public AttributeValue(AttributeId attributeId, float baseValue = 0f, float minValue = 0f, float maxValue = 0f)
        {
            AttributeId = attributeId;
            BaseValue = baseValue;
            MinValue = minValue;
            MaxValue = maxValue;
            _currentValue = baseValue;
        }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public AttributeId AttributeId { get; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public float BaseValue { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                // 주석 정리
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
        /// 주석 정리
        /// </summary>
        public float MinValue { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public float MaxValue { get; set; }
    }
}

