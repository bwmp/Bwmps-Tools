using UnityEditor;
using UnityEngine;

namespace BwmpsTools.Utils
{
    public static class EditorGUIExtensions
    {
        public static string TextFieldWithPlaceholder(Rect position, ref string text, string placeholder)
        {
            GUIStyle textStyle = GUI.skin.textField;
            GUIStyle placeholderStyle = new GUIStyle(textStyle);
            placeholderStyle.normal.textColor = Color.grey;

            if (string.IsNullOrEmpty(text))
            {
                GUI.Label(position, placeholder, placeholderStyle);
            }

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent());

            GUI.SetNextControlName("TextFieldWithPlaceholder");
            text = EditorGUI.TextField(position, text, textStyle);

            if (string.IsNullOrEmpty(text) && GUI.GetNameOfFocusedControl() != "TextFieldWithPlaceholder")
            {
                GUI.Label(position, placeholder, placeholderStyle);
            }

            return text;
        }

        public static void TitleBox(string title, string description)
        {
            using (new EditorGUILayout.VerticalScope(new GUIStyle("box") { stretchWidth = true }))
            {
                EditorGUILayout.LabelField($"<b><size=15>{title}</size></b>", CustomGUIStyles.centeredTitle);
                if (description != null)
                {
                    EditorGUILayout.LabelField($"<b><size=12.5>{description}</size></b>", CustomGUIStyles.centeredDescription);
                }
            }
        }

    }
}
