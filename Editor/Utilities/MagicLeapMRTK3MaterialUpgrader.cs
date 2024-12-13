// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if UNITY_RENDER_PIPELINES_CORE

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using MagicLeap.MRTK.Settings;
using static MagicLeap.MRTK.Settings.MagicLeapMRTK3SettingsUnityEditor;
using System.Linq;

using UnityEngine.Rendering;
using UnityEditor.Rendering;

#if UNITY_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;
using UnityEditor.Rendering.Universal;
#endif // UNITY_RENDER_PIPELINES_UNIVERSAL

#if UNITY_RENDER_PIPELINES_HIGH_DEFINITION
using UnityEngine.Rendering.HighDefinition;
using UnityEditor.Rendering.HighDefinition;
#endif // UNITY_RENDER_PIPELINES_HIGH_DEFINITION


namespace MagicLeap.MRTK.Utilities
{
    /// <summary>
    /// Provides utilities to convert package materials to the URP or HDRP rendering pipelines for convenience.
    /// </summary>
    /// <remarks>
    /// These utilities offer both automated and manual options to convert materials within the package,
    /// as well as materials within imported package samples, to the current rendering pipeline.
    /// </remarks>
    internal class MagicLeapMRTK3MaterialUpgrader : AssetPostprocessor
    {
        private const string ConvertMaterialToHDRPMenuItemText = "Convert Selected Built-in Materials to HDRP";

        private const string StandardShaderName = "Standard";
        private const string StandardSpecularShaderName = "Standard (Specular setup)";

        private static readonly List<string> MagicLeapMRTK3Paths = new() 
        {
            "Packages/com.magicleap.mrtk3",
            "Assets/Samples/Magic Leap MRTK3"
        };

        private static bool PackageMaterialConversionCheckNeeded = false;
        private static bool PackageMaterialConversionInProgress = false;

        private enum UnityRenderPipeline
        {
            Default,
            URP,
            HDRP
        }

        /// <summary>
        /// Dictionary of supported material upgraders per rendering pipeline and per original shader.
        /// </summary>
        private static readonly Dictionary<UnityRenderPipeline, Dictionary<string, Lazy<MaterialUpgrader>>> SupportedMaterialUpgraders = new()
        {
#if UNITY_RENDER_PIPELINES_UNIVERSAL
            { UnityRenderPipeline.URP, new()
                {
                    {  StandardShaderName,         new(() => { return new StandardUpgrader(StandardShaderName); }) },
                    {  StandardSpecularShaderName, new(() => { return new StandardUpgrader(StandardSpecularShaderName); }) },
                }
            },
#endif // UNITY_RENDER_PIPELINES_UNIVERSAL
#if UNITY_RENDER_PIPELINES_HIGH_DEFINITION
            { UnityRenderPipeline.HDRP, new()
                {
                    // Currently, the HDRP StandardsToHDLitMaterialUpgrader does not have public access,
                    // so opting to invoke the HDRP Material Conversion MenuItem instead, which will
                    // cause another dialog prompt before proceeding.
                    // Creating instances within this dictionary to indicate supported shader conversion to HDRP in the material check.
                    {  StandardShaderName,         new(() => { return null; }) },
                    {  StandardSpecularShaderName, new(() => { return null; }) },
                }
            },
#endif // UNITY_RENDER_PIPELINES_HIGH_DEFINITION
        };

        /// <summary>
        /// Detect the current rendering pipeline for the project.
        /// </summary>
        private static UnityRenderPipeline CurrentRenderPipeline
        {
            get
            {
#if UNITY_RENDER_PIPELINES_UNIVERSAL
                if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)
                {
                    return UnityRenderPipeline.URP;
                }
#endif // UNITY_RENDER_PIPELINES_UNIVERSAL

#if UNITY_RENDER_PIPELINES_HIGH_DEFINITION
                if (GraphicsSettings.defaultRenderPipeline is HDRenderPipelineAsset)
                {
                    return UnityRenderPipeline.HDRP;
                }
#endif // UNITY_RENDER_PIPELINES_HIGH_DEFINITION

                return UnityRenderPipeline.Default;
            }
        }

        /// <summary>
        /// The Magic Leap MRTK3 Setting of how to handle automatic package material conversion.
        /// </summary>
        private static ConvertPackageMaterialsOption ConvertPackageMaterialSetting =>
            MagicLeapMRTK3Settings.Instance.GetSettingsObject<MagicLeapMRTK3SettingsUnityEditor>().ConvertPackageMaterials;


