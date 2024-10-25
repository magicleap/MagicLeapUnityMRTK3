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
using UnityEngine.XR.MagicLeap;
using MixedReality.Toolkit;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

namespace MagicLeap.MRTK.Samples.SpatialAwareness
{
    public class MeshingControllerML : MeshingController
    {
        [SerializeField, Tooltip("The MeshingSubsystemComponent.")]
        private MeshingSubsystemComponent meshingSubsystemComponent = null;

        [SerializeField, Space, Tooltip("Flag specifying if mesh extents are bounded.")]
        private bool bounded = false;

        [SerializeField, Space, Tooltip("Size of the bounds extents when bounded setting is enabled.")]
        private Vector3 boundedExtentsSize = new Vector3(2.0f, 2.0f, 2.0f);

        [SerializeField, Space, Tooltip("Size of the bounds extents when bounded setting is disabled.")]
        private Vector3 boundlessExtentsSize = new Vector3(10.0f, 10.0f, 10.0f);

        [SerializeField, Space, Tooltip("Mesh boundary will follow the user on a position change")]
        private bool follow = false;

        private XROrigin xrOrigin = null;

        protected override void Awake()
        {
            // Deactivate this controller and the MeshingSubsystemComponent if ML is not the active loader
            if (!MLDevice.IsMagicLeapLoaderActive())
            {
                if (meshingSubsystemComponent != null)
                {
                    meshingSubsystemComponent.gameObject.SetActive(false);
                }
                enabled = false;
                return;
            }

            if (meshingSubsystemComponent == null)
            {
                Debug.LogError("Error: MeshingSubsystemComponent is not set, disabling script!");
                enabled = false;
                return;
            }

            xrOrigin = PlayspaceUtilities.XROrigin;
            base.Awake();
        }

        /// <summary>
        /// Register callbacks, and initialize renderer, position, bounds.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            meshingSubsystemComponent.meshAdded += HandleOnMeshReady;
            meshingSubsystemComponent.meshUpdated += HandleOnMeshReady;
            meshingSubsystemComponent.gameObject.transform.position = xrOrigin.CameraFloorOffsetObject.transform.position;
            UpdateBounds();
        }

        /// <summary>
        /// Unregister callbacks.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            meshingSubsystemComponent.meshAdded -= HandleOnMeshReady;
            meshingSubsystemComponent.meshUpdated -= HandleOnMeshReady;
        }

        void Update()
        {
            if (follow)
            {
                // We want meshing to move with the headpose if a boundary was set, not just stay at origin
                meshingSubsystemComponent.gameObject.transform.position = xrOrigin.CameraFloorOffsetObject.transform.position;
            }
            if ((bounded && meshingSubsystemComponent.gameObject.transform.localScale != boundedExtentsSize) ||
                (!bounded && meshingSubsystemComponent.gameObject.transform.localScale != boundlessExtentsSize))
            {
                UpdateBounds();
            }

        }

        public override void SetRenderer(RenderMode mode)
        {
            if (renderMode != mode)
            {
                // Set the render mode.
                renderMode = mode;

                // Clear existing meshes to process the new mesh type.
                switch (renderMode)
                {
                    case RenderMode.Wireframe:
                    case RenderMode.Colored:
                    case RenderMode.Occlusion:
                        {
                            meshingSubsystemComponent.requestedMeshType = MeshingSubsystemComponent.MeshType.Triangles;

                            break;
                        }
                    /*case RenderMode.PointCloud:
                        {
                            meshingSubsystemComponent.requestedMeshType = MeshingSubsystemComponent.MeshType.PointCloud;

                            break;
                        }*/
                }

                meshingSubsystemComponent.DestroyAllMeshes();
                meshingSubsystemComponent.RefreshAllMeshes();
            }
        }

        protected override void UpdateMeshRenderer(MeshRenderer meshRenderer)
        {
            if (meshRenderer != null)
            {
                // Toggle the GameObject(s) and set the correct material based on the current RenderMode.
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
            meshingSubsystemComponent.DestroyAllMeshes();
            meshingSubsystemComponent.RefreshAllMeshes();
        }

        private void UpdateBounds()
        {
            meshingSubsystemComponent.gameObject.transform.localScale = bounded ? boundedExtentsSize : boundlessExtentsSize;
        }

#if UNITY_2019_3_OR_NEWER
        /// <summary>
        /// Handles the MeshReady event, which tracks and assigns the correct mesh renderer materials.
        /// </summary>
        /// <param name="meshId">Id of the mesh that got added / updated.</param>
        private void HandleOnMeshReady(MeshId meshId)
        {
            if (meshingSubsystemComponent.meshIdToGameObjectMap.ContainsKey(meshId))
            {
                UpdateMeshRenderer(meshingSubsystemComponent.meshIdToGameObjectMap[meshId].GetComponent<MeshRenderer>());
            }
        }
#else
        /// <summary>
        /// Handles the MeshReady event, which tracks and assigns the correct mesh renderer materials.
        /// </summary>
        /// <param name="meshId">Id of the mesh that got added / updated.</param>
        private void HandleOnMeshReady(TrackableId meshId)
        {
            if (meshingSubsystemComponent.meshIdToGameObjectMap.ContainsKey(meshId))
            {
                UpdateRenderer(meshingSubsystemComponent.meshIdToGameObjectMap[meshId].GetComponent<MeshRenderer>());
            }
        }
#endif
    }
}