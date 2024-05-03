using BwmpsTools.Tools.Modules;
using BwmpsTools.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static BwmpsTools.Utils.Utilities;

namespace BwmpsTools.Tools
{
    public class BasicUtils : EditorWindow
    {

        private enum Tab
        {
            Main,
            MaterialTexture,
            Bone,
            Eye,
            Misc
        }
        private Tab currentTab = Tab.Main;

        private static GameObject targetModel;
        private VRCAvatarDescriptor Descriptor;
        private GameObject OtherModel;

        private MaterialTextureOptions MaterialTextureOptions;
        private BoneOptions BoneOptions;
        private EyeOptions EyeOptions;
        private MiscOptions MiscOptions;

        private static readonly string Title = "Basic Utils";
        private static readonly string Description = "Basic Utilities to aid in the creation avatars";

        [MenuItem("Bwmp's Tools/Basic Utils", false, 0)]
        static void Init()
        {
            BasicUtils window = (BasicUtils)GetWindow(typeof(BasicUtils));
            window.titleContent = new GUIContent(Title1);
            window.Show();
        }

        private Vector2 scrollPos;

        public static GameObject TargetModel { get => targetModel; set => targetModel = value; }

        public static string Title1 => Title;

        public static string Description1 => Description;

        public Vector2 ScrollPos { get => scrollPos; set => scrollPos = value; }

        void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(ScrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) }))
            {
                ScrollPos = scroll.scrollPosition;
                using (new GUILayout.VerticalScope(CustomGUIStyles.group))
                {
                    using (new EditorGUILayout.VerticalScope(new GUIStyle("box") { stretchWidth = true }))
                    {
                        EditorGUILayout.LabelField($"<b><size=15>{Title1}</size></b>", CustomGUIStyles.centeredTitle);
                        EditorGUILayout.LabelField($"<b><size=12.5>{Description1}</size></b>", CustomGUIStyles.centeredDescription);
                    }
                    if (TargetModel == null)
                    {
                        EditorGUILayout.HelpBox("Select a model to begin.", MessageType.Warning);
                    }
                    Descriptor = EditorGUILayout.ObjectField("Target Model", Descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                    if (Descriptor != null)
                    {
                        TargetModel = Descriptor.gameObject;
                    }
                    else
                    {
                        TargetModel = null;
                    }

                    GUILayout.BeginHorizontal();
                    DrawTabButton("Main", Tab.Main);
                    DrawTabButton("Material Texture", Tab.MaterialTexture);
                    DrawTabButton("Bone", Tab.Bone);
                    DrawTabButton("Eye", Tab.Eye);
                    DrawTabButton("Misc", Tab.Misc);
                    GUILayout.EndHorizontal();

                    switch (currentTab)
                    {
                        case Tab.Main:
                            Render();
                            break;
                        case Tab.MaterialTexture:
                            MaterialTextureOptions.Render();
                            break;
                        case Tab.Bone:
                            BoneOptions.Render();
                            break;
                        case Tab.Eye:
                            EyeOptions.Render();
                            break;
                        case Tab.Misc:
                            MiscOptions.Render();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void DrawTabButton(string tabName, Tab tab)
        {
            bool isActive = currentTab == tab;
            if (GUILayout.Toggle(isActive, tabName, "Button"))
            {
                currentTab = tab;
            }
        }

        void Render()
        {
            OtherModel = EditorGUILayout.ObjectField("Other Model", OtherModel, typeof(GameObject), true) as GameObject;
            EditorGUI.BeginDisabledGroup(TargetModel == null);
            if (GUILayout.Button("Duplicate Main Model"))
            {
                GameObject copyModel = Instantiate(TargetModel);
                copyModel.name = TargetModel.name + " (Copy)";

                Undo.IncrementCurrentGroup();

                Undo.RegisterCreatedObjectUndo(copyModel, "Duplicate Main Model");

                Undo.RecordObject(TargetModel, "Modify Target Model");
                Undo.RecordObject(Descriptor, "Modify Descriptor");

                Undo.RegisterCompleteObjectUndo(TargetModel, "Modify Target Model Active State");
                TargetModel.SetActive(false);

                TargetModel = copyModel;
                Descriptor = copyModel.GetComponent<VRCAvatarDescriptor>();

                Undo.RegisterCompleteObjectUndo(TargetModel, "Restore Original Target Model Active State");
                TargetModel.SetActive(true);
            }


            EditorGUI.BeginDisabledGroup(OtherModel == null);
            if (GUILayout.Button(new GUIContent("Copy Materials From Other Model", "Requires other model specified")))
            {
                CopyMaterialsFromOther(TargetModel, OtherModel);
            }
            //if (GUILayout.Button(new GUIContent("Copy Components From Other Model", "Requires other model specified")))
            //{
            //    CopyComponentsByName(TargetModel.transform.FindChildrenByPartialNames(new List<string>() { "armature" })[0], OtherModel.transform.FindChildrenByPartialNames(new List<string>() { "armature" })[0]);
            //}
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(TargetModel == null);
            if (GUILayout.Button("Render Setup"))
            {
                AutoRenderSetup autoRenderSetup = CreateInstance<AutoRenderSetup>();
                autoRenderSetup.Init();
            }
            EditorGUI.EndDisabledGroup();
        }

        void OnEnable()
        {
            MaterialTextureOptions = new MaterialTextureOptions();
            BoneOptions = new BoneOptions();
            EyeOptions = new EyeOptions();
            MiscOptions = new MiscOptions();
        }

        public static void CopyMaterialsFromOther(GameObject parent, GameObject other)
        {

            Undo.IncrementCurrentGroup();

            SkinnedMeshRenderer[] parentRenderers = parent.GetComponentsInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer[] otherRenderers = other.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer parentRenderer in parentRenderers)
            {
                Undo.RecordObject(parentRenderer, "Modify Parent Renderer Materials");
                foreach (SkinnedMeshRenderer otherRenderer in otherRenderers)
                {
                    if (parentRenderer.name == otherRenderer.name)
                    {
                        Material[] parentMaterials = parentRenderer.sharedMaterials;
                        Material[] otherMaterials = otherRenderer.sharedMaterials;
                        if (parentMaterials.Length == otherMaterials.Length)
                        {
                            for (int i = 0; i < parentMaterials.Length; i++)
                            {
                                parentMaterials[i] = otherMaterials[i];
                            }
                            parentRenderer.sharedMaterials = parentMaterials;
                        }
                        else
                        {
                            Debug.LogError("Number of materials on parent and other SkinnedMeshRenderer with the same name do not match!");
                        }
                    }
                }
            }
        }
    }
}
