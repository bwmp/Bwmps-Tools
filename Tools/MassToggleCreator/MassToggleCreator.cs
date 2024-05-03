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

namespace BwmpsTools.Tools.MassToggleCreator
{
    internal class MassToggleCreator : EditorWindow
    {
        private VRCAvatarDescriptor descriptor;
        private VRCExpressionsMenu mainMenu;
        private VRCAvatar avatar;
        private DefaultAsset AnimationFolder;
        private ReorderableList toggleDataList;
        private static readonly string Title = "Mass Toggle Creator";
        private static readonly string Description = "Easily create slightly more detailed toggles";

        private class ToggleData
        {
            public string name;
            public string parameterName;
            public bool defaultState;
            public bool objectDefaultState;
            public GameObject obj;
            public Texture2D icon;
            public bool foldout;
        }

        [MenuItem("Bwmp's Tools/Mass Toggle Creator/Normal", false, 0)]
        static void Init()
        {
            MassToggleCreator window = (MassToggleCreator)GetWindow(typeof(MassToggleCreator));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private void OnEnable()
        {
            toggleDataList = new ReorderableList(new List<ToggleData>(), typeof(ToggleData), true, true, true, true)
            {
                drawElementCallback = (Rect rect, int i, bool isActive, bool isFocused) =>
                {
                    ToggleData toggleData = (ToggleData)toggleDataList.list[i];
                    float elementHeight = CalculateElementHeight();
                    rect.height = elementHeight;
                    DrawToggleData(rect, toggleData);
                },
                elementHeightCallback = (int i) =>
                {
                    ToggleData toggleData = (ToggleData)toggleDataList.list[i];
                    float elementHeight = EditorGUIUtility.singleLineHeight + 5;

                    if (toggleData.foldout)
                    {
                        elementHeight += CalculateElementHeight();
                    }

                    return elementHeight;
                },
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Toggle Objects");
                },
                onAddCallback = (ReorderableList list) =>
                {
                    list.list.Add(new ToggleData());
                },
            };
        }

        private float CalculateElementHeight()
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            return lineHeight * 6 + EditorGUIUtility.standardVerticalSpacing - 10;
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

                toggleDataList.DoLayoutList();

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(toggleDataList.list.Count <= 0 || descriptor == null || mainMenu == null || mainMenu.controls.Count == 8);
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
                    foreach (ToggleData item in toggleDataList.list)
                    {
                        string param = item.parameterName + "_Toggle";
                        CreateAndFillStateLayer(avatar.Controller, param, CreateNewAnimationClip(item.obj, item.parameterName, folderPath, !item.objectDefaultState), CreateNewAnimationClip(item.obj, item.parameterName, folderPath, item.objectDefaultState));
                        CreateNewParameter(avatar.Parameters, param, item.defaultState ? 1 : 0, true, VRCExpressionParameters.ValueType.Bool);
                        avatar.Controller.AddParameter(param, UnityEngine.AnimatorControllerParameterType.Bool);
                        if (mainMenu.controls.Count == 7)
                        {
                            VRCExpressionsMenu newMenu = CreateNewSubMenu(mainMenu, "Next Page", mainMenu.name + "_Next", GetAssetFolder(mainMenu.GetInstanceID()));
                            mainMenu = newMenu;
                        }
                        AddControlToMenu(mainMenu, item.parameterName, param, null, item.icon, ControlType.Toggle);
                    }
                    avatar.Parameters.parameters = new List<VRCExpressionParameters.Parameter>(avatar.Parameters.parameters).ToArray();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private int SelectedInt = 0;

        [MenuItem("GameObject/Bwmp's Tools/Mass Toggle Creator/Normal", false, 0)]
        private static void AddSelectedObjectsToMeshObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            MassToggleCreator window = (MassToggleCreator)GetWindow(typeof(MassToggleCreator));
            GameObject obj = selectedObjects[window.SelectedInt];
            window.toggleDataList.list.Add(new ToggleData
            {
                name = obj.name,
                parameterName = obj.name,
                obj = obj,
                defaultState = false,
                objectDefaultState = true,
                icon = null
            });

            window.SelectedInt += 1;
            if (window.SelectedInt >= selectedObjects.Length)
            {
                window.SelectedInt = 0;
            }
        }

        private void DrawToggleData(Rect rect, ToggleData toggleData)
        {
            rect.y += 2;
            toggleData.foldout = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), toggleData.foldout, toggleData.name);

            if (toggleData.foldout)
            {
                EditorGUI.indentLevel++;

                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Toggle Name");
                toggleData.name = EditorGUI.TextField(new Rect(rect.x + 100, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), toggleData.name);

                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Parameter Name");
                toggleData.parameterName = EditorGUI.TextField(new Rect(rect.x + 100, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), toggleData.parameterName);

                rect.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "Default state");
                toggleData.defaultState = EditorGUI.Toggle(new Rect(rect.x + 100, rect.y, rect.width / 2 - 100, EditorGUIUtility.singleLineHeight), toggleData.defaultState);

                EditorGUI.LabelField(new Rect(rect.x + rect.width / 2 - 20, rect.y, rect.width / 2, EditorGUIUtility.singleLineHeight), "Object default state");
                toggleData.objectDefaultState = EditorGUI.Toggle(new Rect(rect.x + rect.width / 2 + 100, rect.y, rect.width / 2 - 100, EditorGUIUtility.singleLineHeight), toggleData.objectDefaultState);

                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Toggle Object");
                toggleData.obj = EditorGUI.ObjectField(new Rect(rect.x + 100, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), toggleData.obj, typeof(GameObject), true) as GameObject;

                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Menu icon");
                toggleData.icon = EditorGUI.ObjectField(new Rect(rect.x + 100, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight), toggleData.icon, typeof(Texture2D), true) as Texture2D;

                EditorGUI.indentLevel--;
            }
        }


    }
}
