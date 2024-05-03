using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace BwmpsTools.Utils
{
    public class GUIStyles
    {
        public GUIStyle helpbox;
        public GUIStyle button;
        public GUIStyle helpboxSmall;

        public GUIStyles()
        {
            helpbox = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                wordWrap = true
            };

            button = new GUIStyle("button")
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13
            };

            helpboxSmall = new GUIStyle(EditorStyles.helpBox)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = true,
                padding = new RectOffset(4, 4, 1, 2)
            };

        }

    }
}
