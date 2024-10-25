// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if UNITY_XR_MAGICLEAP_PROVIDER
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using MixedReality.Toolkit;
using System;
using UnityEngine.XR.MagicLeap;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Provides general settings specific to when using the MagicLeap XR Provider.
    /// </summary>
    public sealed class MagicLeapMRTK3SettingsGeneral : MagicLeapMRTK3SettingsGeneralBase
    {
        private const uint PermissionsFileVersion = 1;

        [SerializeField]
        [HideInInspector]
        private uint version = PermissionsFileVersion;
        public uint Version => version;

        [SerializeField]
        [Tooltip("Whether to observe the OS Setting for enabling hand navigation for hand interactions in MRTK3.  " +
                 "If this flag is enabled, hand interactions will be available within MRTK3 based on the OS Setting " +
                 "for hand navigation, at Settings > Magic Leap Inputs > Gestures > Hand Navigation.")]
        private bool observeOSHandNavigationSetting = false;

        /// <summary>
        /// Whether to observe the OS Setting for enabling of hand navigation for hand interactions in MRTK3.
        /// 
        /// If this flag is set to true, and the OS Setting for hand navigation is disabled, then hands will
        /// be disabled within MRTK3 on the ML2.
        /// </summary>
        public bool ObserveOSHandNavigationSetting => observeOSHandNavigationSetting;

        /// <summary>
        /// Whether MRTK3 hand interactions are enabled based on the OS hand navigation setting and whether that
        /// setting should be observed or not.  Defaults to not observing the OS setting, so hands are enabled
        /// by default in MRTK3 on the ML2 platform.
        /// </summary>
        public bool MRTK3HandInteractionsEnabled => !observeOSHandNavigationSetting || osHandNavigationEnabled;

        [SerializeField]
        [Tooltip("Whether to instruct the Unity XR plugin to keep the rendered alpha and pass it along " +
                 "to Magic Leap 2 for further processing, resulting in better quality photo/video captures " +
                 "and segmented dimmer results. " +
                 "Note, this feature may not work well when used in combination with any post-processing " +
                 "effects, like HDR, which may need to be disabled to use this setting.")]
#pragma warning disable CS0414 // Field assigned but never used warning
        private bool keepRenderedAlpha = true;
#pragma warning restore CS0414
        // Not exposing a public property for KeepRenderedAlpha at this time as this option may go away with OpenXR.

        /// <inheritdoc/>
        public override bool CompatibleWithActiveXRLoader => MLDevice.IsMagicLeapLoaderActive();

        private const string handNavigationSettingsKey = "enable_pinch_gesture_inputs";
        private bool osHandNavigationEnabled = true;

#if UNITY_EDITOR

        /// <inheritdoc/>
        public override string SettingsXRProviderLabel => MagicLeapMRTK3Settings.XRProviderOption.MagicLeap.ToString();

        /// <inheritdoc/>
        public override bool CompatibleWithSelectedXRProviderInEditor(MagicLeapMRTK3Settings.XRProviderOption selectedXRProvider)
        {
            return selectedXRProvider == MagicLeapMRTK3Settings.XRProviderOption.MagicLeap;
        }

        /// <inheritdoc/>
        public override void DrawSettingsWindowContent()
        {
            // Observe OS Setting for Enabling Hand Navigation
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            const string osHandNavigationLabel = "Observe OS Setting for Enabling Hand Navigation";
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(osHandNavigationLabel)).x + 10.0f;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("observeOSHandNavigationSetting"),
                                                                        new GUIContent(osHandNavigationLabel));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("keepRenderedAlpha"));

            base.DrawSettingsWindowContent();

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

#endif // UNITY_EDITOR

        /// <inheritdoc/>
        public override void ProcessOnBeforeSceneLoad()
        {
            // Subscribe to every loaded scene for possible processing
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <inheritdoc/>
        public override void ProcessOnAfterSceneLoad()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // OS Setting for Hand Navigation
            // Only on device

            // Only need to monitor the OS hand navigation setting if the ObserveOSHandNavigationSetting is set
            if (ObserveOSHandNavigationSetting)
            {
                osHandNavigationEnabled = JavaUtils.GetSystemSetting<int>("getInt", handNavigationSettingsKey) > 0;

                // Start timer checking hand navigation settings option, every 2 seconds
                SynchronizationContext mainSyncContext = SynchronizationContext.Current;
                System.Timers.Timer timer = new System.Timers.Timer(2000);
                timer.Start();
                timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
                {
                    mainSyncContext.Post(_ =>
                    {
                        osHandNavigationEnabled = JavaUtils.GetSystemSetting<int>("getInt", handNavigationSettingsKey) > 0;

                    }, null);
                };
            }

            // Keep Rendered Alpha enabled, set plugin flag
            if (keepRenderedAlpha)
            {
                try
                {
                    SetSegmentedDimmerKeepAlpha(true);
                }
                catch (DllNotFoundException) 
                {
                    Debug.LogError("Unable to load UnityMagicLeap DLL for MRTK3 Magic Leap Setting 'Keep Rendered Alpha'.");
                }
            }
#endif // UNITY_ANDROID && !UNITY_EDITOR
        }

        // The following is run for any loaded scene.
        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (keepRenderedAlpha)
            {
                // In order for the SetSegmentedDimmerKeepAlpha(true) call to work, the camera's
                // clear color must be ensured to be set to zero (0) alpha.

                // Verify the main camera's background color alpha is zero.
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    var backgroundColor = mainCamera.backgroundColor;
                    backgroundColor.a = 0;
                    mainCamera.backgroundColor = backgroundColor;

                    // MRTK3 uses a CameraSettingsManager to modify the main camera at startup depending
                    // on the display type.  Verify the clear color alpha is zero for the TransparentDisplay type.
                    var cameraSettingsManager = mainCamera.GetComponent<CameraSettingsManager>() ?? FindAnyObjectByType<CameraSettingsManager>();
                    if (cameraSettingsManager != null)
                    {
                        var transparencyDisplay = cameraSettingsManager.TransparentDisplay;
                        var clearColor = transparencyDisplay.ClearColor;
                        clearColor.a = 0;
                        transparencyDisplay.ClearColor = clearColor;
                        cameraSettingsManager.TransparentDisplay = transparencyDisplay;
                    }
                }
            }
#endif // UNITY_ANDROID && !UNITY_EDITOR
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport("UnityMagicLeap", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UnityMagicLeap_SegmentedDimmer_KeepAlpha")]
        private static extern void SetSegmentedDimmerKeepAlpha(bool status);
#endif // UNITY_ANDROID && !UNITY_EDITOR
    }
}
#endif // UNITY_XR_MAGICLEAP_PROVIDER