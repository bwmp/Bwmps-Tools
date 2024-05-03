using BwmpsTools.Utils;
using System.Collections.Generic;
using System.Linq;
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
    public class HueShiftHelper : EditorWindow
    {
        private ReorderableList renderers;
        private readonly Dictionary<SkinnedMeshRenderer, List<int>> selectedMaterialIndices = new Dictionary<SkinnedMeshRenderer, List<int>>();
        private VRCAvatar avatar;
        private string swapName = "";
        private VRCAvatarDescriptor descriptor = null;
        private DefaultAsset animationFolder = null;
        private VRCExpressionsMenu expressionMenu = null;
        private AnimationClip onClip;
        private AnimationClip offClip;
        private Vector2 scrollPosition = Vector2.zero;

        private static readonly string Title = "Hue Shift Helper";
        private static readonly string Description = "Easily create hue shifts fast";

        [MenuItem("Bwmp's Tools/Helpers/Hue Shift Helper", false, 0)]
        static void Init()
        {
            HueShiftHelper window = (HueShiftHelper)GetWindow(typeof(HueShiftHelper));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }


        private Vector2 scrollPos;

        private void OnGUI()
        {

            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;

            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                EditorGUIExtensions.TitleBox(Title, Description);
                DrawInputFields();

                GUILayout.Label("Select Materials", EditorStyles.boldLabel);
                DrawMaterialSelection();

                HandleButtonAndActions();
            }
        }

        private void DrawInputFields()
        {
            descriptor = EditorGUILayout.ObjectField("Avatar Descriptor", descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            expressionMenu = EditorGUILayout.ObjectField("Expression Menu", expressionMenu, typeof(VRCExpressionsMenu), true) as VRCExpressionsMenu;
            animationFolder = EditorGUILayout.ObjectField(new GUIContent("Animation Folder", "The folder to search for animations"), animationFolder, typeof(DefaultAsset), true) as DefaultAsset;
            swapName = EditorGUILayout.TextField("Shift Name", swapName);
            renderers.DoLayoutList();
        }

        private void DrawMaterialSelection()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var kvp in selectedMaterialIndices)
            {
                var renderer = kvp.Key;
                var materialIndices = kvp.Value;

                if (renderer == null) continue;

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(renderer.name);
                GUILayout.Label("Selected Materials:");

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    bool isSelected = materialIndices.Contains(i);
                    Material mat = renderer.sharedMaterials[i];
                    bool shaderLocked = mat.shader.name.StartsWith("Hidden/") && mat.GetFloat("_ShaderOptimizerEnabled") == 1f;

                    GUIStyle style = new GUIStyle(GUI.skin.toggle);
                    GUIContent content = new GUIContent(shaderLocked ? $"{mat.name} (MATERIAL LOCKED)" : mat.name);

                    if (shaderLocked)
                    {
                        style.normal.textColor = Color.red;
                        style.hover.textColor = Color.red;
                        style.active.textColor = Color.red;
                        style.fontStyle = FontStyle.Bold;
                        content.tooltip = "Settings will not be applied to this material and it may break, Please unlock it";
                    }

                    bool newSelected = GUILayout.Toggle(isSelected, content, style);

                    if (isSelected && !newSelected)
                        materialIndices.Remove(i);
                    else if (!isSelected && newSelected)
                        materialIndices.Add(i);
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }

        private void HandleButtonAndActions()
        {
            bool canCreateHueShift = descriptor != null && selectedMaterialIndices.Count > 0;
            GUI.enabled = canCreateHueShift;

            if (GUILayout.Button("Create Hue Shift"))
            {
                CreateHueShift();
            }

            GUI.enabled = true;
        }

        private void CreateHueShift()
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

            onClip = CreateHueShiftClip(renderers.list as List<SkinnedMeshRenderer>, swapName + "_Default", 0f, folderPath);
            offClip = CreateHueShiftClip(renderers.list as List<SkinnedMeshRenderer>, swapName + "_Full", 1f, folderPath);

            string param = swapName + "_Shift";
            CreateAndFillStateLayerBlendtree(avatar.Controller, param, onClip, offClip);
            CreateNewParameter(avatar.Parameters, param, 0f, true, VRCExpressionParameters.ValueType.Float);
            avatar.Controller.AddParameter(param, UnityEngine.AnimatorControllerParameterType.Float);

            if (expressionMenu.controls.Count == 7)
            {
                VRCExpressionsMenu newMenu = CreateNewSubMenu(expressionMenu, "Next Page", expressionMenu.name + "_Next", GetAssetFolder(expressionMenu.GetInstanceID()));
                expressionMenu = newMenu;
            }

            AddRadialControlToMenu(expressionMenu, swapName, param, null);
        }


        private void OnEnable()
        {
            renderers = new ReorderableList(selectedMaterialIndices.Keys.ToList(), typeof(SkinnedMeshRenderer), false, true, true, true)
            {
                drawElementCallback = (Rect rect, int i, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    SkinnedMeshRenderer item = (SkinnedMeshRenderer)renderers.list[i];
                    SkinnedMeshRenderer oldObj = item;
                    item = (SkinnedMeshRenderer)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item, typeof(SkinnedMeshRenderer), true);
                    renderers.list[i] = item;
                    if (oldObj != null && oldObj != item)
                    {
                        selectedMaterialIndices.Remove(oldObj);
                    }
                    if (item == null) return;
                    selectedMaterialIndices[item] = selectedMaterialIndices.ContainsKey(item) ? selectedMaterialIndices[item] : new List<int>();
                },
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Hue Shift Objects");
                },
                onAddCallback = (ReorderableList list) =>
                {
                    list.list.Add(null);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    if (list.index < 0) return;
                    SkinnedMeshRenderer renderer = (SkinnedMeshRenderer)list.list[list.index];
                    if (renderer == null)
                    {
                        list.list.RemoveAt(list.index);
                        return;
                    }
                    if (selectedMaterialIndices.ContainsKey(renderer)) selectedMaterialIndices.Remove(renderer);
                    list.list.RemoveAt(list.index);
                }
            };
        }

        private int SelectedInt = 0;
        [MenuItem("GameObject/Bwmp's Tools/Add To Hue Shift", false, 0)]
        private static void AddSelectedSkinnedMesshRenderers()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            HueShiftHelper window = (HueShiftHelper)GetWindow(typeof(HueShiftHelper));
            if (selectedObjects[window.SelectedInt].GetComponent<SkinnedMeshRenderer>() == null)
            {
                Debug.LogError("Selected object does not have a SkinnedMeshRenderer");
                return;
            }
            window.renderers.list.Add(selectedObjects[window.SelectedInt].GetComponent<SkinnedMeshRenderer>());
            window.SelectedInt += 1;
            if (window.SelectedInt >= selectedObjects.Length)
            {
                window.SelectedInt = 0;
            }
        }


        public AnimationClip CreateHueShiftClip(List<SkinnedMeshRenderer> renderers, string name, float value, string path)
        {
            AnimationClip clip = new AnimationClip() { legacy = false };
            foreach (var renderer in renderers)
            {
                selectedMaterialIndices.TryGetValue(renderer, out List<int> indices);
                foreach (int item in indices)
                {
                    Material mat = renderer.sharedMaterials[item];
                    if (!mat.shader.name.StartsWith("Hidden/") && mat.GetFloat("_ShaderOptimizerEnabled") != 1f)
                    {
                        mat.SetFloat("_MainHueShiftToggle", 1f);
                        mat.SetFloat("_MainColorAdjustToggle", 1f);
                        mat.SetOverrideTag(mat.name + "Animated", "2");
                    }

                    clip.SetCurve(
                        renderer.name,
                        typeof(SkinnedMeshRenderer),
                        $"material._MainHueShift_{mat.name}",
                        new AnimationCurve(new Keyframe(0, value, 0, 0), new Keyframe(0.083333336f, value, 0, 0))
                    );
                }
            }

            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{path.TrimEnd('/')}/{name}.anim");
            AssetDatabase.CreateAsset(clip, uniqueAssetPath);

            return clip;
        }
    }
}
