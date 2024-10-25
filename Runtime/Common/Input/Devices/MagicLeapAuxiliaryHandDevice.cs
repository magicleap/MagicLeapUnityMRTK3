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
using UnityEngine.Scripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using XRNode = UnityEngine.XR.XRNode;
using InputTrackingState = UnityEngine.XR.InputTrackingState;
using UnityEngine.XR.MagicLeap;
using System.Collections.Generic;
using MixedReality.Toolkit.Input;
using MagicLeap.MRTK.Settings;

#if MAGICLEAP_UNITY_SDK_2_1_0_OR_NEWER
using MagicLeap.Android;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// State structure for a <see cref="MagicLeapAuxiliaryHandDevice"/> device
    /// </summary>
    public struct MagicLeapAuxiliaryHandState : IInputStateTypeInfo
    {
        public FourCC format => new FourCC("MLAH");

        [InputControl(layout = "Integer")]
        public int trackingState;

        [InputControl(layout = "Button")]
        public bool isTracked;

        [InputControl(layout = "Vector3")]
        public Vector3 devicePosition;

        [InputControl(layout = "Quaternion")]
        public Quaternion deviceRotation;

        [InputControl(layout = "Button")]
        public bool pinchPressed;

        [InputControl(layout = "Axis")]
        public float pinch;

        [InputControl(layout = "Vector3")]
        public Vector3 pointerPosition;

        [InputControl(layout = "Quaternion")]
        public Quaternion pointerRotation;
    }

    /// <summary>
    /// Determines the gesture recognition method(s) to use
    /// </summary>
    internal enum MLGestureType
    {
        /// <summary>
        /// Gesture will be recognized if either type is recognized
        /// </summary>
        Both,
        /// <summary>
        /// Exclusive MagicLeap gesture recognition
        /// </summary>
        MagicLeap,
        /// <summary>
        /// Exclusive MRTK gesture recognition
        /// </summary>
        MRTK
    }


    /// <summary>
    /// InputDevice definition to provide auxiliary input controls for
    /// MagicLeap specialized hand data and algorithms.
    /// The auxiliary hand devices also provide for disambiguation between the
    /// inputs that can be bound to the hands and ML Controller, making it so that
    /// the hand input alone will drive the <see cref="ArticulatedHandController"/>
    /// in the rig and not also potentially the ML Controller input, which can cause
    /// issues if both are active at the same time.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad] // Call static class constructor in editor.
