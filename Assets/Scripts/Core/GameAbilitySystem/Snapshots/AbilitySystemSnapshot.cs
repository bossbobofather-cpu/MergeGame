using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// AbilitySystem??ë¶ˆë? ?¤ëƒ…?·ì…?ˆë‹¤ (?œìˆ˜ C# ëª¨ë¸).
    /// ?¤ë ˆ??ê°??ˆì „???°ì´???„ì†¡???„í•´ ?¬ìš©?©ë‹ˆ??
    /// Unity???˜ì¡´?˜ì? ?Šìœ¼ë©?Host ?˜ê²½?ì„œ ?¬ìš© ê°€?¥í•©?ˆë‹¤.
    /// </summary>
    public sealed class AbilitySystemSnapshot
    {
        /// <summary>
        /// ?ì„± ê°??¤ëƒ…?·ì…?ˆë‹¤.
        /// </summary>
        public IReadOnlyDictionary<AttributeId, float> Attributes { get; }

        /// <summary>
        /// ?Œìœ ???œê·¸ ?¤ëƒ…?·ì…?ˆë‹¤.
        /// </summary>
        public IReadOnlyList<FGameplayTag> OwnedTags { get; }

        /// <summary>
        /// ?¤í‚¬ ëª©ë¡ ?¤ëƒ…?·ì…?ˆë‹¤.
        /// </summary>
        public IReadOnlyList<GameplayAbility> Abilities { get; }

        /// <summary>
        /// ?œì„± ?¨ê³¼ ?¤ëƒ…?·ì…?ˆë‹¤.
        /// </summary>
        public IReadOnlyList<ActiveGameplayEffectSnapshot> ActiveEffects { get; }

        public AbilitySystemSnapshot(
            Dictionary<AttributeId, float> attributes,
            List<FGameplayTag> ownedTags,
            List<GameplayAbility> abilites,
            List<ActiveGameplayEffectSnapshot> activeEffects)
        {
            Attributes = new Dictionary<AttributeId, float>(attributes);
            OwnedTags = new List<FGameplayTag>(ownedTags);
            Abilities = new List<GameplayAbility>(abilites);
            ActiveEffects = new List<ActiveGameplayEffectSnapshot>(activeEffects);
        }
    }

    /// <summary>
    /// ?œì„± ?¨ê³¼??ë¶ˆë? ?¤ëƒ…?·ì…?ˆë‹¤ (?œìˆ˜ C# ëª¨ë¸).
    /// </summary>
    public struct ActiveGameplayEffectSnapshot
    {
        public long EffectUid { get; }
        public GameplayEffect Effect { get; }
        public float EndTime { get; }

        public ActiveGameplayEffectSnapshot(long effectUid, GameplayEffect effect, float endTime)
        {
            EffectUid = effectUid;
            Effect = effect;
            EndTime = endTime;
        }
    }
}

