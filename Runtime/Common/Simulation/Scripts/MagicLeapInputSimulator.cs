// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Input.Simulation;
using MagicLeap.MRTK.Settings;

namespace MagicLeap.MRTK.Input.Simulation
{
    /// <summary>
    /// Magic Leap input simulator.
    /// </summary>
    public class MagicLeapInputSimulator : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The handedness to use for the simulated Magic Leap Controller.  Must be Left or Right.")]
        private Handedness controllerHandedness;

        /// <summary>
        /// The handedness to use for the simulated Magic Leap Controller. Must be <see cref="Handedness.Left"/>
        /// or <see cref="Handedness.Right"/>.
        /// </summary>
        public Handedness ControllerHandedness => controllerHandedness;

        [SerializeField]
        [Tooltip("The settings used to configure and control the simulated controller.")]
        private MagicLeapSimulatedControllerSettings controllerSettings;

        /// <summary>
        /// The settings used to configure and control the simulated controller.
        /// </summary>
        public MagicLeapSimulatedControllerSettings ControllerSettings
        {
            get => controllerSettings;
            set => controllerSettings = value;
        }

        [SerializeField]
        [Tooltip("List of InputActionReferences and corresponding override paths for each.")]
        private List<InputActionOverride> actionPathOverrides = new List<InputActionOverride>();

        /// <summary>
        /// List of <see cref="InputActionReference"/> and corresponding override paths for each.
        /// </summary>
        public List<InputActionOverride> ActionPathOverrides => actionPathOverrides;

        [SerializeField]
        [Tooltip("The Magic Leap Controller prefab to add to the rig in Editor play mode if no ML Controller is detected within the rig.")]
        private GameObject magicLeapControllerPrefab;

        /// <summary>
        /// The Magic Leap Controller prefab to add to the rig in Editor play mode if no ML Controller is detected within the rig.
        /// </summary>
        public GameObject MagicLeapControllerPrefab => magicLeapControllerPrefab;

        [SerializeField]
        [Tooltip("Whether to swap out any FlatScreenModeDetector with the MagicLeapFlatScreenModeDetector, for better support of Magic Leap devices in Editor play mode.")]
        private bool swapOutFlatScreenModeDetector = true;

        /// <summary>
        /// Whether to swap out any FlatScreenModeDetector with the MagicLeapFlatScreenModeDetector, for better support of Magic Leap devices in Editor play mode.
        public bool SwapOutFlatScreenModeDetector => swapOutFlatScreenModeDetector;

        [SerializeField]
        [Tooltip("The optional prefab to instantiate to provide on screen UI controls.")]
        private GameObject simulatorUIPrefab;

        /// <summary>
        /// The optional prefab to instantiate to provide on screen UI controls.
        /// </summary>
        public GameObject SimulatorUIPrefab => simulatorUIPrefab;

        private MagicLeapInputSimulatorUI simulatorUI;

#if UNITY_EDITOR  // Ensure MagicLeapInputSimulator only runs in the Editor

        private const string FlatScreenModeName = "FlatScreen";
        private const string FlatScreenModeDetectorTypeName = "FlatScreenModeDetector";

