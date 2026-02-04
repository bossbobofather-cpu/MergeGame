using System;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// Unity?먯꽌 ?몄쭛 媛?ν븳 ?띿꽦 ?섏젙??酉곗엯?덈떎.
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
        /// 媛?怨꾩궛 諛⑹떇?낅땲??
        /// </summary>
        public AttributeModifierValueMode ValueMode => _valueMode;

        /// <summary>
        /// ?섏젙??????띿꽦 Id?낅땲?? (Static 紐⑤뱶?먯꽌 ?ъ슜)
        /// </summary>
        public string AttributeId => _attributeId;

        /// <summary>
        /// ?곸슜 ?곗궛?낅땲?? (Static 紐⑤뱶?먯꽌 ?ъ슜)
        /// </summary>
        public AttributeModifierOperationType Operation => _operation;

        /// <summary>
        /// ?뺤쟻 ?ш린 媛믪엯?덈떎. (Static 紐⑤뱶?먯꽌 ?ъ슜)
        /// </summary>
        public float Magnitude => _magnitude;

        /// <summary>
        /// ?띿꽦 怨꾩궛湲???낆엯?덈떎. (Calculated 紐⑤뱶?먯꽌 ?ъ슜)
        /// </summary>
        public AttributeCalculatorType CalculatorType => _calculatorType;

        /// <summary>
        /// ?숈씪 ?щ?瑜?鍮꾧탳?⑸땲??
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
        /// ?숈씪 ?щ?瑜?鍮꾧탳?⑸땲??
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is AttributeModifierView other && Equals(other);
        }

        /// <summary>
        /// ?댁떆 肄붾뱶瑜?諛섑솚?⑸땲??
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

