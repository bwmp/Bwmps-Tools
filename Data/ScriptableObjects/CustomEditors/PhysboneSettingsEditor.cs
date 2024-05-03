using UnityEditor;
using UnityEngine;
using BwmpsTools.Data.ScriptableObjects;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using static BwmpsTools.Data.ScriptableObjects.PhysboneSetting;
using UnityEngine.UIElements;

namespace BwmpsTools.Data.ScriptableObjects.CustomEditors
{
    [CustomEditor(typeof(PhysboneSetting))]
    public class PhysboneSettingEditor : Editor
    {
        private SerializedProperty boneList;

        private void OnEnable()
        {
            boneList = serializedObject.FindProperty("BoneList");
        }

        public bool BeginSection(string title, string docTag, SerializedProperty foldoutProperty)
        {
            EditorGUILayout.BeginHorizontal();
            foldoutProperty.boolValue = EditorGUILayout.Foldout(foldoutProperty.boolValue, title, EditorStyles.foldoutHeader);
            if (docTag != null && GUILayout.Button("?", GUILayout.Width(32f)))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/physbones#" + docTag);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            return foldoutProperty.boolValue;
        }


        public void EndSection()
        {
            EditorGUI.indentLevel--;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Add Bone"))
            {
                boneList.arraySize++;
            }

            for (int i = 0; i < boneList.arraySize; i++)
            {
                SerializedProperty bone = boneList.GetArrayElementAtIndex(i);
                SerializedProperty boneSettings = bone.FindPropertyRelative("boneSettings");

                EditorGUILayout.BeginVertical();
                bone.FindPropertyRelative("foldout").boolValue = EditorGUILayout.Foldout(bone.FindPropertyRelative("foldout").boolValue, bone.FindPropertyRelative("name").stringValue, EditorStyles.foldoutHeader);
                if (bone.FindPropertyRelative("foldout").boolValue)
                {
                    if (GUILayout.Button("Remove"))
                    {
                        boneList.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }

                    EditorGUILayout.PropertyField(bone.FindPropertyRelative("name"));

                    EditorGUILayout.PropertyField(bone.FindPropertyRelative("parentBone"));

                    EditorGUILayout.PropertyField(bone.FindPropertyRelative("targetBones"));

                    EditorGUILayout.PropertyField(bone.FindPropertyRelative("excludeWords"));

                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("endpointPosition"));

                    EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("multiChildType"));

                    if (BeginSection("Forces", "forces", boneSettings.FindPropertyRelative("foldout_forces")))
                    {
                        EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("integrationType"));
                        DrawCurveParam("pull", "pullCurve", i);
                        SerializedProperty integrationTypeProp = boneSettings.FindPropertyRelative("integrationType");
                        if ((VRCPhysBoneBase.IntegrationType)integrationTypeProp.enumValueIndex == VRCPhysBoneBase.IntegrationType.Simplified)
                        {
                            DrawCurveParam("spring", "springCurve", i);
                        }
                        else
                        {
                            DrawCurveParam("Momentum", "spring", "springCurve", i);
                            DrawCurveParam("stiffness", "stiffnessCurve", i);
                        }

                        DrawCurveParam("gravity", "gravityCurve", i);
                        EditorGUI.BeginDisabledGroup(boneSettings.FindPropertyRelative("gravity").floatValue == 0f);
                        DrawCurveParam("gravityFalloff", "gravityFalloffCurve", i);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("immobileType"));
                        DrawCurveParam("immobile", "immobileCurve", i);
                    }
                    EndSection();

                    if (BeginSection("Limits", "limits", boneSettings.FindPropertyRelative("foldout_limits")))
                    {
                        SerializedProperty serializedProperty = boneSettings.FindPropertyRelative("limitType");
                        EditorGUILayout.PropertyField(serializedProperty);
                        switch (serializedProperty.enumValueIndex)
                        {
                            case 1:
                                DrawCurveParam("Max Angle", "maxAngleX", "maxAngleXCurve", i);
                                DrawCurveParam("Rotation", "limitRotation", "Pitch", "limitRotationXCurve", "Roll", "limitRotationYCurve", "Yaw", "limitRotationZCurve", i);
                                break;
                            case 2:
                                DrawCurveParam("Max Angle", "maxAngleX", "maxAngleXCurve", i);
                                DrawCurveParam("Rotation", "limitRotation", "Pitch", "limitRotationXCurve", "Roll", "limitRotationYCurve", "Yaw", "limitRotationZCurve", i);
                                break;
                            case 3:
                                DrawCurveParam("Max Pitch", "maxAngleX", "maxAngleXCurve", i);
                                DrawCurveParam("Max Yaw", "maxAngleZ", "maxAngleZCurve", i);
                                DrawCurveParam("Rotation", "limitRotation", "Pitch", "limitRotationXCurve", "Roll", "limitRotationYCurve", "Yaw", "limitRotationZCurve", i);
                                break;
                        }
                    }

