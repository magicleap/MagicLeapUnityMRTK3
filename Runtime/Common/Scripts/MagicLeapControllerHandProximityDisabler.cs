// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine.XR;
using MagicLeap.MRTK.Settings;
using MagicLeap.MRTK.Input;
using System.Collections.Generic;
using UnityEngine.InputSystem.XR;

namespace MagicLeap.MRTK
{
    /// <summary>
    /// Component to manage the enabled state modality of hand controllers that may
    /// be holding the MagicLeap Controller.
    /// </summary>
    /// <remarks>
    /// The closest hand, that is within the proximity threshold, will have its enabled state
    /// and functionality managed based on the multi-modal option in settings.
    /// </remarks>
    public class MagicLeapControllerHandProximityDisabler : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The hand proximity threshold, in meters, under which a hand controller is " +
            "considered possibly holding the Magic Leap Controller.")]
        private float handProximityThreshold = .2f;

        /// <summary>
        /// The hand proximity threshold, in meters, under which a hand controller is
        /// considered possibly holding the Magic Leap Controller.
        /// </summary>
        public float HandProximityThreshold
        {
            get => handProximityThreshold;
            set => handProximityThreshold = value;
        }

        [SerializeField]
        [Tooltip("The time threshold, in seconds, that the closest hand controller target must remain " +
            "consistent before switching the current closest hand controller. This prevents quick " +
            "successive changes.")]
        private float handSwitchTimeThreshold = 0.25f;

        /// <summary>
        /// The time threshold, in seconds, that the closest hand controller target must remain
        /// consistent before switching the current closest hand controller. This prevents quick 
        /// successive changes.
        /// </summary>
        public float HandSwitchTimeThreshold 
        {
            get => handSwitchTimeThreshold;
            set => handSwitchTimeThreshold = value;
        }

        [SerializeField]
        [Tooltip("Whether to ignore controller and hand proximity in Editor play mode.")]
        private bool ignoreProximityInEditorPlayMode = true;

        /// <summary>
        /// Whether to ignore controller and hand proximity in Editor play mode.
        /// </summary>
        public bool IgnoreProximityInEditorPlayMode => ignoreProximityInEditorPlayMode;


        private Dictionary<XRNode, GameObject> handControllers;
        private GameObject currentClosestHandController = null;
        private GameObject targetClosestHandController = null;
        private float closestHandSwitchTimer = 0.0f;
#pragma warning disable CS0618 // ActionBasedController is obsolete
        private ActionBasedController actionBasedController = null;
