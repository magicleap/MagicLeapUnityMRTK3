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
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.Samples.SpatialAwareness
{
    public class MeshingControllerOpenXR : MeshingController
    {
        [SerializeField]
        private ARMeshManager meshManager = null;

        private bool meshRendererDirty = false;

        protected override void Awake()
        {
            // Deactivate this controller and the ARMeshManager, is OpenXR is not the active loader
            if (!MLDevice.IsOpenXRLoaderActive())
            {
                if (meshManager != null)
                {
                    meshManager.gameObject.SetActive(false);
                }
                enabled = false;
                return;
            }

            if (meshManager == null)
            {
                Debug.LogError("Error: ARMeshManager is not set, disabling script!");
                enabled = false;
                return;
            }

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        void Update()
        {
            if (meshRendererDirty)
            {
                if (meshManager.meshPrefab != null)
                {
                    UpdateMeshRenderer(meshManager.meshPrefab.gameObject.GetComponent<MeshRenderer>());
                }
                meshRendererDirty = false;
            }
        }

        public override void SetRenderer(RenderMode mode)
        {
            if (renderMode != mode)
            {
                // Set the render mode.
                renderMode = mode;

                // NOTE: To be implemented, once PointCloud is introduced, via ARPointCloudManager
                switch (renderMode)
                {
                    case RenderMode.Wireframe:
                    case RenderMode.Colored:
                    case RenderMode.Occlusion:
                    //case RenderMode.PointCloud:
                    {
                        break;
                    }
                }
                meshManager.DestroyAllMeshes();
                meshRendererDirty = true;
            }
        }

        protected override void UpdateMeshRenderer(MeshRenderer meshRenderer)
        {
            if (meshRenderer != null)
            {
                // Toggle the GameObject(s) and set the correct material based on the current RenderMode.
                // NOTE: Add an option for PointCloud
                if (renderMode == RenderMode.None)
                {
                    meshRenderer.enabled = false;
                }
                else if (renderMode == RenderMode.Wireframe)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.material = wireframeMaterial;
                }
                else if (renderMode == RenderMode.Colored)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.material = coloredMaterial;
                }
                else if (renderMode == RenderMode.Occlusion)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.material = occlusionMaterial;
                }
            }
        }

        protected override void OnTrackingOriginChanged(XRInputSubsystem inputSubsystem)
        {
            meshManager.DestroyAllMeshes();
        }
    }
}