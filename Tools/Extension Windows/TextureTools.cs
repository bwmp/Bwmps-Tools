using BwmpsTools.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static BwmpsTools.Utils.Structs;
using static BwmpsTools.Utils.Utilities;

namespace BwmpsTools.Tools
{
    public class TextureTools : EditorWindow
    {
        private readonly string[] options = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
        private int selectedOption = 5;

        private VRCAvatar avatar;

        private static readonly string Title = "Texture Tools";
        private static readonly string Description = "Easily resize multiple textures at once";

        private readonly List<Texture> textures = new List<Texture>();
        private readonly Dictionary<Texture, bool> textureToggles = new Dictionary<Texture, bool>();

        public void Init()
        {
            TextureTools window = (TextureTools)GetWindow(typeof(TextureTools), false, Title, true);
            window.titleContent = new GUIContent(Title);
            window.Show();
            GetTextureList(BasicUtils.TargetModel);
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {

            GUILayout.Space(10);

            EditorGUIExtensions.TitleBox(Title, Description);

            selectedOption = EditorGUILayout.Popup("Select an option:", selectedOption, options);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                foreach (Texture texture in textures)
                {
                    textureToggles[texture] = true;
                }
            }
            if (GUILayout.Button("Deselect All"))
            {
                foreach (Texture texture in textures)
                {
                    textureToggles[texture] = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Textures:");
            using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) }))
            {
                scrollPos = scroll.scrollPosition;
                foreach (Texture texture in textures)
                {
                    if (!textureToggles.ContainsKey(texture))
                    {
                        textureToggles[texture] = false;
                    }
                    textureToggles[texture] = GUILayout.Toggle(textureToggles[texture], texture.name + "(" + texture.width + ")");
                }
            }
            if (GUILayout.Button("Resize Textures"))
            {
                foreach (Texture texture in textures)
                {
                    if (textureToggles[texture])
                    {
                        ResizeTexture(texture, int.Parse(options[selectedOption]));
                    }
                }
            }
        }

        private void ResizeTexture(Texture texture, int size)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.maxTextureSize = size;
            AssetDatabase.ImportAsset(path);
        }

        private void GetTextureList(GameObject target)
        {
            textures.Clear();
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            avatar = GetAvatarInfo(target.GetComponent<VRCAvatarDescriptor>());

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        foreach (string propertyName in material.GetTexturePropertyNames())
                        {
                            Texture texture = material.GetTexture(propertyName);
                            if (texture != null && !textures.Contains(texture))
                            {
                                textures.Add(texture);
                            }
                        }
                    }
                }
            }

            if (avatar.Controller != null)
            {
                List<Material> mats = CheckLayers(avatar.Controller);
                foreach (Material material in mats)
                {
                    if (material != null)
                    {
                        foreach (string propertyName in material.GetTexturePropertyNames())
                        {
                            Texture texture = material.GetTexture(propertyName);
                            if (texture != null && !textures.Contains(texture))
                            {
                                textures.Add(texture);
                            }
                        }
                    }
                }
            }

            Debug.Log("Found " + textures.Count + " textures");
        }

        private List<Material> CheckLayers(AnimatorController controller)
        {
            List<Material> mats = new List<Material>();
            if (controller != null)
            {
                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    if (layer == null) continue;
                    List<Material> layerMats = ProcessLayer(layer);
                    if (layerMats == null || layerMats.Count == 0) continue;
                    mats.AddRange(layerMats);
                }
            }
            return mats;
        }

        private List<Material> ProcessLayer(AnimatorControllerLayer layer)
        {
            List<Material> layerMats = new List<Material>();
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                if (state.state.motion is BlendTree blendTree)
                {
                    layerMats.AddRange(ProcessBlendTree(blendTree));
                }
                else if (state.state.motion is AnimationClip clip)
                {
                    layerMats.AddRange(CheckMaterialChangesInClip(clip));
                }
            }
            return layerMats;
        }

        private List<Material> ProcessBlendTree(BlendTree blendTree)
        {
            List<Material> blendTreeMats = new List<Material>();
            foreach (ChildMotion childMotion in blendTree.children)
            {
                if (childMotion.motion is BlendTree childBlendTree)
                {
                    blendTreeMats.AddRange(ProcessBlendTree(childBlendTree));
                }
                else if (childMotion.motion is AnimationClip clip)
                {
                    blendTreeMats.AddRange(CheckMaterialChangesInClip(clip));
                }
            }
            return blendTreeMats;
        }

        private List<Material> CheckMaterialChangesInClip(AnimationClip clip)
        {
            List<Material> mats = new List<Material>();
            EditorCurveBinding[] curveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            foreach (EditorCurveBinding curveBinding in curveBindings)
            {
                if (curveBinding.propertyName.Contains("m_Materials.Array.data") && curveBinding.path != "")
                {
                    ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, curveBinding);

                    foreach (ObjectReferenceKeyframe keyframe in keyframes)
                    {
                        Material material = keyframe.value as Material;

                        if (material != null)
                        {
                            mats.Add(material);
                        }
                    }
                }
            }

            return mats;
        }

        private static TextureTools instance;

        public static TextureTools Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<TextureTools>();
                }
                return instance;
            }
        }
    }
}
