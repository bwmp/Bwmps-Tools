using BwmpsTools.Utils;
using System.IO;
using UnityEditor;
using UnityEngine;
using static BwmpsTools.Utils.Utilities;

namespace BwmpsTools.Tools
{
    internal class BwmpsTools : EditorWindow
    {
        private string licenseKeyInput = "";
        private string licenseKeyFilePath;

        void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(new GUIStyle("box") { stretchWidth = true }))
            {
                EditorGUILayout.LabelField($"<b><size=15>Bwmp's Tools V{Version}</size></b>", CustomGUIStyles.centeredTitle);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Join Discord"))
                {
                    Application.OpenURL("https://discord.gg/T9Ywp6TgWE");
                }
            }

            if (GUILayout.Button("Copy debug info to clipboard"))
            {
                CopyBugReportData();
            }

            EditorGUILayout.LabelField("Created by: AkiraDev / bwmp");
            EditorGUILayout.LabelField("Both are the same person just 2 diff discord accounts");
        }

        public static void CopyBugReportData()
        {
            string DebugString = "=== Bwmps Tools Debug Info ===\n" +
                $"Version: {Version}\n" +
                "======= End Debug Info =======";
            GUIUtility.systemCopyBuffer = DebugString;
        }
    }
}
