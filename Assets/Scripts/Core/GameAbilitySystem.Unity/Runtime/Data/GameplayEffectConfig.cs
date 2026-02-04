using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameCore.Helper
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayEffectConfig")]
    /// <summary>
    /// 게임플레이 효과 설정을 담는 에셋입니다.
    /// </summary>
    public class GameplayEffectConfig : ScriptableObject
    {
        //효과 이름
        [SerializeField] private string _displayName;

        //효과 설명
        [SerializeField] private string _description;

        // 지속 타입
        [SerializeField] private EffectDurationType _durationType = EffectDurationType.Instant;

        // 지속 시간
        [SerializeField] private float _duration = 0f;

        // 주기 시간
        [SerializeField] private float _period = 0f;

        //최대 효과 스택
        [SerializeField] private int _maxStack = 1;

        // 지속시간 정책 타입
        [SerializeField] private EffectDurationPolicyType _durationPolicyType = EffectDurationPolicyType.None;

        // 부여되는 태그
        [SerializeField] private GameplayTagContainerView _grantedTags = new GameplayTagContainerView();

        //활성화 시 필요 태그
        [SerializeField] private GameplayTagContainerView _activationRequiredTags = new GameplayTagContainerView();

        //활성화 시 제외 태그
        [SerializeField] private GameplayTagContainerView _activationBlockedTags = new GameplayTagContainerView();

        [SerializeField] private List<AttributeModifierView> _modifiers = new List<AttributeModifierView>();

        /// <summary>
        /// 표시 이름입니다.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// 설명입니다.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// 지속 타입입니다.
        /// </summary>
        public EffectDurationType DurationType => _durationType;

        /// <summary>
        /// 지속 시간입니다. 지속 타입이 고정 시간일 때만 의미가 있습니다.
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 주기 시간입니다. 지속 타입이 고정 시간 또는 무한일 때만 의미가 있습니다.
        /// </summary>
        public float Period => _period;

        /// <summary>
        /// 최대 효과 스택입니다.
        /// </summary>
        public int MaxStack => _maxStack;

        /// <summary>
        /// 효과로 부여되는 태그입니다.
        /// </summary>
        public GameplayTagContainerView GrantedTags => _grantedTags;

        /// <summary>
        /// 적용 대상이 반드시 가지고 있어야 하는 태그입니다.
        /// </summary>
        public GameplayTagContainerView ActivationRequiredTags => _activationRequiredTags;

        /// <summary>
        /// 적용 대상이 가지고 있으면 적용이 막히는 태그입니다.
        /// </summary>
        public GameplayTagContainerView ActivationBlockedTags => _activationBlockedTags;

        /// <summary>
        /// 부여되는 속성 수정자 목록입니다.
        /// </summary>
        public IReadOnlyList<AttributeModifierView> Modifiers => _modifiers;

        /// <summary>
        /// 지속시간 정책 타입입니다.
        /// </summary>
        public EffectDurationPolicyType DurationPolicyType => _durationPolicyType;
    }
}

