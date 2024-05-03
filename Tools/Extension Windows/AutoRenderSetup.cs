using BwmpsTools.OtherScripts;
using BwmpsTools.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static BwmpsTools.Utils.Structs;
using static BwmpsTools.Utils.Utilities;
namespace BwmpsTools.Tools
{
    internal class AutoRenderSetup : EditorWindow
    {
        [SerializeField]
        private VRCAvatarDescriptor descriptor;
        [SerializeField]
        private bool multipleModels = true;
        [SerializeField]
        private bool syncToggles = true;
        [SerializeField]
        private bool rotateCamera = false;
        [SerializeField]
        private bool walkAnimation = true;
        [SerializeField]
        private float distance = 1.5f;
        [SerializeField]
        private float speed = 0.5f;

        private static readonly string Title = "Render Setup";

        public void Init()
        {
            AutoRenderSetup window = (AutoRenderSetup)GetWindow(typeof(AutoRenderSetup));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUIExtensions.TitleBox(Title, null);
            using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(Vector2.zero, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) }))
            {
                descriptor = EditorGUILayout.ObjectField("Target Model", descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
                multipleModels = GUILayout.Toggle(multipleModels, new GUIContent("2 Models"));
                syncToggles = GUILayout.Toggle(syncToggles, new GUIContent("Sync Toggles"));
                rotateCamera = GUILayout.Toggle(rotateCamera, new GUIContent("Rotate camera around center"));
                walkAnimation = GUILayout.Toggle(walkAnimation, new GUIContent("Walking Animation"));
                distance = EditorGUILayout.FloatField(new GUIContent("Distance", "Distance between the models"), distance);
                speed = EditorGUILayout.FloatField(new GUIContent("Speed", "Speed of the animation"), speed);
                EditorGUI.BeginDisabledGroup(descriptor == null);
                if (GUILayout.Button("Setup Render"))
                {
                    SetupRender(descriptor.transform.gameObject, multipleModels, syncToggles, rotateCamera, walkAnimation, distance, speed);
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private static void SetupRender(GameObject model, bool multipleModels, bool syncToggles, bool rotateCamera, bool walkAnimation, float distance, float speed)
        {
            Undo.IncrementCurrentGroup();

            GameObject renderObject = GameObject.Find("Render Prefab");
            GameObject avatarHolder = GameObject.Find("Avatars");
            GameObject center = GameObject.Find("Center");
            CameraRotator camRotator;

            if (renderObject == null)
            {
                GameObject renderPrefab = LoadBwmpPrefab("Auto Render\\Render Prefab.prefab") as GameObject;
                if (renderPrefab == null)
                {
                    Debug.Log("Prefab not found!");
                    return;
                }
                renderObject = Instantiate(renderPrefab);
                Undo.RegisterCreatedObjectUndo(renderObject, "Setup Render");
                renderObject.name = "Render Prefab";
            }

            Undo.RegisterCompleteObjectUndo(renderObject, "Setup Render");
            renderObject.transform.position = Vector3.zero;

            if (center == null)
            {
                center = new GameObject("Center");
            }

            center.transform.parent = renderObject.transform;
            GameObject cam = renderObject.transform.Find("Render Camera").gameObject;
            camRotator = renderObject.GetOrAddComponent<CameraRotator>();

            if (rotateCamera)
            {
                camRotator.cam = cam;
                camRotator.rotateCenter = center.transform;
                camRotator.speed = speed;
                camRotator.distance = distance;
            }
            else
            {
                camRotator.cam = null;
            }

            if (model.GetComponent<VRCAvatarDescriptor>() == null)
            {
                Debug.Log("Invalid avatar");
                return;
            }

            if (avatarHolder == null)
            {
                avatarHolder = new GameObject("Avatars");
                avatarHolder.transform.parent = renderObject.transform;
            }

            for (int i = avatarHolder.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = avatarHolder.transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            Undo.RecordObject(model, "Setup Render");
            model.SetActive(false);

            Vector3 pos = multipleModels ? new Vector3(0.5f, 0, 0) : Vector3.zero;
            model = Instantiate(model, pos, Quaternion.Euler(0, 0, 0), avatarHolder.transform);
            model.SetActive(true);
            model.name = "RenderModel Forward";

            if (walkAnimation)
            {
                model.GetComponent<Animator>().runtimeAnimatorController = LoadBwmpPrefab("Auto Render\\RenderAnimator.controller") as AnimatorController;
            }

            HBBAndNames hips = new HBBAndNames()
            {
                names = new System.Collections.Generic.List<string>() { "hip", "hips" },
                bone = HumanBodyBones.Hips
            };

            if (multipleModels)
            {
                GameObject model2 = Instantiate(model, new Vector3(-0.5f, 0, 0), Quaternion.Euler(0, 180, 0), avatarHolder.transform);
                model2.name = "RenderModel Backwards";

                if (syncToggles)
                {
                    ToggleSync toggleSync = renderObject.GetOrAddComponent<ToggleSync>();
                    toggleSync.object1 = model;
                    toggleSync.object2 = model2;
                }

                Vector3 centerPos = GetPositionBetweenTwoPoints(
                    model.transform.FindDescendantByHBBOrNames(hips).position,
                    model2.transform.FindDescendantByHBBOrNames(hips).position
                );

                center.transform.position = centerPos;
                cam.transform.position = centerPos + new Vector3(0, 0, cam.transform.position.z);
            }
            else
            {
                center.transform.position = model.transform.FindDescendantByHBBOrNames(hips).position;
            }
        }
    }
}
