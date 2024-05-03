using BwmpsTools.Utils;
using UnityEditor;
using UnityEngine;
using static BwmpsTools.Tools.BasicUtils;
using static BwmpsTools.Utils.Utilities;

namespace BwmpsTools.Tools.Extension_Windows
{
    internal class CalcBounds : EditorWindow
    {
        private SkinnedMeshRenderer targetMesh;

        private static readonly string Title = "Mesh Renderer Bounds";

        public void Init()
        {
            CalcBounds window = (CalcBounds)GetWindow(typeof(CalcBounds));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private void OnGUI()
        {

            targetMesh = EditorGUILayout.ObjectField("Base mesh", targetMesh, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

            if (GUILayout.Button("Calculate Bounds"))
            {
                CalculateBounds();
            }

        }

        private void CalculateBounds()
        {
            Bounds targetBounds = targetMesh.localBounds;
            foreach (SkinnedMeshRenderer mesh in GetAllComponentsInChildren<SkinnedMeshRenderer>(TargetModel, true))
            {
                mesh.localBounds = targetBounds;
            }
        }

        private static CalcBounds instance;

        public static CalcBounds Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<CalcBounds>();
                }
                return instance;
            }
        }

    }
}
