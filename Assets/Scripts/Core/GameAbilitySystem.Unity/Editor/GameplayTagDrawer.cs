using System;
using Noname.GameAbilitySystem;
using Noname.GameCore.Helper;
using UnityEditor;
using UnityEngine;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// FGameplayTagView를 드롭다운으로 선택할 수 있도록 그려주는 커스텀 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(FGameplayTagView))]
    public sealed class GameplayTagDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 52f;
        private const float HelpBoxHeight = 32f;

        private static readonly GUIContent TagsButtonContent = new("Tags");
        private static readonly GUIContent NoneContent = new("<None>");

        private static GameplayTagRegistry _cachedRegistry;
        private static double _lastRegistryCheck;

        /// <summary>
        /// 태그 입력 필드와 드롭다운 버튼을 렌더링합니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 레지스트리 존재 여부에 따라 텍스트 입력 + 드롭다운 또는 텍스트 입력만 노출합니다.
            var valueProp = property.FindPropertyRelative("_value");
            if (valueProp == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            lineRect = EditorGUI.IndentedRect(lineRect);
            var contentRect = EditorGUI.PrefixLabel(lineRect, label);

            var registry = GetRegistry();
            if (registry != null)
            {
                var buttonRect = new Rect(contentRect.xMax - ButtonWidth, contentRect.y, ButtonWidth, contentRect.height);
                var fieldRect = contentRect;
                fieldRect.width -= ButtonWidth + 4f;

                var newValue = EditorGUI.DelayedTextField(fieldRect, GUIContent.none, valueProp.stringValue);
                if (newValue != valueProp.stringValue)
                {
                    valueProp.stringValue = newValue;
                }

                if (EditorGUI.DropdownButton(buttonRect, TagsButtonContent, FocusType.Passive))
                {
                    ShowTagMenu(valueProp, registry);
                }
            }
            else
            {
                var newValue = EditorGUI.DelayedTextField(contentRect, GUIContent.none, valueProp.stringValue);
                if (newValue != valueProp.stringValue)
                {
                    valueProp.stringValue = newValue;
                }
            }

            var message = GetValidationMessage(valueProp.stringValue, registry);
            if (!string.IsNullOrEmpty(message))
            {
                var helpRect = new Rect(
                    position.x,
                    lineRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    HelpBoxHeight);
                helpRect = EditorGUI.IndentedRect(helpRect);
                EditorGUI.HelpBox(helpRect, message, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 경고 메시지 출력 여부를 반영해 Property 높이를 계산합니다.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 검증 메시지가 있으면 HelpBox 높이를 추가합니다.
            var valueProp = property.FindPropertyRelative("_value");
            var registry = GetRegistry();
            var message = valueProp == null ? string.Empty : GetValidationMessage(valueProp.stringValue, registry);

            if (string.IsNullOrEmpty(message))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + HelpBoxHeight;
        }

        /// <summary>
        /// 레지스트리에 등록된 태그 선택 메뉴를 표시합니다.
        /// </summary>
        private static void ShowTagMenu(SerializedProperty valueProp, GameplayTagRegistry registry)
        {
            // 선택된 태그를 SerializedProperty에 반영합니다.
            var menu = new GenericMenu();
            menu.AddItem(NoneContent, string.IsNullOrEmpty(valueProp.stringValue), () =>
            {
                valueProp.stringValue = string.Empty;
                valueProp.serializedObject.ApplyModifiedProperties();
            });

            var tags = registry.GetAllTags(includeParents: true);
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                menu.AddItem(
                    new GUIContent(tag),
                    string.Equals(valueProp.stringValue, tag, StringComparison.Ordinal),
                    () =>
                    {
                        valueProp.stringValue = tag;
                        valueProp.serializedObject.ApplyModifiedProperties();
                    });
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// GameplayTagRegistry 에셋을 캐시 기반으로 조회합니다.
        /// </summary>
        private static GameplayTagRegistry GetRegistry()
        {
            // 캐시가 있으면 즉시 반환합니다.
            if (_cachedRegistry != null)
            {
                return _cachedRegistry;
            }

            // 과도한 에셋 검색을 방지하기 위해 조회 간격을 둡니다.
            if (EditorApplication.timeSinceStartup - _lastRegistryCheck < 1.0f)
            {
                return _cachedRegistry;
            }

            _lastRegistryCheck = EditorApplication.timeSinceStartup;

            var guids = AssetDatabase.FindAssets("t:GameplayTagRegistry");
            if (guids.Length == 0)
            {
                _cachedRegistry = null;
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedRegistry = AssetDatabase.LoadAssetAtPath<GameplayTagRegistry>(path);
            return _cachedRegistry;
        }

        /// <summary>
        /// 태그 문자열의 형식/등록 여부를 검증하고 경고 메시지를 반환합니다.
        /// </summary>
        private static string GetValidationMessage(string value, GameplayTagRegistry registry)
        {
            // 빈 값은 경고 없이 허용합니다.
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // 태그 문자열 형식을 검증합니다.
            if (!GameplayTagUtility.IsValidTagString(value))
            {
                return "Invalid tag format. Use A.B.C with letters, digits, or underscore.";
            }

            // 레지스트리가 있으면 등록 여부를 검증합니다.
            if (registry != null && !registry.IsTagDefined(value, includeParents: true))
            {
                return "Tag not found in GameplayTagRegistry.";
            }

            return string.Empty;
        }
    }
}