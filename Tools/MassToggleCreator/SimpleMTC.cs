using BwmpsTools.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static BwmpsTools.Utils.ExpressionUtils;
using static BwmpsTools.Utils.Structs;
using static BwmpsTools.Utils.Utilities;
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;

namespace BwmpsTools.Tools
{
    public class SimpleMTC : EditorWindow
    {
        private VRCAvatarDescriptor descriptor;
        private VRCExpressionsMenu mainMenu;
        private VRCAvatar avatar;
        private DefaultAsset AnimationFolder;
        private ReorderableList meshObjects;

        private static readonly string Title = "Simple Mass Toggle Creator";
        private static readonly string Description = "Easily create simple toggles";

        [MenuItem("Bwmp's Tools/Mass Toggle Creator/Simple", false, 0)]
        static void Init()
        {
            SimpleMTC window = (SimpleMTC)GetWindow(typeof(SimpleMTC));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private void OnEnable()
        {
            meshObjects = new ReorderableList(new List<KeyValuePair<string, GameObject>>(), typeof(KeyValuePair<string, GameObject>), true, true, true, true)
            {
                drawElementCallback = (Rect rect, int i, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    KeyValuePair<string, GameObject> item = (KeyValuePair<string, GameObject>)meshObjects.list[i];
                    string txt = item.Key;
                    string name = EditorGUIExtensions.TextFieldWithPlaceholder(new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight), ref txt, "Toggle name here");
                    GameObject obj = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + 115, rect.y, rect.width - 105, EditorGUIUtility.singleLineHeight), item.Value, typeof(GameObject), true);
                    meshObjects.list[i] = new KeyValuePair<string, GameObject>(name, obj);
                },
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Toggle Objects");
                },
            };
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {

            EditorGUIExtensions.TitleBox(Title, Description);

            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                descriptor = EditorGUILayout.ObjectField(new GUIContent("Target Model", "The root model to search for descendants"), descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                AnimationFolder = EditorGUILayout.ObjectField(new GUIContent("Animation Folder", "The folder to search for animations"), AnimationFolder, typeof(DefaultAsset), true) as DefaultAsset;
                mainMenu = EditorGUILayout.ObjectField(new GUIContent("Expression Menu", "The expressions menu to add toggles to"), mainMenu, typeof(VRCExpressionsMenu), true) as VRCExpressionsMenu;

                GUILayout.Label("Add meshes here:");

                meshObjects.DoLayoutList();

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(meshObjects.list.Count <= 0 || descriptor == null || mainMenu == null || mainMenu.controls.Count == 8);
                if (GUILayout.Button("Create Toggles"))
                {
                    avatar = GetAvatarInfo(descriptor.GetComponent<VRCAvatarDescriptor>());
                    if (avatar.Parameters == null)
                    {
                        EditorUtility.DisplayDialog("Bwmps", "Missing parameters", "Ok");
                        return;
                    };
                    if (avatar.Controller == null)
                    {
                        EditorUtility.DisplayDialog("Bwmps", "Missing FX layer", "Ok");
                        return;
                    };
                    string folderPath = AssetDatabase.GetAssetPath(AnimationFolder);
                    foreach (KeyValuePair<string, GameObject> item in meshObjects.list)
                    {
                        string param = item.Key + "_Toggle";
                        CreateAndFillStateLayer(avatar.Controller, param, CreateNewAnimationClip(item.Value, item.Key, folderPath, false), CreateNewAnimationClip(item.Value, item.Key, folderPath, true));
                        CreateNewParameter(avatar.Parameters, param, 0, true, VRCExpressionParameters.ValueType.Bool);
                        avatar.Controller.AddParameter(param, UnityEngine.AnimatorControllerParameterType.Bool);
                        if (mainMenu.controls.Count == 7)
                        {
                            VRCExpressionsMenu newMenu = CreateNewSubMenu(mainMenu, "Next Page", mainMenu.name + "_Next", GetAssetFolder(mainMenu.GetInstanceID()));
                            mainMenu = newMenu;
                        }
                        AddControlToMenu(mainMenu, item.Key, param, null, null, ControlType.Toggle);
                    }
                    avatar.Parameters.parameters = new List<VRCExpressionParameters.Parameter>(avatar.Parameters.parameters).ToArray();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                }
                EditorGUI.EndDisabledGroup();
            }
        }
        private int SelectedInt = 0;
        [MenuItem("GameObject/Bwmp's Tools/Mass Toggle Creator/Simple", false, 0)]
        private static void AddSelectedObjectsToMeshObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            SimpleMTC window = (SimpleMTC)GetWindow(typeof(SimpleMTC));
            window.meshObjects.list.Add(new KeyValuePair<string, GameObject>(selectedObjects[window.SelectedInt].name, selectedObjects[window.SelectedInt]));
            window.SelectedInt += 1;
            if (window.SelectedInt >= selectedObjects.Length)
            {
                window.SelectedInt = 0;
            }
        }

    }
}
