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
using static VRC.Dynamics.CollisionBroadphase_HybridSAP;

namespace BwmpsTools.Tools
{
    public class IntegerToggleHelper : EditorWindow
    {
        private VRCAvatarDescriptor descriptor;
        private VRCExpressionsMenu expressionMenu;
        private VRCAvatar avatar;
        private DefaultAsset animationFolder;
        private string swapName;
        private string defaultName;
        private readonly List<AnimationClip> animations = new List<AnimationClip>();
        private ReorderableList meshObjects;

        private const string Title = "Integer Toggle Creator";
        private const string Description = "Easily create an integer toggle";

        [MenuItem("Bwmp's Tools/Helpers/Integer Toggle Creator", false, 0)]
        static void Init()
        {
            IntegerToggleHelper window = (IntegerToggleHelper)GetWindow(typeof(IntegerToggleHelper));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private void OnEnable()
        {
            meshObjects = new ReorderableList(new List<KeyValuePair<string, GameObject>>(), typeof(KeyValuePair<string, GameObject>), true, true, true, true)
            {
                drawElementCallback = DrawMeshObjectElement,
                drawHeaderCallback = DrawMeshObjectHeader,
            };
        }

        private void DrawMeshObjectElement(Rect rect, int i, bool isActive, bool isFocused)
        {
            rect.y += 2;
            KeyValuePair<string, GameObject> item = (KeyValuePair<string, GameObject>)meshObjects.list[i];
            string txt = item.Key;
            string name = EditorGUIExtensions.TextFieldWithPlaceholder(new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight), ref txt, "Toggle name here");
            GameObject obj = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + 115, rect.y, rect.width - 105, EditorGUIUtility.singleLineHeight), item.Value, typeof(GameObject), true);
            meshObjects.list[i] = new KeyValuePair<string, GameObject>(name, obj);
        }

        private void DrawMeshObjectHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Toggle Objects");
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {

            EditorGUIExtensions.TitleBox(Title, Description);

            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                descriptor = EditorGUILayout.ObjectField(new GUIContent("Avatar", "The target avatar"), descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                animationFolder = EditorGUILayout.ObjectField(new GUIContent("Animation Folder", "The folder to save animations"), animationFolder, typeof(DefaultAsset), true) as DefaultAsset;
                expressionMenu = EditorGUILayout.ObjectField(new GUIContent("Expression Menu", "The expressions menu to add toggles to"), expressionMenu, typeof(VRCExpressionsMenu), true) as VRCExpressionsMenu;
                swapName = EditorGUILayout.TextField("Parameter", swapName);
                defaultName = EditorGUILayout.TextField(new GUIContent("Default Name", "the name of the menu item that has everything togled off"), defaultName);
                GUILayout.Label("Add meshes here:");
                meshObjects.DoLayoutList();

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(descriptor == null);
                if (GUILayout.Button("Generate"))
                {
                    GenerateIntegerToggle();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private void GenerateIntegerToggle()
        {
            avatar = GetAvatarInfo(descriptor);

            if (avatar.Parameters == null)
            {
                EditorUtility.DisplayDialog("Bwmps", "Missing parameters", "Ok");
                return;
            }

            if (avatar.Controller == null)
            {
                EditorUtility.DisplayDialog("Bwmps", "Missing FX layer", "Ok");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(animationFolder);
            animations.Clear();

            List<GameObject> meshObjectsList = new List<GameObject>();
            foreach (KeyValuePair<string, GameObject> item in meshObjects.list)
            {
                meshObjectsList.Add(item.Value);
            }

            AnimationClip defaultClip = CreateAnimationClip(null, meshObjectsList, "default_" + swapName, folderPath);

            foreach (GameObject obj in meshObjectsList)
            {
                animations.Add(CreateAnimationClip(obj, meshObjectsList, obj.name + "_" + swapName, folderPath));
            }

            avatar.Controller.AddParameter(swapName, UnityEngine.AnimatorControllerParameterType.Int);
            CreateAndFillStateLayer(avatar.Controller, swapName, defaultClip, animations);
            CreateNewParameter(avatar.Parameters, swapName, 0, true, VRCExpressionParameters.ValueType.Int);;

            if (expressionMenu.controls.Count == 7)
            {
                VRCExpressionsMenu newMenu = CreateNewSubMenu(expressionMenu, "Next Page", expressionMenu.name + "_Next", GetAssetFolder(expressionMenu.GetInstanceID()));
                expressionMenu = newMenu;
            }

            AddControlToMenu(expressionMenu, defaultName, swapName, 0f, null, VRCExpressionsMenu.Control.ControlType.Toggle);

            for (int i = 0; i < meshObjects.list.Count; i++)
            {
                KeyValuePair<string, GameObject> item = (KeyValuePair<string, GameObject>)meshObjects.list[i];
                if (expressionMenu.controls.Count == 7)
                {
                    VRCExpressionsMenu newMenu = CreateNewSubMenu(expressionMenu, "Next Page", expressionMenu.name + "_Next", GetAssetFolder(expressionMenu.GetInstanceID()));
                    expressionMenu = newMenu;
                }
                AddControlToMenu(expressionMenu, item.Key, swapName, i + 1, null, VRCExpressionsMenu.Control.ControlType.Toggle);
            }

        }

        private int SelectedInt = 0;
        [MenuItem("GameObject/Bwmp's Tools/Add To Integer Toggle Creator", false, 0)]
        private static void AddSelectedObjectsToMeshObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            IntegerToggleHelper window = (IntegerToggleHelper)GetWindow(typeof(IntegerToggleHelper));
            window.meshObjects.list.Add(new KeyValuePair<string, GameObject>(selectedObjects[window.SelectedInt].name, selectedObjects[window.SelectedInt]));
            window.SelectedInt += 1;
            if (window.SelectedInt >= selectedObjects.Length)
            {
                window.SelectedInt = 0;
            }
        }

        public AnimationClip CreateAnimationClip(GameObject onObject, List<GameObject> Objects, string name, string path)
        {
            AnimationClip clip = new AnimationClip() { legacy = false };

            foreach (GameObject obj in Objects)
            {
                int value = (obj == onObject) ? 1 : 0;

                clip.SetCurve(
                    GetGameObjectsPath(obj.transform),
                    typeof(GameObject),
                    "m_IsActive",
                    new AnimationCurve(new Keyframe(0, value, 0, 0), new Keyframe(0.016666668f, value, 0, 0))
                );
            }

            AssetDatabase.CreateAsset(clip, $"{path}/{name}.anim");

            return clip;
        }
    }
}