        // Menu Items to manually convert materials

#if UNITY_RENDER_PIPELINES_UNIVERSAL
        [MenuItem("Magic Leap/MRTK3/Convert package materials to URP.")]
        private static void ConvertPackageMaterialsToURP()
        {
            var pipeline = UnityRenderPipeline.URP;
            ConvertMaterials(GetPackageMaterialsToConvert(pipeline, true), pipeline);
        }

        [MenuItem("Magic Leap/MRTK3/Convert package materials to URP.", true)]
        private static bool ValidateConvertPackageMaterialsToURP()
        {
            return CurrentRenderPipeline == UnityRenderPipeline.URP;
        }
#endif // UNITY_RENDER_PIPELINES_UNIVERSAL

#if UNITY_RENDER_PIPELINES_HIGH_DEFINITION
        [MenuItem("Magic Leap/MRTK3/Convert package materials to HDRP.")]
        private static void ConvertPackageMaterialsToHDRP()
        {
            var pipeline = UnityRenderPipeline.HDRP;
            ConvertMaterials(GetPackageMaterialsToConvert(pipeline, true), pipeline);
        }

        [MenuItem("Magic Leap/MRTK3/Convert package materials to HDRP.", true)]
        private static bool ValidateConvertPackageMaterialsToHDRP()
        {
            return CurrentRenderPipeline == UnityRenderPipeline.HDRP;
        }
#endif // UNITY_RENDER_PIPELINES_HIGH_DEFINITION


        /// <summary>
        /// Handles when any assets have been post-processed within the project and is used to detect if any
        /// Magic Leap MRTK3 package materials might need conversion to the current rendering pipeline.
        /// </summary>
        /// <remarks>
        /// See Project Settings > MRTK3 > Magic Leap Settings for persistent conversion options.
        /// </remarks>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // No need to proceed if not doing automatic conversion or the default, built-in render pipeline is active.
            if (ConvertPackageMaterialSetting == ConvertPackageMaterialsOption.Never ||
                CurrentRenderPipeline == UnityRenderPipeline.Default)
            {
                return;
            }

            if (!PackageMaterialConversionCheckNeeded)
            {
                PackageMaterialConversionCheckNeeded = AssetChangesDetectedInPaths(importedAssets, MagicLeapMRTK3Paths) ||
                                                       AssetChangesDetectedInPaths(deletedAssets, MagicLeapMRTK3Paths) ||
                                                       AssetChangesDetectedInPaths(movedAssets, MagicLeapMRTK3Paths);
            }

            static void ResetAssetProcessingStatus()
            {
                PackageMaterialConversionCheckNeeded = false;
                PackageMaterialConversionInProgress = false;
            }

