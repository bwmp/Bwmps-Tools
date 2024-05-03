using BwmpsTools.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace BwmpsTools.Tools
{
    public class TwistBoneHelper : EditorWindow
    {
        List<Transform> TwistBones = new List<Transform>();
        List<Transform> bones = new List<Transform>();
        private static readonly string Title = "Twist Bone Helper";
        private static readonly string Description = "Setup and configure twist bones in one click";

        public void Init()
        {
            TwistBoneHelper window = (TwistBoneHelper)GetWindow(typeof(TwistBoneHelper), false, Title, true);
            window.titleContent = new GUIContent(Title);
            window.Show();
            TwistBones = BasicUtils.TargetModel.transform.FindDescendantsByPartialNames(new List<string>() { "twist" });
            bones = new List<Transform>(TwistBones);

            for (int i = 0; i < TwistBones.Count; i++)
            {
                bones[i] = TwistBones[i].parent.Cast<Transform>().FirstOrDefault(child => child != TwistBones[i]);
            }
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {
            EditorGUIExtensions.TitleBox(Title, Description);
            using EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(scrollPos, new GUIStyle() { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(10, 10, 10, 10) });
            scrollPos = scroll.scrollPosition;
            using (new GUILayout.VerticalScope(CustomGUIStyles.group))
            {
                GUILayout.Label("Twist Bones:");

                if (BasicUtils.TargetModel == null)
                {
                    GUILayout.Label("No target model selected");
                    return;
                }

                for (int i = 0; i < TwistBones.Count; i++)
                {
                    Transform bone = bones[i];

                    GUILayout.BeginHorizontal();

                    RotationConstraint twistConstraint = TwistBones[i].gameObject.GetComponent<RotationConstraint>();
                    bool hasRotationConstraint = twistConstraint != null;

                    bool settingsMatch = hasRotationConstraint
                        && twistConstraint.rotationAxis == Axis.Y
                        && twistConstraint.locked
                        && twistConstraint.constraintActive
                        && ContainsSource(twistConstraint, bone);

                    GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = settingsMatch ? Color.green : (hasRotationConstraint ? Color.yellow : Color.red) } };
                    GUILayout.Label(TwistBones[i].name, labelStyle, GUILayout.Width(100));
                    bone = EditorGUILayout.ObjectField(bone, typeof(Transform), true, GUILayout.ExpandWidth(true)) as Transform;

                    if (GUILayout.Button("Add", GUILayout.Width(50)))
                    {
                        RotationConstraint rotationConstraint = TwistBones[i].gameObject.GetOrAddComponent<RotationConstraint>();
                        rotationConstraint.rotationAxis = Axis.Y;
                        rotationConstraint.locked = true;
                        rotationConstraint.constraintActive = true;
                        AddSourceIfNotExists(rotationConstraint, bone, 1);

                        Undo.RegisterCompleteObjectUndo(rotationConstraint, "Add Twist Bone Constraint");
                    }
                    if (GUILayout.Button("Remove", GUILayout.Width(75)))
                    {

                        if (hasRotationConstraint)
                        {
                            Undo.DestroyObjectImmediate(twistConstraint);
                        }
                        else
                        {
                            Debug.Log("Twist bone does not have a RotationConstraint");
                        }
                    }
                    GUILayout.EndHorizontal();
                    bones[i] = bone;
                }

                GUILayout.Space(20);
                GUILayout.Label("Not setup", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
                GUILayout.Label("Setup", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } });
                GUILayout.Label("Setup, but settings don't match", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } });
            }
        }

        bool ContainsSource(RotationConstraint constraint, Transform source)
        {
            for (int i = 0; i < constraint.sourceCount; i++)
            {
                if (constraint.GetSource(i).sourceTransform == source) return true;
            }
            return false;
        }

        void AddSourceIfNotExists(RotationConstraint rotationConstraint, Transform sourceTransform, float weight)
        {
            for (int i = 0; i < rotationConstraint.sourceCount; i++)
            {
                if (rotationConstraint.GetSource(i).sourceTransform == sourceTransform)
                {
                    return;
                }
            }
            rotationConstraint.AddSource(new ConstraintSource { sourceTransform = sourceTransform, weight = weight });
        }

        private static TwistBoneHelper instance;

        public static TwistBoneHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<TwistBoneHelper>();
                }
                return instance;
            }
        }
    }
}