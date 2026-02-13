using Noname.GameCore.Helper;
using UnityEditor;
using UnityEngine;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// AttributeModifierView를 Inspector에서 그려주는 커스텀 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(AttributeModifierView))]
    public sealed class AttributeModifierViewDrawer : PropertyDrawer
    {
        private const float Padding = 2f;

        /// <summary>
        /// AttributeModifierView 필드를 Inspector에 렌더링합니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 펼침 상태와 값 모드에 따라 표시할 필드를 분기합니다.
            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var currentY = position.y;

            var foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            currentY += lineHeight + spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var modeProp = property.FindPropertyRelative("_valueMode");

                var rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(rect, modeProp);
                currentY += lineHeight + spacing;

                var valueMode = (AttributeModifierValueMode)modeProp.enumValueIndex;
                if (valueMode == AttributeModifierValueMode.Static)
                {
                    // Static 모드: AttributeId/Operation/Magnitude를 노출합니다.
                    var attributeIdProp = property.FindPropertyRelative("_attributeId");
                    var operationProp = property.FindPropertyRelative("_operation");
                    var magnitudeProp = property.FindPropertyRelative("_magnitude");

                    rect = new Rect(position.x, currentY, position.width, lineHeight);
                    EditorGUI.PropertyField(rect, attributeIdProp);
                    currentY += lineHeight + spacing;

                    rect = new Rect(position.x, currentY, position.width, lineHeight);
                    EditorGUI.PropertyField(rect, operationProp);
                    currentY += lineHeight + spacing;

                    rect = new Rect(position.x, currentY, position.width, lineHeight);
                    EditorGUI.PropertyField(rect, magnitudeProp);
                }
                else
                {
                    // Calculated 모드: CalculatorType/Coefficient를 노출합니다.
                    var calculatorTypeProp = property.FindPropertyRelative("_calculatorType");
                    var coefficientProp = property.FindPropertyRelative("_coefficient");

                    rect = new Rect(position.x, currentY, position.width, lineHeight);
                    EditorGUI.PropertyField(rect, calculatorTypeProp);
                    currentY += lineHeight + spacing;

                    rect = new Rect(position.x, currentY, position.width, lineHeight);
                    EditorGUI.PropertyField(rect, coefficientProp);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 현재 펼침 상태에 맞는 Property 높이를 계산합니다.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 펼침/모드에 따라 필요한 라인 수를 계산합니다.
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
            {
                return lineHeight + Padding;
            }

            var modeProp = property.FindPropertyRelative("_valueMode");
            var valueMode = (AttributeModifierValueMode)modeProp.enumValueIndex;

            int lineCount;
            if (valueMode == AttributeModifierValueMode.Static)
            {
                // Foldout + ValueMode + AttributeId + Operation + Magnitude 구성
                lineCount = 5;
            }
            else
            {
                // Foldout + ValueMode + CalculatorType + Coefficient 구성
                lineCount = 4;
            }

            return (lineCount * lineHeight) + ((lineCount - 1) * spacing) + Padding;
        }
    }
}