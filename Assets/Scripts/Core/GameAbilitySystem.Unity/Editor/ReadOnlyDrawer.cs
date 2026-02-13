using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
        /// 요약 설명입니다.
        /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public sealed class ReadOnlyDrawer : PropertyDrawer
    {
        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = wasEnabled;
        }

        /// <summary>
        /// 요약 설명입니다.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
