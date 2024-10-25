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
using UnityEngine.XR.Management;

namespace MagicLeap.MRTK.Samples.SpatialAwareness
{
    public abstract class MeshingController : MonoBehaviour
    {
        public enum RenderMode
        {
            None,
            Wireframe,
            Colored,
            Occlusion,
            // PointCloud, 
        }

        [SerializeField, Tooltip("The Render Mode")]
        protected RenderMode renderMode = RenderMode.Wireframe;

        [SerializeField, Tooltip("The material to apply for occlusion.")]
        protected Material occlusionMaterial = null;

        [SerializeField, Tooltip("The material to apply for wireframe rendering.")]
        protected Material wireframeMaterial = null;

        [SerializeField, Tooltip("The material to apply for colored rendering.")]
        protected Material coloredMaterial = null;

        protected XRInputSubsystem inputSubsystem;

        protected virtual void Awake()
        {
            if (occlusionMaterial == null)
            {
                Debug.LogError("Error: occlusionMaterial is not set, disabling script!");
                enabled = false;
                return;
            }
            if (wireframeMaterial == null)
            {
                Debug.LogError("Error: wireframeMaterial is not set, disabling script!");
                enabled = false;
                return;
            }
            if (coloredMaterial == null)
            {
                Debug.LogError("Error: coloredMaterial is not set, disabling script!");
                enabled = false;
                return;
            }
            inputSubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRInputSubsystem>();
        }

        protected virtual void Start()
        {
            inputSubsystem.trackingOriginUpdated += OnTrackingOriginChanged;
        }

        protected virtual void OnDestroy()
        {
            inputSubsystem.trackingOriginUpdated -= OnTrackingOriginChanged;
        }

        /// <summary>
        /// Set the renderer
        /// </summary>
        /// <param name="mode">The render mode that should be used on the material.</param>
        public abstract void SetRenderer(RenderMode mode);

        /// <summary>
        /// Updates the currently selected render material on the MeshRenderer.
        /// </summary>
        /// <param name="meshRenderer">The MeshRenderer that should be updated.</param>
        protected abstract void UpdateMeshRenderer(MeshRenderer meshRenderer);

        /// <summary>
        /// Handle in charge of refreshing all meshes if a new session occurs
        /// </summary>
        /// <param name="inputSubsystem"> The inputSubsystem that invoked this event. </param>
        protected abstract void OnTrackingOriginChanged(XRInputSubsystem inputSubsystem);
    }
}