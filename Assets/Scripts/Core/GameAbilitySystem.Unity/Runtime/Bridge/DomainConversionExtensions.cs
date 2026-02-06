using System.Collections.Generic;
using System.Linq;
using Noname.GameAbilitySystem;

namespace Noname.GameCore.Helper
{
    /// <summary>
    /// Domain과 Presentation 간 변환을 위한 확장 메서드입니다.
    /// </summary>
    public static class DomainConversionExtensions
    {
        /// <summary>
        /// Unity GameplayTagContainer를 Domain GameplayTagContainer로 변환합니다.
        /// </summary>
        public static GameplayTagContainer ToDomain(this GameplayTagContainerView container)
        {
            var model = new GameplayTagContainer();
            if (container != null)
            {
                foreach (var tag in container.Tags)
                {
                    model.AddTag(tag.ToDomain());
                }
            }
            return model;
        }

        public static AttributeSet ToDomain(this AttributeConfig definition)
        {
            if(definition == null) return null;

            var attributeSet = new AttributeSet();

            attributeSet.SetAttribute(
                definition.Id,
                definition.DefaultBaseValue,
                definition.MinValue,
                definition.MaxValue);

            return attributeSet;
        }

        /// <summary>
        /// List<AttributeConfig>를 Domain AttributeSet으로 변환합니다.
        /// </summary>
        public static AttributeSet ToDomain(this List<AttributeConfig> definitions)
        {
            if(definitions == null) return null;

            var attributeSet = new AttributeSet();

            foreach(var definition in definitions)
            {
                if(definition == null) continue;

                attributeSet.SetAttribute(
                    definition.Id,
                    definition.DefaultBaseValue,
                    definition.MinValue,
                    definition.MaxValue);
            }

            return attributeSet;
        }

        public static GameplayAbility ToDomain(this GameplayAbilityConfig config)
        {
            if(config == null) return null;

            var ability = new GameplayAbility
            {
                AbilityTag = config.AbilityTag.ToDomain(),
                DisplayName = config.DisplayName,
                Description = config.Description,
                CostEffects = config.CostEffects.ToDomain(),
                AppliedEffects = config.AppliedEffects.ToDomain(),
                ActivationRequiredTags = config.ActivationRequiredTags.ToDomain(),
                ActivationBlockedTags = config.ActivationBlockedTags.ToDomain(),
            };

            return ability;
        }

        public static List<GameplayAbility> ToDomain(this List<GameplayAbilityConfig> config)
        {
            if(config == null) return null;

            var abilities = new List<GameplayAbility>();
            foreach(var ability in config)
            {
                if(ability == null) continue;

                abilities.Add(ability.ToDomain());
            }
        
            return abilities;
        }

        /// <summary>
        /// GameplayEffectConfig를 Domain GameplayEffect로 변환합니다.
        /// </summary>
        public static GameplayEffect ToDomain(this GameplayEffectConfig config)
        {
            if (config == null) return null;

            var effect = new GameplayEffect
            {
                EffectId = config.name,
                DisplayName = config.name,
                Description = config.Description,
                DurationType = config.DurationType,
                Duration = config.Duration,
                Period = config.Period,
                MaxStack = config.MaxStack,
                Modifiers = config.Modifiers.ToList().ToDomain(),
                GrantedTags = config.GrantedTags.ToDomain(),
                RequiredTags = config.ActivationRequiredTags.ToDomain(),
                BlockedTags = config.ActivationBlockedTags.ToDomain(),
            };

            return effect;
        }

        /// <summary>
        /// GameplayEffectConfig 목록을 Domain GameplayEffect 목록으로 변환합니다.
        /// </summary>
        public static List<GameplayEffect> ToDomain(this List<GameplayEffectConfig> config)
        {
            if (config == null) return null;

            var effects = new List<GameplayEffect>();
            foreach(var effect in config)
            {
                if(effect == null) continue;

                effects.Add(effect.ToDomain());
            }
        
            return effects;
        }

        public static List<AttributeModifier> ToDomain(this List<AttributeModifierView> config)
        {
            if (config == null) return null;

            var modifiers = new List<AttributeModifier>();

            foreach(var modifier in config)
            {
                modifiers.Add
                (
                    new AttributeModifier
                    {
                        AttributeId = new AttributeId(modifier.AttributeId),
                        Operation = modifier.Operation,
                        ValueMode = modifier.ValueMode,
                        Magnitude = modifier.Magnitude,
                        CalculatorType = modifier.CalculatorType,
                    }
                );
            }

            return modifiers;
        }
    }
}
