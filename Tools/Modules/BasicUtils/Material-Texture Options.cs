using BwmpsTools.Tools.Extension_Windows;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static BwmpsTools.Tools.BasicUtils;

namespace BwmpsTools.Tools.Modules
{
    internal class MaterialTextureOptions : BaseModule
    {
        private Shader newShader;
        private static Dictionary<string, Material> copiedMaterials = new Dictionary<string, Material>();

        public Shader NewShader { get => newShader; set => newShader = value; }
        public static Dictionary<string, Material> CopiedMaterials { get => copiedMaterials; set => copiedMaterials = value; }

        public MaterialTextureOptions() : base("Material/Texture Options")
        {
        }

        public override void Render(Action renderContent = null)
        {
            base.Render(() =>
            {
                NewShader = EditorGUILayout.ObjectField("New Shader", NewShader, typeof(Shader), true) as Shader;

                EditorGUI.BeginDisabledGroup(TargetModel == null || NewShader == null);
                if (GUILayout.Button(new GUIContent("Copy Materials and Change Shader", "Requires new shader to be set")))
                {
                    if (NewShader != null)
                    {
                        string folderName = EditorUtility.OpenFolderPanel("Select Destination Folder", Application.dataPath, "");
                        if (!string.IsNullOrEmpty(folderName))
                        {
                            string assetsFolderPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
                            string relativeFolderPath = folderName.Replace(assetsFolderPath, "");

                            if (AssetDatabase.IsValidFolder(relativeFolderPath))
                            {
                                Undo.RegisterFullObjectHierarchyUndo(TargetModel, "Copy Materials and Change Shader");

                                CopiedMaterials.Clear();
                                CreateNewMaterialCopies(TargetModel.transform, relativeFolderPath, NewShader);
                            }
                            else
                            {
                                Debug.LogError("Invalid destination folder selected: " + relativeFolderPath);
                            }
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(TargetModel == null);
                if (GUILayout.Button("Get Texture List")) TextureTools.Instance.Init();
                if (GUILayout.Button("List Materials and Shaders")) MaterialShaderList.Instance.Init();
                EditorGUI.EndDisabledGroup();
                renderContent?.Invoke();
            });
        }

        private void CreateNewMaterialCopies(Transform transform, string folderPath, Shader shader)
        {
            if (transform.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
            {
                Material[] materials = renderer.sharedMaterials;
                Material[] newMaterials = new Material[materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!CopiedMaterials.TryGetValue(materials[i].name, out Material newMaterial))
                    {
                        newMaterial = new Material(shader)
                        {
                            shader = shader,
                            name = materials[i].name + "_Copy"
                        };

                        Material oldMat = materials[i];
                        if (oldMat == null) continue;
                        MaterialProperty[] oldProperties = MaterialEditor.GetMaterialProperties(new Material[] { oldMat });
                        MaterialProperty[] newProperties = MaterialEditor.GetMaterialProperties(new Material[] { newMaterial });

                        foreach (MaterialProperty oldProp in oldProperties)
                        {
                            // Check if the property exists in the new material
                            MaterialProperty newProp = Array.Find(newProperties, prop => prop.name == oldProp.name && prop.type == oldProp.type);

                            // If the property exists in both materials, copy its value from the old material to the new material
                            if (newProp != null)
                            {
                                switch (oldProp.type)
                                {
                                    case MaterialProperty.PropType.Color:
                                        newMaterial.SetColor(oldProp.name, oldMat.GetColor(oldProp.name));
                                        break;
                                    case MaterialProperty.PropType.Float:
                                    case MaterialProperty.PropType.Range:
                                        newMaterial.SetFloat(oldProp.name, oldMat.GetFloat(oldProp.name));
                                        break;
                                    case MaterialProperty.PropType.Texture:
                                        newMaterial.SetTexture(oldProp.name, oldMat.GetTexture(oldProp.name));
                                        break;
                                    case MaterialProperty.PropType.Vector:
                                        newMaterial.SetVector(oldProp.name, oldMat.GetVector(oldProp.name));
                                        break;
                                }
                            }
                        }

                        string materialPath = folderPath + "/" + newMaterial.name + ".mat";
                        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(materialPath);
                        AssetDatabase.CreateAsset(newMaterial, uniquePath);
                        Debug.Log("Material copy created at: " + uniquePath);
                        CopiedMaterials[materials[i].name] = newMaterial;
                    }
                    newMaterials[i] = newMaterial;
                }
                renderer.sharedMaterials = newMaterials;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                CreateNewMaterialCopies(transform.GetChild(i).transform, folderPath, shader);
            }
        }

    }
}
