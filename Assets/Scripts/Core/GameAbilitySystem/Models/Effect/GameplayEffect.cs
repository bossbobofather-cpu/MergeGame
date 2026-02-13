using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 능력/상태 변화에 적용되는 이펙트 데이터입니다.
    /// </summary>
    [Serializable]
    public class GameplayEffect
    {
        /// <summary>
        /// 이펙트 식별 태그입니다.
        /// </summary>
        public FGameplayTag EffectTag { get; set; }

        /// <summary>
        /// 표시 이름입니다.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 설명 문자열입니다.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 지속 타입입니다.
        /// </summary>
        public EffectDurationType DurationType { get; set; }

        /// <summary>
        /// 지속 시간(초)입니다.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// 주기(초)입니다.
        /// </summary>
        public float Period { get; set; }

        /// <summary>
        /// 최대 중첩 수입니다.
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// 속성 수정 목록입니다.
        /// </summary>
        public List<AttributeModifier> Modifiers { get; set; }

        /// <summary>
        /// 적용 시 부여할 태그입니다.
        /// </summary>
        public GameplayTagContainer GrantedTags { get; set; }

        /// <summary>
        /// 적용에 필요한 태그입니다.
        /// </summary>
        public GameplayTagContainer RequiredTags { get; set; }

        /// <summary>
        /// 적용을 차단하는 태그입니다.
        /// </summary>
        public GameplayTagContainer BlockedTags { get; set; }

        /// <summary>
        /// Duration 계산 정책입니다.
        /// </summary>
        public IEffectDurationPolicy DurationPolicy { get; set; }
        /// <summary>
        /// GameplayEffect 함수를 처리합니다.
        /// </summary>

        public GameplayEffect()
        {
            // 핵심 로직을 처리합니다.
            Modifiers = new List<AttributeModifier>();
            GrantedTags = new GameplayTagContainer();
            RequiredTags = new GameplayTagContainer();
            BlockedTags = new GameplayTagContainer();
            DurationType = EffectDurationType.Instant;
            MaxStack = 1;
        }
    }
}
