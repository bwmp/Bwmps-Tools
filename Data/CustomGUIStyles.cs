using UnityEngine;

namespace BwmpsTools.Utils
{
    public class CustomGUIStyles
    {
        public static readonly GUIStyle centeredTitle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        public static readonly GUIStyle centeredDescription = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            richText = true,
            wordWrap= true,
        };

        public static readonly GUIStyle group = new GUIStyle(GUI.skin.box)
        {
            richText = true,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(4, 4, 4, 4),
            margin = new RectOffset(0, 0, 0, 0)
        };

        public static readonly GUIStyle header = new GUIStyle(GUI.skin.box)
        {
            stretchWidth = true
        };
    }
}
