// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using UnityEngine.Scripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input.Simulation;

#if USE_INPUT_SYSTEM_POSE_CONTROL
using PoseControl = UnityEngine.InputSystem.XR.PoseControl;
#else
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
#endif

namespace MagicLeap.MRTK.Input.Simulation
{
    /// <summary>
    /// Provides simulated device input for the Magic Leap Controller.
    /// </summary>
    /// <remarks>
    /// This device should be able to bind to input actions that use a path containing the
    /// <MagicLeapController> layout name, and derives from <see cref="TrackedDevice"/>
    /// to avoid binding with actions that may use the generic <XRController> in the path,
    /// such as the default MRTK3 input actions that are used for hand controllers.
    /// </remarks>
    [Preserve, InputControlLayout(displayName = "MagicLeapController (Simulated)",
                                  isGenericTypeOfDevice = false,
                                  commonUsages = new[] { "LeftHand", "RightHand" })]
    public class MagicLeapController : TrackedDevice
    {
        [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device")]
        public PoseControl devicePose { get; private set; }

        [Preserve, InputControl(offset = 64, alias = "aimPose", usage = "Pointer")]
        public PoseControl pointer { get; private set; }

#if USE_INPUT_SYSTEM_POSE_CONTROL
        [Preserve, InputControl(offset = 0, bit = 0, sizeInBits = 8)]
#else
        [Preserve, InputControl(offset = 0, bit = 0, sizeInBits = 1)]
#endif
        public new ButtonControl isTracked { get; private set; }

        [Preserve, InputControl(offset = 4)]
        public new IntegerControl trackingState { get; private set; }

        [Preserve, InputControl(offset = 8, alias = "gripPosition")]
        public new Vector3Control devicePosition { get; private set; }

        [Preserve, InputControl(offset = 20, alias = "gripOrientation")]
        public new QuaternionControl deviceRotation { get; private set; }

        [Preserve, InputControl(offset = 36, alias = "gripVelocity")]
        public Vector3Control deviceVelocity { get; private set; }

        [Preserve, InputControl(offset = 48, alias = "gripAngularVelocity")]
        public Vector3Control deviceAngularVelocity { get; protected set; }

        [Preserve, InputControl(offset = 72)]
        public Vector3Control pointerPosition { get; private set; }

        [Preserve, InputControl(offset = 84, alias = "pointerOrientation")]
        public QuaternionControl pointerRotation { get; private set; }

        [Preserve, InputControl(aliases = new[] { "GripButton", "shoulderClicked" }, usage = "GripButton")]
        public ButtonControl gripPressed { get; private set; }

        [Preserve, InputControl(aliases = new[] { "Primary", "menubutton" }, usage = "MenuButton")]
        public ButtonControl menu { get; private set; }

        [Preserve, InputControl(alias = "triggeraxis", usage = "Trigger")]
        public AxisControl trigger { get; private set; }

        [Preserve, InputControl(alias = "triggerbutton", usage = "TriggerButton")]
        public ButtonControl triggerPressed { get; private set; }

        [Preserve, InputControl(aliases = new[] { "Primary2DAxis", "touchpadaxes", "touchpad" }, usage = "Primary2DAxis")]
        public Vector2Control trackpad { get; private set; }

        [Preserve, InputControl(aliases = new[] { "joystickorpadpressed", "touchpadpressed" }, usage = "Primary2DAxisClick")]
        public ButtonControl trackpadClicked { get; private set; }

        [Preserve, InputControl(aliases = new[] { "joystickorpadtouched", "touchpadtouched" }, usage = "Primary2DAxisTouch")]
        public ButtonControl trackpadTouched { get; private set; }

        [Preserve, InputControl(alias = "touchpadForce", usage = "Secondary2DAxisForce")]
        public AxisControl trackpadForce { get; private set; }

#if UNITY_OPENXR_1_9_0_OR_NEWER
        [Preserve, InputControl(usage = "Haptic")]
        public UnityEngine.XR.OpenXR.Input.HapticControl haptic { get; private set; }
#endif

        protected override void FinishSetup()
        {
            base.FinishSetup();

            devicePose = GetChildControl<PoseControl>(nameof(devicePose));
            pointer = GetChildControl<PoseControl>(nameof(pointer));

            isTracked = GetChildControl<ButtonControl>(nameof(isTracked));
            trackingState = GetChildControl<IntegerControl>(nameof(trackingState));
            devicePosition = GetChildControl<Vector3Control>(nameof(devicePosition));
            deviceRotation = GetChildControl<QuaternionControl>(nameof(deviceRotation));
            deviceVelocity = GetChildControl<Vector3Control>(nameof(deviceVelocity));
            deviceAngularVelocity = GetChildControl<Vector3Control>(nameof(deviceAngularVelocity));
            pointerPosition = GetChildControl<Vector3Control>(nameof(pointerPosition));
            pointerRotation = GetChildControl<QuaternionControl>(nameof(pointerRotation));

            gripPressed = GetChildControl<ButtonControl>(nameof(gripPressed));
            menu = GetChildControl<ButtonControl>(nameof(menu));
            trigger = GetChildControl<AxisControl>(nameof(trigger));
            triggerPressed = GetChildControl<ButtonControl>(nameof(triggerPressed));
            trackpad = GetChildControl<Vector2Control>(nameof(trackpad));
            trackpadClicked = GetChildControl<ButtonControl>(nameof(trackpadClicked));
            trackpadTouched = GetChildControl<ButtonControl>(nameof(trackpadTouched));
            trackpadForce = GetChildControl<AxisControl>(nameof(trackpadForce));

#if UNITY_OPENXR_1_9_0_OR_NEWER
            haptic = GetChildControl<UnityEngine.XR.OpenXR.Input.HapticControl>(nameof(haptic));
#endif
        }
    }

