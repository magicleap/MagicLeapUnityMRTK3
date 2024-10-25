// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
using UnityEngine;
using UnityEngine.XR.MagicLeap;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Provides general settings specific to when using the OpenXR XR Provider.
    /// </summary>
    public sealed class MagicLeapMRTK3SettingsOpenXRGeneral : MagicLeapMRTK3SettingsGeneralBase
    {
        private const uint PermissionsFileVersion = 1;

        [SerializeField]
        [HideInInspector]
        private uint version = PermissionsFileVersion;
        public uint Version => version;

        /// <inheritdoc/>
        public override bool CompatibleWithActiveXRLoader => MLDevice.IsOpenXRLoaderActive();

#if UNITY_EDITOR

        /// <inheritdoc/>
        public override string SettingsXRProviderLabel => MagicLeapMRTK3Settings.XRProviderOption.OpenXR.ToString();

        /// <inheritdoc/>
        public override bool CompatibleWithSelectedXRProviderInEditor(MagicLeapMRTK3Settings.XRProviderOption selectedXRProvider)
        {
            return selectedXRProvider == MagicLeapMRTK3Settings.XRProviderOption.OpenXR;
        }

        /// <inheritdoc/>
        public override void DrawSettingsWindowContent()
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            const string MultimodalLabel = "Hand Controller Multimodal Type";
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(MultimodalLabel)).x + 10.0f;

            base.DrawSettingsWindowContent();

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

#endif // UNITY_EDITOR

        /// <inheritdoc/>
        public override void ProcessOnBeforeSceneLoad()
        {
        }

        /// <inheritdoc/>
        public override void ProcessOnAfterSceneLoad()
        {
        }
    }
}
#endif // UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER