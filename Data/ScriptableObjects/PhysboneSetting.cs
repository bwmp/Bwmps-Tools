using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.Dynamics;
using static BwmpsTools.Utils.Structs;
using static VRC.Dynamics.VRCPhysBoneBase;

namespace BwmpsTools.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PhysboneSetting", menuName = "Bwmp's Tools/Physbone Setting")]
    public class PhysboneSetting : ScriptableObject
    {
        [Serializable]
        public class BonePreset
        {
            public bool foldout = true;
            public string name;
            public HBBAndNames parentBone;
            public List<string> targetBones = new List<string>();
            public List<string> excludeWords = new List<string>();
            public PhysboneSettings boneSettings;
            public bool foldout_boneSettings = true;
        }

        [Serializable]
        public class PhysboneSettings
        {
            public bool foldout_transforms = true;

            public bool foldout_forces = true;

            public bool foldout_collision = true;

            public bool foldout_stretchsquish = true;

            public bool foldout_limits = true;

            public bool foldout_grabpose = true;

            public bool foldout_options = true;

            [Tooltip("Determines how forces are applied.  Certain kinds of motion may require using a specific integration type.")]
            public IntegrationType integrationType;

            [Tooltip("The transform where this component begins.  If left blank, we assume we start at this game object.")]
            public Transform rootTransform;

            [Tooltip("List of ignored transforms that shouldn't be affected by this component.  Ignored transforms automatically include any of that transform's children.")]
            public List<Transform> ignoreTransforms = new List<Transform>();

            [Tooltip("Vector used to create additional bones at each endpoint of the chain. Only used if the value is non-zero.")]
            public Vector3 endpointPosition = Vector3.zero;

            [Tooltip("Determines how transforms with multiple children are handled. By default those transforms are ignored.")]
            public MultiChildType multiChildType;

            [Tooltip("Amount of force used to return bones to their rest position.")]
            [Range(0f, 1f)]
            public float pull = 0.2f;

            public AnimationCurve pullCurve;

            [Tooltip("Amount bones will wobble when trying to reach their rest position.")]
            [Range(0f, 1f)]
            public float spring = 0.2f;

            public AnimationCurve springCurve;

            [Tooltip("Amount bones will try and stay at their current orientation.")]
            [Range(0f, 1f)]
            public float stiffness = 0.2f;

            public AnimationCurve stiffnessCurve;

            [Tooltip("Amount of gravity applied to bones.  Positive value pulls bones down, negative pulls upwards.")]
            [Range(-1f, 1f)]
            public float gravity;

            public AnimationCurve gravityCurve;

            [Tooltip("Reduces gravity while bones are at their rest orientation.  Gravity will increase as bones rotate away from their rest orientation, reaching full gravity at 90 degress from rest.")]
            [Range(0f, 1f)]
            public float gravityFalloff;

            public AnimationCurve gravityFalloffCurve;

            [Tooltip("Determines how immobile is calculated.\n\nAll Motion - Reduces any motion as calculated from the root transform's parent.World - Reduces positional movement from locomotion, any movement due to animations or IK still affect bones normally.\n\n")]
            public ImmobileType immobileType;

            [Tooltip("Reduces the effect movement has on bones. The greater the value the less motion affects the chain as determined by the Immobile Type.")]
            [Range(0f, 1f)]
            public float immobile;

            public AnimationCurve immobileCurve;

            [Tooltip("Allows collision with colliders other than the ones specified on this component.  Currently the only other colliders are each player's hands as defined by their avatar.")]
            public AdvancedBool allowCollision = AdvancedBool.True;

            public PermissionFilter collisionFilter = new PermissionFilter(value: true);

            [Tooltip("Collision radius around each bone.  Used for both collision and grabbing.")]
            public float radius;

            public AnimationCurve radiusCurve;

            [Tooltip("List of colliders that specifically collide with these bones.")]
            public List<VRCPhysBoneColliderBase> colliders = new List<VRCPhysBoneColliderBase>();

            [Tooltip("Type of angular limit applied to each bone.")]
            public LimitType limitType;

            [Tooltip("Maximum angle each bone can rotate from its rest position.")]
            [Range(0f, 180f)]
            public float maxAngleX = 45f;

            public AnimationCurve maxAngleXCurve;

            [Tooltip("Maximum angle each bone can rotate from its rest position.")]
            [Range(0f, 90f)]
            public float maxAngleZ = 45f;

            public AnimationCurve maxAngleZCurve;

            [Tooltip("Rotates the angular limits on each axis.")]
            public Vector3 limitRotation;

            public AnimationCurve limitRotationXCurve;

            public AnimationCurve limitRotationYCurve;

            public AnimationCurve limitRotationZCurve;

            [NonSerialized]
            public Vector3 staticFreezeAxis;

            [Tooltip("Allows players to grab the bones.")]
            [FormerlySerializedAs("isGrabbable")]
            public AdvancedBool allowGrabbing = AdvancedBool.True;

            public PermissionFilter grabFilter = new PermissionFilter(value: true);

            [Tooltip("Allows players to pose the bones after grabbing.")]
            [FormerlySerializedAs("isPoseable")]
            public AdvancedBool allowPosing = AdvancedBool.True;

            public PermissionFilter poseFilter = new PermissionFilter(value: true);

            [Tooltip("When a bone is grabbed it will snap to the hand grabbing it.")]
            public bool snapToHand;

            [Tooltip("Controls how grabbed bones move.\nA value of zero results in bones using pull & spring to reach the grabbed position.\nA value of one results in bones immediately moving to the grabbed position.")]
            [Range(0f, 1f)]
            public float grabMovement = 0.5f;

            [Tooltip("Maximum amount the bones can stretch.  This value is a multiple of the original bone length.")]
            public float maxStretch;

            public AnimationCurve maxStretchCurve;

            [Tooltip("Maximum amount the bones can shrink.  This value is a multiple of the original bone length.")]
            [Range(0f, 1f)]
            public float maxSquish;

            public AnimationCurve maxSquishCurve;

            [Tooltip("The amount motion will affect the stretch/squish of the bones.  A value of zero means bones will only stretch/squish as a result of grabbing or collisions.")]
            [Range(0f, 1f)]
            public float stretchMotion;

            public AnimationCurve stretchMotionCurve;

            [Tooltip("Allows bone transforms to be animated.  Each frame bone rest position will be updated according to what was animated.")]
            public bool isAnimated;

            [Tooltip("When this component becomes disabled, the bones will automatially reset to their default rest position.")]
            public bool resetWhenDisabled;

            [Tooltip("Keyname used to provide multiple parameters to the avatar controller.")]
            public string parameter;
        }

        public List<BonePreset> BoneList = new List<BonePreset>();
    }
}
