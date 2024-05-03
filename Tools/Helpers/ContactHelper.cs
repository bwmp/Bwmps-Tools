using BwmpsTools.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using static BwmpsTools.Utils.Structs;
using static BwmpsTools.Utils.ExpressionUtils;

namespace BwmpsTools.Tools.Helpers
{
    internal class ContactHelper : EditorWindow
    {

        private VRCAvatarDescriptor descriptor = null;
        private VRCAvatar avatar;
        private SkinnedMeshRenderer headMesh;
        private GameObject particlePrefab;
        private AudioClip audioClip;
        private GameObject audioClipObject;
        private GameObject particlePrefabObject;
        private DefaultAsset animationFolder = null;
        private ContactArea selectedContactArea = ContactArea.Head;
        private string param = "contact_HeadPat";
        private readonly List<string> selectedBlendShapes = new List<string>();


        private const string Title = "Contact Helper";
        private const string Description = "Easily setup differnet contacts";

        [MenuItem("Bwmp's Tools/Helpers/Contacts", false, 0)]
        static void Init()
        {
            ContactHelper window = (ContactHelper)GetWindow(typeof(ContactHelper));
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private void OnGUI()
        {

            EditorGUIExtensions.TitleBox(Title, Description);
            DrawInputFields();
            DrawBlendShapeCheckboxes();

            if (GUILayout.Button("Add Contact"))
            {
                avatar = Utilities.GetAvatarInfo(descriptor);

                switch (selectedContactArea)
                {
                    case ContactArea.Head:
                        param = "contact_HeadPat";
                        break;
                    case ContactArea.LeftEye:
                        param = "contact_LeftEyePat";
                        break;
                    case ContactArea.RightEye:
                        param = "contact_RightEyePat";
                        break;
                    case ContactArea.Nose:
                        param = "contact_NosePat";
                        break;
                }

                SetupContact();
                SetupObjects();

                string folderPath = AssetDatabase.GetAssetPath(animationFolder);
                AnimationClip onClip = CreateClip(param + "_On", true, 0.5f, folderPath);
                AnimationClip offClip = CreateClip(param + "_Off", false, 0.5f, folderPath);
                avatar.Controller.AddParameter(param, AnimatorControllerParameterType.Bool);
                CreateAndFillStateLayer(avatar.Controller, param, onClip, offClip);
            }

        }

        private void DrawInputFields()
        {
            selectedContactArea = (ContactArea)EditorGUILayout.EnumPopup("Contact Area", selectedContactArea);
            descriptor = EditorGUILayout.ObjectField("Avatar Descriptor", descriptor, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
            headMesh = EditorGUILayout.ObjectField("Head mesh", headMesh, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            particlePrefab = EditorGUILayout.ObjectField("Particle Prefab", particlePrefab, typeof(GameObject), true) as GameObject;
            audioClip = EditorGUILayout.ObjectField("Audio Clip", audioClip, typeof(AudioClip), true) as AudioClip;
            animationFolder = EditorGUILayout.ObjectField("Animation Folder", animationFolder, typeof(DefaultAsset), true) as DefaultAsset;
        }

        private Vector2 scrollPos;
        private void DrawBlendShapeCheckboxes()
        {
            if (headMesh == null)
            {
                EditorGUILayout.HelpBox("Please select a Head mesh.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Blend Shapes to use:", EditorStyles.boldLabel);

            Mesh mesh = headMesh.sharedMesh;
            if (mesh != null)
            {
                int blendShapeCount = mesh.blendShapeCount;
                using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
                scrollPos = scroll.scrollPosition;
                for (int i = 0; i < blendShapeCount; i++)
                {
                    string blendShapeName = mesh.GetBlendShapeName(i);
                    bool isSelected = selectedBlendShapes.Contains(blendShapeName);

                    EditorGUI.BeginChangeCheck();
                    isSelected = EditorGUILayout.ToggleLeft(blendShapeName, isSelected);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (isSelected && !selectedBlendShapes.Contains(blendShapeName))
                        {
                            selectedBlendShapes.Add(blendShapeName);
                        }
                        else if (!isSelected && selectedBlendShapes.Contains(blendShapeName))
                        {
                            selectedBlendShapes.Remove(blendShapeName);
                        }
                    }
                }
            }
        }

        public AnimationClip CreateClip(string name, bool onClip, float length, string path)
        {
            AnimationClip clip = new AnimationClip() { legacy = false };
            string rendererName = headMesh.name;
            float blendshapeStart = onClip ? 0f : 100f;
            float blendshapeEnd = onClip ? 100f : 0f;
            foreach (string blendshape in selectedBlendShapes)
            {
                clip.SetCurve(
                    rendererName,
                    typeof(SkinnedMeshRenderer),
                    $"blendShape.{blendshape}",
                    new AnimationCurve(new Keyframe(0, blendshapeStart, 0, 0), new Keyframe(length, blendshapeEnd, 0, 0))
                );
            }
            if (audioClip != null)
            {
                string transformPath = AnimationUtility.CalculateTransformPath(audioClipObject.transform, audioClipObject.transform.root.gameObject.transform);
                AnimationCurve animationCurve = AnimationCurve.Constant(0.0f, clip.length, onClip ? 1f : 0.0f);
                clip.SetCurve(transformPath, typeof(GameObject), "m_IsActive", animationCurve);
            }
            if (particlePrefab != null)
            {
                string transformPath = AnimationUtility.CalculateTransformPath(particlePrefabObject.transform, particlePrefabObject.transform.root.gameObject.transform);
                AnimationCurve animationCurve = AnimationCurve.Constant(0.0f, clip.length, onClip ? 1f : 0.0f);
                clip.SetCurve(transformPath, typeof(GameObject), "m_IsActive", animationCurve);

            }
            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{path.TrimEnd('/')}/{name}.anim");
            AssetDatabase.CreateAsset(clip, uniqueAssetPath);

            return clip;
        }

        public enum ContactArea
        {
            Head,
            LeftEye,
            RightEye,
            Nose
        }

        HBBAndNames head = new HBBAndNames()
        {
            names = new List<string>() { "head" },
            bone = HumanBodyBones.Head
        };
        HBBAndNames leftEye = new HBBAndNames()
        {
            names = new List<string>() { "lefteye" },
            bone = HumanBodyBones.LeftEye
        };
        HBBAndNames rightEye = new HBBAndNames()
        {
            names = new List<string>() { "righteye" },
            bone = HumanBodyBones.RightEye
        };

        public void SetupContact()
        {
            HBBAndNames targetBone = selectedContactArea switch
            {
                ContactArea.Head => head,
                ContactArea.LeftEye => leftEye,
                ContactArea.RightEye => rightEye,
                ContactArea.Nose => head,
                _ => head,
            };
            Transform parentBone = descriptor.transform.FindDescendantByHBBOrNames(targetBone);
            GameObject newChild = new GameObject(param);
            newChild.transform.SetParent(parentBone);
            VRCContactReceiver receriver = newChild.AddComponent<VRCContactReceiver>();
            receriver.radius = selectedContactArea switch
            {
                ContactArea.Head => 0.08f,
                ContactArea.LeftEye => 0.02f,
                ContactArea.RightEye => 0.02f,
                ContactArea.Nose => 0.01f,
                _ => 0.08f,
            };
            receriver.position = selectedContactArea switch
            {
                ContactArea.Head => new Vector3(0, 0.05f, 0),
                ContactArea.LeftEye => new Vector3(0, 0, 0),
                ContactArea.RightEye => new Vector3(0, 0, 0),
                ContactArea.Nose => new Vector3(0, 0.02f, 0.075f),
                _ => new Vector3(0, 0.05f, 0),
            };
            switch (selectedContactArea)
            {
                case ContactArea.Head:
                    receriver.collisionTags.Add("Hand");
                    break;
                case ContactArea.LeftEye:
                    receriver.collisionTags.Add("Finger");
                    break;
                case ContactArea.RightEye:
                    receriver.collisionTags.Add("Finger");
                    break;
                case ContactArea.Nose:
                    receriver.collisionTags.Add("Finger");
                    break;
                default:
                    receriver.collisionTags.Add("Hand");
                    break;
            }
            receriver.parameter = param;
            receriver.receiverType = ContactReceiver.ReceiverType.Constant;
            newChild.transform.localPosition = Vector3.zero;
        }

        public void SetupObjects()
        {
            HBBAndNames targetBone = selectedContactArea switch
            {
                ContactArea.Head => head,
                ContactArea.LeftEye => leftEye,
                ContactArea.RightEye => rightEye,
                ContactArea.Nose => head,
                _ => head,
            };
            Transform parentBone = descriptor.transform.FindDescendantByHBBOrNames(targetBone);
            if (audioClip != null)
            {
                audioClipObject = new GameObject(param + "_Audio");
                audioClipObject.transform.SetParent(parentBone);
                audioClipObject.transform.localPosition = Vector3.zero;
                audioClipObject.transform.localScale = Vector3.zero;
                AudioSource audioSource = audioClipObject.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                VRCSpatialAudioSource vRCSpatialAudioSource = audioClipObject.AddComponent<VRCSpatialAudioSource>();
                vRCSpatialAudioSource.Far = 2;
                audioClipObject.SetActive(false);
            }
            if (particlePrefab != null)
            {
                particlePrefabObject = Instantiate(particlePrefab);
                particlePrefabObject.name = param + "_Particles";
                particlePrefabObject.transform.SetParent(parentBone);
                particlePrefabObject.transform.localPosition = Vector3.zero;
                particlePrefabObject.transform.localScale = particlePrefabObject.transform.lossyScale;
                particlePrefabObject.SetActive(false);
            }
        }

    }
}
