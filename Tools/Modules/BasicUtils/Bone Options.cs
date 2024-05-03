using BwmpsTools.Data.ScriptableObjects;
using BwmpsTools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using static BwmpsTools.Tools.BasicUtils;
using static BwmpsTools.Utils.Structs;
using static VRC.Dynamics.VRCPhysBoneBase;
using static BwmpsTools.Utils.Utilities;
using static BwmpsTools.Data.ScriptableObjects.PhysboneSetting;

namespace BwmpsTools.Tools.Modules
{
    internal class BoneOptions : BaseModule
    {
        private List<PhysboneSetting> physboneSettings;
        private int selectedBaseIndex;
        public BoneOptions() : base("Bone Options")
        {
            physboneSettings = LoadAllPhysboneSettings();
        }
        public override void Render(Action renderContent = null)
        {
            base.Render(() =>
            {
                bool anySettingIsNull = physboneSettings.Any(item => item == null);
                if (anySettingIsNull) RefreshPhysboneSettings();

                string[] baseNames = physboneSettings.Select(settings => settings.name).ToArray();
                EditorGUI.BeginDisabledGroup(TargetModel == null);
                EditorGUILayout.BeginHorizontal();
                selectedBaseIndex = EditorGUILayout.Popup(new GUIContent("Selected Base", "The base's settings to use for physbones"), selectedBaseIndex, baseNames);
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(30)))
                {
                    RefreshPhysboneSettings();
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Auto Setup Physbones"))
                {
                    AutoPhysbones(TargetModel.transform);
                    GameObject temp = TargetModel;
                    TargetModel = null;
                    TargetModel = temp;
                }
                if (GUILayout.Button("Remove End Bones"))
                {
                    DeleteEndObjectsRecursive(TargetModel.transform);
                }
                if (GUILayout.Button("Get Physbone List")) PhysboneTools.Instance.Init();
                if (GUILayout.Button("Merge Physbones")) MergePhysbones(TargetModel.transform);
                if (GUILayout.Button("Twist Bone Helper")) TwistBoneHelper.Instance.Init();
                EditorGUI.EndDisabledGroup();
                renderContent?.Invoke();
            });
        }

        private void RefreshPhysboneSettings()
        {
            physboneSettings = LoadAllPhysboneSettings();
        }


