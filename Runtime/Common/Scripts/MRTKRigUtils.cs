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
        /// Utility method to find the hand controller objects within the MRTK rig.
        /// </summary>
        /// <remarks>
        /// The returned Dictionary will have an entry for <see cref="XRNode.LeftHand"/> and/or <see cref="XRNode.RightHand"/>
        /// when found.  Finding either or both will return successfully.
        /// The object representing each hand controller is a top-level, general GameObject, when found, as the components used
        /// within the hand controller hierarchy vary depending on the version of the MRTK rig used within the scene.
        /// </remarks>
        /// <returns>True if either, or both, hand controllers were found, false if no hand controller was found.</returns>
        public static bool TryFindHandControllers(out Dictionary<XRNode, GameObject> handControllers)
        {
            handControllers = new();
            bool handsFound = false;

#if MRTK_INPUT_4_0_0_OR_NEWER
            // Attempt to find hands on the post-XRI3 based rig introduced in MRTK v4.0.0

            // First attempt to get references via TrackedPoseDriverLookup
            var trackedPoseDriverLookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>(true);
            if (trackedPoseDriverLookup != null)
            {
                if (trackedPoseDriverLookup.LeftHandTrackedPoseDriver != null)
                {
                    handControllers[XRNode.LeftHand] = trackedPoseDriverLookup.LeftHandTrackedPoseDriver.gameObject;
                }
                if (trackedPoseDriverLookup.RightHandTrackedPoseDriver != null)
                {
                    handControllers[XRNode.RightHand] = trackedPoseDriverLookup.RightHandTrackedPoseDriver.gameObject;
                }
                handsFound = true;
            }

            // If no hands, attempt to find HandPoseDriver directly
            if (!handsFound)
            {
                foreach (var handPoseDriver in FindObjectUtility.FindObjectsByType<HandPoseDriver>(true))
                {
                    handControllers[handPoseDriver.HandNode] = handPoseDriver.gameObject;
                    handsFound = true;
                }
            }
#endif

            if (!handsFound)
            {
                // Attempt to find hands within the legacy rig using ControllerLookup and ArticulatedHandControllers

#pragma warning disable CS0618 // ControllerLookup is obsolete
                var controllerLookup = FindObjectUtility.FindAnyObjectByType<ControllerLookup>(true);
#pragma warning restore CS0618 // ControllerLookup is obsolete
                if (controllerLookup != null)
                {
                    if (controllerLookup.LeftHandController != null)
                    {
                        handControllers[XRNode.LeftHand] = controllerLookup.LeftHandController.gameObject;
                    }
                    if (controllerLookup.RightHandController != null)
                    {
                        handControllers[XRNode.RightHand] = controllerLookup.RightHandController.gameObject;
                    }
                    handsFound = true;
                }

                // If no hands, attempt to find ArticulatedHandControllers directly in the legacy rig
                if (!handsFound)
                {
#pragma warning disable CS0612 // ArticulatedHandController is obsolete
                    foreach (var articulatedHandController in FindObjectUtility.FindObjectsByType<ArticulatedHandController>(true))
#pragma warning restore CS0612 // ArticulatedHandController is obsolete
                    {
                        handControllers[articulatedHandController.HandNode] = articulatedHandController.gameObject;
                        handsFound = true;
                    }
                }
            }

            return handsFound;
        }
    }
}
