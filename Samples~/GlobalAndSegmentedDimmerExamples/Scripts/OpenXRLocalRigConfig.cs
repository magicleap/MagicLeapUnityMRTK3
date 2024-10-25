// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.Samples.GlobalSegmentedDimmer
{
    public class OpenXRLocalRigConfig : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset openXRInputActionMap;
        [SerializeField]
        private GameObject openXRControllerPrefab;
        [SerializeField]
        private GameObject mlControllerPrefab;
        [SerializeField]
        private GameObject openXRHandMeshPrefab_Left;
        [SerializeField]
        private GameObject openXRHandMeshPrefab_Right;

        void Awake()
        {
            bool isOpenXR = MLDevice.IsOpenXRLoaderActive();
            if (!isOpenXR)
            {
                enabled = false;
                return;
            }

            // Swap out the controller prefab
            if (mlControllerPrefab != null)
            {
                Transform controllerParent = mlControllerPrefab.transform.parent;
                if (controllerParent != null && openXRControllerPrefab != null)
                {
                    mlControllerPrefab.SetActive(false);
                    var openXRController = GameObject.Instantiate(openXRControllerPrefab, controllerParent);
                    openXRController.name = openXRControllerPrefab.name;

                    // Re-initialize interaction mode detectors.
                    InteractionModeManager modeManager = GameObject.FindObjectOfType<InteractionModeManager>();
                    if (modeManager != null)
                    {
                        modeManager.InitializeInteractionModeDetectors();
                    }
                }
            }

            // Add OpenXR Input Action Map
            InputActionManager inputActionManager = GetComponentInChildren<InputActionManager>();
            if (inputActionManager != null && openXRInputActionMap != null)
            {
                inputActionManager.actionAssets.Add(openXRInputActionMap);
                inputActionManager.EnableInput();
            }

            // Swap out left/right hand mesh prefab
            ArticulatedHandController[] handControllers = GetComponentsInChildren<ArticulatedHandController>();
            SetHandControllerModelPrefab(handControllers, openXRHandMeshPrefab_Left, XRNode.LeftHand);
            SetHandControllerModelPrefab(handControllers, openXRHandMeshPrefab_Right, XRNode.RightHand);

            void SetHandControllerModelPrefab(ArticulatedHandController[] handControllers, GameObject prefab, XRNode handNode)
            {
                if (prefab != null)
                {
                    foreach (ArticulatedHandController handController in handControllers)
                    {
                        if (handController.HandNode == handNode)
                        {
                            handController.modelPrefab = prefab.transform;
                        }
                    }
                }
            }
        }
    }
}