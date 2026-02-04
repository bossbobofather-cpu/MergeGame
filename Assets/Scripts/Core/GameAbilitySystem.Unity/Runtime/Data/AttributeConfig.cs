using System.Collections.Generic;
using UnityEngine;
using Noname.GameAbilitySystem;

namespace Noname.GameCore.Helper
{
    // AttributeId??Domain ?덉씠?대줈 ?대룞?덉뒿?덈떎.
    // Domain.AttributeId瑜??ъ슜?섏꽭??

    /// <summary>
    /// ?띿꽦 ?뺤쓽??ScriptableObject?낅땲??
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/AttributeConfig")]
    public sealed class AttributeConfig : ScriptableObject
    {
        [SerializeField] private string _attributeName;
        [SerializeField] private float _defaultBaseValue = 0f;
        [SerializeField] private float _minValue = 0f;
        [SerializeField] private float _maxValue = 0f;
        /// <summary>
        /// ?띿꽦 ?앸퀎?먯엯?덈떎 (Domain ?덉씠??.
        /// </summary>
        public AttributeId Id => new AttributeId(_attributeName);

        /// <summary>
        /// 湲곕낯 踰좎씠??媛믪엯?덈떎.
        /// </summary>
        public float DefaultBaseValue => _defaultBaseValue;
        /// <summary>
        /// 理쒖냼媛믪엯?덈떎.
        /// </summary>
        public float MinValue => _minValue;
        /// <summary>
        /// 理쒕?媛믪엯?덈떎.
        /// </summary>
        public float MaxValue => _maxValue;
    }
}

