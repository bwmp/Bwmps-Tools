using BwmpsTools.Tools.Extension_Windows;
using BwmpsTools.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static BwmpsTools.Tools.BasicUtils;

namespace BwmpsTools.Tools
{
    internal class MissingScripts : EditorWindow
    {
        private readonly List<GameObject> objectsWithMissingScripts = new List<GameObject>();

        private static readonly string Title = "Missing Scripts";

        public void Init()
        {
            MissingScripts window = (MissingScripts)GetWindow(typeof(MissingScripts), false, Title, true);
            window.titleContent = new GUIContent(Title);
            window.Show();
            FindObjectsWithMissingScripts(TargetModel);
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {

            EditorGUIExtensions.TitleBox(Title, null);
            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                foreach (GameObject obj in objectsWithMissingScripts)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                    EditorGUI.EndDisabledGroup();

                    if (HasMissingScripts(obj) && GUILayout.Button("Remove Missing Script"))
                    {
                        Undo.RecordObject(obj, "Remove Missing Scripts");
                        RemoveMissingScripts(obj);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void FindObjectsWithMissingScripts(GameObject obj)
        {
            if (HasMissingScripts(obj))
            {
                objectsWithMissingScripts.Add(obj);
            }
            foreach (Transform child in obj.transform)
            {
                FindObjectsWithMissingScripts(child.gameObject);
            }
        }

        bool HasMissingScripts(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component == null)
                {
                    return true;
                }
            }
            return false;
        }

        void RemoveMissingScripts(GameObject obj)
        {
            var components = obj.GetComponents<Component>();
            var serializedObject = new SerializedObject(obj);
            var prop = serializedObject.FindProperty("m_Component");

            int r = 0;
            for (int j = 0; j < components.Length; j++)
            {
                if (components[j] == null)
                {
                    prop.DeleteArrayElementAtIndex(j - r);
                    r++;
                }
            }
            serializedObject.ApplyModifiedProperties();
            Debug.Log($"Missing scripts removed from {obj.name}.");
        }

        private static MissingScripts instance;

        public static MissingScripts Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<MissingScripts>();
                }
                return instance;
            }
        }

    }
}
