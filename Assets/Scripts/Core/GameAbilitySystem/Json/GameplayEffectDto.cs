using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// GameplayEffect JSON DTO
    /// </summary>
    [Serializable]
    public class GameplayEffectDto
    {
        public string EffectId;
        public string DisplayName;
        public string Description;

        /// <summary>
        /// ì§€???€?? "Instant", "Infinite", "HasDuration"
        /// </summary>
        
        public string DurationType;

        public float Duration;
        public float Period;
        public int MaxStack;

        public List<AttributeModifierDto> Modifiers;
        public List<string> GrantedTags;
        public List<string> RequiredTags;
        public List<string> BlockedTags;

        public DurationPolicyDto DurationPolicy;
    }
}