        private void Start()
        {
            // Override input action paths to enable better binding in the Editor play session where needed.
            foreach (var actionOverride in ActionPathOverrides)
            {
                actionOverride.actionRef?.action?.ApplyBindingOverride(actionOverride.overridePath);
            }

            // Whether we've made changes that necessitate initializing the InteractionModeManager's mode detectors.
            bool initializeInteractionModeDetectors = false;

            // Add the ML Controller rig prefab when no existing ML controller detected.
            if (!MRTKRigUtils.TryFindMagicLeapController(out _) && magicLeapControllerPrefab != null && PlayspaceUtilities.XROrigin != null)
            {
                Instantiate(magicLeapControllerPrefab, PlayspaceUtilities.XROrigin.CameraFloorOffsetObject.transform);
                initializeInteractionModeDetectors = true;
            }

            if (swapOutFlatScreenModeDetector && MRTKRigUtils.TryFindMRTKRigParent(out GameObject mrtkRigParent))
            {
                // Unfortunately, FlatScreenModeDetector is an internal class, so must attempt to find it in a roundabout way.
                foreach (IInteractionModeDetector detector in mrtkRigParent.GetComponentsInChildren<IInteractionModeDetector>())
                {
                    // If we've detected a FlatScreenModeDetector, swap it out
                    if (detector.ModeOnDetection.Name.Equals(FlatScreenModeName) &&
                        detector.TryGetMonoBehaviour(out var behavior) &&
                        behavior.GetType().Name.Equals(FlatScreenModeDetectorTypeName) &&
                        behavior is not MagicLeapFlatScreenModeDetector)
                    {
                        var mlFlatScreenModeDetector = behavior.gameObject.AddComponent<MagicLeapFlatScreenModeDetector>();
                        mlFlatScreenModeDetector.FlatScreenInteractionMode = detector.ModeOnDetection;
#if MRTK_INPUT_4_0_0_OR_NEWER
                        mlFlatScreenModeDetector.Controllers = detector.GetInteractorGroups();
#else
                        mlFlatScreenModeDetector.Controllers = detector.GetControllers();
#endif

                        // Remove the old flat screen mode detector
                        // Must destroy immediately so that it won't be picked up in initialization of mode detectors.
                        DestroyImmediate(behavior);
                        initializeInteractionModeDetectors = true;
                    }
                }
            }

            // Initialize the InteractionModeManager's mode detectors if needed.
            if (initializeInteractionModeDetectors)
            {
                InteractionModeManager modeManager = InteractionModeManager.Instance;
                if (modeManager != null)
                {
                    modeManager.InitializeInteractionModeDetectors();
                }
            }

            // Instantiate simulator ui
            if (simulatorUIPrefab != null)
            {
                var prefabObject = Instantiate(simulatorUIPrefab, transform);
                simulatorUI = prefabObject != null ? prefabObject.GetComponent<MagicLeapInputSimulatorUI>() : null;
                if (simulatorUI != null)
                {
                    simulatorUI.gameObject.SetActive(false);

                    simulatorUI.LatchChanged += (latched) => {
                        if (controllerSettings != null)
                        {
                            controllerSettings.ToggledState = latched;
                        }
                    };
                }
                else
                {
                    Debug.LogError("Unable to obtain a reference to a MagicLeapInputSimulatorUI component in the UI prefab, on screen UI may not work.");
                }
            }
        }

        private void Update()
        {
            UpdateSimulatedController();
        }

        private void OnDisable()
        {
            DisableSimulatedController();
        }

#endif // UNITY_EDITOR

        private MagicLeapSimulatedController simulatedController = null;
        private readonly MagicLeapSimulatedControllerControls controllerControls = new();

        private float triggerSmoothVelocity;
        private readonly bool shouldUseTriggerButton = false;
        private readonly float triggerSmoothTime = 0.05f;
        private readonly float triggerSmoothDeadzone = 0.005f;
        private bool mouseScreenPositionMatchEnabled = true;

        private void EnableSimulatedController(
            Handedness handedness,
            Vector3 startPosition)
        {
            if (!IsSupportedHandedness(handedness))
            {
                Debug.LogError($"Unable to enable simulated controller. Unsupported handedness ({handedness}).");
                return;
            }

            if (simulatedController == null)
            {
                simulatedController = new MagicLeapSimulatedController(handedness, startPosition);
                controllerControls.Reset();
            }

            if (simulatorUI != null)
            {
                simulatorUI.gameObject.SetActive(true);
                simulatorUI.ClearState();
                simulatorUI.SetLatched(ControllerSettings.ToggledState);
            }
        }

        private void DisableSimulatedController()
        {
            if (simulatedController != null)
            {
                simulatedController.Dispose();
                simulatedController = null;
            }

            if (simulatorUI != null)
            {
                simulatorUI.gameObject.SetActive(false);
            }
        }