#endif
    [InputControlLayout(stateType = typeof(MagicLeapAuxiliaryHandState),
                        displayName = "MagicLeapAuxiliaryHandDevice",
                        commonUsages = new[] { "LeftHand", "RightHand" })]
    public class MagicLeapAuxiliaryHandDevice : TrackedDevice, IInputUpdateCallbackReceiver
    {
#if UNITY_EDITOR
        static MagicLeapAuxiliaryHandDevice()
        {
            // In Editor, listen for changes in play mode to clean up devices
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    CleanupEditorDeviceInstances();
                }
            };
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void CreateRuntimeInstances()
        {
            if (!MagicLeapMRTK3Settings.RuntimeIsCompatible())
            {
                return;
            }

#if UNITY_EDITOR
            CleanupEditorDeviceInstances();
#endif
            // Create new instances only if running on MagicLeap and hand tracking is granted
            bool handTrackingPermissionGranted = false;
#if MAGICLEAP_UNITY_SDK_2_1_0_OR_NEWER
            handTrackingPermissionGranted = Permissions.CheckPermission(MLPermission.HandTracking);
#else
            handTrackingPermissionGranted = MLPermissions.CheckPermission(MLPermission.HandTracking).IsOk;
#endif
            if (handTrackingPermissionGranted)
            {
                MagicLeapAuxiliaryHandDevice leftHand = InputSystem.AddDevice<MagicLeapAuxiliaryHandDevice>(
                    $"{nameof(MagicLeapAuxiliaryHandDevice)} - {CommonUsages.LeftHand}");
                leftHand.HandNode = XRNode.LeftHand;

                MagicLeapAuxiliaryHandDevice rightHand = InputSystem.AddDevice<MagicLeapAuxiliaryHandDevice>(
                    $"{nameof(MagicLeapAuxiliaryHandDevice)} - {CommonUsages.RightHand}");
                rightHand.HandNode = XRNode.RightHand;
            }
        }

        private XRNode handNode;

        /// <summary>
        /// The <see cref="XRNode"/> hand node assigned to this device
        /// </summary>
        public XRNode HandNode {
            get => handNode;
            private set
            {
                handNode = value;
                InputSystem.SetDeviceUsage(this, value == XRNode.RightHand ?
                                                          CommonUsages.RightHand :
                                                          CommonUsages.LeftHand);
            }
        }


        [Preserve]
        [InputControl]
        public ButtonControl pinchPressed { get; private set; }

        [Preserve]
        [InputControl]
        public AxisControl pinch { get; private set; }

        [Preserve]
        [InputControl]
        public Vector3Control pointerPosition { get; private set; }

        [Preserve]
        [InputControl]
        public QuaternionControl pointerRotation { get; private set; }


        private static HandsAggregatorSubsystem HandSubsystem => XRSubsystemHelpers.HandsAggregator as HandsAggregatorSubsystem;
        private static readonly List<MagicLeapAuxiliaryHandDevice> AuxHandDevices = new();

        private bool wasTrackedLastFrame = false;
        private HandRay mrtkHandRay = new HandRay();
        private bool mrtkPinchedLastFrame = false;
        private const float mrtkPinchClosedThreshold = 1.0f;
        private const float mrtkPinchOpenThreshold = 0.85f;
        private MLGestureType gestureType = MLGestureType.Both;

        private bool UseMLGestures => gestureType == MLGestureType.MagicLeap || gestureType == MLGestureType.Both;

        private bool UseMRTKGestures => gestureType == MLGestureType.MRTK || gestureType == MLGestureType.Both;


        protected override void FinishSetup()
        {
            base.FinishSetup();

            pinchPressed = GetChildControl<ButtonControl>(nameof(pinchPressed));
            pinch = GetChildControl<AxisControl>(nameof(pinch));
            pointerPosition = GetChildControl<Vector3Control>(nameof(pointerPosition));
            pointerRotation = GetChildControl<QuaternionControl>(nameof(pointerRotation));
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            AuxHandDevices.Add(this);
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();
            AuxHandDevices.Remove(this);
        }

        public void OnUpdate()
        {
            if (HandSubsystem == null)
                return;

            // Only update state if, at a minimum, we get a valid tracked hand Palm
            if (HandSubsystem.TryGetJoint(TrackedHandJoint.Palm, HandNode, out HandJointPose pose))
            {
                wasTrackedLastFrame = true;

                var state = new MagicLeapAuxiliaryHandState();
                state.isTracked = true;
                state.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);

                // Device Position/Rotation
                // Input actions expected in XR scene-origin-space
                HandJointPose xrDevicePose = PlayspaceUtilities.InverseTransformPose(pose);
                state.devicePosition = xrDevicePose.Position;
                state.deviceRotation = xrDevicePose.Rotation;

                // Select & Select Value (progress)
                if (TryGetPinch(out bool isPinching, out float pinchAmount))
                {
                    state.pinchPressed = isPinching;
                    state.pinch = pinchAmount;
                }

                // Pointer Position/Rotation (Hand Ray)
                if (TryGetHandRayPose(out Pose handRayPose))
                {
                    // Input actions expected in XR scene-origin-space
                    Pose xrHandRayPose = PlayspaceUtilities.InverseTransformPose(handRayPose);
                    state.pointerPosition = xrHandRayPose.position;
                    state.pointerRotation = xrHandRayPose.rotation;
                }

                InputSystem.QueueStateEvent(this, state);
            }
            else if (wasTrackedLastFrame)
            {
                // If the hand is no longer tracked, reset the state once until tracked again
                InputSystem.QueueStateEvent(this, new MagicLeapAuxiliaryHandState());
                wasTrackedLastFrame = false;
            }
        }

        /// <summary>
        /// Gets pinch state and the normalized pinch amount 
        /// </summary>
        /// <param name="isPinching">Hand is pinching</param>
        /// <param name="pinchAmount">The degree the hand is pinching</param>
        /// <returns>True, if a pinch recognition executes</returns>
        private bool TryGetPinch(out bool isPinching, out float pinchAmount)
        {
            bool validPinch = false;
            isPinching = false;
            pinchAmount = 0;

            bool validPinchML = false;
            bool isPinchingML = false;
            float pinchAmountML = 0;

            bool validPinchMRTK = false;
            bool isPinchingMRTK = false;
            float pinchAmountMRTK = 0;

            // ML Gesture Classification pinch detection 
            if (UseMLGestures)
            {
                validPinchML = MagicLeapAuxiliaryHandUtils.TryGetMLPinch(HandNode, out isPinchingML, out pinchAmountML);
            }

            // stock MRTK pinch detection
            if (UseMRTKGestures && HandSubsystem.TryGetPinchProgress(HandNode, out _, out _, out pinchAmountMRTK))
            {
                // Debounce pinch
                isPinchingMRTK = pinchAmountMRTK >= (mrtkPinchedLastFrame ? mrtkPinchOpenThreshold : mrtkPinchClosedThreshold);
                mrtkPinchedLastFrame = isPinchingMRTK;
                validPinchMRTK = true;
            }

            switch (gestureType)
            {
                case MLGestureType.Both:
                    validPinch = validPinchML || validPinchMRTK;
                    isPinching = isPinchingML || isPinchingMRTK;
                    pinchAmount = pinchAmountMRTK; // Prefer MRTK pinch amount if using both.
                    break;
                case MLGestureType.MagicLeap:
                    validPinch = validPinchML;
                    isPinching = isPinchingML;
                    pinchAmount = pinchAmountML;
                    break;
                case MLGestureType.MRTK:
                    validPinch = validPinchMRTK;
                    isPinching = isPinchingMRTK;
                    pinchAmount = pinchAmountMRTK;
                    break;
            }

            return validPinch;
        }

        /// <summary>
        /// Gets hand ray pose in world space
        /// 
        /// ML Hand Ray - based on MagicLeap hand ray algorithm
        /// Default MRTK Ray - based on MRTK's PolyfillHandRayPoseSource::TryGetPose
        /// </summary>
        private bool TryGetHandRayPose(out Pose pose)
        {
            switch (MagicLeapAuxiliaryHandUtils.GetHandRayTypeSettingWithDefault())
            {
                case HandRayTypeOption.MLHandRay:
                    return MagicLeapAuxiliaryHandUtils.TryGetMLHandRayPose(HandNode, out pose);

                case HandRayTypeOption.MRTKHandRay:
                default:
                    // Tick the hand ray generator function. Uses index knuckle for position.
                    if (HandSubsystem.TryGetJoint(TrackedHandJoint.IndexProximal, HandNode, out HandJointPose knuckle) &&
                        HandSubsystem.TryGetJoint(TrackedHandJoint.Palm, HandNode, out HandJointPose palm))
                    {
                        mrtkHandRay.Update(knuckle.Position, -palm.Up, Camera.main.transform, handNode.ToHandedness());
                        pose = new Pose(mrtkHandRay.Ray.origin,
                                        Quaternion.LookRotation(mrtkHandRay.Ray.direction, palm.Up));
                        return true;
                    }
                    break;
            }

            pose = Pose.identity;
            return false;
        }

#if UNITY_EDITOR
        private static void CleanupEditorDeviceInstances()
        {
            for (int i = AuxHandDevices.Count - 1; i >= 0; i--)
            {
                InputSystem.RemoveDevice(AuxHandDevices[i]);
            }
        }
#endif
    }
}