    /// <summary>
    /// Manages simulated Magic Leap Controller device state.
    /// </summary>
    public class MagicLeapSimulatedController : IDisposable
    {
        private readonly MagicLeapController simulatedController = null;

        /// <summary>
        /// Returns the current camera relative target pose of the simulated controller.
        /// </summary>
        public Pose CameraRelativeTargetPose { get; private set; } = Pose.identity;

        /// <summary>
        /// Returns the current position, in world space, of the simulated controller.
        /// </summary>
        public Vector3 WorldPosition =>
            PlayspaceUtilities.XROrigin.CameraFloorOffsetObject.transform.TransformPoint(deviceLocalPose.position);

        /// <summary>
        /// Returns the current rotation, in world space, of the simulated controller.
        /// </summary>
        public Quaternion WorldRotation =>
            PlayspaceUtilities.XROrigin.CameraFloorOffsetObject.transform.rotation * deviceLocalPose.rotation;

        /// <summary>
        /// The handedness (ex: Left or Right) assigned to the simulated controller.
        /// </summary>
        public Handedness Handedness { get; private set; } = Handedness.Right;

        /// <summary>
        /// Initializes a new instance of a <see cref="MagicLeapSimulatedController"/> class.
        /// </summary>
        public MagicLeapSimulatedController(
            Handedness handedness,
            Vector3 initialRelativePosition,
            float rayHalfLife = 0.01f)
        {
            Handedness = handedness;

            simulatedController = InputSystem.AddDevice<MagicLeapController>();
            if (simulatedController == null)
            {
                Debug.LogError($"Failed to create the {typeof(MagicLeapController)}.");
                return;
            }

            CameraRelativeTargetPose = new Pose(initialRelativePosition, Quaternion.identity);

            SetUsage();

            SetRelativePoseWithOffset(
                CameraRelativeTargetPose,
                Vector3.zero,
                ControllerRotationMode.UserControl);
        }

        ~MagicLeapSimulatedController()
        {
            Dispose();
        }

        /// <summary>
        /// Cleans up references to resources used by the camera simulation.
        /// </summary>
        public void Dispose()
        {
            if ((simulatedController != null) && simulatedController.added)
            {
                UnsetUsage();
                InputSystem.RemoveDevice(simulatedController);
            }
            GC.SuppressFinalize(this);
        }

        private static readonly ProfilerMarker UpdatePerfMarker =
            new ProfilerMarker("MagicLeapSimulatedController.Update");

        // Smoothing time for the controller position.
        private const float MoveSmoothingTime = 0.02f;

        // Smoothed move delta.
        private Vector3 smoothedMoveDelta = Vector3.zero;

        // Device rotation Euler angles (for more controlled/clamped rotation)
        private float yawAngle = 0f;
        private float pitchAngle = 0f;
        private float rollAngle = 0f;

