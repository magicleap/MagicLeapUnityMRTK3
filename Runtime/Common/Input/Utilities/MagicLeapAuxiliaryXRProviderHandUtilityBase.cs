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
using UnityEngine.XR;
using MagicLeap.MRTK.Settings;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// Base class for XR provider specific auxiliary hand utilities.
    /// </summary>
    internal abstract class MagicLeapAuxiliaryXRProviderHandUtilityBase : IMagicLeapAuxiliaryXRProviderHandUtility
    {
        /// <inheritdoc/>
        public abstract bool CompatibleWithActiveXRLoader { get; }

        /// <inheritdoc/>
        public abstract HandRayTypeOption HandRayType { get; }

        /// <inheritdoc/>
        public abstract HandControllerMultimodalTypeOption HandControllerMultimodalType { get; }

        /// <inheritdoc/>
        public abstract bool TryGetPinch(XRNode handNode, out bool isPinching, out float pinchAmount);

        /// <inheritdoc/>
        public bool TryGetHandRayPose(XRNode handNode, out Pose pose)
        {
            // The standard ML hand ray algorithm
            if (HandSubsystem.TryGetJoint(TrackedHandJoint.IndexProximal, handNode, out HandJointPose indexKnucklePose))
            {
                pose.position = indexKnucklePose.Position;
                Transform hmd = Camera.main.transform;
                float extraRayRotationX = -20.0f;
                float extraRayRotationY = 25.0f * ((handNode == XRNode.LeftHand) ? 1.0f : -1.0f);
                Quaternion targetRotation = Quaternion.LookRotation(pose.position - hmd.position, Vector3.up);
                Vector3 euler = targetRotation.eulerAngles + new Vector3(extraRayRotationX, extraRayRotationY, 0.0f);
                pose.rotation = Quaternion.Euler(euler);
                return true;
            }

            pose = Pose.identity;
            return false;
        }

        protected static HandsAggregatorSubsystem HandSubsystem => XRSubsystemHelpers.HandsAggregator as HandsAggregatorSubsystem;
        protected static readonly float PinchClosedThreshold = 0.97f;
        protected static readonly float PinchOpenThreshold = 0.95f;
    }
}