                    EndSection();
                    if (BeginSection("Collision", "collision", boneSettings.FindPropertyRelative("foldout_collision")))
                    {
                        DrawCurveParam("radius", "radiusCurve", i);
                        DrawPermission("allowCollision", "collisionFilter", i);
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("colliders"));
                    }

                    EndSection();
                    if (BeginSection("Stretch & Squish", "stretch--squish", boneSettings.FindPropertyRelative("foldout_stretchsquish")))
                    {
                        DrawCurveParam("stretchMotion", "stretchMotionCurve", i);
                        DrawCurveParam("maxStretch", "maxStretchCurve", i);
                        DrawCurveParam("maxSquish", "maxSquishCurve", i);
                    }

                    EndSection();
                    if (BeginSection("Grab & Pose", "grab--pose", boneSettings.FindPropertyRelative("foldout_grabpose")))
                    {
                        DrawPermission("allowGrabbing", "grabFilter", i);
                        DrawPermission("allowPosing", "poseFilter", i);
                        EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("grabMovement"));
                        EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("snapToHand"));
                    }

                    EndSection();
                    if (BeginSection("Options", "options", boneSettings.FindPropertyRelative("foldout_options")))
                    {
                        EditorGUI.BeginDisabledGroup(Application.isPlaying);
                        SerializedProperty parameterProp = boneSettings.FindPropertyRelative("parameter");
                        EditorGUILayout.PropertyField(parameterProp);
                        if (!string.IsNullOrEmpty(parameterProp.stringValue))
                        {
                            EditorGUILayout.HelpBox("Click [?] button in the Options section to read full documentation.\n" + parameterProp.stringValue + "_IsGrabbed [Bool]\n" + parameterProp.stringValue + "_IsPosed [Bool]\n" + parameterProp.stringValue + "_Angle [Float 0-1]\n" + parameterProp.stringValue + "_Stretch [Float 0-1]\n" + parameterProp.stringValue + "_Squish [Float 0-1]", MessageType.Info);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Choose a parameter name to use this feature.", MessageType.Info);
                        }
                        EditorGUI.EndDisabledGroup();
                        SerializedProperty serializedProperty2 = boneSettings.FindPropertyRelative("isAnimated");
                        EditorGUILayout.PropertyField(serializedProperty2);
                        if (serializedProperty2.boolValue)
                        {
                            EditorGUILayout.HelpBox("Only enable IsAnimated if you are animating the bone transforms affected by this component. Leaving this disabled is better for performance.", MessageType.Warning);
                        }
                        EditorGUILayout.PropertyField(boneSettings.FindPropertyRelative("resetWhenDisabled"));
                    }
                    EndSection();
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCurveParam(string label, string property, string curveProperty, int index)
        {
            SerializedProperty bone = boneList.GetArrayElementAtIndex(index);
            SerializedProperty boneSettings = bone.FindPropertyRelative("boneSettings");

            SerializedProperty property2 = boneSettings.FindPropertyRelative(property);
            SerializedProperty serializedProperty = boneSettings.FindPropertyRelative(curveProperty);
            GUIContent label2 = ((!string.IsNullOrEmpty(label)) ? new GUIContent(label) : null);
            if (serializedProperty.animationCurveValue != null && serializedProperty.animationCurveValue.length > 0 && !serializedProperty.hasMultipleDifferentValues)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property2, label2);
                if (GUILayout.Button("X", GUILayout.Width(32f)))
                {
                    serializedProperty.animationCurveValue = new AnimationCurve();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.BeginChangeCheck();
                AnimationCurve animationCurveValue = EditorGUILayout.CurveField(" ", serializedProperty.animationCurveValue, Color.cyan, new Rect(0f, 0f, 1f, 1f));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedProperty.animationCurveValue = animationCurveValue;
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property2, label2);
                if (GUILayout.Button(serializedProperty.hasMultipleDifferentValues ? "-" : "C", GUILayout.Width(32f)))
                {
                    AnimationCurve animationCurve3 = new AnimationCurve();
                    animationCurve3.AddKey(new Keyframe(0f, 1f));
                    animationCurve3.AddKey(new Keyframe(1f, 1f));
                    serializedProperty.animationCurveValue = animationCurve3;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawCurveParam(string property, string curveProperty, int index)
        {
            DrawCurveParam(null, property, curveProperty, index);
        }

        private void DrawCurveParam(string label, string property, string labelX, string curvePropertyX, string labelY, string curvePropertyY, string labelZ, string curvePropertyZ, int index)
        {
            SerializedProperty bone = boneList.GetArrayElementAtIndex(index);
            SerializedProperty boneSettings = bone.FindPropertyRelative("boneSettings");

            SerializedProperty serializedProperty = boneSettings.FindPropertyRelative(property);
            SerializedProperty serializedProperty2 = boneSettings.FindPropertyRelative(curvePropertyX);
            SerializedProperty serializedProperty3 = boneSettings.FindPropertyRelative(curvePropertyY);
            SerializedProperty serializedProperty4 = boneSettings.FindPropertyRelative(curvePropertyZ);
            if ((serializedProperty2.animationCurveValue != null && serializedProperty2.animationCurveValue.length > 0) || (serializedProperty3.animationCurveValue != null && serializedProperty3.animationCurveValue.length > 0) || (serializedProperty4.animationCurveValue != null && serializedProperty4.animationCurveValue.length > 0))
            {
                EditorGUILayout.BeginHorizontal();
                DrawVector3(label, serializedProperty, labelX, labelY, labelZ);
                EditorGUILayout.LabelField("", GUILayout.Width(32f));
                EditorGUILayout.EndHorizontal();
                _ = serializedProperty.vector3Value;
                EditorGUI.indentLevel++;
                DrawCurve(labelX, serializedProperty2);
                DrawCurve(labelY, serializedProperty3);
                DrawCurve(labelZ, serializedProperty4);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawVector3(label, serializedProperty, labelX, labelY, labelZ);
                if (GUILayout.Button("C", GUILayout.Width(32f)))
                {
                    AnimationCurve animationCurve = new AnimationCurve();
                    animationCurve.AddKey(new Keyframe(0f, 1f));
                    animationCurve.AddKey(new Keyframe(1f, 1f));
                    serializedProperty2.animationCurveValue = animationCurve;
                    serializedProperty3.animationCurveValue = animationCurve;
                    serializedProperty4.animationCurveValue = animationCurve;
                }

                EditorGUILayout.EndHorizontal();
            }

            static void DrawCurve(string curveLabel, SerializedProperty curveParam)
            {
                bool num = curveParam.animationCurveValue != null && curveParam.animationCurveValue.length > 0;
                EditorGUILayout.BeginHorizontal();
                if (num)
                {
                    EditorGUI.BeginChangeCheck();
                    AnimationCurve animationCurveValue = EditorGUILayout.CurveField(curveLabel, curveParam.animationCurveValue, Color.cyan, new Rect(0f, -1f, 1f, 2f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        curveParam.animationCurveValue = animationCurveValue;
                    }

                    if (GUILayout.Button(curveParam.hasMultipleDifferentValues ? "-" : "X", GUILayout.Width(32f)))
                    {
                        curveParam.animationCurveValue = new AnimationCurve();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(curveLabel);
                    if (GUILayout.Button(curveParam.hasMultipleDifferentValues ? "-" : "C", GUILayout.Width(32f)))
                    {
                        AnimationCurve animationCurve2 = new AnimationCurve();
                        animationCurve2.AddKey(new Keyframe(0f, 1f));
                        animationCurve2.AddKey(new Keyframe(1f, 1f));
                        curveParam.animationCurveValue = animationCurve2;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawVector3(string label, SerializedProperty property, string labelX, string labelY, string labelZ)
        {
            Vector3 vector3Value = property.vector3Value;
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUILayout.BeginVertical();
            }

            EditorGUILayout.PrefixLabel(label);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUILayout.LabelField("", GUILayout.Width(32f));
            }

            DrawField(labelX, ref vector3Value.x);
            DrawField(labelY, ref vector3Value.y);
            DrawField(labelZ, ref vector3Value.z);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.indentLevel = indentLevel;
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                property.vector3Value = vector3Value;
            }

            if (!EditorGUIUtility.wideMode)
            {
                EditorGUILayout.EndVertical();
            }

            static void DrawField(string fieldLabel, ref float value)
            {
                GUIContent gUIContent = new GUIContent(fieldLabel);
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(gUIContent).x;
                value = EditorGUILayout.FloatField(gUIContent, value, GUILayout.MinWidth(32f));
            }
        }

        private void DrawPermission(string mainProperty, string filterProperty, int index)
        {
            SerializedProperty bone = boneList.GetArrayElementAtIndex(index);
            SerializedProperty boneSettings = bone.FindPropertyRelative("boneSettings");

            SerializedProperty serializedProperty = boneSettings.FindPropertyRelative(mainProperty);
            SerializedProperty serializedProperty2 = boneSettings.FindPropertyRelative(filterProperty);
            SerializedProperty property = serializedProperty2.FindPropertyRelative("allowSelf");
            SerializedProperty property2 = serializedProperty2.FindPropertyRelative("allowOthers");
            EditorGUILayout.PropertyField(serializedProperty);
            if (serializedProperty.enumValueIndex == 2)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property);
                EditorGUILayout.PropertyField(property2);
                EditorGUI.indentLevel--;
            }
        }

    }
}
