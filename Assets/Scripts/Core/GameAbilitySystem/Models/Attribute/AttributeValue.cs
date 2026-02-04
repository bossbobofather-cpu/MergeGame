namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?°í????ì„± ê°?ì»¨í…Œ?´ë„ˆ?…ë‹ˆ??(?œìˆ˜ C# ëª¨ë¸).
    /// Unity???˜ì¡´?˜ì? ?Šìœ¼ë©?Host ?˜ê²½?ì„œ ?¬ìš© ê°€?¥í•©?ˆë‹¤.
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
        /// ?ì„± ?ë³„?ì…?ˆë‹¤.
        /// </summary>
        public AttributeId AttributeId { get; }

        /// <summary>
        /// ë² ì´??ê°’ì…?ˆë‹¤.
        /// </summary>
        public float BaseValue { get; set; }

        /// <summary>
        /// ?„ì¬ ê°’ì…?ˆë‹¤.
        /// </summary>
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                // ë²”ìœ„ ?ˆìœ¼ë¡?ë³´ì •
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
        /// ìµœì†Œê°’ì…?ˆë‹¤.
        /// </summary>
        public float MinValue { get; set; }

        /// <summary>
        /// ìµœë?ê°’ì…?ˆë‹¤.
        /// </summary>
        public float MaxValue { get; set; }
    }
}

