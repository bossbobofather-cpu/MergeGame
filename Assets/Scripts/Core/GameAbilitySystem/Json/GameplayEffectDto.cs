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
        public string EffectTag;

        // Legacy field for backward compatibility with old exported json.
        public string EffectId;

        public string DisplayName;
        public string Description;

        /// <summary>
        /// 二쇱꽍 ?뺣━
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
