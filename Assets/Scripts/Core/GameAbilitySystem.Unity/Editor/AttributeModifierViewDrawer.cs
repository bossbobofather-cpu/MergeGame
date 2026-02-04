using Noname.GameCore.Helper;
using UnityEditor;
using UnityEngine;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// AttributeModifierView용 커스텀 프로퍼티 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(AttributeModifierView))]
    public sealed class AttributeModifierViewDrawer : PropertyDrawer
    {
        private const float Padding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var currentY = position.y;

            // Foldout
            var foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            currentY += lineHeight + spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var modeProp = property.FindPropertyRelative("_valueMode");

                // ValueMode (항상 표시)
                var rect = new Rect(position.x, currentY, position.width, lineHeight);
                EditorGUI.PropertyField(rect, modeProp);
                currentY += lineHeight + spacing;

                var valueMode = (AttributeModifierValueMode)modeProp.enumValueIndex;

                if (valueMode == AttributeModifierValueMode.Static)
                {
                    // Static 모드: AttributeId, Operation, Magnitude
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
                    // Calculated 모드: CalculatorType, Coefficient
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
            {
                return lineHeight + Padding;
            }

            var modeProp = property.FindPropertyRelative("_valueMode");
            var valueMode = (AttributeModifierValueMode)modeProp.enumValueIndex;

            // Foldout(1) + ValueMode(1) + 모드별 필드
            int lineCount;
            if (valueMode == AttributeModifierValueMode.Static)
            {
                // Static: Foldout + ValueMode + AttributeId + Operation + Magnitude = 5줄
                lineCount = 5;
            }
            else
            {
                // Calculated: Foldout + ValueMode + CalculatorType + Coefficient = 4줄
                lineCount = 4;
            }

            return (lineCount * lineHeight) + ((lineCount - 1) * spacing) + Padding;
        }
    }
}
