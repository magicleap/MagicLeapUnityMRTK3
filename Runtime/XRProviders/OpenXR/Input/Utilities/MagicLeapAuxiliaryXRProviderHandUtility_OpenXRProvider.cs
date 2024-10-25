// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if UNITY_OPENXR_1_9_0_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using MagicLeap.MRTK.Settings;

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// Provides auxiliary hand utilities when using the OpenXR XR Provider.
    /// </summary>
    internal class MagicLeapAuxiliaryXRProviderHandUtility_OpenXRProvider : MagicLeapAuxiliaryXRProviderHandUtilityBase
    {
        /// <inheritdoc/>
        public override bool CompatibleWithActiveXRLoader => MLDevice.IsOpenXRLoaderActive();

        /// <inheritdoc/>
        public override HandRayTypeOption HandRayType => GeneralSettings.Value.HandRayType;

        /// <inheritdoc/>
        public override HandControllerMultimodalTypeOption HandControllerMultimodalType => GeneralSettings.Value.HandControllerMultimodalType;

        /// <inheritdoc/>
        public override bool TryGetPinch(XRNode handNode, out bool isPinching, out float pinchAmount)
        {
            ref bool pinchedLastFrame = ref (handNode == XRNode.LeftHand ? ref MRTKPinchLastFrame.Item1 : ref MRTKPinchLastFrame.Item2);

            // Under OpenXR, for now, just utilize MRTK's pinch detection.
            // ML OS pinch logic will be incorporated into OpenXR at a later date.
            if (HandSubsystem.TryGetPinchProgress(handNode, out _, out _, out pinchAmount))
            {
                // Debounce pinch
                isPinching = pinchAmount >= (pinchedLastFrame ? MRTKPinchOpenThreshold : MRTKPinchClosedThreshold);
                pinchedLastFrame = isPinching;
                return true;
            }

            pinchedLastFrame = isPinching = false;
            pinchAmount = 0;
            return false;
        }


        private static (bool, bool) MRTKPinchLastFrame = (false, false);
        private const float MRTKPinchClosedThreshold = 1.0f;
        private const float MRTKPinchOpenThreshold = 0.85f;

        private static readonly Lazy<MagicLeapMRTK3SettingsOpenXRGeneral> GeneralSettings = new(() =>
        {
            return MagicLeapMRTK3Settings.Instance.GetSettingsObject<MagicLeapMRTK3SettingsOpenXRGeneral>();
        });

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            if (!MagicLeapMRTK3Settings.DeviceIsCompatible())
            {
                return;
            }

            var instance = new MagicLeapAuxiliaryXRProviderHandUtility_OpenXRProvider();
            MagicLeapAuxiliaryHandUtils.RegisterXRProviderHandUtility(instance);
        }
    }
}
#endif // UNITY_OPENXR_1_9_0_OR_NEWER

