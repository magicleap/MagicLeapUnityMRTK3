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
using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using MagicLeap.MRTK.Settings;
using UnityEngine.InputSystem;
using System.Linq;
using InputDevice = UnityEngine.InputSystem.InputDevice;
using HandInteractionDevice = UnityEngine.XR.OpenXR.Features.Interactions.HandInteractionProfile.HandInteraction;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;

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
            ref HandInteractionDevice handInteractionDevice = ref (handNode == XRNode.LeftHand ? ref HandInteractionDevices.Item1 : ref HandInteractionDevices.Item2);
            ref bool pinchedLastFrame = ref (handNode == XRNode.LeftHand ? ref OpenXRPinchLastFrame.Item1 : ref OpenXRPinchLastFrame.Item2);
            if (handInteractionDevice != null && handInteractionDevice.added)
            {
                bool pinchReady = handInteractionDevice.pinchReady.ReadValue() == 1.0f;
                if (pinchReady)
                {
                    pinchAmount = handInteractionDevice.pinchValue.ReadValue();
                    // Debounce pinch
                    isPinching = pinchAmount >= (pinchedLastFrame ? PinchOpenThreshold : PinchClosedThreshold);
                    pinchedLastFrame = isPinching;
                    return true;
                }
            }

            pinchedLastFrame = isPinching = false;
            pinchAmount = 0;
            return false;
        }


        private static (bool, bool) OpenXRPinchLastFrame = (false, false);

        private static (HandInteractionDevice, HandInteractionDevice) HandInteractionDevices = (null, null);

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
            InputSystem.onDeviceChange += OnInputDeviceChange;
        }

        private static void OnInputDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch(change)
            {
                case InputDeviceChange.Added:
                    if (device is HandInteractionDevice)
                    {
                        if (device.usages.Contains(CommonUsages.LeftHand))
                        {
                            HandInteractionDevices.Item1 = device as HandInteractionDevice;
                        }

                        if (device.usages.Contains(CommonUsages.RightHand))
                        {
                            HandInteractionDevices.Item2 = device as HandInteractionDevice;
                        }
                    }
                    break;
                case InputDeviceChange.Removed:
                    // Copies of the HandInteraction device get added and removed from the system as it starts up.
                    // Only clear the reference if its the active device.
                    if (device is HandInteractionDevice)
                    {
                        if (device.usages.Contains(CommonUsages.LeftHand) &&
                            device == HandInteractionDevices.Item1)
                        {
                            HandInteractionDevices.Item1 = null;
                        }

                        if (device.usages.Contains(CommonUsages.RightHand) &&
                            device == HandInteractionDevices.Item2)
                        {
                            HandInteractionDevices.Item2 = null;
                        }
                    }
                    break;
                default : break;
            }
        }
    }
}
#endif // UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER

