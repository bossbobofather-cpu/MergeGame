using Noname.GameCore.Helper;
using UnityEditor;
using UnityEngine;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// GameplayAbilityConfig ?몄뒪?숉꽣瑜??ъ슜???뺤쓽濡??쒖떆?⑸땲??
    /// </summary>
    [CustomEditor(typeof(GameplayAbilityConfig))]
    public sealed class GameplayAbilityConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _abilityTagProp;
        private SerializedProperty _dpNameProp;
        private SerializedProperty _dpDescProp;
        private SerializedProperty _cooldownEffectProp;
        private SerializedProperty _costEffectsProp;
        private SerializedProperty _appliedEffectsProp;
        private SerializedProperty _activationRequiredTagsProp;
        private SerializedProperty _activationBlockedTagsProp;

        // Targeting Strategy
        private SerializedProperty _targetingStrategyTypeProp;
        private SerializedProperty _targetingMaxRangeProp;
        private SerializedProperty _targetingMaxTargetsProp;
        private SerializedProperty _targetingRadiusProp;
        /// <summary>
        /// OnEnable 함수를 처리합니다.
        /// </summary>

        private void OnEnable()
        {
            // 핵심 로직을 처리합니다.
            _abilityTagProp = serializedObject.FindProperty("_abilityTag");
            _dpNameProp = serializedObject.FindProperty("_dpName");
            _dpDescProp = serializedObject.FindProperty("_dpDesc");
            _cooldownEffectProp = serializedObject.FindProperty("_cooldownEffect");
            _costEffectsProp = serializedObject.FindProperty("_costEffects");
            _appliedEffectsProp = serializedObject.FindProperty("_appliedEffects");
            _activationRequiredTagsProp = serializedObject.FindProperty("_activationRequiredTags");
            _activationBlockedTagsProp = serializedObject.FindProperty("_activationBlockedTags");

            // Targeting Strategy
            _targetingStrategyTypeProp = serializedObject.FindProperty("_targetingStrategyType");
            _targetingMaxRangeProp = serializedObject.FindProperty("_targetingMaxRange");
            _targetingMaxTargetsProp = serializedObject.FindProperty("_targetingMaxTargets");
            _targetingRadiusProp = serializedObject.FindProperty("_targetingRadius");
        }
        /// <summary>
        /// OnInspectorGUI 함수를 처리합니다.
        /// </summary>

        public override void OnInspectorGUI()
        {
            // 핵심 로직을 처리합니다.
            serializedObject.Update();

            EditorGUILayout.LabelField("湲곕낯 ?뺣낫", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_abilityTagProp, new GUIContent("Ability Tag"));
            EditorGUILayout.PropertyField(_dpNameProp, new GUIContent("Display Name"));
            EditorGUILayout.PropertyField(_dpDescProp, new GUIContent("Description"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("?④낵", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_cooldownEffectProp, new GUIContent("Cooldown Effect"));
            EditorGUILayout.PropertyField(_costEffectsProp, new GUIContent("Cost Effects"), true);
            EditorGUILayout.PropertyField(_appliedEffectsProp, new GUIContent("Applied Effects"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("?쒓렇 議곌굔", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_activationRequiredTagsProp, new GUIContent("Required Tags"), true);
            EditorGUILayout.PropertyField(_activationBlockedTagsProp, new GUIContent("Blocked Tags"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("?寃뚰똿 ?꾨왂", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_targetingStrategyTypeProp, new GUIContent("Strategy Type"));
            EditorGUILayout.PropertyField(_targetingMaxRangeProp, new GUIContent("Max Range"));
            EditorGUILayout.PropertyField(_targetingMaxTargetsProp, new GUIContent("Max Targets"));
            EditorGUILayout.PropertyField(_targetingRadiusProp, new GUIContent("Radius"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
