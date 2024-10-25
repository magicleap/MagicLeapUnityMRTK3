// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace MagicLeap.MRTK
{
    /// <summary>
    /// A collection of MRTK Rig Utilities.
    /// </summary>
    public static class MRTKRigUtils
    {
        /// <summary>
        /// Try to find the parent <see cref="GameObject"/> of the MRTK rig.
        /// </summary>
        /// <remarks>
        /// This will either be the top level of the MRTK rig, or a parent <see cref="GameObject"/> the rig resides within.
        /// </remarks>
        /// <returns><see langword="true"/> if the rig parent was found, <see langword="false"/> if not.</returns>
        public static bool TryFindMRTKRigParent(out GameObject mrtkRigParent)
        {
            if (cachedMRTKRigParent == null)
            {
                XROrigin xrOrigin = PlayspaceUtilities.XROrigin;
                cachedMRTKRigParent = xrOrigin != null ? xrOrigin.transform.root.gameObject : null;
                Debug.Assert(cachedMRTKRigParent != null, DebugAssertXROrigin);
            }

            mrtkRigParent = cachedMRTKRigParent;
            return mrtkRigParent != null;
        }

        /// <summary>
        /// Try to find the parent <see cref="GameObject"/> of the MRTK rig controllers.
        /// </summary>
        /// <remarks>
        /// The MRTK rig controller parent is the <see cref="GameObject"/> under which all controller objects
        /// (hands, gaze, tracked controllers) are added to.
        /// </remarks>
        /// <returns><see langword="true"/> if the rig controller parent was found, <see langword="false"/> if not.</returns>
        public static bool TryFindMRTKRigControllerParent(out GameObject mrtkRigControllerParent)
        {
            if (cachedMRTKRigControllerParent == null)
            {
                XROrigin xrOrigin = PlayspaceUtilities.XROrigin;
                cachedMRTKRigControllerParent = xrOrigin != null ? xrOrigin.CameraFloorOffsetObject : null;
                Debug.Assert(cachedMRTKRigControllerParent != null, DebugAssertXROrigin);
            }

            mrtkRigControllerParent = cachedMRTKRigControllerParent;
            return mrtkRigControllerParent != null;
        }

        /// <summary>
        /// Utility method to find the hand controller objects within the MRTK rig.
        /// </summary>
        /// <remarks>
        /// The returned Dictionary will have an entry for <see cref="XRNode.LeftHand"/> and/or <see cref="XRNode.RightHand"/>
        /// when found.  Finding either or both will return successfully.
        /// The object representing each hand controller is a top-level, general <see cref="GameObject"/> when found as the components used
        /// within the hand controller hierarchy vary depending on the version of the MRTK rig used within the scene.
        /// </remarks>
        /// <returns><see langword="true"/> if either, or both, hand controllers were found, <see langword="false"/> if no hand controller was found.</returns>
        public static bool TryFindHandControllers(out Dictionary<XRNode, GameObject> handControllers)
        {
            // Attempt to use cached values initially
            bool handsFound = cachedHandControllers.Count > 0;

            // Ensure cached values are still valid.
            if (handsFound)
            {
                foreach (var (_, cachedHandController) in cachedHandControllers)
                {
                    if (cachedHandController == null)
                    {
                        handsFound = false;
                        cachedHandControllers.Clear();
                        break;
                    }
                }
            }

#if MRTK_INPUT_4_0_0_OR_NEWER
            // Attempt to find hands on the post-XRI3 based rig introduced in MRTK v4.0.0
            
            if (!handsFound)
            {
                // First attempt to get references via TrackedPoseDriverLookup
                var trackedPoseDriverLookup = ComponentCache<TrackedPoseDriverLookup>.FindFirstActiveInstance();
                if (trackedPoseDriverLookup != null)
                {
                    if (trackedPoseDriverLookup.LeftHandTrackedPoseDriver != null)
                    {
                        cachedHandControllers[XRNode.LeftHand] = trackedPoseDriverLookup.LeftHandTrackedPoseDriver.gameObject;
                        handsFound = true;
                    }
                    if (trackedPoseDriverLookup.RightHandTrackedPoseDriver != null)
                    {
                        cachedHandControllers[XRNode.RightHand] = trackedPoseDriverLookup.RightHandTrackedPoseDriver.gameObject;
                        handsFound = true;
                    }
                }
            }

            if (!handsFound)
            {
                // Attempt to find HandPoseDrivers directly
                foreach (var handPoseDriver in FindObjectUtility.FindObjectsByType<HandPoseDriver>(true))
                {
                    cachedHandControllers[handPoseDriver.HandNode] = handPoseDriver.gameObject;
                    handsFound = true;
                }
            }
#endif

            if (!handsFound)
            {
                // Attempt to find hands within the legacy rig using ControllerLookup

#pragma warning disable CS0618 // ControllerLookup is obsolete
                var controllerLookup = ComponentCache<ControllerLookup>.FindFirstActiveInstance();
#pragma warning restore CS0618 // ControllerLookup is obsolete
                if (controllerLookup != null)
                {
                    if (controllerLookup.LeftHandController != null)
                    {
                        cachedHandControllers[XRNode.LeftHand] = controllerLookup.LeftHandController.gameObject;
                        handsFound = true;
                    }
                    if (controllerLookup.RightHandController != null)
                    {
                        cachedHandControllers[XRNode.RightHand] = controllerLookup.RightHandController.gameObject;
                        handsFound = true;
                    }
                }
            }

            if (!handsFound)
            {
                // Attempt to find ArticulatedHandControllers directly in the legacy rig

#pragma warning disable CS0612 // ArticulatedHandController is obsolete
                foreach (var articulatedHandController in FindObjectUtility.FindObjectsByType<ArticulatedHandController>(true))
#pragma warning restore CS0612 // ArticulatedHandController is obsolete
                {
                    cachedHandControllers[articulatedHandController.HandNode] = articulatedHandController.gameObject;
                    handsFound = true;
                }
            }

            handControllers = cachedHandControllers;
            return handsFound;
        }

        /// <summary>
        /// Utility method to find the Magic Leap Controller object within the MRTK rig.
        /// </summary>
        /// <returns><see langword="true"/> if a magic leap controller object is found, <see langword="false"/> if not.</returns>
        public static bool TryFindMagicLeapController(out GameObject magicLeapController)
        {
            if (cachedMagicLeapController == null)
            {
                // Attempt to find ML Controller via MagicLeapControllerHandProximityDisabler, even if disabled.
                var mlControllerHandProximityDisabler = FindObjectUtility.FindAnyObjectByType<MagicLeapControllerHandProximityDisabler>(true);
                if (mlControllerHandProximityDisabler != null)
                {
                    cachedMagicLeapController = mlControllerHandProximityDisabler.gameObject;
                }
            }

            magicLeapController = cachedMagicLeapController;
            return magicLeapController != null;
        }

        private static GameObject cachedMRTKRigParent;
        private static GameObject cachedMRTKRigControllerParent;
        private static Dictionary<XRNode, GameObject> cachedHandControllers = new();
        private static GameObject cachedMagicLeapController;

        private static readonly string DebugAssertXROrigin = "MRTKRigUtils requires the use of an XROrigin. Check if your main camera is a child of an XROrigin.";
    }
}
