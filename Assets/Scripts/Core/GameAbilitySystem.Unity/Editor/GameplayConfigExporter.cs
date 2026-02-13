using System.Collections.Generic;
using System.IO;
using Noname.GameAbilitySystem;
using Noname.GameAbilitySystem.Json;
using Noname.GameCore.Helper;
using UnityEditor;
using UnityEngine;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// GameplayAbilityConfig/EffectConfig를 JSON으로 내보내는 에디터 도구입니다.
    /// </summary>
    public static class GameplayConfigExporter
    {
        private const string DefaultExportPath = "Assets/Data/Abilities";
        /// <summary>
        /// ExportAllAbilityConfigs 함수를 처리합니다.
        /// </summary>

        [MenuItem("GameAbilitySystem/Export All Ability Configs to JSON")]
        public static void ExportAllAbilityConfigs()
        {
            // 핵심 로직을 처리합니다.
            var configs = FindAllAssets<GameplayAbilityConfig>();
            if (configs.Count == 0)
            {
                Debug.LogWarning("[GameplayConfigExporter] No GameplayAbilityConfig assets found.");
                return;
            }

            EnsureDirectoryExists(DefaultExportPath);

            var exportedCount = 0;
            foreach (var config in configs)
            {
                var dto = ConvertToDto(config);
                var json = JsonUtility.ToJson(dto, true);
                var filePath = Path.Combine(DefaultExportPath, $"{config.name}.json");
                File.WriteAllText(filePath, json);
                exportedCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[GameplayConfigExporter] Exported {exportedCount} ability configs to {DefaultExportPath}");
        }
        /// <summary>
        /// ExportAllEffectConfigs 함수를 처리합니다.
        /// </summary>

        [MenuItem("GameAbilitySystem/Export All Effect Configs to JSON")]
        public static void ExportAllEffectConfigs()
        {
            // 핵심 로직을 처리합니다.
            var configs = FindAllAssets<GameplayEffectConfig>();
            if (configs.Count == 0)
            {
                Debug.LogWarning("[GameplayConfigExporter] No GameplayEffectConfig assets found.");
                return;
            }

            var exportPath = "Assets/Data/Effects";
            EnsureDirectoryExists(exportPath);

            var exportedCount = 0;
            foreach (var config in configs)
            {
                var dto = ConvertToDto(config);
                var json = JsonUtility.ToJson(dto, true);
                var filePath = Path.Combine(exportPath, $"{config.name}.json");
                File.WriteAllText(filePath, json);
                exportedCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[GameplayConfigExporter] Exported {exportedCount} effect configs to {exportPath}");
        }
        /// <summary>
        /// ExportSelectedConfig 함수를 처리합니다.
        /// </summary>

        [MenuItem("GameAbilitySystem/Export Selected Config to JSON")]
        public static void ExportSelectedConfig()
        {
            // 핵심 로직을 처리합니다.
            var selected = Selection.activeObject;
            if (selected == null)
            {
                Debug.LogWarning("[GameplayConfigExporter] No asset selected.");
                return;
            }

            string json = null;
            string fileName = null;
            string exportPath = null;

            if (selected is GameplayAbilityConfig abilityConfig)
            {
                var dto = ConvertToDto(abilityConfig);
                json = JsonUtility.ToJson(dto, true);
                fileName = $"{abilityConfig.name}.json";
                exportPath = DefaultExportPath;
            }
            else if (selected is GameplayEffectConfig effectConfig)
            {
                var dto = ConvertToDto(effectConfig);
                json = JsonUtility.ToJson(dto, true);
                fileName = $"{effectConfig.name}.json";
                exportPath = "Assets/Data/Effects";
            }
            else
            {
                Debug.LogWarning($"[GameplayConfigExporter] Selected asset is not a GameplayConfig: {selected.GetType().Name}");
                return;
            }

            EnsureDirectoryExists(exportPath);
            var filePath = Path.Combine(exportPath, fileName);
            File.WriteAllText(filePath, json);
            AssetDatabase.Refresh();
            Debug.Log($"[GameplayConfigExporter] Exported to {filePath}");
        }

        #region Conversion Methods

        /// <summary>
        /// GameplayAbilityConfig -> GameplayAbilityDto 변환
        /// </summary>
        public static GameplayAbilityDto ConvertToDto(GameplayAbilityConfig config)
        {
            // 핵심 로직을 처리합니다.
            if (config == null) return null;

            return new GameplayAbilityDto
            {
                AbilityTag = config.AbilityTag.Value,
                DisplayName = config.DisplayName,
                Description = config.Description,
                CooldownEffect = config.CooldownEffect.ConvertToDto(),
                CostEffects = config.CostEffects.ConvertToDTO(),
                AppliedEffects = config.AppliedEffects.ConvertToDTO(),
                ActivationRequiredTags = config.ActivationRequiredTags.ConvertToDTO(),
                ActivationBlockedTags = config.ActivationBlockedTags.ConvertToDTO(),
                TargetingStrategy = config.ConvertTargetingToDTO()
            };
        }

        /// <summary>
        /// GameplayEffectConfig -> GameplayEffectDto 변환
        /// </summary>
        public static GameplayEffectDto ConvertToDto(this GameplayEffectConfig config)
        {
            // 핵심 로직을 처리합니다.
            if (config == null) return null;

            return new GameplayEffectDto
            {
                EffectTag = config.name,
                DisplayName = config.DisplayName ?? config.name,
                Description = config.Description ?? "",
                DurationType = config.DurationType.ToString(),
                Duration = config.Duration,
                Period = config.Period,
                MaxStack = config.MaxStack,
                Modifiers = config.Modifiers.ConvertToDTO(),
                GrantedTags = config.GrantedTags.ConvertToDTO(),
                RequiredTags = config.ActivationRequiredTags.ConvertToDTO(),
                BlockedTags = config.ActivationBlockedTags.ConvertToDTO(),
                DurationPolicy = config.DurationPolicyType.ConvertToDTO()
            };
        }

        /// <summary>
        /// EffectDurationPolicyType -> DurationPolicyDto 변환
        /// </summary>
        private static DurationPolicyDto ConvertToDTO(this EffectDurationPolicyType policyType)
        {
            // 핵심 로직을 처리합니다.
            if (policyType == EffectDurationPolicyType.None) return null;

            return new DurationPolicyDto
            {
                Type = policyType.ToString()
            };
        }

        /// <summary>
        /// GameplayAbilityConfig의 타겟팅 옵션 -> TargetingStrategyDto 변환
        /// </summary>
        public static TargetingStrategyDto ConvertTargetingToDTO(this GameplayAbilityConfig config)
        {
            // 핵심 로직을 처리합니다.
            if (config.TargetingStrategyType == TargetingStrategyType.None) return null;

            return new TargetingStrategyDto
            {
                Type = config.TargetingStrategyType.ToString(),
                MaxRange = config.TargetingMaxRange,
                MaxTargets = config.TargetingMaxTargets,
                Radius = config.TargetingRadius
            };
        }
        /// <summary>
        /// ConvertToDTO 함수를 처리합니다.
        /// </summary>

        private static List<GameplayEffectDto> ConvertToDTO(this List<GameplayEffectConfig> configs)
        {
            // 핵심 로직을 처리합니다.
            if (configs == null || configs.Count == 0) return null;

            var result = new List<GameplayEffectDto>(configs.Count);
            foreach (var config in configs)
            {
                if (config != null)
                {
                    result.Add(ConvertToDto(config));
                }
            }
            return result.Count > 0 ? result : null;
        }
        /// <summary>
        /// ConvertToDTO 함수를 처리합니다.
        /// </summary>

        private static List<AttributeModifierDto> ConvertToDTO(this IReadOnlyList<AttributeModifierView> modifiers)
        {
            // 핵심 로직을 처리합니다.
            if (modifiers == null || modifiers.Count == 0) return null;

            var result = new List<AttributeModifierDto>(modifiers.Count);
            foreach (var modifier in modifiers)
            {
                result.Add(new AttributeModifierDto
                {
                    AttributeId = modifier.AttributeId,
                    Operation = modifier.Operation.ToString(),
                    ValueMode = modifier.ValueMode.ToString(),
                    Magnitude = modifier.Magnitude,
                    CalculatorType = modifier.CalculatorType,
                });
            }
            return result;
        }
        /// <summary>
        /// ConvertToDTO 함수를 처리합니다.
        /// </summary>

        private static List<string> ConvertToDTO(this GameplayTagContainerView container)
        {
            // 핵심 로직을 처리합니다.
            if (container == null || container.Tags == null || container.Tags.Count == 0)
                return null;

            var result = new List<string>(container.Tags.Count);
            foreach (var tag in container.Tags)
            {
                if (tag.IsValid)
                {
                    result.Add(tag.Value);
                }
            }
            return result.Count > 0 ? result : null;
        }

        #endregion

        #region Utility Methods

        private static List<T> FindAllAssets<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var result = new List<T>(guids.Length);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    result.Add(asset);
                }
            }

            return result;
        }
        /// <summary>
        /// EnsureDirectoryExists 함수를 처리합니다.
        /// </summary>

        private static void EnsureDirectoryExists(string path)
        {
            // 핵심 로직을 처리합니다.
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #endregion
    }
}

