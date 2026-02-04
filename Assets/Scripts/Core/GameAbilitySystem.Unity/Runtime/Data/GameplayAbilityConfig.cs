using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayAbilityConfig")]
    /// <summary>
    /// ?대퉴由ы떚 ?뺤쓽 ?곗씠?곕? ?대뒗 ?먯뀑?낅땲??
    /// </summary>
    public sealed class GameplayAbilityConfig : ScriptableObject
    {
        [SerializeField] private FGameplayTagView _abilityTag;
        [SerializeField] private string _dpName;
        [SerializeField] private string _dpDesc;
        [SerializeField] private GameplayEffectConfig _cooldownEffect;        
        [SerializeField] private List<GameplayEffectConfig> _appliedEffects;
        [SerializeField] private List<GameplayEffectConfig> _costEffects;
        [SerializeField] private GameplayTagContainerView _activationRequiredTags;
        [SerializeField] private GameplayTagContainerView _activationBlockedTags;

        [Header("Targeting Strategy")]
        [SerializeField] private TargetingStrategyType _targetingStrategyType = TargetingStrategyType.None;
        [SerializeField] private float _targetingMaxRange = float.PositiveInfinity;
        [SerializeField] private int _targetingMaxTargets = 1;
        [SerializeField] private float _targetingRadius = 0f;

        /// <summary>
        /// ?대퉴由ы떚 ?쒓렇?낅땲??
        /// </summary>
        public FGameplayTagView AbilityTag => _abilityTag;

        /// <summary>
        /// ?쒖떆?섎뒗 ?대쫫?낅땲??
        /// </summary>
        public string DisplayName => _dpName;

        /// <summary>
        /// ?쒖떆?섎뒗 ?ㅻ챸?낅땲??
        /// </summary>
        public string Description => _dpDesc;    
        
        /// <summary>
        /// ?λ젰 荑⑤떎???④낵?낅땲??
        /// </summary>
        public GameplayEffectConfig CooldownEffect => _cooldownEffect;

        /// <summary>
        /// 鍮꾩슜?쇰줈 ?뚮え?섎뒗 ?④낵 紐⑸줉?낅땲??
        /// </summary>
        public List<GameplayEffectConfig> CostEffects => _costEffects;

        /// <summary>
        /// ?곸슜?섎뒗 ?④낵 紐⑸줉?낅땲??
        /// </summary>
        public List<GameplayEffectConfig> AppliedEffects => _appliedEffects;

        /// <summary>
        /// ?쒖꽦???꾩닔 ?쒓렇 紐⑸줉?낅땲??
        /// </summary>
        public GameplayTagContainerView  ActivationRequiredTags => _activationRequiredTags;

        /// <summary>
        /// ?쒖꽦??李⑤떒 ?쒓렇 紐⑸줉?낅땲??
        /// </summary>
        public GameplayTagContainerView ActivationBlockedTags => _activationBlockedTags;

        /// <summary>
        /// ?寃잜똿 ?꾨왂 ??낆엯?덈떎.
        /// </summary>
        public TargetingStrategyType TargetingStrategyType => _targetingStrategyType;

        /// <summary>
        /// ?寃잜똿 理쒕? 踰붿쐞?낅땲?? (NearestEnemy, NearestN?먯꽌 ?ъ슜)
        /// </summary>
        public float TargetingMaxRange => _targetingMaxRange;

        /// <summary>
        /// ?寃잜똿 理쒕? ????섏엯?덈떎. (NearestN?먯꽌 ?ъ슜)
        /// </summary>
        public int TargetingMaxTargets => _targetingMaxTargets;

        /// <summary>
        /// ?寃잜똿 諛섍꼍?낅땲?? (Area?먯꽌 ?ъ슜)
        /// </summary>
        public float TargetingRadius => _targetingRadius;
    }
}