        private static readonly ProfilerMarker UpdateSimulatedControllerPerfMarker =
            new ProfilerMarker("MagicLeapInputSimulator.UpdateSimulatedController");

        private void UpdateSimulatedController()
        {
            using (UpdateSimulatedControllerPerfMarker.Auto())
            {
                if (controllerSettings == null) 
                { 
                    return; 
                }

                // Has the user toggled latched tracking?
                if (controllerSettings.Toggle.action.WasPerformedThisFrame())
                {
                    controllerSettings.ToggledState = !controllerSettings.ToggledState;
                    if (simulatorUI != null)
                    {
                        simulatorUI.SetLatched(ControllerSettings.ToggledState);
                    }
                }

                // Is momentary tracking enabled?
                bool isTracked = controllerSettings.Track.action.IsPressed();

                if (controllerSettings.ToggledState || isTracked)
                {
                    if (simulatedController == null)
                    {
                        // Get the start position for the controller.
                        Vector3 startPosition = controllerSettings.DefaultPosition;
                        // Set the X position based on handedness
                        float rightSideHandedness = startPosition.x >= 0f ? 1.0f : -1.0f;
                        startPosition.x *= controllerHandedness == Handedness.Right ? rightSideHandedness : -rightSideHandedness;

                        // We only match the mouse screen position when momentary tracking.
                        mouseScreenPositionMatchEnabled = isTracked && !controllerSettings.ToggledState;
                        if (mouseScreenPositionMatchEnabled)
                        {
                            Vector3 screenPos = new Vector3(
                                Mouse.current.position.ReadValue().x,
                                Mouse.current.position.ReadValue().y,
                                controllerSettings.DefaultPosition.z);
                            startPosition = ScreenToCameraRelative(screenPos);
                        }

                        // Create the simulated controller.
                        EnableSimulatedController(controllerHandedness, startPosition);
                    }
                }
                else
                {
                    DisableSimulatedController();
                }

                if (simulatedController == null) 
                { 
                    return; 
                }

                bool isControlledByMouse = controllerSettings.MoveHorizontal.action.RaisedByMouse() ||
                                           controllerSettings.MoveVertical.action.RaisedByMouse();

                bool isControllingRotation = controllerSettings.Pitch.action.IsPressed() ||
                                             controllerSettings.Yaw.action.IsPressed() ||
                                             controllerSettings.Roll.action.IsPressed() ||
                                             MagicLeapInputSimulatorUI.TransformingYawPitch ||
                                             MagicLeapInputSimulatorUI.TransformingRoll;

                Vector3 positionDelta = Vector3.zero;
                Vector3 rotationDelta = Vector3.zero;

                // Update the rotation mode if the user wants to face the camera
                if (controllerSettings.FaceTheCamera.action.WasPerformedThisFrame())
                {
                    controllerSettings.RotationMode = (controllerSettings.RotationMode == ControllerRotationMode.FaceCamera) ?
                        ControllerRotationMode.CameraAligned : ControllerRotationMode.FaceCamera;
                }
                else if (isControllingRotation)
                {
                    // Store rotation delta as a Vector3 representing (pitch, yaw, roll) delta values.
                    rotationDelta = new Vector3(
                        // Unity appears to invert the controller pitch by default (move forward to look down)
                        (controllerSettings.Pitch.action.ReadValue<float>() + MagicLeapInputSimulatorUI.TransformYawPitchDelta.y) * (!controllerSettings.InvertPitch ? -1 : 1),
                        controllerSettings.Yaw.action.ReadValue<float>() + MagicLeapInputSimulatorUI.TransformYawPitchDelta.x,
                        controllerSettings.Roll.action.ReadValue<float>() + MagicLeapInputSimulatorUI.TransformRollDelta);

                    if (rotationDelta != Vector3.zero) 
                    { 
                        controllerSettings.RotationMode = ControllerRotationMode.UserControl;
                    }

                    // After the user has modified controller rotation, disable mouse screen position matching since the mouse has moved 
                    // relative to the controller location during rotation.
                    mouseScreenPositionMatchEnabled = false;
                }
                else
                {
                    // If controlling using mouse, the controller is momentarily enabled, and matching mouse screen position is still enabled,
                    // match the mouse screen position.
                    if (isControlledByMouse && !controllerSettings.ToggledState && mouseScreenPositionMatchEnabled)
                    {
                        Vector3 mouseScreenPos = new Vector3(
                            Mouse.current.position.ReadValue().x,
                            Mouse.current.position.ReadValue().y,
                            controllerSettings.DefaultPosition.z);

                        Vector3 inputPosition = ScreenToCameraRelative(mouseScreenPos);

                        positionDelta = inputPosition - simulatedController.CameraRelativeTargetPose.position;
                        positionDelta.z = controllerSettings.MoveDepth.action.ReadValue<float>();
                    }
                    else
                    {
                        positionDelta = new Vector3(
                            controllerSettings.MoveHorizontal.action.ReadValue<float>() + MagicLeapInputSimulatorUI.TransformXYDelta.x,
                            controllerSettings.MoveVertical.action.ReadValue<float>() + MagicLeapInputSimulatorUI.TransformXYDelta.y,
                            controllerSettings.MoveDepth.action.ReadValue<float>() + MagicLeapInputSimulatorUI.TransformDepthDelta);
                    }
                }

                // Update simulated controller input controls
                controllerControls.TrackpadClicked = MagicLeapInputSimulatorUI.TouchpadPressed;
                controllerControls.TrackpadTouched = MagicLeapInputSimulatorUI.TouchpadPressed;
                controllerControls.TrackpadForce = MagicLeapInputSimulatorUI.TouchpadForce;
                controllerControls.TrackpadPosition = MagicLeapInputSimulatorUI.TouchpadPosition;

                bool triggerIsPressed = controllerSettings.TriggerButton.action.IsPressed() || MagicLeapInputSimulatorUI.TriggerPressed;
                float triggerTargetValue = triggerIsPressed ? 1 : 0;

                controllerControls.TriggerAxis = Mathf.SmoothDamp(controllerControls.TriggerAxis,
                                                                  triggerTargetValue,
                                                                  ref triggerSmoothVelocity,
                                                                  triggerSmoothTime);

                if (Mathf.Abs(controllerControls.TriggerAxis - triggerTargetValue) < triggerSmoothDeadzone)
                {
                    controllerControls.TriggerAxis = triggerTargetValue;
                }

                if (shouldUseTriggerButton)
                {
                    controllerControls.TriggerButton = triggerIsPressed;
                }
                else
                {
                    controllerControls.TriggerButton = controllerControls.TriggerAxis >= .75f;
                }

                controllerControls.MenuButton = controllerSettings.MenuButton.action.IsPressed() || MagicLeapInputSimulatorUI.MenuPressed;
                controllerControls.GripButton = controllerSettings.GripButton.action.IsPressed() || MagicLeapInputSimulatorUI.BumperPressed;

                controllerControls.IsTracked = controllerSettings.ToggledState || isTracked;
                controllerControls.TrackingState = controllerControls.IsTracked ?
                    (InputTrackingState.Position | InputTrackingState.Rotation) : InputTrackingState.None;

                simulatedController.UpdateRelative(
                    positionDelta,
                    rotationDelta,
                    controllerControls,
                    controllerSettings.RotationMode,
                    controllerSettings.IsMovementSmoothed,
                    controllerSettings.DepthSensitivity,
                    controllerSettings.JitterStrength);
            }
        }

        private Vector3 ScreenToCameraRelative(Vector3 screenPos)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            return Camera.main.transform.InverseTransformPoint(worldPos);
        }

        private bool IsSupportedHandedness(Handedness handedness)
        {
            return !((handedness != Handedness.Left) && (handedness != Handedness.Right));
        }
    }
}
