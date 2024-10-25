// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Provides the base implementation for general settings.
    /// </summary>
    public abstract class MagicLeapMRTK3SettingsGeneralBase : MagicLeapMRTK3SettingsObject
    {
        [SerializeField]
        [Tooltip("Allows the selection of the hand ray type. By default, we use the stock MRTK ray algorithm.")]
        private HandRayTypeOption handRayType = HandRayTypeOption.MRTKHandRay;

        /// <summary>
        /// The type of hand ray. 
        ///
        /// These types correlate to different algorithms for calculating the ray origin transform, smoothing,
        /// and other ray characteristics.
        /// </summary>
        public HandRayTypeOption HandRayType => handRayType;

        [SerializeField]
        [Tooltip("Allows the selection of the hand and controller multimodal behavior. By default, the hand " +
                 "holding the controller is deactivated.")]
        private HandControllerMultimodalTypeOption handControllerMultimodalType = HandControllerMultimodalTypeOption.HandHoldingControllerFullyDisabled;

        /// <summary>
        /// The hand controller multimodal type option.
        /// </summary>
        public HandControllerMultimodalTypeOption HandControllerMultimodalType => handControllerMultimodalType;

#if UNITY_EDITOR

        /// <inheritdoc/>
        public override string SettingsWindowLabel => "General Settings";

        /// <inheritdoc/>
        public override void DrawSettingsWindowContent()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handRayType"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("handControllerMultimodalType"));
        }

#endif // UNITY_EDITOR

    }
}