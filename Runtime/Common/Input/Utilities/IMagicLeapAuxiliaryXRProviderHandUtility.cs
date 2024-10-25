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
using UnityEngine.XR;
using MagicLeap.MRTK.Settings;

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// Interface for XR provider specific auxiliary hand utilities.
    /// </summary>
    internal interface IMagicLeapAuxiliaryXRProviderHandUtility
    {
        /// <summary>
        /// Whether the hand utility is compatible with the active XR loader.
        /// </summary>
        public bool CompatibleWithActiveXRLoader { get; }

        /// <summary>
        /// The hand ray type setting for the active XR loader.
        /// </summary>
        public HandRayTypeOption HandRayType { get; }

        /// <summary>
        /// The hand and controller multimodal type for the active XR loader.
        /// </summary>
        public HandControllerMultimodalTypeOption HandControllerMultimodalType { get; }

        /// <summary>
        /// Attempt to obtain pinch gesture details.
        /// </summary>
        public bool TryGetPinch(XRNode handNode, out bool isPinching, out float pinchAmount);

        /// <summary>
        /// Attempt to obtain the hand ray pose.
        /// </summary>
        public bool TryGetHandRayPose(XRNode handNode, out Pose pose);
    }
}
