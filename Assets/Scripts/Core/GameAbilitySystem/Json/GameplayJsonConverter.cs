using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Json
{
    /// <summary>
    /// 二쇱꽍 ?뺣━
    /// </summary>
    public static class GameplayJsonConverter
    {
        #region DTO -> Domain 蹂??

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static GameplayAbility FromDto(GameplayAbilityDto dto)
        {
            // 핵심 로직을 처리합니다.
            if (dto == null) return null;

            return new GameplayAbility
            {
                AbilityTag = new FGameplayTag(dto.AbilityTag),
                DisplayName = dto.DisplayName,
                Description = dto.Description,
                CooldownEffect = FromDto(dto.CooldownEffect),
                CostEffects = FromDtoList(dto.CostEffects),
                AppliedEffects = FromDtoList(dto.AppliedEffects),
                ActivationRequiredTags = TagsFromStrings(dto.ActivationRequiredTags),
                ActivationBlockedTags = TagsFromStrings(dto.ActivationBlockedTags),
                TargetingStrategy = CreateTargetingStrategy(dto.TargetingStrategy),
            };
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static GameplayEffect FromDto(GameplayEffectDto dto)
        {
            // 핵심 로직을 처리합니다.
            if (dto == null) return null;

            return new GameplayEffect
            {
                EffectTag = new FGameplayTag(!string.IsNullOrEmpty(dto.EffectTag) ? dto.EffectTag : dto.EffectId),
                DisplayName = dto.DisplayName,
                Description = dto.Description ?? "",
                DurationType = ParseDurationType(dto.DurationType),
                Duration = dto.Duration,
                Period = dto.Period,
                MaxStack = dto.MaxStack > 0 ? dto.MaxStack : 1,
                Modifiers = FromDtoList(dto.Modifiers),
                GrantedTags = TagsFromStrings(dto.GrantedTags),
                RequiredTags = TagsFromStrings(dto.RequiredTags),
                BlockedTags = TagsFromStrings(dto.BlockedTags),
                DurationPolicy = CreateDurationPolicy(dto.DurationPolicy),
            };
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static AttributeModifier FromDto(AttributeModifierDto dto)
        {
            // 핵심 로직을 처리합니다.
            if (dto == null) return default;

            return new AttributeModifier
            {
                ValueMode = ParseValueMode(dto.ValueMode),
                // 二쇱꽍 ?뺣━
                AttributeId = new AttributeId(dto.AttributeId ?? ""),
                Operation = ParseOperation(dto.Operation),
                Magnitude = dto.Magnitude,
                // 二쇱꽍 ?뺣━
                CalculatorType = dto.CalculatorType,
            };
        }

        #endregion

        #region Domain -> DTO 蹂??

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static GameplayAbilityDto ToDto(GameplayAbility ability)
        {
            // 핵심 로직을 처리합니다.
            if (ability == null) return null;

            return new GameplayAbilityDto
            {
                AbilityTag = ability.AbilityTag.Value,
                DisplayName = ability.DisplayName,
                Description = ability.Description,
                CooldownEffect = ToDto(ability.CooldownEffect),
                CostEffects = ToDtoList(ability.CostEffects),
                AppliedEffects = ToDtoList(ability.AppliedEffects),
                ActivationRequiredTags = TagsToStrings(ability.ActivationRequiredTags),
                ActivationBlockedTags = TagsToStrings(ability.ActivationBlockedTags),
                TargetingStrategy = ToDto(ability.TargetingStrategy),
            };
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static GameplayEffectDto ToDto(GameplayEffect effect)
        {
            // 핵심 로직을 처리합니다.
            if (effect == null) return null;

            return new GameplayEffectDto
            {
                EffectTag = effect.EffectTag.Value,
                DisplayName = effect.DisplayName,
                Description = effect.Description,
                DurationType = effect.DurationType.ToString(),
                Duration = effect.Duration,
                Period = effect.Period,
                MaxStack = effect.MaxStack,
                Modifiers = ToDtoList(effect.Modifiers),
                GrantedTags = TagsToStrings(effect.GrantedTags),
                RequiredTags = TagsToStrings(effect.RequiredTags),
                BlockedTags = TagsToStrings(effect.BlockedTags),
                DurationPolicy = ToDto(effect.DurationPolicy),
            };
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static AttributeModifierDto ToDto(AttributeModifier modifier)
        {
            // 핵심 로직을 처리합니다.
            return new AttributeModifierDto
            {
                ValueMode = modifier.ValueMode.ToString(),
                // 二쇱꽍 ?뺣━
                AttributeId = modifier.AttributeId.Name,
                Operation = modifier.Operation.ToString(),
                Magnitude = modifier.Magnitude,
                // 二쇱꽍 ?뺣━
                CalculatorType = modifier.CalculatorType,
            };
        }

        #endregion

        #region 蹂???뱀뀡

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static ITargetingStrategy CreateTargetingStrategy(TargetingStrategyDto dto)
        {
            // 핵심 로직을 처리합니다.
            if (dto == null || string.IsNullOrEmpty(dto.Type)) return null;

            return TargetingStrategyFactory.Create(
                dto.Type,
                dto.MaxRange > 0 ? dto.MaxRange : float.PositiveInfinity,
                dto.MaxTargets > 0 ? dto.MaxTargets : 1,
                dto.Radius
            );
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static TargetingStrategyDto ToDto(ITargetingStrategy strategy)
        {
            // 핵심 로직을 처리합니다.
            if (strategy == null) return null;

            return strategy switch
            {
                SelfTargetingStrategy => new TargetingStrategyDto { Type = "Self" },
                RandomTargetingStrategy => new TargetingStrategyDto { Type = "Random" },
                NearestEnemyTargetingStrategy s => new TargetingStrategyDto
                {
                    Type = "NearestEnemy",
                    MaxRange = GetMaxRange(s)
                },
                NearestNEnemiesTargetingStrategy s => new TargetingStrategyDto
                {
                    Type = "NearestN",
                    MaxTargets = GetMaxTargets(s),
                    MaxRange = GetMaxRange(s)
                },
                LowestHpTargetingStrategy => new TargetingStrategyDto { Type = "LowestHp" },
                AreaTargetingStrategy s => new TargetingStrategyDto
                {
                    Type = "Area",
                    Radius = GetRadius(s)
                },
                _ => null
            };
        }

        #endregion

        #region 蹂???뱀뀡

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static IEffectDurationPolicy CreateDurationPolicy(DurationPolicyDto dto)
        {
            // 핵심 로직을 처리합니다.
            if (dto == null || string.IsNullOrEmpty(dto.Type)) return null;
            return EffectDurationPolicyFactory.Create(dto.Type);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static IEffectDurationPolicy CreateDurationPolicy(EffectDurationPolicyType type)
        {
            // 핵심 로직을 처리합니다.
            return EffectDurationPolicyFactory.Create(type);
        }

        /// <summary>
        /// 二쇱꽍 ?뺣━
        /// </summary>
        public static DurationPolicyDto ToDto(IEffectDurationPolicy policy)
        {
            // 핵심 로직을 처리합니다.
            if (policy == null) return null;

            var typeName = EffectDurationPolicyFactory.GetTypeName(policy);
            return typeName != null ? new DurationPolicyDto { Type = typeName } : null;
        }

        #endregion

        #region 蹂???뱀뀡
        /// <summary>
        /// FromDtoList 함수를 처리합니다.
        /// </summary>

        private static List<GameplayEffect> FromDtoList(List<GameplayEffectDto> dtos)
        {
            // 핵심 로직을 처리합니다.
            if (dtos == null) return new List<GameplayEffect>();
            var result = new List<GameplayEffect>(dtos.Count);
            foreach (var dto in dtos)
            {
                var effect = FromDto(dto);
                if (effect != null) result.Add(effect);
            }
            return result;
        }
        /// <summary>
        /// FromDtoList 함수를 처리합니다.
        /// </summary>

        private static List<AttributeModifier> FromDtoList(List<AttributeModifierDto> dtos)
        {
            // 핵심 로직을 처리합니다.
            if (dtos == null) return new List<AttributeModifier>();
            var result = new List<AttributeModifier>(dtos.Count);
            foreach (var dto in dtos)
            {
                result.Add(FromDto(dto));
            }
            return result;
        }
        /// <summary>
        /// ToDtoList 함수를 처리합니다.
        /// </summary>

        private static List<GameplayEffectDto> ToDtoList(List<GameplayEffect> effects)
        {
            // 핵심 로직을 처리합니다.
            if (effects == null) return null;
            var result = new List<GameplayEffectDto>(effects.Count);
            foreach (var effect in effects)
            {
                var dto = ToDto(effect);
                if (dto != null) result.Add(dto);
            }
            return result;
        }
        /// <summary>
        /// ToDtoList 함수를 처리합니다.
        /// </summary>

        private static List<AttributeModifierDto> ToDtoList(List<AttributeModifier> modifiers)
        {
            // 핵심 로직을 처리합니다.
            if (modifiers == null) return null;
            var result = new List<AttributeModifierDto>(modifiers.Count);
            foreach (var modifier in modifiers)
            {
                result.Add(ToDto(modifier));
            }
            return result;
        }
        /// <summary>
        /// TagsFromStrings 함수를 처리합니다.
        /// </summary>

        private static GameplayTagContainer TagsFromStrings(List<string> tags)
        {
            // 핵심 로직을 처리합니다.
            var container = new GameplayTagContainer();
            if (tags == null) return container;
            foreach (var tag in tags)
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    container.AddTag(new FGameplayTag(tag));
                }
            }
            return container;
        }
        /// <summary>
        /// TagsToStrings 함수를 처리합니다.
        /// </summary>

        private static List<string> TagsToStrings(GameplayTagContainer container)
        {
            // 핵심 로직을 처리합니다.
            if (container == null) return null;
            var result = new List<string>();
            foreach (var tag in container.Tags)
            {
                if (tag.IsValid)
                {
                    result.Add(tag.Value);
                }
            }
            return result.Count > 0 ? result : null;
        }
        /// <summary>
        /// ParseDurationType 함수를 처리합니다.
        /// </summary>

        private static EffectDurationType ParseDurationType(string value)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(value)) return EffectDurationType.Instant;
            return value.ToLowerInvariant() switch
            {
                "instant" => EffectDurationType.Instant,
                "infinite" => EffectDurationType.Infinite,
                "hasduration" => EffectDurationType.HasDuration,
                _ => EffectDurationType.Instant
            };
        }
        /// <summary>
        /// ParseOperation 함수를 처리합니다.
        /// </summary>

        private static AttributeModifierOperationType ParseOperation(string value)
        {
            // 핵심 로직을 처리합니다.
            if (string.IsNullOrEmpty(value)) return AttributeModifierOperationType.Add;
            return value.ToLowerInvariant() switch
            {
                "add" => AttributeModifierOperationType.Add,
                "addpercent" => AttributeModifierOperationType.AddPercent,
                "multiply" => AttributeModifierOperationType.Multiply,
                "override" => AttributeModifierOperationType.Override,
                _ => AttributeModifierOperationType.Add
            };
        }
        /// <summary>
        /// ParseValueMode 함수를 처리합니다.
        /// </summary>

        private static AttributeModifierValueMode ParseValueMode(string value)
        {
            // 핵심 로직을 처리합니다.
            if(Enum.TryParse<AttributeModifierValueMode>(value, out var type))
            {
                return type;
            }
            
            return AttributeModifierValueMode.None;
        }
        /// <summary>
        /// ParseCalculatorType 함수를 처리합니다.
        /// </summary>

        private static AttributeCalculatorType ParseCalculatorType(string value)
        {
            // 핵심 로직을 처리합니다.
            if(Enum.TryParse<AttributeCalculatorType>(value, out var type))
            {
                return type;
            }
            
            return AttributeCalculatorType.None;
        }

        // 二쇱꽍 ?뺣━
        private static float GetMaxRange(NearestEnemyTargetingStrategy s) => float.PositiveInfinity;
        private static float GetMaxRange(NearestNEnemiesTargetingStrategy s) => float.PositiveInfinity;
        private static int GetMaxTargets(NearestNEnemiesTargetingStrategy s) => 1;
        private static float GetRadius(AreaTargetingStrategy s) => 0f;

        #endregion
    }
}



