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
using UnityEngine;
using MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// A specialized FlatScreenModeDetector to add support for Magic Leap input in flat screen mode detection.
    /// </summary>
    /// <remarks>
    /// Allows for manual addition of controller objects (rig controllers) to check the tracking state of
    /// in order to determine if the flat screen mode should be detected.  The built-in behavior only checks for
    /// the left and right hand controllers, while this specialization should allow for additional controllers,
    /// such as the Magic Leap Controller.
    /// This behavior will attempt to find and add the MRTK Rig's hand and Magic Leap controllers automatically
    /// during startup.
    /// </remarks>
    internal class MagicLeapFlatScreenModeDetector : MonoBehaviour, IInteractionModeDetector
    {
        [SerializeField]
        [Tooltip("The interaction mode to be set when the interactor is hovering over an interactable.")]
        private InteractionMode flatScreenInteractionMode;

#if UNITY_EDITOR
        /// <summary>
        /// The interaction mode to be set when the interactor is hovering over an interactable.
        /// </summary>
        public InteractionMode FlatScreenInteractionMode
        {
            get => flatScreenInteractionMode;
            set => flatScreenInteractionMode = value;
        }
#endif

        [SerializeField]
        [Tooltip("List of controllers that this interaction mode detector has jurisdiction over. Interaction modes will be set on all specified controllers.")]
        private List<GameObject> controllers;

#if UNITY_EDITOR
        /// <summary>
        /// List of controllers that this interaction mode detector has jurisdiction over. Interaction modes will be set on all specified controllers.
        /// </summary>
        public List<GameObject> Controllers
        {
            get => controllers;
            set => controllers = value;
        }
#endif

        public InteractionMode ModeOnDetection => flatScreenInteractionMode;

        private void Awake()
        {
            // Add hand controllers
            if (MRTKRigUtils.TryFindHandControllers(out var handControllers))
            {
                foreach (var (_, handController) in handControllers)
                {
                    AddControllerObject(handController);
                }
            }

            // Add ML Controller
            if (MRTKRigUtils.TryFindMagicLeapController(out var mlController))
            {
                AddControllerObject(mlController);
            }
        }

        /// <inheritdoc />
        public List<GameObject> GetControllers() => controllers;

#if MRTK_INPUT_4_0_0_OR_NEWER
        /// <inheritdoc />
        public List<GameObject> GetInteractorGroups() => controllers;
#endif

        private Dictionary<GameObject, (XRControllerState, TrackedPoseDriver)> rigControllerObjects = new();

        /// <inheritdoc />
        public bool IsModeDetected()
        {
            foreach(var (controller, (xrControllerState, trackedPoseDriver)) in rigControllerObjects)
            {
                if (xrControllerState != null && xrControllerState.inputTrackingState.HasPositionAndRotation())
                {
                    return false;
                }

#if MRTK_INPUT_4_0_0_OR_NEWER
                if (trackedPoseDriver != null && trackedPoseDriver.GetInputTrackingState().HasPositionAndRotation())
                {
                    return false;
                }
#endif
            }

            return true;
        }

        /// <summary>
        /// Add a controller object to the list of objects to check the tracking state of.
        /// </summary>
        /// <param name="controllerObject">The controller object to add.</param>
        public void AddControllerObject(GameObject controllerObject)
        {
            if (!rigControllerObjects.ContainsKey(controllerObject))
            {
#pragma warning disable CS0618 // XRBaseController is obsolete
                var xrBaseController = controllerObject.GetComponent<XRBaseController>();
#pragma warning restore CS0618 // XRBaseController is obsolete
                var xrControllerState = xrBaseController != null ? xrBaseController.currentControllerState: null;
                var trackedPoseDriver = controllerObject.GetComponent<TrackedPoseDriver>();

                rigControllerObjects.Add(controllerObject, (xrControllerState, trackedPoseDriver));
            }
        }

        /// <summary>
        /// Remove a controller object from the list of objects to check the tracking state of.
        /// </summary>
        /// <param name="controllerObject">The controller object to remove.</param>
        public void RemoveControllerObject(GameObject controllerObject)
        {
            rigControllerObjects.Remove(controllerObject);
        }

        /// <summary>
        /// Clear the list of objects to check the tracking state of.
        public void ClearControllerObjects()
        {
            rigControllerObjects.Clear();
        }
    }
}
