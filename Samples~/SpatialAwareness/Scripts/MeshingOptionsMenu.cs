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

namespace MagicLeap.MRTK.Samples.SpatialAwareness
{
    public class MeshingOptionsMenu : MonoBehaviour
    {
        [SerializeField] private GameObject meshingObject;

        private MeshingController meshingController;

        void Start()
        {
            if (meshingObject == null)
            {
                enabled = false;
                return;
            }

            // Sets the enabled MeshingController, corresponding to the XR Loader
            MeshingController[] controllers = meshingObject.GetComponentsInChildren<MeshingController>();
            foreach(MeshingController controller in controllers)
            {
                if (controller.enabled)
                {
                    meshingController = controller;
                }
            }
        }

        public void OnToggleChanged(int selectedToggle)
        {
            if (meshingController == null)
            {
                return;
            }
            if (selectedToggle == 0)
            {
                meshingController.SetRenderer(MeshingController.RenderMode.Wireframe);
            }
            else if (selectedToggle == 1)
            {
                meshingController.SetRenderer(MeshingController.RenderMode.Colored);
            }
            else
            {
                meshingController.SetRenderer(MeshingController.RenderMode.Occlusion);
            }
        }
    }
}