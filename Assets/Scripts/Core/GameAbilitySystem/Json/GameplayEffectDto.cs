using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// GameplayEffect 설정 JSON DTO입니다.
    /// </summary>
    [Serializable]
    public class GameplayEffectDto
    {
        public string EffectTag;

        // 이전에 내보낸 JSON과의 호환을 위한 레거시 필드입니다.
        public string EffectId;

        public string DisplayName;
        public string Description;

        /// <summary>
        /// 지속시간 타입 문자열입니다.
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
