using BwmpsTools.Data.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using static BwmpsTools.Utils.Structs;
using AnimatorController = UnityEditor.Animations.AnimatorController;

namespace BwmpsTools.Utils
{
    public static class Utilities
    {
        public static readonly string Version = "1.1.8";

        public static UnityEngine.Object LoadBwmpPrefab(string assetName)
        {
            string scriptPath = GetAssetPath("Prefabs", assetName);
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
        }
        
        public static UnityEngine.Object LoadBwmpPreset(string assetName)
        {
            string scriptPath = GetAssetPath("Presets", assetName);
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath);
        }

        public static List<PhysboneSetting> LoadAllPhysboneSettings()
        {
            string folderPath = GetAssetPath("Presets", "Physbones"); // Modify the folder path
            string[] assetGUIDs = AssetDatabase.FindAssets("t:PhysboneSetting", new[] { folderPath });

            List<PhysboneSetting> assets = new List<PhysboneSetting>();
            
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                PhysboneSetting asset = AssetDatabase.LoadAssetAtPath<PhysboneSetting>(assetPath);
                assets.Add(asset);
                Debug.Log(asset.name);
            }

            return assets;
        }

        public static string GetAssetPath(string subfolder, string assetName)
        {
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(ScriptableObject.CreateInstance(typeof(Tools.BwmpsTools))));
            string scriptFolder = Path.GetDirectoryName(scriptPath);
            return Path.Combine(scriptFolder, subfolder, assetName);
        }

        public static VRCAvatar GetAvatarInfo(VRCAvatarDescriptor avatar)
        {
            var avatarInfo = new VRCAvatar
            {
                Avatar = avatar.transform,
                Animator = avatar.GetComponent<Animator>(),
                Menu = avatar.expressionsMenu,
                Parameters = avatar.expressionParameters,
                AvatarDescriptor = avatar
            };

            if (avatarInfo.AvatarDescriptor.baseAnimationLayers.Length >= 5 &&
                avatarInfo.AvatarDescriptor.baseAnimationLayers[4].animatorController != null)
            {
                avatarInfo.Controller = (AnimatorController)avatarInfo.AvatarDescriptor.baseAnimationLayers[4].animatorController;
            }
            else
            {
                Debug.LogError("Avatar controller missing");
            }

            if (avatarInfo.Menu == null)
            {
                Debug.LogError("Main expression menu missing");
            }

            if (avatarInfo.Parameters == null)
            {
                Debug.LogError("Expression parameters missing");
            }

            return avatarInfo;
        }

        public static string GetGameObjectsPath(Transform transform)
        {
            var path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (transform.parent != null)
                {
                    path = transform.name + "/" + path;
                }
            }
            return path;
        }

        public static List<GameObject> GetDescendants(GameObject parent)
        {
            var descendants = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                descendants.Add(child.gameObject);
                descendants.AddRange(GetDescendants(child.gameObject));
            }
            return descendants;
        }

        public static Vector3 GetPositionBetweenTwoPoints(Vector3 pos1, Vector3 pos2)
        {
            return (pos1 + pos2) / 2;
        }

        public static void CopyComponentsByName(Transform parentTransform, Transform otherTransform)
        {
            Undo.IncrementCurrentGroup();

            foreach (Transform parentChild in parentTransform)
            {
                Transform transform = otherTransform.FindDescendant(parentChild.name);
                if (transform != null)
                {
                    Component[] components = transform.GetComponents<Component>();
                    foreach (Component comp in components)
                    {
                        Type type = comp.GetType();
                        if (!type.IsSubclassOf(typeof(Transform)) && parentChild.GetComponent(type) == null)
                        {
                            GameObject copiedObject = parentChild.gameObject;
                            Component copiedComponent = comp;

                            Undo.RegisterCompleteObjectUndo(copiedObject, "Copy Component");

                            ComponentUtility.CopyComponent(copiedComponent);

                            Undo.RegisterCreatedObjectUndo(copiedObject, "Paste Component");
                            ComponentUtility.PasteComponentAsNew(copiedObject);
                        }
                    }
                }

                CopyComponentsByName(parentChild, otherTransform);
            }
        }

        public static List<T> GetAllComponentsInChildren<T>(GameObject gameObject, bool includeInactive = false) where T : Component
        {
            List<T> components = new List<T>();

            // Get all components of type T attached to the GameObject itself
            T[] componentsOnGameObject = gameObject.GetComponents<T>();
            components.AddRange(componentsOnGameObject);

            // Get all components of type T attached to descendants of the GameObject
            Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>(includeInactive);
            foreach (Transform childTransform in childTransforms)
            {
                T[] componentsOnChild = childTransform.GetComponents<T>();
                components.AddRange(componentsOnChild);
            }

            return components;
        }

        public static string GetAssetFolder(int asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string folderPath = Path.GetDirectoryName(assetPath);
            int index = folderPath.IndexOf("Assets");
            folderPath = folderPath.Substring(index);
            return folderPath;
        }

        public static string GetBwmpAssetPath(string asset)
        {
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(ScriptableObject.CreateInstance(typeof(Tools.BwmpsTools))));
            string scriptFolder = Path.GetDirectoryName(scriptPath);
            return Path.Combine(scriptFolder, asset);
        }

        public static void DebugLog(string message)
        {
            if (SettingsManager.Instance.DebugMode)
            {
                Debug.Log(message);
            }
        }
    }
}