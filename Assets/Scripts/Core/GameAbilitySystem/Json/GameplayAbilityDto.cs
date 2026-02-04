using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// GameplayAbility JSON DTO
    /// </summary>
    [Serializable]
    public class GameplayAbilityDto
    {
        public string AbilityTag;
        public string AbilityId;
        public string DisplayName;
        public string Description;

        public GameplayEffectDto CooldownEffect;
        public List<GameplayEffectDto> CostEffects;
        public List<GameplayEffectDto> AppliedEffects;

        public List<string> ActivationRequiredTags;
        public List<string> ActivationBlockedTags;

        public TargetingStrategyDto TargetingStrategy;
    }
}

