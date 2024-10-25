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
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using MagicLeap.MRTK.Settings;
using UnityEngine.XR.MagicLeap.MLHandActions;

using GestureClassification = UnityEngine.XR.MagicLeap.InputSubsystem.Extensions.MLGestureClassification;
using XRInputDevice = UnityEngine.XR.InputDevice;
using XRInputDevices = UnityEngine.XR.InputDevices;

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// Provides auxiliary hand utilities when using the MagicLeap XR Provider.
    /// </summary>
    internal class MagicLeapAuxiliaryXRProviderHandUtility_MLProvider : MagicLeapAuxiliaryXRProviderHandUtilityBase
    {
        /// <inheritdoc/>
        public override bool CompatibleWithActiveXRLoader => MLDevice.IsMagicLeapLoaderActive();

        /// <inheritdoc/>
        public override HandRayTypeOption HandRayType => GeneralSettings.Value.HandRayType;

        /// <inheritdoc/>
        public override HandControllerMultimodalTypeOption HandControllerMultimodalType => GeneralSettings.Value.HandControllerMultimodalType;

        /// <inheritdoc/>
        public override bool TryGetPinch(XRNode handNode, out bool isPinching, out float pinchAmount)
        {
            ref XRInputDevice gestureDevice = ref (handNode == XRNode.LeftHand ? ref MLGesturesHands.Item1 : ref MLGesturesHands.Item2);
            ref bool pinchedLastFrame = ref (handNode == XRNode.LeftHand ? ref MLPinchedLastFrame.Item1 : ref MLPinchedLastFrame.Item2);

            if (MLGesturesStarted.Value && GestureClassification.TryGetFingerState(gestureDevice, GestureClassification.FingerType.Index, out GestureClassification.FingerState indexFingerState))
            {
                pinchAmount = 1 - indexFingerState.PostureData.PinchNormalAngle;
                bool isGesturePinchClosed = pinchAmount >= PinchClosedThreshold;
                bool isGesturePinchOpen = pinchAmount < PinchOpenThreshold;

                // the action pinch open enables us to persist occluded pinches
                bool isActionPinchOpen = true;

#if !UNITY_EDITOR && UNITY_ANDROID
                if (MLPinchAction.Active)
                {
                    isActionPinchOpen = handNode == XRNode.RightHand ? !MLPinchAction.RightPinchDown : !MLPinchAction.LeftPinchDown;
                }
#endif

                if (pinchedLastFrame)
                {
                    isPinching = !(isGesturePinchOpen && isActionPinchOpen);
                }
                else
                {
                    isPinching = isGesturePinchClosed;
                }

                pinchedLastFrame = isPinching;
                return true;
            }

            pinchedLastFrame = isPinching = false;
            pinchAmount = 0.0f;
            return false;
        }


        private static readonly Lazy<bool> MLGesturesStarted = new(() =>
        {
            GestureClassification.StartTracking();
            return true;
        });
        private static readonly Lazy<MagicLeapMRTK3SettingsGeneral> GeneralSettings = new(() =>
        {
            return MagicLeapMRTK3Settings.Instance.GetSettingsObject<MagicLeapMRTK3SettingsGeneral>();
        });
        private static (InputDevice, InputDevice) MLGesturesHands = (new XRInputDevice(), new XRInputDevice());
        private static (bool, bool) MLPinchedLastFrame = (false, false);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            if (!MagicLeapMRTK3Settings.DeviceIsCompatible())
            {
                return;
            }

            var instance = new MagicLeapAuxiliaryXRProviderHandUtility_MLProvider();
            MagicLeapAuxiliaryHandUtils.RegisterXRProviderHandUtility(instance);

            XRInputDevices.deviceConnected += XRInputDevices_deviceConnectChanged;
            XRInputDevices.deviceDisconnected += XRInputDevices_deviceConnectChanged;
        }

        private static void XRInputDevices_deviceConnectChanged(XRInputDevice device)
        {
            if (device.name == GestureClassification.LeftGestureInputDeviceName)
            {
                MLGesturesHands.Item1 = device;
            }
            else if (device.name == GestureClassification.RightGestureInputDeviceName)
            {
                MLGesturesHands.Item2 = device;
            }
        }
    }
}
#endif // UNITY_XR_MAGICLEAP_PROVIDER