#pragma warning restore CS0618 // ActionBasedController is obsolete
        private TrackedPoseDriver trackedPoseDriver = null;
        private HandControllerMultimodalTypeOption handControllerMultimodalType = HandControllerMultimodalTypeOption.HandHoldingControllerFullyDisabled;
        private bool magicLeapControllerTracking = false;

        private HandsAggregatorSubsystem HandSubsystem => XRSubsystemHelpers.HandsAggregator as HandsAggregatorSubsystem;

        private void Awake()
        {
            if (Application.isEditor && ignoreProximityInEditorPlayMode)
            {
                handControllerMultimodalType = HandControllerMultimodalTypeOption.HandsAndControllerAlwaysActive;
            }
            else
            {
                handControllerMultimodalType = MagicLeapAuxiliaryHandUtils.GetHandControllerMultimodalTypeSettingWithDefault();
            }

            if (handControllerMultimodalType == HandControllerMultimodalTypeOption.HandsAndControllerAlwaysActive)
            {
                // Disable the script, hands and controller always active
                enabled = false;
            }

            // Attempt to obtain references to the top level tracking behaviors for the ML Controller hierarchy.
            // The behavior used for tracking may vary depending on the version of this prefab being used.
#pragma warning disable CS0618 // ActionBasedController is obsolete
            actionBasedController = GetComponent<ActionBasedController>();
#pragma warning restore CS0618 // ActionBasedController is obsolete
            trackedPoseDriver = GetComponent<TrackedPoseDriver>();
        }

        private void Start()
        {
            MRTKRigUtils.TryFindHandControllers(out handControllers);
        }

        private void OnDisable()
        {
            SetClosestHandController(null);
        }

        private bool GetControllerIsTracking()
        {
            if (actionBasedController != null)
            {
                return actionBasedController.currentControllerState.inputTrackingState.HasPositionAndRotation();
            }
            else if (trackedPoseDriver != null)
            {
                return ((InputTrackingState)(trackedPoseDriver.trackingStateInput.action?.ReadValue<int>() ?? default)).HasPositionAndRotation();
            }

            return false;
        }

        void Update()
        {
            // Update controller tracking state changes
            bool isControllerTracking = GetControllerIsTracking();
            bool controllerTrackingStateChanged = magicLeapControllerTracking != isControllerTracking;
            magicLeapControllerTracking = isControllerTracking;

            // Handle the developer setting which disables both hands when the controller is tracking, and the state has changed.
            if (controllerTrackingStateChanged &&
                handControllerMultimodalType == HandControllerMultimodalTypeOption.HandsDisabledWhileControllerActive)
            {
                // Note: The InputTrackingState is not immediately set to None, when the controller is set down
                foreach (var (_, controller) in handControllers)
                {
                    ActivateHand(controller, !magicLeapControllerTracking);
                }
                return;
            }

            // Handle the controller to hand proximity calculation
            GameObject newClosestHandController = null;
            if (magicLeapControllerTracking && HandSubsystem != null)
            {
                float closestDistance = float.MaxValue;
                Vector3 mlControllerPosition = transform.position;

                foreach (var (handNode, controller) in handControllers)
                {
                    // Note: Using palm joint position for hand position
                    if (HandSubsystem.TryGetJoint(TrackedHandJoint.Palm,
                                                  handNode,
                                                  out HandJointPose pose))
                    {
                        Vector3 handPosition = pose.Position;
                        float distance = Vector3.Distance(handPosition, mlControllerPosition);
                        // Favor the current closest hand
                        float compareDistance = distance * (currentClosestHandController == controller ? .8f : 1.0f);
                        if (distance < HandProximityThreshold && compareDistance < closestDistance)
                        {
                            newClosestHandController = controller;
                            closestDistance = compareDistance;
                        }
                    }
                }
            }

            // Filter quick successive changes by requiring the new closest hand controller target
            // to be consistent beyond a time threshold.
            if (newClosestHandController == targetClosestHandController)
            {
                closestHandSwitchTimer += Time.deltaTime;
            }
            else
            {
                targetClosestHandController = newClosestHandController;
                closestHandSwitchTimer = 0.0f;
            }

            // Switch the current closest hand if:
            // The target has changed from current, and
            // - The controller tracking state has changed this frame, or
            // - Transitioning from no hand to a hand, or
            // - The time threshold is met to switch hands or go to no hand
            if (targetClosestHandController != currentClosestHandController && (
                controllerTrackingStateChanged ||
                currentClosestHandController == null ||
                closestHandSwitchTimer >= HandSwitchTimeThreshold))
            {
                SetClosestHandController(targetClosestHandController);
            }
        }

        private void SetClosestHandController(GameObject newClosestHandController)
        {
            bool handRayDisabled = handControllerMultimodalType == HandControllerMultimodalTypeOption.HandHoldingControllerRayDisabled;
            bool handFullyDisabled = handControllerMultimodalType == HandControllerMultimodalTypeOption.HandHoldingControllerFullyDisabled;
            if (currentClosestHandController == newClosestHandController ||
                !(handFullyDisabled || handRayDisabled))
            {
                return;
            }
            
            // Activate/Deactivate the current hand or hand ray holding the controller.
            if (currentClosestHandController != null)
            {
                if (handFullyDisabled)
                {
                    ActivateHand(currentClosestHandController, true);
                }
                else if (handRayDisabled)
                {
                    ActivateHandRay(currentClosestHandController, true);
                }
            }

            currentClosestHandController = newClosestHandController;

            if (currentClosestHandController != null)
            {
                if (handFullyDisabled)
                {
                    ActivateHand(currentClosestHandController, false);
                }
                else if (handRayDisabled)
                {
                    ActivateHandRay(currentClosestHandController, false);
                }
            }
        }

        private void ActivateHandRay(GameObject handController, bool active)
        {
            if (handController != null)
            {
                foreach (var farRayInteractor in handController.GetComponentsInChildren<MRTKRayInteractor>(true))
                {
                    if(farRayInteractor != null)
                    {
                        farRayInteractor.gameObject.SetActive(active);
                    }
                }
            }
        }

        private void ActivateHand(GameObject handController, bool active)
        {
            if (handController != null)
            {
                handController.SetActive(active);
            }
        }
    }
}
