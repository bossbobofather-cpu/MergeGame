using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ?ì„± ì§‘í•©??ê´€ë¦¬í•©?ˆë‹¤ (?œìˆ˜ C# ëª¨ë¸).
    /// Unity???˜ì¡´?˜ì? ?Šìœ¼ë©?Host ?˜ê²½?ì„œ ?¬ìš© ê°€?¥í•©?ˆë‹¤.
    /// </summary>
    public sealed class AttributeSet
    {
        private readonly Dictionary<AttributeId, AttributeValue> _values = new();

        /// <summary>
        /// ëª¨ë“  ?ì„± ê°’ì„ ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        public IReadOnlyCollection<AttributeValue> Values => _values.Values;

        /// <summary>
        /// ?ì„±??ì¶”ê??˜ê±°???…ë°?´íŠ¸?©ë‹ˆ??
        /// </summary>
        public void SetAttribute(AttributeId id, float baseValue, float minValue = 0f, float maxValue = 0f)
        {
            if (_values.TryGetValue(id, out var existing))
            {
                existing.BaseValue = baseValue;
                existing.MinValue = minValue;
                existing.MaxValue = maxValue;
                existing.CurrentValue = baseValue;
            }
            else
            {
                _values[id] = new AttributeValue(id, baseValue, minValue, maxValue);
            }
        }

        /// <summary>
        /// ?ë³„?ë¡œ ê°’ì„ ì¡°íšŒ?©ë‹ˆ??
        /// </summary>
        public bool TryGet(AttributeId id, out AttributeValue value)
        {
            return _values.TryGetValue(id, out value);
        }

        /// <summary>
        /// ?ì„± ê°’ì„ ?¤ì •?©ë‹ˆ??
        /// </summary>
        public void Set(AttributeId id, float value)
        {
            if (_values.TryGetValue(id, out var attr))
            {
                attr.CurrentValue = value;
            }
            else
            {
                _values[id] = new AttributeValue(id, value, 0f, 0f);
            }
        }

        /// <summary>
        /// ?ì„± ê°’ì„ ê°€?¸ì˜µ?ˆë‹¤. ?†ìœ¼ë©?0??ë°˜í™˜?©ë‹ˆ??
        /// </summary>
        public float Get(AttributeId id)
        {
            return _values.TryGetValue(id, out var attr) ? attr.CurrentValue : 0f;
        }

        /// <summary>
        /// ëª¨ë“  ?ì„±???œê±°?©ë‹ˆ??
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }
    }
}

