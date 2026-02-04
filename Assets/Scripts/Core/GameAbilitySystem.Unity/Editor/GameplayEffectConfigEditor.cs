using Noname.GameAbilitySystem;
using Noname.GameCore.Helper;
using UnityEditor;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// GameplayEffectConfig 인스펙터를 사용자 정의로 표시합니다.
    /// </summary>
    [CustomEditor(typeof(GameplayEffectConfig))]
    public sealed class GameplayEffectConfigEditor : UnityEditor.Editor
    {
        /// <summary>
        /// 옵션에 따라 인스펙터 필드를 펼쳐냅니다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var displayNameProp = serializedObject.FindProperty("_displayName");
            var descriptionProp = serializedObject.FindProperty("_description");
            var durationTypeProp = serializedObject.FindProperty("_durationType");
            var durationProp = serializedObject.FindProperty("_duration");
            var periodProp = serializedObject.FindProperty("_period");
            var maxStackProp = serializedObject.FindProperty("_maxStack");
            var durationPolicyTypeProp = serializedObject.FindProperty("_durationPolicyType");
            var grantedTagsProp = serializedObject.FindProperty("_grantedTags");
            var requiredTagsProp = serializedObject.FindProperty("_activationRequiredTags");
            var blockedTagsProp = serializedObject.FindProperty("_activationBlockedTags");
            var modifiersProp = serializedObject.FindProperty("_modifiers");

            if (displayNameProp == null
                || descriptionProp == null
                || durationTypeProp == null
                || durationProp == null
                || periodProp == null
                || maxStackProp == null
                || durationPolicyTypeProp == null
                || grantedTagsProp == null
                || requiredTagsProp == null
                || blockedTagsProp == null
                || modifiersProp == null)
            {
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // 기본 정보
            EditorGUILayout.PropertyField(displayNameProp);
            EditorGUILayout.PropertyField(descriptionProp);

            // 지속 시간
            EditorGUILayout.PropertyField(durationTypeProp);
            EditorGUILayout.PropertyField(durationPolicyTypeProp);

            var durationType = (EffectDurationType)durationTypeProp.enumValueIndex;
            if (durationType == EffectDurationType.HasDuration)
            {
                EditorGUILayout.PropertyField(durationProp);
                EditorGUILayout.PropertyField(periodProp);
            }
            else if (durationType == EffectDurationType.Infinite)
            {
                EditorGUILayout.PropertyField(periodProp);
            }

            EditorGUILayout.PropertyField(maxStackProp);

            // 태그 및 수정자
            EditorGUILayout.PropertyField(grantedTagsProp, true);
            EditorGUILayout.PropertyField(requiredTagsProp, true);
            EditorGUILayout.PropertyField(blockedTagsProp, true);
            EditorGUILayout.PropertyField(modifiersProp, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
