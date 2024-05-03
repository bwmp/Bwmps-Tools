using BwmpsTools.Tools.Extension_Windows;
using System;
using UnityEditor;
using UnityEngine;
using static BwmpsTools.Tools.BasicUtils;

namespace BwmpsTools.Tools.Modules
{
    internal class MiscOptions : BaseModule
    {
        public MiscOptions() : base("Misc Options") { }

        public override void Render(Action renderContent = null)
        {
            base.Render(() =>
            {
                EditorGUI.BeginDisabledGroup(TargetModel == null);
                if (GUILayout.Button("Missing Scripts")) MissingScripts.Instance.Init();
                if (GUILayout.Button("Bounds")) CalcBounds.Instance.Init();
                EditorGUI.EndDisabledGroup();
                renderContent?.Invoke();
            });
        }
    }
}
