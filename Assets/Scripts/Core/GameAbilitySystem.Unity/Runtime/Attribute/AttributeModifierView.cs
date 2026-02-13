using System;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// Unity에서 사용 가능한 속성 수정자 뷰입니다.
    /// </summary>
    [Serializable]
    public struct AttributeModifierView : IEquatable<AttributeModifierView>
    {
        [SerializeField] private AttributeModifierValueMode _valueMode;

        [Header("Static Mode")]
        [SerializeField] private string _attributeId;
        [SerializeField] private AttributeModifierOperationType _operation;
        [SerializeField] private float _magnitude;

        [Header("Calculated Mode")]
        [SerializeField] private AttributeCalculatorType _calculatorType;

        /// <summary>
        /// 값 계산 방식입니다.
        /// </summary>
        public AttributeModifierValueMode ValueMode => _valueMode;

        /// <summary>
        /// 수정할 속성 Id입니다. (Static 모드에서 사용)
        /// </summary>
        public string AttributeId => _attributeId;

        /// <summary>
        /// 적용 연산입니다. (Static 모드에서 사용)
        /// </summary>
        public AttributeModifierOperationType Operation => _operation;

        /// <summary>
        /// 정적 값입니다. (Static 모드에서 사용)
        /// </summary>
        public float Magnitude => _magnitude;

        /// <summary>
        /// 속성 계산기 타입입니다. (Calculated 모드에서 사용)
        /// </summary>
        public AttributeCalculatorType CalculatorType => _calculatorType;

        /// <summary>
        /// 동일한 수정자인지 비교합니다.
        /// </summary>
        public bool Equals(AttributeModifierView other)
        {
            return _valueMode == other._valueMode
                && string.Equals(_attributeId, other._attributeId, StringComparison.Ordinal)
                && _operation == other._operation
                && Mathf.Approximately(_magnitude, other._magnitude)
                && _calculatorType == other._calculatorType;
        }

        /// <summary>
        /// 동일한 수정자인지 비교합니다.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is AttributeModifierView other && Equals(other);
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)_valueMode;
                hash = (hash * 397) ^ (_attributeId != null ? StringComparer.Ordinal.GetHashCode(_attributeId) : 0);
                hash = (hash * 397) ^ (int)_operation;
                hash = (hash * 397) ^ _magnitude.GetHashCode();
                hash = (hash * 397) ^ (int)_calculatorType;
                return hash;
            }
        }
    }
}
