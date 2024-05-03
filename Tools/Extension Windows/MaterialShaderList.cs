using BwmpsTools.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace BwmpsTools.Tools.Extension_Windows
{
    internal class MaterialShaderList : EditorWindow
    {
        readonly Dictionary<Renderer, List<Material>> rendererToMaterials = new Dictionary<Renderer, List<Material>>();
        readonly Dictionary<Renderer, bool> foldoutStates = new Dictionary<Renderer, bool>();

        private static readonly string Title = "Material Shader List";
        private static readonly string Description = "List all materials on each object and the shader they use";

        public void Init()
        {
            MaterialShaderList window = (MaterialShaderList)GetWindow(typeof(MaterialShaderList), false, Title, true);
            window.titleContent = new GUIContent(Title);
            window.Show();
            GetMaterialList(BasicUtils.TargetModel);
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                EditorGUIExtensions.TitleBox(Title, Description);
                using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
                scrollPos = scroll.scrollPosition;
                foreach (var kvp in rendererToMaterials)
                {
                    Renderer renderer = kvp.Key;
                    if (!foldoutStates.ContainsKey(renderer))
                    {
                        foldoutStates.Add(renderer, false);
                    }
                    foldoutStates[renderer] = EditorGUILayout.Foldout(foldoutStates[renderer], renderer.gameObject.name);

                    if (foldoutStates[renderer])
                    {
                        EditorGUI.indentLevel++;

                        if (GUILayout.Button("Focus"))
                        {
                            Selection.activeGameObject = renderer.gameObject;
                        }

                        foreach (Material material in kvp.Value)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(material.name);
                            EditorGUILayout.ObjectField(material.shader, typeof(Shader), false);
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void GetMaterialList(GameObject obj)
        {
            rendererToMaterials.Clear();
            foldoutStates.Clear();
            FindRenderersWithMaterials(obj);
        }

        void FindRenderersWithMaterials(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();

            if (renderer != null)
            {
                List<Material> matchingMaterials = new List<Material>(renderer.sharedMaterials);
                if (matchingMaterials.Count > 0)
                {
                    rendererToMaterials.Add(renderer, matchingMaterials);
                }
            }

            foreach (Transform child in obj.transform)
            {
                FindRenderersWithMaterials(child.gameObject);
            }
        }

        private static MaterialShaderList instance;

        public static MaterialShaderList Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<MaterialShaderList>();
                }
                return instance;
            }
        }

    }
}