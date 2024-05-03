using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace BwmpsTools.Utils
{
    public static class ExpressionUtils
    {
        public static AnimatorControllerLayer CreateControllerStateLayer(
            AnimatorController controller,
            string layerName)
        {
            var controllerStateLayer = new AnimatorControllerLayer
            {
                name = layerName.Replace(".", "_"),
                defaultWeight = 1f
            };

            var animatorStateMachine = new AnimatorStateMachine
            {
                name = controllerStateLayer.name,
                hideFlags = HideFlags.HideInHierarchy
            };

            controllerStateLayer.stateMachine = animatorStateMachine;

            var controllerAssetPath = AssetDatabase.GetAssetPath(controller);
            if (!string.IsNullOrEmpty(controllerAssetPath))
            {
                AssetDatabase.AddObjectToAsset(animatorStateMachine, controllerAssetPath);
                AssetDatabase.ImportAsset(controllerAssetPath);
            }

            controller.AddLayer(controllerStateLayer);

            return controllerStateLayer;
        }

        public static void CreateNewParameter(
            VRCExpressionParameters expressionParameters,
            string parameterName,
            float defaultValue,
            bool saveState,
            VRCExpressionParameters.ValueType valueType = VRCExpressionParameters.ValueType.Int)
        {
            SerializedObject serializedObject = new SerializedObject(expressionParameters);
            SerializedProperty property = serializedObject.FindProperty("parameters");
            int arraySize = property.arraySize;

            property.InsertArrayElementAtIndex(arraySize);

            SerializedProperty newParameter = property.GetArrayElementAtIndex(arraySize);
            newParameter.FindPropertyRelative("name").stringValue = parameterName;
            newParameter.FindPropertyRelative(nameof(valueType)).intValue = (int)valueType;
            newParameter.FindPropertyRelative(nameof(defaultValue)).floatValue = defaultValue;
            newParameter.FindPropertyRelative("saved").boolValue = saveState;
            newParameter.FindPropertyRelative("networkSynced").boolValue = true;
            serializedObject.ApplyModifiedProperties();
        }

        public static void CreateAndFillStateLayer(
            AnimatorController controller,
            string parameterName,
            AnimationClip onAnimationClip,
            AnimationClip offAnimationClip)
        {
            AnimatorStateMachine stateMachine = CreateControllerStateLayer(controller, parameterName).stateMachine;
            Vector3 offset = new Vector3(250f, 0.0f, 0.0f);
            Vector3 entryStatePosition = stateMachine.entryPosition + offset;

            AnimatorState animatorStateOff = stateMachine.AddState("Off", entryStatePosition);
            AnimatorState animatorStateOn = stateMachine.AddState("On", entryStatePosition + offset);

            AnimatorStateTransition OffCondition = animatorStateOff.AddTransition(animatorStateOn);
            OffCondition.AddCondition(AnimatorConditionMode.If, 0.0f, parameterName);
            OffCondition.hasExitTime = false;
            OffCondition.duration = 0.0f;
            animatorStateOff.motion = offAnimationClip;
            animatorStateOff.writeDefaultValues = false;


            AnimatorStateTransition OnCondition = animatorStateOn.AddTransition(animatorStateOff);
            OnCondition.AddCondition(AnimatorConditionMode.IfNot, 0.0f, parameterName);
            OnCondition.hasExitTime = false;
            OnCondition.duration = 0.0f;
            animatorStateOn.motion = onAnimationClip;
            animatorStateOn.writeDefaultValues = false;
            
        }

        public static void CreateAndFillStateLayerBlendtree(
          AnimatorController controller,
          string parameterName,
          AnimationClip onAnimationClip,
          AnimationClip offAnimationClip)
        {
            CreateControllerStateLayer(controller, parameterName);

            AnimatorState treeInController = controller.CreateBlendTreeInController("Blend Tree", out BlendTree blendTree, controller.layers.Length - 1);
            blendTree.hideFlags = HideFlags.HideAndDontSave;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = parameterName;
            blendTree.AddChild(offAnimationClip, 0.0f);
            blendTree.AddChild(onAnimationClip, 1f);

            treeInController.writeDefaultValues = false;
        }

        public static void CreateAndFillStateLayer(
            AnimatorController controller,
            string parameterName,
            AnimationClip defaultAnimationClip,
            List<AnimationClip> animationClips)
        {
            AnimatorStateMachine stateMachine = CreateControllerStateLayer(controller, parameterName).stateMachine;
            Vector3 offset = new Vector3(250f, 0.0f, 0.0f);
            Vector3 entryStatePosition = stateMachine.entryPosition + offset;

            AnimatorState animatorStateDefault = stateMachine.AddState("Default", entryStatePosition);
            animatorStateDefault.motion = defaultAnimationClip;
            animatorStateDefault.writeDefaultValues = false;

            int middleIndex = animationClips.Count / 2; // Calculate the middle index
            Vector3 clipPosition = entryStatePosition + new Vector3(250f, 50f * middleIndex, 0f);

            for (int i = 0; i < animationClips.Count; i++)
            {
                clipPosition = entryStatePosition + new Vector3(250f, 50f * (i - middleIndex), 0f);

                AnimatorState animatorStateClip = stateMachine.AddState("Clip" + i, clipPosition);
                animatorStateClip.motion = animationClips[i];
                animatorStateClip.writeDefaultValues = false;

                AnimatorStateTransition toClipTransition = animatorStateDefault.AddTransition(animatorStateClip);
                toClipTransition.AddCondition(AnimatorConditionMode.Equals, i + 1, parameterName);
                toClipTransition.hasExitTime = false;
                toClipTransition.duration = 0.0f;

                AnimatorStateTransition toDefaultTransition = animatorStateClip.AddTransition(animatorStateDefault);
                toDefaultTransition.AddCondition(AnimatorConditionMode.NotEqual, i + 1, parameterName);
                toDefaultTransition.hasExitTime = false;
                toDefaultTransition.duration = 0.0f;
            }
        }


        public static void CreateAndFillStateLayerBlendtree(
          AnimatorController controller,
          string parameterName,
          List<AnimationClip> animationClips,
          bool padClips = false)
        {
            CreateControllerStateLayer(controller, parameterName);

            AnimatorState treeInController = controller.CreateBlendTreeInController("Blend Tree", out BlendTree blendTree, controller.layers.Length - 1);
            blendTree.hideFlags = HideFlags.HideAndDontSave;
            blendTree.blendType = BlendTreeType.Simple1D;
            blendTree.blendParameter = parameterName;
            blendTree.useAutomaticThresholds = true;
            for (int i = 0; i < animationClips.Count; i++)
            {
                if(padClips && i != 0 && i != animationClips.Count - 1) blendTree.AddChild(animationClips[i]);
                blendTree.AddChild(animationClips[i]);
            }

            treeInController.writeDefaultValues = false;
        }

        public static VRCExpressionsMenu CreateNewSubMenu(
            VRCExpressionsMenu parentMenu,
            string subMenuName,
            string fileName,
            string folder)
        {
            VRCExpressionsMenu instance = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder.TrimEnd('/')}/{fileName.Replace(" ", "_")}.asset");
            AssetDatabase.CreateAsset(instance, uniqueAssetPath);
            parentMenu.controls.Add(new VRCExpressionsMenu.Control()
            {
                name = subMenuName
            });
            SerializedObject serializedObject = new SerializedObject(parentMenu);
            SerializedProperty property = serializedObject.FindProperty("controls");
            SerializedProperty arrayElementAtIndex = property.GetArrayElementAtIndex(property.arraySize - 1);
            arrayElementAtIndex.FindPropertyRelative("name").stringValue = subMenuName;
            arrayElementAtIndex.FindPropertyRelative("type").intValue = 103;
            arrayElementAtIndex.FindPropertyRelative("parameter").FindPropertyRelative("name").stringValue = "";
            arrayElementAtIndex.FindPropertyRelative("subMenu").objectReferenceValue = (Object)instance;
            serializedObject.ApplyModifiedProperties();
            return instance;
        }

        public static void AddControlToMenu(
            VRCExpressionsMenu parentMenu,
            string menuLabel,
            string parameterName,
            float? parameterValue = null,
            Texture2D icon = null,
            VRCExpressionsMenu.Control.ControlType controlType = VRCExpressionsMenu.Control.ControlType.Button)
        {
            var newControl = new VRCExpressionsMenu.Control
            {
                name = menuLabel,
                type = controlType,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = parameterName },
            };

            if (icon != null)
            {
                newControl.icon = icon;
            }

            if (parameterValue.HasValue)
            {
                newControl.value = parameterValue.Value;
            }

            parentMenu.controls.Add(newControl);
        }

        public static void AddRadialControlToMenu(
            VRCExpressionsMenu mainMenu,
            string menuLabel,
            string parameterName,
            Texture2D icon = null)
        {
            AddControlToMenu(mainMenu, menuLabel, "", null, icon, VRCExpressionsMenu.Control.ControlType.RadialPuppet);

            SerializedObject serializedObject = new SerializedObject(mainMenu);
            SerializedProperty controlsProperty = serializedObject.FindProperty("controls");
            int lastControlIndex = controlsProperty.arraySize - 1;
            SerializedProperty subParametersProperty = controlsProperty.GetArrayElementAtIndex(lastControlIndex).FindPropertyRelative("subParameters");
            subParametersProperty.arraySize = 1;
            SerializedProperty parameterProperty = subParametersProperty.GetArrayElementAtIndex(0);
            parameterProperty.FindPropertyRelative("name").stringValue = parameterName;

            serializedObject.ApplyModifiedProperties();
        }


        public static AnimationClip CreateNewAnimationClip(GameObject targetObject, string parameterName, string folder, bool onState)
        {
            string transformPath = AnimationUtility.CalculateTransformPath(targetObject.transform, targetObject.transform.root.gameObject.transform);
            string clipName = $"{parameterName}{(onState ? "On" : "Off")}.anim";

            AnimationClip newAnimationClip = new AnimationClip();
            AnimationCurve animationCurve = AnimationCurve.Constant(0.0f, newAnimationClip.length, onState ? 1f : 0.0f);
            animationCurve.RemoveKey(1);
            newAnimationClip.SetCurve(transformPath, typeof(GameObject), "m_IsActive", animationCurve);

            string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder.TrimEnd('/')}/{clipName}");
            AssetDatabase.CreateAsset(newAnimationClip, uniqueAssetPath);

            return newAnimationClip;
        }
    }
}
