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
    public class MaterialSwapHelper : EditorWindow
    {
        private ReorderableList renderers;
        private ReorderableList materials;
        private readonly Dictionary<SkinnedMeshRenderer, List<int>> selectedMaterialIndices = new Dictionary<SkinnedMeshRenderer, List<int>>();
        private VRCAvatar avatar;
        private string swapName = "";
        private VRCAvatarDescriptor descriptor = null;
        private DefaultAsset animationFolder = null;
        private VRCExpressionsMenu expressionMenu = null;
        private readonly List<AnimationClip> animations = new List<AnimationClip>();
        private Vector2 scrollPosition = Vector2.zero;

        private const string Title = "Material Swap Helper";
        private const string Description = "Easily create material swaps";


        [MenuItem("Bwmp's Tools/Helpers/Material Swap Helper", false, 0)]
        static void Init()
        {
            MaterialSwapHelper window = (MaterialSwapHelper)GetWindow(typeof(MaterialSwapHelper));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {

            EditorGUIExtensions.TitleBox(Title, Description);

            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, GUIStyle.none);
            scrollPos = scroll.scrollPosition;

            GUILayout.BeginVertical(CustomGUIStyles.group);

            descriptor = EditorGUILayout.ObjectField("Avatar Descriptor", descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            expressionMenu = EditorGUILayout.ObjectField("Expression Menu", expressionMenu, typeof(VRCExpressionsMenu), true) as VRCExpressionsMenu;
            animationFolder = EditorGUILayout.ObjectField(new GUIContent("Animation Folder", "The folder to search for animations"), animationFolder, typeof(DefaultAsset), true) as DefaultAsset;
            swapName = EditorGUILayout.TextField("Swap Name", swapName);
            renderers.DoLayoutList();
            materials.DoLayoutList();
            GUILayout.Space(10);

            DrawSelectedMaterials();
            DrawCreateMaterialSwapButton();

            GUILayout.EndVertical();
        }

        private void DrawSelectedMaterials()
        {
            GUILayout.Label("Select Materials", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (KeyValuePair<SkinnedMeshRenderer, List<int>> kvp in selectedMaterialIndices)
            {
                SkinnedMeshRenderer renderer = kvp.Key;
                List<int> materialIndices = kvp.Value;

                if (renderer == null)
                    continue;

                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(renderer.name);
                GUILayout.Label("Selected Materials:");

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    bool isSelected = materialIndices.Contains(i);
                    bool newSelected = GUILayout.Toggle(isSelected, renderer.sharedMaterials[i].name);

                    if (isSelected && !newSelected)
                    {
                        materialIndices.Remove(i);
                    }
                    else if (!isSelected && newSelected)
                    {
                        materialIndices.Add(i);
                    }
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }

        private void DrawCreateMaterialSwapButton()
        {
            EditorGUI.BeginDisabledGroup(descriptor == null);

            if (GUILayout.Button("Create Material Swap"))
            {
                CreateMaterialSwaps();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void OnEnable()
        {
            SetupRenderersList();
            SetupMaterialsList();
        }

        private void SetupRenderersList()
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
                    EditorGUI.LabelField(rect, "Swap Objects");
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

        private void SetupMaterialsList()
        {
            materials = new ReorderableList(new List<Material>(), typeof(Material), true, true, true, true)
            {
                drawElementCallback = (Rect rect, int i, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    Material item = (Material)materials.list[i];
                    materials.list[i] = (Material)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item, typeof(Material), true);
                },
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Materials");
                },
                onAddCallback = (ReorderableList list) =>
                {
                    list.list.Add(null);
                }
            };
        }

        [MenuItem("Assets/Bwmp's Tools/Add To Material Swap", false, 0)]
        private static void AddSelectedMaterials()
        {
            MaterialSwapHelper window = (MaterialSwapHelper)GetWindow(typeof(MaterialSwapHelper));
            foreach (var obj in Selection.objects)
            {
                if (obj.GetType() != typeof(Material))
                    continue;
                Material selectedMaterial = obj as Material;
                if (selectedMaterial != null)
                {
                    window.materials.list.Add(selectedMaterial);
                }
            }
        }

        private int SelectedInt = 0;
        [MenuItem("GameObject/Bwmp's Tools/Add To Material Swap", false, 0)]
        private static void AddSelectedSkinnedMesshRenderers()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            MaterialSwapHelper window = (MaterialSwapHelper)GetWindow(typeof(MaterialSwapHelper));
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

        private void CreateMaterialSwaps()
        {
            animations.Clear();
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

            for (int i = 0; i < materials.list.Count; i++)
            {
                animations.Add(CreateMatSwapClip(renderers.list as List<SkinnedMeshRenderer>, materials.list[i] as Material, swapName + i, folderPath));
            }

            string param = swapName + "_Swap";
            CreateAndFillStateLayerBlendtree(avatar.Controller, param, animations);
            CreateNewParameter(avatar.Parameters, param, 0f, true, VRCExpressionParameters.ValueType.Float);
            avatar.Controller.AddParameter(param, UnityEngine.AnimatorControllerParameterType.Float);

            if (expressionMenu.controls.Count == 7)
            {
                VRCExpressionsMenu newMenu = CreateNewSubMenu(expressionMenu, "Next Page", expressionMenu.name + "_Next", GetAssetFolder(expressionMenu.GetInstanceID()));
                expressionMenu = newMenu;
            }

            AddRadialControlToMenu(expressionMenu, swapName, param, null);
        }

        private AnimationClip CreateMatSwapClip(List<SkinnedMeshRenderer> renderers, Material material, string name, string path)
        {
            AnimationClip clip = new AnimationClip() { legacy = false };

            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                selectedMaterialIndices.TryGetValue(renderer, out List<int> indices);

                foreach (int item in indices)
                {
                    EditorCurveBinding binding = new EditorCurveBinding
                    {
                        type = typeof(SkinnedMeshRenderer),
                        path = renderer.name,
                        propertyName = "m_Materials.Array.data[" + item + "]"
                    };

                    ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[2];
                    keyframes[0] = new ObjectReferenceKeyframe
                    {
                        time = 0f,
                        value = material
                    };
                    keyframes[1] = new ObjectReferenceKeyframe
                    {
                        time = 0.083333336f,
                        value = material
                    };

                    AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
                }
            }

            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{path.TrimEnd('/')}/{name}.anim");
            AssetDatabase.CreateAsset(clip, uniqueAssetPath);

            return clip;
        }

    }
}