        /// <summary>
        /// Update the controller simulation with relative per-frame delta position and rotation.
        /// </summary>
        /// <param name="moveDelta">The change in the controller position.</param>
        /// <param name="rotationDelta">The change in the controller rotation, represented by a (pitch, yaw, roll) Vector3.</param>
        /// <param name="controls">The desired state of the controller's controls.</param>
        /// <param name="rotationMode">The <see cref="ControllerRotationMode"/> in which the controller is operating.</param>
        /// <param name="isMovementSmoothed">Set to <see langword="true"/> to smooth controller movement along the Z axis.</param>
        /// <param name="depthSensitivity">The sensitivity multiplier for depth movement.</param>
        /// <param name="jitterStrength">How strong should be the simulated controller jitter (inaccuracy)?</param>
        public void UpdateRelative(
            Vector3 moveDelta,
            Vector3 rotationDelta,
            MagicLeapSimulatedControllerControls controls,
            ControllerRotationMode rotationMode,
            bool isMovementSmoothed = true,
            float depthSensitivity = 1f,
            float jitterStrength = 0f)
        {
            using (UpdatePerfMarker.Auto())
            {
                if (simulatedController == null) 
                {
                    return; 
                }

                if (controls.IsTracked)
                {
                    // Apply depth sensitivity
                    moveDelta.z *= depthSensitivity;

                    // Perform smoothing on the move delta.
                    // This is not framerate independent due to an apparent Unity editor issue
                    // where the *polling rate* of the mouse can cause lag in the editor. This
                    // causes delta times to vary dramatically while moving the mouse.
                    // Using fixedDeltaTime is a reasonable workaround until this issue is resolved.

                    // Only smooth on z for now. Smoothing on other axes causes problems with
                    // the screen-space positioning logic.
                    smoothedMoveDelta = new Vector3(moveDelta.x, moveDelta.y,
                        isMovementSmoothed ? Smoothing.SmoothTo(
                        smoothedMoveDelta.z,
                        moveDelta.z,
                        MoveSmoothingTime, Time.fixedDeltaTime) : moveDelta.z);

                    // This value helps control jitter severity.
                    const float jitterSeverity = 0.002f;
                    Vector3 jitter = jitterStrength * jitterSeverity * UnityEngine.Random.insideUnitSphere;

                    // Calculate new pitch, yaw, roll values from delta.
                    yawAngle += rotationDelta.y;
                    pitchAngle += rotationDelta.x;
                    // Clamp pitch between down/up vertical values.
                    pitchAngle = Mathf.Clamp(pitchAngle, -90f, 90f);
                    rollAngle += rotationDelta.z;

                    CameraRelativeTargetPose = new Pose(
                        CameraRelativeTargetPose.position + smoothedMoveDelta + jitter,

                        // If we not have been told to face the camera, apply the rotation delta.
                        rotationMode == ControllerRotationMode.UserControl ?
                            Quaternion.Euler(pitchAngle, yawAngle, rollAngle) :
                            CameraRelativeTargetPose.rotation
                    );

                    SetRelativePoseWithOffset(CameraRelativeTargetPose, Vector3.zero, rotationMode);
                }

                ApplyState(controls);
            }
        }

        /// <summary>
        /// Updates this controller with the current values in <paramref name="controls"/>.
        /// </summary>
        /// <remarks>Often used to update tracking state without changing the controller's pose.</remarks>
        /// <param name="controls">Persistent controls data to apply.</param>
        public void UpdateControls(MagicLeapSimulatedControllerControls controls)
        {
            if (simulatedController == null) 
            { 
                return; 
            }

            ApplyState(controls);
        }

        private static readonly ProfilerMarker UpdateAbsolutePerfMarker =
            new ProfilerMarker("MagicLeapSimulatedController.UpdateAbsolute");

        /// <summary>
        /// Update the controller simulation with a specified absolute pose in world-space.
        /// </summary>
        /// <param name="worldPose">The world space controller pose.</param>
        /// <param name="controls">The desired state of the controller's controls.</param>
        /// <param name="rotationMode">The <see cref="ControllerRotationMode"/> in which the controller is operating.</param>
        public void UpdateAbsolute(
            Pose worldPose,
            MagicLeapSimulatedControllerControls controls,
            ControllerRotationMode rotationMode)
        {
            using (UpdateAbsolutePerfMarker.Auto())
            {
                if (simulatedController == null) 
                { 
                    return; 
                }

                SetWorldPose(worldPose, rotationMode);
                ApplyState(controls);
            }
        }

