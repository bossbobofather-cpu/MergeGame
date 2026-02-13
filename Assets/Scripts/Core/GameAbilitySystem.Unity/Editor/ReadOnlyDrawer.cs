using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Noname.GameCore.Helper.Editor
{
    /// <summary>
    /// ReadOnlyAttribute媛 遺숈? ?꾨뱶瑜??쎄린 ?꾩슜?쇰줈 ?쒖떆?⑸땲??
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public sealed class ReadOnlyDrawer : PropertyDrawer
    {
        /// <summary>
        /// GUI ?곹깭瑜??쇱떆 鍮꾪솢?깊솕???쎄린 ?꾩슜?쇰줈 洹몃┰?덈떎.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 湲곗〈 GUI ?곹깭 蹂댁〈
            var wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = wasEnabled;
        }

        /// <summary>
        /// 湲곕낯 ?꾨줈?쇳떚 ?믪씠瑜?洹몃?濡??ъ슜?⑸땲??
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 湲곕낯 ?믪씠瑜?洹몃?濡??ъ슜
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
