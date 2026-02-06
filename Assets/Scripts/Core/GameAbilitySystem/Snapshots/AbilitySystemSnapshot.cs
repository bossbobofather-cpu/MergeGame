using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 주석 정리
    /// 주석 정리
    /// 주석 정리
    /// </summary>
    public sealed class AbilitySystemSnapshot
    {
        /// <summary>
        /// 주석 정리
        /// </summary>
        public IReadOnlyDictionary<AttributeId, float> Attributes { get; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public IReadOnlyList<FGameplayTag> OwnedTags { get; }

        /// <summary>
        /// 주석 정리
        /// </summary>
        public IReadOnlyList<GameplayAbility> Abilities { get; }

        /// <summary>
        /// 주석 정리
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
    /// 주석 정리
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

