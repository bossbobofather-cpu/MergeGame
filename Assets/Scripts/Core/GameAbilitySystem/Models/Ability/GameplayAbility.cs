using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ê²Œì„?Œë ˆ???´ë¹Œë¦¬í‹° ?¤ì •?…ë‹ˆ??(?œìˆ˜ C# ëª¨ë¸).
    /// </summary>
    [Serializable]
    public sealed class GameplayAbility
    {
        /// <summary>
        /// ?´ë¹Œë¦¬í‹° ?œê·¸?…ë‹ˆ??
        /// </summary>
        public FGameplayTag AbilityTag { get; set; }

        /// <summary>
        /// ?œì‹œ ?´ë¦„?…ë‹ˆ??
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// ?¤ëª…?…ë‹ˆ??
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ì¿¨ë‹¤???¨ê³¼?…ë‹ˆ??
        /// ?œì„±?????ì‹ ?ê²Œ ?ìš©?˜ì–´ ì¿¨ë‹¤???œê·¸ë¥?ë¶€?¬í•©?ˆë‹¤.
        /// </summary>
        public GameplayEffect CooldownEffect { get; set; }

        /// <summary>
        /// ë¹„ìš©?¼ë¡œ ?Œëª¨?˜ëŠ” ?¨ê³¼ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public List<GameplayEffect> CostEffects { get; set; }

        /// <summary>
        /// ?ìš©?˜ëŠ” ?¨ê³¼ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public List<GameplayEffect> AppliedEffects { get; set; }

        /// <summary>
        /// ?œì„±???„ìˆ˜ ?œê·¸ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public GameplayTagContainer  ActivationRequiredTags { get; set; }

        /// <summary>
        /// ?œì„±??ì°¨ë‹¨ ?œê·¸ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags { get; set; }

        /// <summary>
        /// ?€ê²?? ì • ?„ëµ?…ë‹ˆ??
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