        private static readonly ProfilerMarker ApplyStatePerfMarker =
            new ProfilerMarker("MagicLeapSimulatedController.ApplyState");

        private Pose deviceLocalPose = Pose.identity;

        private void ApplyState(MagicLeapSimulatedControllerControls controls)
        {
            using (ApplyStatePerfMarker.Auto())
            {
                using (StateEvent.FromDefaultStateFor(simulatedController, out var eventPtr))
                {
                    simulatedController.isTracked.WriteValueIntoEvent(controls.IsTracked ? 1f : 0f, eventPtr);
                    simulatedController.trackingState.WriteValueIntoEvent((int)(controls.TrackingState), eventPtr);

                    simulatedController.pointerPosition.WriteValueIntoEvent(deviceLocalPose.position, eventPtr);
                    simulatedController.pointerRotation.WriteValueIntoEvent(deviceLocalPose.rotation, eventPtr);

                    simulatedController.devicePosition.WriteValueIntoEvent(deviceLocalPose.position, eventPtr);
                    simulatedController.deviceRotation.WriteValueIntoEvent(deviceLocalPose.rotation, eventPtr);

                    simulatedController.gripPressed.WriteValueIntoEvent(controls.GripButton ? 1f : 0f, eventPtr);
                    simulatedController.menu.WriteValueIntoEvent(controls.MenuButton ? 1f : 0f, eventPtr);

                    simulatedController.trigger.WriteValueIntoEvent(controls.TriggerAxis, eventPtr);
                    simulatedController.triggerPressed.WriteValueIntoEvent(controls.TriggerButton ? 1f : 0f, eventPtr);

                    simulatedController.trackpad.WriteValueIntoEvent(controls.TrackpadPosition, eventPtr);
                    simulatedController.trackpadClicked.WriteValueIntoEvent(controls.TrackpadClicked ? 1f : 0f, eventPtr);
                    simulatedController.trackpadTouched.WriteValueIntoEvent(controls.TrackpadTouched ? 1f : 0f, eventPtr);
                    simulatedController.trackpadForce.WriteValueIntoEvent(controls.TrackpadForce, eventPtr);

                    // Queue event.
                    InputSystem.QueueEvent(eventPtr);
                }
            }
        }

        /// <summary>
        /// Sets a controller to the specified camera-relative pose.
        /// </summary>
        /// <param name="cameraRelativePose">The desired controller pose, in camera relative space.</param>
        /// <param name="offset">The amount to offset the controller, in world space</param>
        /// <param name="rotationMode">The <see cref="ControllerRotationMode"/> in which the controller is operating.</param>
        /// <remarks>
        /// <para>
        /// The incoming camera relative space pose is first being transformed into world space because the
        /// camera relative space is not necessarily the same as the relative space of the main MRTK game object rig; 
        /// this is due to the offset game object in between.
        /// <br/>
        /// This will Transform the parameters into world space and call <see cref="SetWorldPose"/>, where the parameters will be transformed into rig relative space.
        /// </para>
        /// </remarks>
        private void SetRelativePoseWithOffset(
            Pose cameraRelativePose,
            Vector3 offset,
            ControllerRotationMode rotationMode)
        {
            Pose worldPose = new Pose(
                Camera.main.transform.TransformPoint(cameraRelativePose.position) + offset,
                Camera.main.transform.rotation * cameraRelativePose.rotation
            );

            SetWorldPose(worldPose, rotationMode);
        }

        private static readonly ProfilerMarker SetWorldPosePerfMarker =
            new ProfilerMarker("MagicLeapSimulatedController.SetWorldPose");

        /// <summary>
        /// Sets a controller to the specified world space pose.
        /// </summary>
        /// <param name="worldPose">The desired controller pose, in world space.</param>
        /// <param name="rotationMode">The <see cref="ControllerRotationMode"/> in which the controller is operating.</param>
        private void SetWorldPose(
            Pose worldPose,
            ControllerRotationMode rotationMode)
        {
            using (SetWorldPosePerfMarker.Auto())
            {
                Pose cameraOffsetLocalPose = PlayspaceUtilities.InverseTransformPose(worldPose);
                SetCameraOffsetLocalPose(cameraOffsetLocalPose, rotationMode);
            }
        }

        private static readonly ProfilerMarker SetRigLocalPosePerfMarker =
            new ProfilerMarker("MagicLeapSimulatedController.SetRigLocalPose");

