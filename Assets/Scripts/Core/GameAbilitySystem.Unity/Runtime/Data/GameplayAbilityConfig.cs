using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayAbilityConfig")]
    /// <summary>
    /// 어빌리티 정의 데이터를 담는 ScriptableObject입니다.
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
        /// 어빌리티 태그입니다.
        /// </summary>
        public FGameplayTagView AbilityTag => _abilityTag;

        /// <summary>
        /// 표시 이름입니다.
        /// </summary>
        public string DisplayName => _dpName;

        /// <summary>
        /// 표시 설명입니다.
        /// </summary>
        public string Description => _dpDesc;    
        
        /// <summary>
        /// 쿨다운 효과입니다.
        /// </summary>
        public GameplayEffectConfig CooldownEffect => _cooldownEffect;

        /// <summary>
        /// 비용으로 소모되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffectConfig> CostEffects => _costEffects;

        /// <summary>
        /// 적용되는 효과 목록입니다.
        /// </summary>
        public List<GameplayEffectConfig> AppliedEffects => _appliedEffects;

        /// <summary>
        /// 활성화 필수 태그 목록입니다.
        /// </summary>
        public GameplayTagContainerView  ActivationRequiredTags => _activationRequiredTags;

        /// <summary>
        /// 활성화 차단 태그 목록입니다.
        /// </summary>
        public GameplayTagContainerView ActivationBlockedTags => _activationBlockedTags;

        /// <summary>
        /// 타겟팅 전략 타입입니다.
        /// </summary>
        public TargetingStrategyType TargetingStrategyType => _targetingStrategyType;

        /// <summary>
        /// 타겟팅 최대 범위입니다. (NearestEnemy, NearestN에서 사용)
        /// </summary>
        public float TargetingMaxRange => _targetingMaxRange;

        /// <summary>
        /// 타겟팅 최대 대상 수입니다. (NearestN에서 사용)
        /// </summary>
        public int TargetingMaxTargets => _targetingMaxTargets;

        /// <summary>
        /// 타겟팅 반경입니다. (Area에서 사용)
        /// </summary>
        public float TargetingRadius => _targetingRadius;
    }
}
