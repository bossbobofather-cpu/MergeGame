using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    [Serializable]
    public sealed class GameplayAbility
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public FGameplayTag AbilityTag { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameplayEffect CooldownEffect { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public List<GameplayEffect> CostEffects { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public List<GameplayEffect> AppliedEffects { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameplayTagContainer  ActivationRequiredTags { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags { get; set; }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public ITargetingStrategy TargetingStrategy { get; set; }
        /// <summary>
        /// GameplayAbility 메서드입니다.
        /// </summary>

        public GameplayAbility()
        {            
            AbilityTag = new FGameplayTag();
            DisplayName = "";
            Description = "";
            CooldownEffect = null;
            CostEffects = new List<GameplayEffect>();
            AppliedEffects = new List<GameplayEffect>();
            ActivationRequiredTags = new GameplayTagContainer();
            ActivationBlockedTags = new GameplayTagContainer();
            TargetingStrategy = null;
        }
    }
}

