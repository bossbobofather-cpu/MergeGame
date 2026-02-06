using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// </summary>
    [Serializable]
    public class GameplayEffect
    {
        /// <summary>
        /// 주석 정리
        /// </summary>
        public string EffectId { get; set; }

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
        /// </summary>
        public EffectDurationType DurationType { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public float Period { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public List<AttributeModifier> Modifiers { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagContainer GrantedTags { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagContainer RequiredTags { get; set; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public GameplayTagContainer BlockedTags { get; set; }

        public IEffectDurationPolicy DurationPolicy { get; set; }

        public GameplayEffect()
        {
            Modifiers = new List<AttributeModifier>();
            GrantedTags = new GameplayTagContainer();
            RequiredTags = new GameplayTagContainer();
            BlockedTags = new GameplayTagContainer();
            DurationType = EffectDurationType.Instant;
            MaxStack = 1;
        }
    }
}

