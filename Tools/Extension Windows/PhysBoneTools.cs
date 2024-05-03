using BwmpsTools.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using static BwmpsTools.Utils.Utilities;

namespace BwmpsTools.Tools
{
    public class PhysboneTools : EditorWindow
    {
        private static readonly string Title = "Physbone Tools";

        public void Init()
        {
            PhysboneTools window = (PhysboneTools)GetWindow(typeof(PhysboneTools), false, Title, true);
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {
            EditorGUIExtensions.TitleBox(Title, null);
            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                GUILayout.Label("Components:");
                if (BasicUtils.TargetModel == null)
                {
                    GUILayout.Label("No target model selected");
                    return;
                }
                List<VRCPhysBone> physBones = GetAllComponentsInChildren<VRCPhysBone>(BasicUtils.TargetModel.gameObject);

                foreach (VRCPhysBone physBone in physBones)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(physBone, typeof(VRCPhysBone), true);
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Delete"))
                    {
                        Undo.DestroyObjectImmediate(physBone);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private static PhysboneTools instance;

        public static PhysboneTools Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<PhysboneTools>();
                }
                return instance;
            }
        }

    }
}
