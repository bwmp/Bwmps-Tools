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
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;

namespace BwmpsTools.Tools
{
    public class DissolveToggleHelper : EditorWindow
    {
        private ReorderableList renderers;
        private readonly Dictionary<SkinnedMeshRenderer, List<int>> selectedMaterialIndices = new Dictionary<SkinnedMeshRenderer, List<int>>();
        private readonly Dictionary<Material, DissolveData> materialData = new Dictionary<Material, DissolveData>();
        private readonly Dictionary<SkinnedMeshRenderer, Material> simpleMaterials = new Dictionary<SkinnedMeshRenderer, Material>();
        private VRCAvatar avatar;
        private string swapName = "";
        private float length = 2f;
        private bool startType = true;
        private bool advanced;
        private VRCAvatarDescriptor descriptor = null;
        private DefaultAsset animationFolder = null;
        private VRCExpressionsMenu expressionMenu = null;
        private Vector2 scrollPosition = Vector2.zero;

        private static readonly string Title = "Dissolve Toggle Helper";
        private static readonly string Description = "Dissolve toggles yes";

        private DissolveData copiedDissolveData;

        private class DissolveData
        {
            public Color EdgeColor;
            public Color DissolvedColor;
            public Texture DissolveNoise;
            public Texture DissolveDetailNoise;
            public float DissolveDetailStrength;
        }

        private void OnEnable()
        {
            renderers = new ReorderableList(selectedMaterialIndices.Keys.ToList(), typeof(SkinnedMeshRenderer), false, true, true, true)
            {
                drawElementCallback = DrawRendererElement,
                drawHeaderCallback = DrawRenderersHeader,
                onAddCallback = list => list.list.Add(null),
                onRemoveCallback = OnRemoveRenderer
            };
        }