            if (PackageMaterialConversionCheckNeeded && !PackageMaterialConversionInProgress)
            {
                PackageMaterialConversionInProgress = true;

                // Wait to handle detecting materials needing conversion after current Editor processing
                EditorApplication.delayCall += () =>
                {
                    var currentRenderPipeline = CurrentRenderPipeline;

                    List<Material> materialsToConvert = GetPackageMaterialsToConvert(currentRenderPipeline);

                    if (materialsToConvert.Count == 0)
                    {
                        ResetAssetProcessingStatus();
                        return;
                    }

                    if (ConvertPackageMaterialSetting == ConvertPackageMaterialsOption.AlwaysNoPrompt ||
                        EditorUtility.DisplayDialog(
                            "Magic Leap MRTK3 Package Material Conversion",
                            $"The Magic Leap MRTK3 package contains {materialsToConvert.Count} Materials that need conversion to {currentRenderPipeline}.  Proceed with conversion?\n\n" +
                            "Please see Project\u00A0Settings > MRTK3 > Magic\u00A0Leap\u00A0Settings for persistent conversion options.",
                            "Yes",
                            "No"))
                    {
                        ConvertMaterials(materialsToConvert, currentRenderPipeline);
                    }

                    ResetAssetProcessingStatus();
                };
            }
        }

        /// <summary>
        /// Convert the list of materials to a rendering pipeline.
        /// </summary>
        private static void ConvertMaterials(List<Material> materials, UnityRenderPipeline renderPipeline)
        {
            if (materials.Count == 0)
            {
                return;
            }

            Debug.Log($"Magic Leap MRTK3 Package Material Conversion to {renderPipeline}: Attempting to convert {materials.Count} Materials");

            switch (renderPipeline)
            {
#if UNITY_RENDER_PIPELINES_HIGH_DEFINITION
                // Special handling of HDRP conversion to call a MenuItem since HDRP material upgraders are not currently public.
                // This technique does invoke a confirmation dialog from the MenuItem.
                case UnityRenderPipeline.HDRP:
                    if (TryFindMenuItemPath(ConvertMaterialToHDRPMenuItemText, out string convertMaterialsToHDRPMenuItemPath))
                    {
                        var originalSelection = Selection.objects;
                        Selection.objects = materials.ToArray();

                        EditorApplication.ExecuteMenuItem(convertMaterialsToHDRPMenuItemPath);

                        Selection.objects = originalSelection;
                    }
                    else
                    {
                        Debug.LogWarning($"Magic Leap MRTK3 Package Material Conversion to {renderPipeline}: Unable to find HDRP Material Conversion MenuItem, no Materials converted.");
                    }
                    break;
#endif // UNITY_RENDER_PIPELINES_HIGH_DEFINITION
                default:
                    if (SupportedMaterialUpgraders.TryGetValue(renderPipeline, out var materialUpgraders))
                    {
                        foreach (Material mat in materials)
                        {
                            if (materialUpgraders.TryGetValue(mat.shader.name, out var materialUpgrader) && materialUpgrader.Value != null)
                            {
                                materialUpgrader.Value.Upgrade(mat, MaterialUpgrader.UpgradeFlags.None);
                            }
                        }
                    }
                    break;
            }

            Debug.Log($"Magic Leap MRTK3 Package Material Conversion to {renderPipeline}: Completed");
        }

        /// <summary>
        /// Detects if any asset paths within a list starts within certain starting paths.
        /// </summary>
        private static bool AssetChangesDetectedInPaths(string[] assetPaths, List<string> startingPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                foreach (string startingPath in startingPaths)
                {
                    if (assetPath.StartsWith(startingPath))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Search package paths for all materials that can be converted to a rendering pipeline.
        /// </summary>
        private static List<Material> GetPackageMaterialsToConvert(UnityRenderPipeline renderPipeline, bool logResults = false)
        {
            List<Material> materials = new();

            List<string> existingPaths = new();
            foreach (string path in MagicLeapMRTK3Paths)
            {
                if (Directory.Exists(path))
                {
                    existingPaths.Add(path);
                }
            }

            if (existingPaths.Count == 0)
            {
                return materials;
            }

            string[] guids = AssetDatabase.FindAssets("t:Material", existingPaths.ToArray());
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material != null && MaterialConversionSupported(material, renderPipeline))
                {
                    materials.Add(material);
                }
            }

            if (logResults)
            {
                Debug.Log($"Magic Leap MRTK3 Package Material Conversion to {renderPipeline}: Standard Built-In Materials Detected: {materials.Count}");
            }

            return materials;
        }

        /// <summary>
        /// Detects if conversion is supported for a material to a specific render pipeline.
        /// </summary>
        private static bool MaterialConversionSupported(Material material, UnityRenderPipeline renderPipeline)
        {
            if (SupportedMaterialUpgraders.TryGetValue(renderPipeline, out var upgraders))
            {
                if (upgraders.Keys.Any(key => key.Contains(material.shader.name)))
                {
                    return true; 
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to find a MenuItem path that contains a search string.
        /// </summary>
        private static bool TryFindMenuItemPath(string searchString, out string menuItemPath)
        {
            menuItemPath = "";

            TypeCache.MethodCollection menuItemMethods = TypeCache.GetMethodsWithAttribute<MenuItem>();
            foreach (MethodInfo methodInfo in menuItemMethods)
            {
                MenuItem menuItem = methodInfo.GetCustomAttributes(typeof(MenuItem), inherit: false).FirstOrDefault() as MenuItem;
                if (menuItem != null && menuItem.menuItem.Contains(searchString))
                {
                    menuItemPath = menuItem.menuItem;
                    return true;
                }
            }

            return false;
        }
    }
}

#endif // UNITY_RENDER_PIPELINES_CORE