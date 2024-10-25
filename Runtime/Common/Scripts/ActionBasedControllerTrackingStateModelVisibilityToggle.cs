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
using MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;

namespace MagicLeap.MRTK
{
    /// <summary>
    /// Component to manage the visibility of an ActionBasedController's model based on
    /// the status of the TrackingState input action.
    /// </summary>
    [RequireComponent(typeof(ActionBasedController))]
    public class ActionBasedControllerTrackingStateModelVisibilityToggle : MonoBehaviour
    {
        private ActionBasedController actionBasedController;
        private bool hideModel = false;

        private void Awake()
        {
            actionBasedController = GetComponent<ActionBasedController>();

            // Hide model initially and update to visible once we have valid tracking state.
            HideControllerModel(true);
        }

        void Update()
        {
            bool shouldHideModel = !actionBasedController.currentControllerState.inputTrackingState.HasPositionAndRotation();
            HideControllerModel(shouldHideModel);
        }

        private void HideControllerModel(bool shouldHideModel)
        {
            if (shouldHideModel != hideModel)
            {
                hideModel = shouldHideModel;
                actionBasedController.hideControllerModel = hideModel;
            }
        }
    }
}
