using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public sealed class AttributeSet
    {
        private readonly Dictionary<AttributeId, AttributeValue> _values = new();

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public IReadOnlyCollection<AttributeValue> Values => _values.Values;

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public void SetAttribute(AttributeId id, float baseValue, float minValue = 0f, float maxValue = 0f)
        {
            // 핵심 로직을 처리합니다.
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
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public bool TryGet(AttributeId id, out AttributeValue value)
        {
            // 핵심 로직을 처리합니다.
            return _values.TryGetValue(id, out value);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public void Set(AttributeId id, float value)
        {
            // 핵심 로직을 처리합니다.
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
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public float Get(AttributeId id)
        {
            // 핵심 로직을 처리합니다.
            return _values.TryGetValue(id, out var attr) ? attr.CurrentValue : 0f;
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public void Clear()
        {
            // 핵심 로직을 처리합니다.
            _values.Clear();
        }
    }
}

