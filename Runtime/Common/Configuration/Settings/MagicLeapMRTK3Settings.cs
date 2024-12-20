// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Management;
using System.Linq;
using UnityEngine.XR.MagicLeap;

#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
using UnityEngine.XR.OpenXR;
#if MAGICLEAP_UNITY_SDK_2_3_0_OR_NEWER
using MagicLeap.OpenXR.Features;
#else
using UnityEngine.XR.OpenXR.Features.MagicLeapSupport;
#endif
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#endif

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Magic Leap MRTK3 settings available in Editor and Runtime.
    /// </summary>
    [Serializable]
    [ScriptableSettingsPath("Assets")]
    public sealed class MagicLeapMRTK3Settings : ScriptableSettings<MagicLeapMRTK3Settings>
    {
        private const uint SettingsFileVersion = 1;

        [SerializeField]
        [HideInInspector]
        private uint version = SettingsFileVersion;
        public uint Version => version;

        // Common settings

        [SerializeField]
        private MagicLeapMRTK3SettingsUnityEditor editorSettings = null;

        [SerializeField]
        private MagicLeapMRTK3SettingsPermissionsConfig permissionsConfig = null;

        // XR Provider dependent settings

#if UNITY_XR_MAGICLEAP_PROVIDER
        [SerializeField]
        private MagicLeapMRTK3SettingsGeneral generalSettings = null;

        [SerializeField]
        private MagicLeapMRTK3SettingsRigConfig rigConfig = null;
#endif

#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
        [SerializeField]
        private MagicLeapMRTK3SettingsOpenXRGeneral openXRGeneralSettings = null;

        [SerializeField]
        private MagicLeapMRTK3SettingsOpenXRRigConfig openXRRigConfig = null;
#endif

        private static Lazy<bool> DeviceNameContainsMagicLeap = new(() =>
        {
            const string MagicLeap = "Magic Leap";
            return SystemInfo.deviceName.Contains(MagicLeap) || SystemInfo.deviceModel.Contains(MagicLeap);
        });

        private static Lazy<bool> MagicLeapOpenXRFeatureEnabled = new(() =>
        {
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
            MagicLeapFeature mlOpenXRFeature = OpenXRSettings.Instance.GetFeature<MagicLeapFeature>();
            return mlOpenXRFeature != null ? mlOpenXRFeature.enabled : false;
#else
            return false;
#endif
        });


        /// <summary>
        /// Provides enumerable access to all contained <see cref="MagicLeapMRTK3SettingsObject"/>s.
        /// </summary>
        public IEnumerable<MagicLeapMRTK3SettingsObject> SettingsObjects
        {
            get
            {
                // Common settings

                yield return editorSettings;
                yield return permissionsConfig;

                // XR Provider dependent settings

#if UNITY_XR_MAGICLEAP_PROVIDER
                yield return generalSettings;
                yield return rigConfig;
#endif
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
                yield return openXRGeneralSettings;
                yield return openXRRigConfig;
#endif
            }
        }

        /// <summary>
        /// Gets the specified type of <see cref="MagicLeapMRTK3SettingsObject"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="MagicLeapMRTK3SettingsObject"/>.</typeparam>
        /// <returns>The contained <see cref="MagicLeapMRTK3SettingsObject"/>, or a default instance if not present.</returns>
        public T GetSettingsObject<T>() where T : MagicLeapMRTK3SettingsObject
        {
            if (TryGetSettingsObject(out T settingsObject))
            {
                return settingsObject;
            }

            // Create a default instance if no contained settings object.
            return CreateInstance<T>();
        }

        /// <summary>
        /// Attempts to retrieve a specific <see cref="MagicLeapMRTK3SettingsObject"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="MagicLeapMRTK3SettingsObject"/>.</typeparam>
        /// <param name="settingsObjectOut">The returned <see cref="MagicLeapMRTK3SettingsObject"/>.</param>
        /// <returns>True if the collection contains a settings object of the type, or false if not.</returns>
        public bool TryGetSettingsObject<T>(out T settingsObjectOut) where T : MagicLeapMRTK3SettingsObject
        {
            foreach (var settingsObject in SettingsObjects)
            {
                if (settingsObject.GetType() == typeof(T))
                {
                    settingsObjectOut = settingsObject as T;
                    return settingsObjectOut != null;
                }
            }

            settingsObjectOut = null;
            return false;
        }

        /// <summary>
        /// Determines if the current device is compatible with Magic Leap 2 settings and systems,
        /// not counting active runtime XR Providers.
        /// </summary>
        /// <remarks>
        /// This method is useful for early checks before runtime subsystems have been started.
        /// </remarks>
        internal static bool DeviceIsCompatible()
        {
            if (Application.isEditor)
            {
                return true;
            }
            else
            {
                return DeviceNameContainsMagicLeap.Value;
            }
        }

        /// <summary>
        /// Determines if the current runtime is compatible with Magic Leap 2 settings and systems,
        /// including device and active runtime XR Providers.
        /// </summary>
        /// <remarks>
        /// This method is only valid after runtime subsystems have been started.
        /// </remarks>
        internal static bool RuntimeIsCompatible()
        {
            if (!DeviceIsCompatible() || !MLDevice.IsMagicLeapOrOpenXRLoaderActive())
            {
                return false;
            }

            // Under OpenXR, ensure the MagicLeapFeature is present and enabled.
            if (MLDevice.IsOpenXRLoaderActive() && !MagicLeapOpenXRFeatureEnabled.Value)
            {
                return false;
            }

            return true;
        }

        private void OnEnable()
        {
            // Deserialization has occurred at this point, so validate settings objects.
            ValidateSettingsObject(ref editorSettings);
            ValidateSettingsObject(ref permissionsConfig);
            
#if UNITY_XR_MAGICLEAP_PROVIDER
            ValidateSettingsObject(ref generalSettings);
            ValidateSettingsObject(ref rigConfig);
#endif
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
            ValidateSettingsObject(ref openXRGeneralSettings);
            ValidateSettingsObject(ref openXRRigConfig);
#endif

#if UNITY_EDITOR
            serializedObject = new SerializedObject(this);
#endif
        }

        private void ValidateSettingsObject<T>(ref T settingsObject) where T : MagicLeapMRTK3SettingsObject
        {
            if (settingsObject == null)
            {
                settingsObject = CreateInstance<T>();
            }
        }

#if UNITY_EDITOR

        internal enum SettingsObjectType
        {
            Common,
            XRProviderDependent
        }

        public enum XRProviderOption
        {
#if UNITY_XR_MAGICLEAP_PROVIDER
            MagicLeap,
#endif
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
            OpenXR,
#endif
        }

        public XRProviderOption SelectedXRProvider;

        // Maintain a SerializedObject in Editor
        private SerializedObject serializedObject;

        private static readonly string DefaultPathBase = "Packages/com.magicleap.mrtk3/Editor/Settings/Defaults/";

        /// <summary>
        /// Initialize and validate setting objects in Editor
        /// </summary>
        public void Initialize()
        {
            ValidateSettingsObjectInEditor(ref editorSettings);
            ValidateSettingsObjectInEditor(ref permissionsConfig);

#if UNITY_XR_MAGICLEAP_PROVIDER
            ValidateSettingsObjectInEditor(ref generalSettings);
            ValidateSettingsObjectInEditor(ref rigConfig);
#endif
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
            ValidateSettingsObjectInEditor(ref openXRGeneralSettings);
            ValidateSettingsObjectInEditor(ref openXRRigConfig);
#endif

            // Validate selected XR Provider
            if (!Enum.IsDefined(typeof(XRProviderOption), SelectedXRProvider))
            {
                SelectedXRProvider = default;
            }
        }

        private void ValidateSettingsObjectInEditor<T>(ref T settingsObject) where T : MagicLeapMRTK3SettingsObject
        {
            // In Editor, if the settings object doesn't exist or hasn't been added to the parent settings object yet,
            // attempt to load the pre-setup, default, asset and then add to parent settings object.
            if (settingsObject == null || !AssetDatabase.IsSubAsset(settingsObject))
            {
                LoadDefault(ref settingsObject);
            }
        }

        /// <summary>
        /// Load default settings manually in Editor
        /// </summary>
        public void LoadDefaults()
        {
            LoadDefault(ref editorSettings);
            LoadDefault(ref permissionsConfig);

#if UNITY_XR_MAGICLEAP_PROVIDER
            LoadDefault(ref generalSettings);
            LoadDefault(ref rigConfig);
#endif
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
            LoadDefault(ref openXRGeneralSettings);
            LoadDefault(ref openXRRigConfig);
#endif
        }

        private void LoadDefault<T>(ref T settingsObject) where T : MagicLeapMRTK3SettingsObject
        {
            // Remove any existing sub object settingsObject
            if (settingsObject != null)
            {
                AssetDatabase.RemoveObjectFromAsset(settingsObject);
            }
            // Load and clone default asset
            var defaultPath = $"{DefaultPathBase}{typeof(T).Name}_Default.asset";
            var defaults = AssetDatabase.LoadAssetAtPath<T>(defaultPath);
            settingsObject = defaults != null ? Instantiate(defaults) :
                                                CreateInstance<T>();
            settingsObject.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(settingsObject, this);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(settingsObject);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Draws the settings in Editor.
        /// </summary>
        public void OnGUI()
        {
            serializedObject.Update();

            // Load Defaults Button
            if (GUILayout.Button("Load Defaults", GUILayout.Width(100)))
            {
                LoadDefaults();
            }
            EditorGUILayout.Space(8);

            // Draw common settings first
            OnGUI(SettingsObjectType.Common);

            GUILayout.BeginVertical(new GUIStyle("Window") { padding = new RectOffset(20, 20, 20, 0) });

            // XR Provider Settings Dropdown
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            const string XRProviderLabel = "Settings For XR Provider:";
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(XRProviderLabel)).x + 10.0f;
            SelectedXRProvider = (XRProviderOption)EditorGUILayout.EnumPopup(XRProviderLabel, SelectedXRProvider, GUILayout.Width(EditorGUIUtility.labelWidth + 100));
            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUILayout.Space(16);

            // Draw XR provider dependent settings
            OnGUI(SettingsObjectType.XRProviderDependent);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            serializedObject.ApplyModifiedProperties();
        }

        internal void OnGUI(SettingsObjectType type)
        {
            switch (type)
            {
                case SettingsObjectType.Common:
                    foreach (var settingsObject in SettingsObjects)
                    {
                        if (!settingsObject.DependentOnXRProvider)
                        {
                            settingsObject.OnGUI();
                            EditorGUILayout.Space(20);
                        }
                    }
                    break;
                case SettingsObjectType.XRProviderDependent:
                    foreach (var settingsObject in SettingsObjects)
                    {
                        if (settingsObject.DependentOnXRProvider &&
                            settingsObject.CompatibleWithSelectedXRProviderInEditor(SelectedXRProvider))
                        {
                            settingsObject.OnGUI();
                            EditorGUILayout.Space(20);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Utility method to determine if a specific XR Provider is selected for a build target group.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="XRLoaderHelper"/>.</typeparam>
        /// <param name="target">The <see cref="BuildTargetGroup"/></param>
        /// <returns><see langword="true"/> if the XR Provider is selected for the build target group, otherwise <see langword="false"/>.</returns>
        public static bool IsXRProviderSelectedForBuildTarget<T>(BuildTargetGroup target) where T : XRLoaderHelper
        {
            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(target);

            if (settings != null && settings.AssignedSettings != null)
            {
                return settings.AssignedSettings.activeLoaders.Any(loader => loader is T);
            }

            return false;
        }

#endif // UNITY_EDITOR

    }
}