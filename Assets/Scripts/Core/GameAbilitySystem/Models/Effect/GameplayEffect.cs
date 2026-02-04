using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// ê²Œì„?Œë ˆ???¨ê³¼ ?¤ì •?…ë‹ˆ??(?œìˆ˜ C# ëª¨ë¸).
    /// </summary>
    [Serializable]
    public class GameplayEffect
    {
        /// <summary>
        /// ?¨ê³¼ ID?…ë‹ˆ??
        /// </summary>
        public string EffectId { get; set; }

        /// <summary>
        /// ?œì‹œ ?´ë¦„?…ë‹ˆ??
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// ?¤ëª…?…ë‹ˆ??
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ì§€???€?…ì…?ˆë‹¤.
        /// </summary>
        public EffectDurationType DurationType { get; set; }

        /// <summary>
        /// ì§€???œê°„?…ë‹ˆ??(ì´?.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// ì£¼ê¸° ?œê°„?…ë‹ˆ??(ì´?.
        /// </summary>
        public float Period { get; set; }

        /// <summary>
        /// ìµœë? ?¤íƒ ?˜ì…?ˆë‹¤.
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// ?˜ì •??ê·¸ë£¹ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public List<AttributeModifier> Modifiers { get; set; }

        /// <summary>
        /// ë¶€?¬ë˜???œê·¸ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public GameplayTagContainer GrantedTags { get; set; }

        /// <summary>
        /// ?ìš© ?„ìˆ˜ ?œê·¸ ëª©ë¡?…ë‹ˆ??
        /// </summary>
        public GameplayTagContainer RequiredTags { get; set; }

        /// <summary>
        /// ?ìš© ì°¨ë‹¨ ?œê·¸ ëª©ë¡?…ë‹ˆ??
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

