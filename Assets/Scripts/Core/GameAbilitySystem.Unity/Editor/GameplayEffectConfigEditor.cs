using Noname.GameAbilitySystem;
using Noname.GameCore.Helper;
using UnityEditor;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// GameplayEffectConfig ?몄뒪?숉꽣瑜??ъ슜???뺤쓽濡??쒖떆?⑸땲??
    /// </summary>
    [CustomEditor(typeof(GameplayEffectConfig))]
    public sealed class GameplayEffectConfigEditor : UnityEditor.Editor
    {
        /// <summary>
        /// ?듭뀡???곕씪 ?몄뒪?숉꽣 ?꾨뱶瑜??쇱퀜?낅땲??
        /// </summary>
        public override void OnInspectorGUI()
        {
            // 핵심 로직을 처리합니다.
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

            // 湲곕낯 ?뺣낫
            EditorGUILayout.PropertyField(displayNameProp);
            EditorGUILayout.PropertyField(descriptionProp);

            // 吏???쒓컙
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

            // ?쒓렇 諛??섏젙??            EditorGUILayout.PropertyField(grantedTagsProp, true);
            EditorGUILayout.PropertyField(requiredTagsProp, true);
            EditorGUILayout.PropertyField(blockedTagsProp, true);
            EditorGUILayout.PropertyField(modifiersProp, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
