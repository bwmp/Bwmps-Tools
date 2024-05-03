using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static VRC.Dynamics.VRCPhysBoneBase;

namespace BwmpsTools.Utils
{
    public class Structs
    {
        public struct VRCAvatar
        {
            private Transform avatar;
            private Animator animator;
            private VRCExpressionsMenu menu;
            private VRCExpressionParameters parameters;
            private VRCAvatarDescriptor avatarDescriptor;
            private AnimatorController controller;

            public Transform Avatar { readonly get => avatar; set => avatar = value; }
            public Animator Animator { readonly get => animator; set => animator = value; }
            public VRCExpressionsMenu Menu { readonly get => menu; set => menu = value; }
            public VRCExpressionParameters Parameters { readonly get => parameters; set => parameters = value; }
            public VRCAvatarDescriptor AvatarDescriptor { readonly get => avatarDescriptor; set => avatarDescriptor = value; }
            public AnimatorController Controller { readonly get => controller; set => controller = value; }
        }

        [Serializable]
        public class HBBAndNames
        {
            public List<string> names;
            public HumanBodyBones bone;
        }

        [Serializable]
        public struct PhysboneSettings
        {
            public IntegrationType IntegrationType { get; set; }
            public Vector3 EndpointPosition { get; set; }
            public float Pull { get; set; }
            public float Spring { get; set; }
            public float Stiffness { get; set; }
            public float Gravity { get; set; }
            public float GravityFalloff { get; set; }
            public ImmobileType ImmobileType { get; set; }
            public float Immobile { get; set; }
            public LimitType LimitType { get; set; }
            public float MaxAngleX { get; set; }
            public float MaxAngleZ { get; set; }
            public float GrabMovement { get; set; }
            public float MaxStretch { get; set; }
            public float MaxSquish { get; set; }
            public float StretchMotion { get; set; }
            public float CollisionRadius { get; set; }
        }

        public struct Toggle
        {
            public AnimationClip onClip;
            public AnimationClip offClip;
        }
    }
}