        private void DeleteEndObjectsRecursive(Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform childTransform = transform.GetChild(i);
                if (childTransform.name.Contains("_end"))
                {
                    Undo.DestroyObjectImmediate(childTransform.gameObject);
                }
                else
                {
                    DeleteEndObjectsRecursive(childTransform);
                }
            }
        }

        private void AutoPhysbones(Transform transform)
        {
            PhysboneSetting SelectedBase = physboneSettings[selectedBaseIndex];
            Undo.IncrementCurrentGroup();

            foreach (BonePreset preset in SelectedBase.BoneList)
            {
                List<Transform> boneTransforms = transform.FindDescendantByHBBOrNames(preset.parentBone).FindChildrenByPartialNames(preset.targetBones, preset.excludeWords);
                foreach (Transform boneTransform in boneTransforms)
                {
                    VRCPhysBone physbone = boneTransform.gameObject.GetOrAddComponent<VRCPhysBone>();
                    Undo.RecordObject(physbone, "Modify PhysBone Settings");
                    CopyPhysboneSettings(physbone, preset.boneSettings);
                }
            }
        }


        private void CopyPhysboneSettings(VRCPhysBone physbone, PhysboneSetting.PhysboneSettings settings)
        {
            physbone.integrationType = settings.integrationType;
            physbone.endpointPosition = settings.endpointPosition;
            physbone.multiChildType = settings.multiChildType;
            physbone.pull = settings.pull;
            physbone.pullCurve = settings.pullCurve;
            physbone.spring = settings.spring;
            physbone.springCurve = settings.springCurve;
            physbone.stiffness = settings.stiffness;
            physbone.stiffnessCurve = settings.stiffnessCurve;
            physbone.gravity = settings.gravity;
            physbone.gravityCurve = settings.gravityCurve;
            physbone.gravityFalloff = settings.gravityFalloff;
            physbone.gravityFalloffCurve = settings.gravityFalloffCurve;
            physbone.immobileType = settings.immobileType;
            physbone.immobile = settings.immobile;
            physbone.immobileCurve = settings.immobileCurve;
            physbone.allowCollision = settings.allowCollision;
            physbone.radius = settings.radius;
            physbone.radiusCurve = settings.radiusCurve;
            physbone.limitType = settings.limitType;
            physbone.maxAngleX = settings.maxAngleX;
            physbone.maxAngleXCurve = settings.maxAngleXCurve;
            physbone.maxAngleZ = settings.maxAngleZ;
            physbone.maxAngleZCurve = settings.maxAngleZCurve;
            physbone.limitRotation = settings.limitRotation;
            physbone.limitRotationXCurve = settings.limitRotationXCurve;
            physbone.limitRotationYCurve = settings.limitRotationYCurve;
            physbone.limitRotationZCurve = settings.limitRotationZCurve;
            physbone.allowGrabbing = settings.allowGrabbing;
            physbone.grabFilter = settings.grabFilter;
            physbone.allowPosing = settings.allowPosing;
            physbone.poseFilter = settings.poseFilter;
            physbone.snapToHand = settings.snapToHand;
            physbone.grabMovement = settings.grabMovement;
            physbone.maxStretch = settings.maxStretch;
            physbone.maxStretchCurve = settings.maxStretchCurve;
            physbone.maxSquish = settings.maxSquish;
            physbone.maxSquishCurve = settings.maxSquishCurve;
            physbone.stretchMotion = settings.stretchMotion;
            physbone.stretchMotionCurve = settings.stretchMotionCurve;
            physbone.isAnimated = settings.isAnimated;
            physbone.resetWhenDisabled = settings.resetWhenDisabled;
            physbone.parameter = settings.parameter;
        }


        private void MergePhysbones(Transform mainTransform)
        {
            // Start a new undo group
            Undo.IncrementCurrentGroup();

            VRCPhysBone[] physBones = mainTransform.GetComponentsInChildren<VRCPhysBone>(true);

            if (physBones == null || physBones.Length == 0)
            {
                Debug.LogWarning("No VRCPhysBone components found.");
                return;
            }

            Dictionary<int, List<VRCPhysBone>> physBoneGroups = new Dictionary<int, List<VRCPhysBone>>();

            // Group physbones by their settings hashcode
            foreach (VRCPhysBone physBone in physBones)
            {
                if (physBone == null || physBone.gameObject == null)
                {
                    Debug.LogWarning("Encountered null VRCPhysBone component.");
                    continue;
                }

                try
                {
                    int settingsHashCode = CalculateSettingsHashCode(physBone);

                    if (!physBoneGroups.ContainsKey(settingsHashCode))
                    {
                        physBoneGroups[settingsHashCode] = new List<VRCPhysBone>();
                    }
                    physBoneGroups[settingsHashCode].Add(physBone);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message} + {physBone.name}");
                }
            }

            // Merge physbones within each group
            foreach (KeyValuePair<int, List<VRCPhysBone>> group in physBoneGroups)
            {
                if (group.Value.Count <= 1) continue; // Skip if there's only one physbone in the group

                // Create an undo group for each merged physbone group
                Undo.RegisterFullObjectHierarchyUndo(group.Value[0].transform.parent, "Merge PhysBones");

                List<Transform> ignoreTransforms = new List<Transform>();
                GameObject mergedPhysBoneObject = new GameObject("MergedPhysBone");

                mergedPhysBoneObject.transform.parent = group.Value[0].transform.parent;

                VRCPhysBone mergedPhysBone = ApplySettings(mergedPhysBoneObject.GetOrAddComponent<VRCPhysBone>(), group.Value[0]);
                mergedPhysBoneObject.GetComponent<VRCPhysBone>();

                foreach (VRCPhysBone physBone in group.Value)
                {
                    // Record changes to the parent hierarchy for undo
                    Undo.SetTransformParent(physBone.transform, mergedPhysBoneObject.transform, "Merge PhysBones");
                    Undo.DestroyObjectImmediate(physBone);
                    ignoreTransforms.AddRange(physBone.ignoreTransforms);
                }

                List<Transform> uniqueTransforms = new List<Transform>(new HashSet<Transform>(ignoreTransforms));
                ignoreTransforms = uniqueTransforms;

                // Record changes to ignoreTransforms and multiChildType for undo
                Undo.RecordObject(mergedPhysBone, "Merge PhysBones");
                mergedPhysBone.ignoreTransforms = ignoreTransforms;
                mergedPhysBone.multiChildType = MultiChildType.Ignore;
            }
        }


        private int CalculateSettingsHashCode(VRCPhysBone physBone)
        {
            if (physBone == null)
            {
                Debug.LogError("physBone is null!");
                return 0;
            }

            int hashCode = physBone.integrationType.GetHashCode() ^
                           physBone.multiChildType.GetHashCode() ^
                           physBone.pull.GetHashCode() ^
                           physBone.spring.GetHashCode() ^
                           physBone.stiffness.GetHashCode() ^
                           physBone.gravity.GetHashCode() ^
                           physBone.gravityFalloff.GetHashCode() ^
                           physBone.immobileType.GetHashCode() ^
                           physBone.immobile.GetHashCode() ^
                           physBone.allowCollision.GetHashCode() ^
                           physBone.collisionFilter.GetHashCode() ^
                           physBone.radius.GetHashCode() ^
                           physBone.limitType.GetHashCode() ^
                           physBone.maxAngleX.GetHashCode() ^
                           physBone.maxAngleZ.GetHashCode() ^
                           physBone.limitRotation.GetHashCode() ^
                           physBone.allowGrabbing.GetHashCode() ^
                           physBone.allowPosing.GetHashCode() ^
                           physBone.poseFilter.GetHashCode() ^
                           physBone.snapToHand.GetHashCode() ^
                           physBone.grabMovement.GetHashCode() ^
                           physBone.maxStretch.GetHashCode() ^
                           physBone.isAnimated.GetHashCode() ^
                           physBone.resetWhenDisabled.GetHashCode();

            if (physBone.parameter != null)
                hashCode += physBone.parameter.GetHashCode();


            return hashCode;
        }

        private VRCPhysBone ApplySettings(VRCPhysBone physBone, VRCPhysBone copyFrom)
        {
            var fields = typeof(VRCPhysBone).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                field.SetValue(physBone, field.GetValue(copyFrom));
            }

            var properties = typeof(VRCPhysBone).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    property.SetValue(physBone, property.GetValue(copyFrom));
                }
            }

            return physBone;
        }


    }
}
