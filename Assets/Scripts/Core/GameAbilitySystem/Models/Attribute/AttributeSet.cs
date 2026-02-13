using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    public sealed class AttributeSet
    {
        private readonly Dictionary<AttributeId, AttributeValue> _values = new();

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public IReadOnlyCollection<AttributeValue> Values => _values.Values;

        /// <summary>
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
        /// </summary>
        public bool TryGet(AttributeId id, out AttributeValue value)
        {
            return _values.TryGetValue(id, out value);
        }

        /// <summary>
        /// 요약 설명입니다.
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
        /// 요약 설명입니다.
        /// </summary>
        public float Get(AttributeId id)
        {
            return _values.TryGetValue(id, out var attr) ? attr.CurrentValue : 0f;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }
    }
}

