using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    [Serializable]
    public sealed class GameplayAbility
    {
        /// <summary>
        /// 주석 정리
        /// </summary>
        public FGameplayTag AbilityTag { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 주석 정리
        /// 주석 정리
        /// </summary>
        public GameplayEffect CooldownEffect { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public List<GameplayEffect> CostEffects { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public List<GameplayEffect> AppliedEffects { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagContainer  ActivationRequiredTags { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public ITargetingStrategy TargetingStrategy { get; set; }

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

