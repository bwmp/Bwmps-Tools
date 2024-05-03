using BwmpsTools.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static BwmpsTools.Tools.BasicUtils;
using static BwmpsTools.Utils.Structs;
using static BwmpsTools.Utils.Utilities;

namespace BwmpsTools.Tools.Modules
{
    internal class EyeOptions : BaseModule
    {
        private float eyeRotationAngle = 20f;
        public EyeOptions() : base("Eye Options") { }

        public float EyeRotationAngle { get => eyeRotationAngle; set => eyeRotationAngle = value; }

        public override void Render(Action renderContent = null)
        {
            base.Render(() =>
            {
                EyeRotationAngle = EditorGUILayout.FloatField("Eye Rotation Amount", EyeRotationAngle);
                EditorGUI.BeginDisabledGroup(TargetModel == null);
                if (GUILayout.Button("Auto Set Eye Look (buggy)"))
                {
                    AutoEyeLook(TargetModel.transform);
                }
                if (GUILayout.Button("Auto Set View Position (buggy)"))
                {
                    AutoSetViewpoint(TargetModel.transform);
                }
                EditorGUI.EndDisabledGroup();
                renderContent?.Invoke();
            });
        }

        private void AutoEyeLook(Transform transform)
        {
            VRCAvatarDescriptor descriptor = TargetModel.GetComponent<VRCAvatarDescriptor>();
            Undo.RecordObject(descriptor, "Auto Set Eye Look");
            if (descriptor != null)
            {
                VRCAvatarDescriptor.CustomEyeLookSettings eyeLookSettings = descriptor.customEyeLookSettings;
                HBBAndNames leftEyes = new HBBAndNames()
                {
                    names = new List<string>() { "lefteye", "eyeleft" },
                    bone = HumanBodyBones.LeftEye
                };
                HBBAndNames rightEyes = new HBBAndNames()
                {
                    names = new List<string>() { "righteye", "eyeright" },
                    bone = HumanBodyBones.RightEye
                };
                Transform leftEye = transform.FindDescendantByHBBOrNames(leftEyes);
                Transform rightEye = transform.FindDescendantByHBBOrNames(rightEyes);
                descriptor.enableEyeLook = true;
                descriptor.customEyeLookSettings.leftEye = leftEye.transform;
                descriptor.customEyeLookSettings.rightEye = rightEye.transform;
                Dictionary<string, Quaternion> eyeLookRotations = new Dictionary<string, Quaternion>()
            {
                {"eyesLookingUp", Quaternion.Euler(-EyeRotationAngle, 0, 0)},
                {"eyesLookingDown", Quaternion.Euler(EyeRotationAngle, 0, 0)},
                {"eyesLookingLeft", Quaternion.Euler(0, -EyeRotationAngle, 0)},
                {"eyesLookingRight", Quaternion.Euler(0, EyeRotationAngle, 0)},
            };
                foreach (KeyValuePair<string, Quaternion> kvp in eyeLookRotations)
                {
                    FieldInfo field = eyeLookSettings.GetType().GetField(kvp.Key);
                    VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations rotations = (VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations)field.GetValue(eyeLookSettings);
                    rotations.left = kvp.Value;
                    rotations.right = kvp.Value;
                    field.SetValue(eyeLookSettings, rotations);
                }

            }
        }

        private void AutoSetViewpoint(Transform transform)
        {
            VRCAvatarDescriptor descriptor = TargetModel.GetComponent<VRCAvatarDescriptor>();
            Undo.RecordObject(descriptor, "Auto Set Viewpoint");
            if (descriptor != null)
            {
                HBBAndNames leftEyes = new HBBAndNames()
                {
                    names = new List<string>() { "eye_l", "lefteye" },
                    bone = HumanBodyBones.LeftEye
                };
                HBBAndNames rightEyes = new HBBAndNames()
                {
                    names = new List<string>() { "eye_r", "righteye" },
                    bone = HumanBodyBones.RightEye
                };

                Transform leftEye = transform.FindDescendantByHBBOrNames(leftEyes);
                Transform rightEye = transform.FindDescendantByHBBOrNames(rightEyes);

                Vector3 leftEyePos = leftEye.position - transform.position;
                Vector3 rightEyePos = rightEye.position - transform.position;

                descriptor.ViewPosition = GetPositionBetweenTwoPoints(leftEyePos, rightEyePos);
                descriptor.ViewPosition.z += 0.05f;
            }
        }

    }
}
