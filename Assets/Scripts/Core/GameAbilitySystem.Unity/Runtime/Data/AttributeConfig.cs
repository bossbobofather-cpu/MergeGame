using System.Collections.Generic;
using UnityEngine;
using Noname.GameAbilitySystem;

namespace Noname.GameCore.Helper
{
    // AttributeId는 Domain 레이어로 이동했습니다.
    // Domain.AttributeId를 사용하세요.

    /// <summary>
    /// 속성 정의 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/AttributeConfig")]
    public sealed class AttributeConfig : ScriptableObject
    {
        [SerializeField] private string _attributeName;
        [SerializeField] private float _defaultBaseValue = 0f;
        [SerializeField] private float _minValue = 0f;
        [SerializeField] private float _maxValue = 0f;
        /// <summary>
        /// 속성 식별자입니다. (Domain AttributeId)
        /// </summary>
        public AttributeId Id => new AttributeId(_attributeName);

        /// <summary>
        /// 기본 베이스 값입니다.
        /// </summary>
        public float DefaultBaseValue => _defaultBaseValue;
        /// <summary>
        /// 최소 값입니다.
        /// </summary>
        public float MinValue => _minValue;
        /// <summary>
        /// 최대 값입니다.
        /// </summary>
        public float MaxValue => _maxValue;
    }
}
