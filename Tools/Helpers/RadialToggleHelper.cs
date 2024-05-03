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

namespace BwmpsTools.Tools
{
    public class RadialToggleHelper : EditorWindow
    {
        private VRCAvatarDescriptor descriptor;
        private VRCExpressionsMenu expressionMenu;
        private VRCAvatar avatar;
        private DefaultAsset animationFolder;
        private string swapName;
        private readonly List<AnimationClip> animations = new List<AnimationClip>();
        private ReorderableList meshObjects;

        private const string Title = "Radial Toggle Helper";
        private const string Description = "Easily create radial wheel toggles";

        [MenuItem("Bwmp's Tools/Helpers/Radial Toggle Helper", false, 0)]
        static void Init()
        {
            RadialToggleHelper window = (RadialToggleHelper)GetWindow(typeof(RadialToggleHelper));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }


        private void OnEnable()
        {
            meshObjects = new ReorderableList(new List<GameObject>(), typeof(GameObject), true, true, true, true)
            {
                drawElementCallback = DrawMeshObjectElement,
                drawHeaderCallback = DrawMeshObjectHeader,
                onAddCallback = AddMeshObject,
            };
        }

        private void DrawMeshObjectElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            GameObject item = (GameObject)meshObjects.list[index];
            meshObjects.list[index] = (GameObject)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item, typeof(GameObject), true);
        }

        private void DrawMeshObjectHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Swap Objects");
        }

        private void AddMeshObject(ReorderableList list)
        {
            list.list.Add(null);
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
                expressionMenu = EditorGUILayout.ObjectField(new GUIContent("Expression Menu", "The expressions menu to add toggle to"), expressionMenu, typeof(VRCExpressionsMenu), true) as VRCExpressionsMenu;
                swapName = EditorGUILayout.TextField("Radial Name", swapName);

                GUILayout.Label("Add meshes here:");
                meshObjects.DoLayoutList();

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(descriptor == null);
                if (GUILayout.Button("Generate"))
                {
                    GenerateRadialToggle();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private void GenerateRadialToggle()
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

            for (int i = 0; i < meshObjects.list.Count; i++)
            {
                animations.Add(CreateAnimationClip(meshObjects.list[i] as GameObject, meshObjects.list as List<GameObject>, swapName + i, folderPath));
            }

            string param = swapName + "_Swap";
            avatar.Controller.AddParameter(param, UnityEngine.AnimatorControllerParameterType.Float);
            CreateAndFillStateLayerBlendtree(avatar.Controller, param, animations, true);
            CreateNewParameter(avatar.Parameters, param, 0f, true, VRCExpressionParameters.ValueType.Float);

            if (expressionMenu.controls.Count == 7)
            {
                VRCExpressionsMenu newMenu = CreateNewSubMenu(expressionMenu, "Next Page", expressionMenu.name + "_Next", GetAssetFolder(expressionMenu.GetInstanceID()));
                expressionMenu = newMenu;
            }

            AddRadialControlToMenu(expressionMenu, swapName, param, null);
        }

        private int SelectedInt = 0;
        [MenuItem("GameObject/Bwmp's Tools/Add To Radial Toggle Helper", false, 0)]
        private static void AddSelectedObjectsToMeshObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            RadialToggleHelper window = (RadialToggleHelper)GetWindow(typeof(RadialToggleHelper));
            window.meshObjects.list.Add(selectedObjects[window.SelectedInt]);
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
