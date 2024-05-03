using BwmpsTools.Utils;
using UnityEditor;
using UnityEngine;

namespace BwmpsTools.Tools
{
    public class ExtractMaterialsEditor : EditorWindow
    {
        DefaultAsset matFolder;
        GameObject FBX;
        Shader shader;

        private static readonly string Title = "Extract Materials";
        private static readonly string Description = "Extract materials from fbx and set their shader";

        [MenuItem("Assets/Bwmp's Tools/Extract Materials", false, 0)]
        private static void Init()
        {
            ExtractMaterialsEditor window = (ExtractMaterialsEditor)GetWindow(typeof(ExtractMaterialsEditor));
            window.titleContent = new GUIContent(Title);
            window.Show();
            window.FBX = Selection.activeObject as GameObject;
        }

        [MenuItem("Assets/Bwmp's Tools/Extract Materials", true, 0)]
        private static bool ValidateInit()
        {
            return Selection.activeObject is GameObject;
        }

        private Vector2 scrollPos;
        void OnGUI()
        {
            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                EditorGUIExtensions.TitleBox(Title, Description);
                GUILayout.Label("Select a FBX file to extract its materials.", EditorStyles.boldLabel);
                matFolder = EditorGUILayout.ObjectField("Material Folder", matFolder, typeof(DefaultAsset), false) as DefaultAsset;
                FBX = EditorGUILayout.ObjectField("FBX", FBX, typeof(GameObject), false) as GameObject;
                shader = EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false) as Shader;
                if (GUILayout.Button("Extract Materials"))
                {
                    string path = AssetDatabase.GetAssetPath(FBX);
                    if (matFolder == null || shader == null || FBX == null)
                    {
                        Debug.LogError("Please select a folder, shader and FBX file.");
                        return;
                    }
                    if (path.EndsWith(".fbx"))
                    {
                        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                        foreach (Object asset in assets)
                        {
                            if (asset is Material nat)
                            {
                                Material material = new Material(shader)
                                {
                                    name = nat.name
                                };
                                string folderPath = AssetDatabase.GetAssetPath(matFolder);
                                string fileName = material.name + ".mat";
                                AssetDatabase.CreateAsset(material, folderPath + "/" + fileName);
                            }
                        }
                    }
                }
            }
        }
    }
}