        /// <summary>
        /// Sets a controller to the specified camera-offset-local pose.
        /// </summary>
        /// <param name="localPose">The desired controller pose, in camera-offset-local space.</param>
        /// <param name="rotationMode">The <see cref="ControllerRotationMode"/> in which the controller is operating.</param>
        private void SetCameraOffsetLocalPose(
            Pose localPose,
            ControllerRotationMode rotationMode)
        {
            using (SetRigLocalPosePerfMarker.Auto())
            {
                deviceLocalPose.position = localPose.position;

                if (rotationMode == ControllerRotationMode.FaceCamera)
                {
                    Quaternion localLookAt = Quaternion.LookRotation(Camera.main.transform.localPosition - localPose.position);
                    deviceLocalPose.rotation = Smoothing.SmoothTo(
                        deviceLocalPose.rotation,
                        localLookAt,
                        0.1f,
                        MoveSmoothingTime);
                }
                else if (rotationMode == ControllerRotationMode.CameraAligned)
                {
                    deviceLocalPose.rotation = Smoothing.SmoothTo(
                        deviceLocalPose.rotation,
                        Camera.main.transform.localRotation,
                        0.1f,
                        MoveSmoothingTime);
                }
                else
                {
                    deviceLocalPose.rotation = localPose.rotation;
                }
            }
        }

        private void SetUsage()
        {
            Debug.Assert(
                (simulatedController != null) && simulatedController.added,
                "Cannot set device usage: simulated controller is either null or has not been added to the input system.");

            InputSystem.SetDeviceUsage(
                simulatedController,
                (Handedness == Handedness.Left) ?
                    UnityEngine.InputSystem.CommonUsages.LeftHand :
                    UnityEngine.InputSystem.CommonUsages.RightHand);
        }

        private void UnsetUsage()
        {
            Debug.Assert(
                (simulatedController != null) && simulatedController.added,
                "Cannot unset device usage: simulated controller is either null or has not been added to the input system.");

            InputSystem.RemoveDeviceUsage(
                simulatedController,
                Handedness == Handedness.Left ?
                    UnityEngine.InputSystem.CommonUsages.LeftHand :
                    UnityEngine.InputSystem.CommonUsages.RightHand);
        }
    }

    /// <summary>
    /// Set of Magic Leap Controller input controls supported by the Magic Leap input simulator.
    /// This is a class (instead of a struct), so that IEnumerator-based tests
    /// can obtain a reference to these controls. (Iterators cannot have ref-locals.)
    /// </summary>
    public class MagicLeapSimulatedControllerControls
    {
        /// <summary>
        /// Indicates whether or not the controller is tracked.
        /// </summary>
        public bool IsTracked { get; internal set; }

        /// <summary>
        /// The tracked values (ex: position, rotation) for this controller.
        /// </summary>
        public InputTrackingState TrackingState { get; internal set; }

        /// <summary>
        /// Axis implemented trigger control.
        /// </summary>
        public float TriggerAxis { get; internal set; }

        /// <summary>
        /// Button implemented trigger control.
        /// </summary>
        public bool TriggerButton { get; internal set; }

        /// <summary>
        /// Button implemented grip (bumper) control.
        /// </summary>
        public bool GripButton { get; internal set; }

        /// <summary>
        /// Button implemented menu control.
        /// </summary>
        public bool MenuButton { get; internal set; }

        /// <summary>
        /// Vector2 implemented trackpad position control.
        /// </summary>
        public Vector2 TrackpadPosition { get; internal set; }

        /// <summary>
        /// Button implemented trackpad clicked control.
        /// </summary>
        public bool TrackpadClicked { get; internal set; }

        /// <summary>
        /// Button implemented trackpad touched control.
        /// </summary>
        public bool TrackpadTouched { get; internal set; }

        /// <summary>
        /// Axis implemented trackpad force control.
        /// </summary>
        public float TrackpadForce { get; internal set; }

        /// <summary>
        /// Resets the control state to initial conditions.
        /// </summary>
        public void Reset()
        {
            IsTracked = false;
            TrackingState = InputTrackingState.None;

            TriggerAxis = default;
            TriggerButton = default;
            GripButton = default;
            MenuButton = default;

            TrackpadPosition = default;
            TrackpadClicked = default;
            TrackpadTouched = default;
            TrackpadForce = default;
        }
    }
}