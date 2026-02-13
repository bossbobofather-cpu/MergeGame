using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    [Serializable]
    public sealed class GameplayAbility
    {
        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public FGameplayTag AbilityTag { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public GameplayEffect CooldownEffect { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public List<GameplayEffect> CostEffects { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public List<GameplayEffect> AppliedEffects { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public GameplayTagContainer  ActivationRequiredTags { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags { get; set; }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public ITargetingStrategy TargetingStrategy { get; set; }
        /// <summary>
        /// GameplayAbility 함수를 처리합니다.
        /// </summary>

        public GameplayAbility()
        {            
            // 핵심 로직을 처리합니다.
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

