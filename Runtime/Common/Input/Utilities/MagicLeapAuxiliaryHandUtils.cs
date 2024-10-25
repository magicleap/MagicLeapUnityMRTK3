// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MagicLeap.MRTK.Settings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// Auxiliary hand utilities
    /// </summary>
    internal class MagicLeapAuxiliaryHandUtils
    {
        /// <summary>
        /// Get the hand ray type setting for the active XR loader, with default.
        /// </summary>
        public static HandRayTypeOption GetHandRayTypeSettingWithDefault(HandRayTypeOption defaultType = HandRayTypeOption.MRTKHandRay)
        {
            foreach (var providerHandUtility in RegisteredXRProviderHandUtils)
            {
                if (providerHandUtility.CompatibleWithActiveXRLoader)
                {
                    return providerHandUtility.HandRayType;
                }
            }

            return defaultType;
        }

        /// <summary>
        /// Get the hand and controller multimodal type setting for the active XR loader, with default.
        /// </summary>
        public static HandControllerMultimodalTypeOption GetHandControllerMultimodalTypeSettingWithDefault(
            HandControllerMultimodalTypeOption defaultType = HandControllerMultimodalTypeOption.HandHoldingControllerFullyDisabled)
        {
            foreach (var providerHandUtility in RegisteredXRProviderHandUtils)
            {
                if (providerHandUtility.CompatibleWithActiveXRLoader)
                {
                    return providerHandUtility.HandControllerMultimodalType;
                }
            }

            return defaultType;
        }

        /// <summary>
        /// Attempt to obtain ML pinch gesture details based on the active XR loader.
        /// </summary>
        public static bool TryGetMLPinch(XRNode handNode, out bool isPinching, out float pinchAmount)
        {
            foreach (var providerHandUtility in RegisteredXRProviderHandUtils)
            {
                if (providerHandUtility.CompatibleWithActiveXRLoader)
                {
                    return providerHandUtility.TryGetPinch(handNode, out isPinching, out pinchAmount);
                }
            }

            isPinching = false;
            pinchAmount = 0;
            return false;
        }

        /// <summary>
        /// Attempt to obtain the ML hand ray pose based on the active XR loader.
        /// </summary>
        public static bool TryGetMLHandRayPose(XRNode handNode, out Pose pose)
        {
            foreach (var providerHandUtility in RegisteredXRProviderHandUtils)
            {
                if (providerHandUtility.CompatibleWithActiveXRLoader)
                {
                    return providerHandUtility.TryGetHandRayPose(handNode, out pose);
                }
            }

            pose = Pose.identity;
            return false;
        }

        /// <summary>
        /// Register an XR Provider specific hand utility.
        /// </summary>
        public static void RegisterXRProviderHandUtility(IMagicLeapAuxiliaryXRProviderHandUtility xrProviderHandUtil)
        {
            RegisteredXRProviderHandUtils.Add(xrProviderHandUtil);
        }


        /// <summary>
        /// A list of the registered XR Provider hand utilities
        /// </summary>
        public static IReadOnlyList<IMagicLeapAuxiliaryXRProviderHandUtility> XRProviderHandUtils => RegisteredXRProviderHandUtils;

        private static List<IMagicLeapAuxiliaryXRProviderHandUtility> RegisteredXRProviderHandUtils = new();
    }
}
