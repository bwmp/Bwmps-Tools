using BwmpsTools.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace BwmpsTools.Tools.Modules
{
    internal class BaseModule
    {
        string Title { get; set; }
        public BaseModule(string title)
        {
            Title = title;
        }

        public virtual void Render(Action renderContent = null)
        {
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                EditorGUIExtensions.TitleBox(Title, null);

                renderContent?.Invoke();
            }
        }
    }
}