        private void DrawRendererElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            SkinnedMeshRenderer item = (SkinnedMeshRenderer)renderers.list[index];
            SkinnedMeshRenderer oldObj = item;
            item = (SkinnedMeshRenderer)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item, typeof(SkinnedMeshRenderer), true);
            renderers.list[index] = item;
            if (oldObj != null && oldObj != item)
            {
                selectedMaterialIndices.Remove(oldObj);
            }
            if (item == null) return;
            selectedMaterialIndices[item] = selectedMaterialIndices.ContainsKey(item) ? selectedMaterialIndices[item] : new List<int>();
        }

        private void DrawRenderersHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Dissolve Objects");
        }

        private void OnRemoveRenderer(ReorderableList list)
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

        private void DrawDissolveData(DissolveData dissolveData)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Dissolve Noise");
            dissolveData.DissolveNoise = EditorGUILayout.ObjectField(dissolveData.DissolveNoise, typeof(Texture), true, GUILayout.Height(50), GUILayout.Width(50)) as Texture;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Detail Noise");
            dissolveData.DissolveDetailNoise = EditorGUILayout.ObjectField(dissolveData.DissolveDetailNoise, typeof(Texture), true, GUILayout.Height(50), GUILayout.Width(50)) as Texture;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            dissolveData.EdgeColor = EditorGUILayout.ColorField("Edge Color", dissolveData.EdgeColor);
            dissolveData.DissolvedColor = EditorGUILayout.ColorField("Dissolve Color", dissolveData.DissolvedColor);
            dissolveData.DissolveDetailStrength = EditorGUILayout.FloatField("Detail Strength", dissolveData.DissolveDetailStrength);
            GUILayout.EndVertical();
            if (GUILayout.Button("Copy"))
            {
                copiedDissolveData = new DissolveData()
                {
                    DissolveNoise = dissolveData.DissolveNoise,
                    DissolveDetailNoise = dissolveData.DissolveDetailNoise,
                    EdgeColor = dissolveData.EdgeColor,
                    DissolvedColor = dissolveData.DissolvedColor,
                    DissolveDetailStrength = dissolveData.DissolveDetailStrength
                };
            }
            if (GUILayout.Button("Paste"))
            {
                if (copiedDissolveData != null)
                {
                    dissolveData.DissolveNoise = copiedDissolveData.DissolveNoise;
                    dissolveData.DissolveDetailNoise = copiedDissolveData.DissolveDetailNoise;
                    dissolveData.EdgeColor = copiedDissolveData.EdgeColor;
                    dissolveData.DissolvedColor = copiedDissolveData.DissolvedColor;
                    dissolveData.DissolveDetailStrength = copiedDissolveData.DissolveDetailStrength;
                }
            }
        }


        [MenuItem(itemName: "Bwmp's Tools/Helpers/Dissolve Toggle Helper", false, 0)]
        static void Init()
        {
            DissolveToggleHelper window = (DissolveToggleHelper)GetWindow(typeof(DissolveToggleHelper));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }


        private Vector2 scrollPos;
        private void OnGUI()
        {
            foreach (Material material in simpleMaterials.Values.Where(mat => !materialData.ContainsKey(mat)))
            {
                materialData.Add(material, new DissolveData()
                {
                    DissolvedColor = Color.clear,
                    EdgeColor = Color.white,
                    DissolveDetailStrength = 0.34f
                });
            }

            EditorGUIExtensions.TitleBox(Title, Description);

            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                descriptor = EditorGUILayout.ObjectField("Avatar Descriptor", descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                expressionMenu = EditorGUILayout.ObjectField("Expression Menu", expressionMenu, typeof(VRCExpressionsMenu), true) as VRCExpressionsMenu;
                animationFolder = EditorGUILayout.ObjectField(new GUIContent("Animation Folder", "The folder to search for animations"), animationFolder, typeof(DefaultAsset), true) as DefaultAsset;
                swapName = EditorGUILayout.TextField("Dissolve Name", swapName);
                length = EditorGUILayout.FloatField("Length", length);
                startType = EditorGUILayout.Toggle(new GUIContent("Starting state", "on means the object starts on"), startType);
                advanced = EditorGUILayout.Toggle("Advanced Mode", advanced);
                renderers.DoLayoutList();

                GUILayout.Label("Select Materials", EditorStyles.boldLabel);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                foreach (KeyValuePair<SkinnedMeshRenderer, List<int>> kvp in selectedMaterialIndices)
                {
                    SkinnedMeshRenderer renderer = kvp.Key;
                    List<int> materialIndices = kvp.Value;

                    string cachedRendererName = renderer.name;

                    if (renderer != null)
                    {
                        GUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.Label(renderer.name);
                        if (!advanced)
                        {
                            if (!simpleMaterials.TryGetValue(renderer, out var material))
                            {
                                Shader newshader;
                                if (Shader.Find(".poiyomi/Poiyomi 7.3/• Poiyomi Toon •") != null) newshader = Shader.Find(".poiyomi/Poiyomi 7.3/• Poiyomi Toon •");
                                if (Shader.Find(".poiyomi/• Poiyomi Toon •") != null) newshader = Shader.Find(".poiyomi/• Poiyomi Toon •");
                                else newshader = Shader.Find("Standard");
                                material = new Material(newshader)
                                {
                                    name = renderer.name,
                                };

                                simpleMaterials[renderer] = material;
                            }
                            else if (materialData.TryGetValue(material, out var dissolveData))
                            {
                                DrawDissolveData(dissolveData);
                            }
                        }
                        GUILayout.Label("Selected Materials:");
                        int sharedMaterialsLength = renderer.sharedMaterials.Length;
                        for (int i = 0; i < sharedMaterialsLength; i++)
                        {
                            Material mat = renderer.sharedMaterials[i];
                            string matName = mat.name;
                            bool isSelected = materialIndices.Contains(i);
                            bool shaderLocked = mat.shader.name.StartsWith("Hidden/") && mat.GetFloat("_ShaderOptimizerEnabled") == 1f;
                            bool newSelected = isSelected;
                            GUIStyle style = new GUIStyle(GUI.skin.toggle)
                            {
                                richText = true
                            };
                            GUIContent content = new GUIContent($"<color=green>{matName}</color>");
                            if (shaderLocked)
                            {
                                content.text = $"<color=yellow>{matName} (LOCKED)</color>";
                                content.tooltip = "Settings will not be applied to this material but it might still work";
                                if (GetSettings(mat, out List<string> falseParams) == false)
                                {
                                    style.fontStyle = FontStyle.Bold;
                                    content.text = $"<color=red>{matName} (LOCKED AND WILL BREAK)</color>";
                                    content.tooltip = $"You must unlock the shader or fix these issues {string.Join(", ", falseParams)}";
                                }
                            }
                            newSelected = GUILayout.Toggle(newSelected, content, style);
                            if (isSelected && !newSelected)
                            {
                                materialIndices.Remove(i);
                            }
                            else if (!isSelected && newSelected)
                            {
                                materialIndices.Add(i);
                            }
                            if (!isSelected) continue;
                            DissolveData dissolveData = GetDissolveData(mat);
                            if (!advanced) continue;
                            if (!shaderLocked) DrawDissolveData(dissolveData);
                            materialData[mat] = dissolveData;
                        }
                        GUILayout.EndVertical();
                    }
                }
                GUILayout.EndScrollView();
                EditorGUI.BeginDisabledGroup(descriptor == null || renderers.list.Count == 0 || !selectedMaterialIndices.Any());
                if (GUILayout.Button("Create Dissolve Toggle"))
                {
                    avatar = GetAvatarInfo(descriptor);
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
                    string folderPath = AssetDatabase.GetAssetPath(animationFolder);
                    if (!advanced)
                    {
                        foreach (SkinnedMeshRenderer renderer in renderers.list)
                        {
                            if (!simpleMaterials.TryGetValue(renderer, out Material mat)) continue;
                            if (!materialData.TryGetValue(mat, out DissolveData dissolveData)) continue;
                            foreach (Material material in renderer.sharedMaterials)
                            {
                                if (material.shader.name.StartsWith("Hidden/") && material.GetFloat("_ShaderOptimizerEnabled") == 1f) continue;
                                SetupSettings(material);
                                ApplyDissolveDataToMaterial(material, dissolveData);
                            }
                        }
                    }
                    else
                    {
                        foreach (var matData in materialData)
                        {
                            Material mat = matData.Key;
                            if (mat.shader.name.StartsWith("Hidden/") && mat.GetFloat("_ShaderOptimizerEnabled") == 1f) continue;
                            SetupSettings(mat);
                            DissolveData dissolveData = matData.Value;
                            if (dissolveData == null) continue;
                            ApplyDissolveDataToMaterial(mat, dissolveData);
                        }
                    }
                    float onState = startType ? 1f : 0f;
                    float offState = startType ? 0f : 1f;
                    AnimationClip offClip = CreateClip(renderers.list as List<SkinnedMeshRenderer>, swapName + "_Off", onState, length, folderPath);
                    AnimationClip onClip = CreateClip(renderers.list as List<SkinnedMeshRenderer>, swapName + "_On", offState, length, folderPath);
                    string param = swapName + "_Dissolve";
                    avatar.Controller.AddParameter(param, UnityEngine.AnimatorControllerParameterType.Bool);
                    CreateNewParameter(avatar.Parameters, param, 0, true, VRCExpressionParameters.ValueType.Bool);
                    CreateAndFillStateLayer(avatar.Controller, param, onClip, offClip);
                    if (expressionMenu.controls.Count == 7)
                    {
                        VRCExpressionsMenu newMenu = CreateNewSubMenu(expressionMenu, "Next Page", expressionMenu.name + "_Next", GetAssetFolder(expressionMenu.GetInstanceID()));
                        expressionMenu = newMenu;
                    }
                    AddControlToMenu(expressionMenu, swapName, param, null, null, ControlType.Toggle);
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private DissolveData GetDissolveData(Material material)
        {
            if (materialData.TryGetValue(material, out DissolveData dissolveData))
            {
                return dissolveData;
            }
            else
            {
                dissolveData = new DissolveData()
                {
                    EdgeColor = material.GetColor("_DissolveEdgeColor"),
                    DissolvedColor = material.GetColor("_DissolveTextureColor"),
                    DissolveNoise = material.GetTexture("_DissolveNoiseTexture"),
                    DissolveDetailNoise = material.GetTexture("_DissolveDetailNoise"),
                    DissolveDetailStrength = material.GetFloat("_DissolveDetailStrength")
                };
                materialData.Add(material, dissolveData);
                return dissolveData;
            }
        }

        private int SelectedInt = 0;
        [MenuItem("GameObject/Bwmp's Tools/Add To Dissolve Toggle", false, 0)]
        private static void AddSelectedSkinnedMesshRenderers()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            DissolveToggleHelper window = (DissolveToggleHelper)GetWindow(typeof(DissolveToggleHelper));
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


        public AnimationClip CreateClip(List<SkinnedMeshRenderer> renderers, string name, float value, float length, string path)
        {
            AnimationClip clip = new AnimationClip() { legacy = false };
            foreach (var renderer in renderers)
            {
                selectedMaterialIndices.TryGetValue(renderer, out List<int> indices);
                string rendererName = renderer.name;
                foreach (int item in indices)
                {
                    float endValue = (value == 1f ? 0f : 1f);
                    clip.SetCurve(
                        rendererName,
                        typeof(SkinnedMeshRenderer),
                        $"material._DissolveAlpha_{renderer.sharedMaterials[item].name}",
                        new AnimationCurve(new Keyframe(0, value, 0, 0), new Keyframe(length, endValue, 0, 0))
                    );
                }
                if (renderer.sharedMaterials.Length == indices.Count)
                {
                    clip.SetCurve(
                        rendererName,
                        typeof(GameObject),
                        "m_IsActive",
                        new AnimationCurve(new Keyframe(0, 1f, 0, 0), new Keyframe(length, value, 0, 0))
                    );
                }
            }
            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{path.TrimEnd('/')}/{name}.anim");
            AssetDatabase.CreateAsset(clip, uniqueAssetPath);

            return clip;
        }

        private void ApplyDissolveDataToMaterial(Material material, DissolveData dissolveData)
        {
            if (material.shader.name.StartsWith("Hidden/") && material.GetFloat("_ShaderOptimizerEnabled") == 1f) return;

            SetupSettings(material);

            material.SetColor("_DissolveEdgeColor", dissolveData.EdgeColor);
            material.SetColor("_DissolveTextureColor", dissolveData.DissolvedColor);
            material.SetTexture("_DissolveNoiseTexture", dissolveData.DissolveNoise);
            material.SetTexture("_DissolveDetailNoise", dissolveData.DissolveDetailNoise);
            material.SetFloat("_DissolveDetailStrength", dissolveData.DissolveDetailStrength);
        }


        public void SetupSettings(Material material)
        {
            material.SetFloat("_Mode", 9);
            material.SetFloat("_EnableDissolve", 1);
            material.SetFloat("_DissolveType", 2);
            material.SetOverrideTag("_DissolveAlphaAnimated", "2");
            material.SetColor("_DissolveTextureColor", Color.clear);
        }

        public bool GetSettings(Material material, out List<string> falseParams)
        {
            falseParams = new List<string>();

            Dictionary<string, float> expectedValues = new Dictionary<string, float>
            {
                { "_Mode", 3f },
                { "_EnableDissolve", 1f },
                { "_DissolveType", 2f }
            };
            Dictionary<string, string> tagValues = new Dictionary<string, string>
            {
                { "_DissolveAlphaAnimated", "1" }
            };

            bool isValid = true;

            foreach (KeyValuePair<string, float> expectedValue in expectedValues)
            {
                if (material.GetFloat(expectedValue.Key) != expectedValue.Value)
                {
                    falseParams.Add(expectedValue.Key);
                    isValid = false;
                }
            }

            foreach (KeyValuePair<string, string> tagValue in tagValues)
            {
                if (material.GetTag(tagValue.Key, true) != tagValue.Value)
                {
                    falseParams.Add(tagValue.Key);
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}