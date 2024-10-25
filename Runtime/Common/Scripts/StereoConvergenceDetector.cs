// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using Unity.XR.CoreUtils;
using MixedReality.Toolkit;
using UnityEditor;
using MagicLeap.MRTK.Settings;

#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.MagicLeapSupport;
using UnityEngine.XR.OpenXR.Features.Interactions;
#endif

#if MAGICLEAP_UNITY_SDK_2_1_0_OR_NEWER
using MagicLeap.Android;
#endif

#if UNITY_EDITOR
using UnityEditor.XR.Management;
#endif

namespace MagicLeap.MRTK.Utilities
{
    /// <summary>
    /// Detects and sets the camera focus distance by utilizing the eye tracking fixation point either
    /// directly or in conjunction with sphere casting colliders in the scene.
    /// If eye tracking is not used or not available, this detector will fall back to
    /// sphere casting from headpose.
    /// </summary>
    public class StereoConvergenceDetector : MonoBehaviour
    {
        /// <summary>
        /// The eye tracking options to use when detecting the focus distance.
        /// </summary>
        [Serializable]
        public enum EyeTrackingOptions
        {
            /// <summary>
            /// Do a sphere cast into the scene from the headpose position along the headpose forward direction, no eye tracking.
            /// </summary>
            DoNotUseEyeTracking_UseHeadpose,

            /// <summary>
            /// Do a sphere cast into the scene from the headpose position along the eye gaze pose direction.
            /// </summary>
            SphereCastAlongEyeGazeDirection,

            /// <summary>
            /// Use the eye tracking fixation point directly, without sphere casting the scene.  Not recommended, can be noisy.
            /// </summary>
            UseEyeFixationPointDirectlyAsFocusPoint
        }

        [Space(10)]

        [SerializeField]
        [Tooltip("Choose if eye tracking is used at all along with how to utilize the eye tracking fixation point.  " +
            "Headpose vector will provide a fallback if eye tracking is not used or not available.")]
        private EyeTrackingOptions eyeTrackingOption = EyeTrackingOptions.SphereCastAlongEyeGazeDirection;

        /// <summary>
        /// The <see cref="EyeTrackingOption"/> mode used to detect focus distance.
        /// </summary>
        public EyeTrackingOptions EyeTrackingOption => eyeTrackingOption;

        [SerializeField]
        [Tooltip("The interval in seconds between detecting the focus point via sphere cast or direct eye fixation point.")]
        private float detectionInterval = .1f;

        /// <summary>
        /// The interval in seconds between detecting the focus point via sphere cast or direct eye fixation point.
        /// </summary>
        public float DetectionInterval
        {
            get => detectionInterval;
            set => detectionInterval = value;
        }

        [Header("Sphere Casting")]

        [SerializeField]
        [Tooltip("The radius to use for the sphere cast when sphere casting is used.")]
        private float sphereCastRadius = .075f;

        /// <summary>
        /// The radius to use for the sphere cast.
        /// </summary>
        public float SphereCastRadius
        {
            get => sphereCastRadius;
            set => sphereCastRadius = value;
        }

        [SerializeField]
        [Tooltip("The layer mask for the sphere cast.")]
        private LayerMask sphereCastMask = Physics.DefaultRaycastLayers;

        /// <summary>
        /// The layer mask for the sphere cast.
        /// </summary>
        public LayerMask SphereCastMask
        {
            get => sphereCastMask;
            set => sphereCastMask = value;
        }

        [Header("Debug Visuals")]

        [SerializeField]
        [Tooltip("Whether to show debug visuals for focus point detection.")]
        private bool showDebugVisuals = false;

        /// <summary>
        /// Whether to show debug visuals for focus point detection.
        /// </summary>
        public bool ShowDebugVisuals
        {
            get => showDebugVisuals;
            set => showDebugVisuals = value;
        }

        [SerializeField]
        [Tooltip("Material used to represent sphere cast radius and focus point location.")]
        private Material sphereCastMaterial;

        /// <summary>
        /// Material used to represent sphere cast radius and focus point location.
        /// </summary>
        public Material SphereCastMaterial
        {
            get => sphereCastMaterial;
        }

        [SerializeField]
        [Tooltip("Material used to represent sphere cast hit point.")]
        private Material hitPointMaterial;

        /// <summary>
        /// Material used to represent sphere cast hit point."
        /// </summary>
        public Material HitPointMaterial
        {
            get => hitPointMaterial;
        }


        private GameObject sphereCastVisual = null;
        private GameObject hitPointVisual = null;
        private bool debugVisualsVisible = false;
        private Coroutine rayCastRoutine = null;
        private InputDevice eyesDevice;
        private XROrigin xrOrigin = null;
        private Camera mainCamera = null;
#pragma warning disable CS0618 // Type or member is obsolete
        private MagicLeapCamera magicLeapCamera = null;
#pragma warning restore CS0618 // Type or member is obsolete
        private Transform focusPointTransform = null;
        private MagicLeapInputs mlInputs = null;
        private MagicLeapInputs.EyesActions eyesActions;
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
        private MagicLeapRenderingExtensionsFeature mlRenderingExtensionFeature = null;
        private MagicLeapRenderingExtensionsFeature MLRenderingExtensionFeature
        {
            get
            {
                if (mlRenderingExtensionFeature == null)
                {
                    mlRenderingExtensionFeature = OpenXRSettings.Instance?.GetFeature<MagicLeapRenderingExtensionsFeature>();
                }

                return mlRenderingExtensionFeature;
            }
        }
#endif


        private void Awake()
        {
            InitializeReferences();

            if (mainCamera == null)
            {
                Debug.LogError($"{GetType().Name} requires a Camera.main to function, disabling.");
                enabled = false;
                return;
            }

            CreateDebugVisuals();

            // Request EyeTracking when an eye tracking option is selected
            if (eyeTrackingOption != EyeTrackingOptions.DoNotUseEyeTracking_UseHeadpose)
            {
                var permissionsConfig = MagicLeapMRTK3Settings.Instance.GetSettingsObject<MagicLeapMRTK3SettingsPermissionsConfig>();
                if (permissionsConfig != null)
                {
                    permissionsConfig.AddDangerousPermissionToRequest(MLPermission.EyeTracking);
                }
            }
        }

        private void OnEnable()
        {
            rayCastRoutine = StartCoroutine(DetectConvergencePointCoroutine());
        }

        private void OnDisable()
        {
            if (rayCastRoutine != null)
            {
                StopCoroutine(rayCastRoutine);
                rayCastRoutine = null;
            }

            UpdateDebugVisualVisibility(false);
        }

        private void OnDestroy()
        {
            if (rayCastRoutine != null)
            {
                StopCoroutine(rayCastRoutine);
                rayCastRoutine = null;
            }

            if (mlInputs != null)
            {
                mlInputs.Disable();
                mlInputs.Dispose();
                mlInputs = null;
            }
        }

        private void InitializeReferences()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }

            xrOrigin = PlayspaceUtilities.XROrigin;

            if (MLDevice.IsMagicLeapLoaderActive())
            {
                // Obtain existing, or create, MagicLeapCamera to facilitate setting focus distance.
                // Note:  MagicLeapCamera is deprecated, but it is mandatory when using the ML XR Provider.
#pragma warning disable CS0618 // Type or member is obsolete
                magicLeapCamera = mainCamera.GetComponent<MagicLeapCamera>();
#pragma warning restore CS0618 // Type or member is obsolete

                if (magicLeapCamera == null)
                {
                    // Due to altercations MagicLeapCamera makes automatically (fixProblemsOnStartup defaults true),
                    // save and then restore certain settings.
                    // Save
                    float nearClipPlane = mainCamera.nearClipPlane;
                    CameraClearFlags clearFlags = mainCamera.clearFlags;
                    Color backgroundColor = mainCamera.backgroundColor;
                    Vector3 localPosition = mainCamera.transform.localPosition;
                    Quaternion localRotation = mainCamera.transform.localRotation;
                    Vector3 localScale = mainCamera.transform.localScale;

#pragma warning disable CS0618 // Type or member is obsolete
                    magicLeapCamera = mainCamera.gameObject.AddComponent<MagicLeapCamera>();
#pragma warning restore CS0618 // Type or member is obsolete

#if MAGICLEAP_UNITY_SDK_1_10_0_OR_NEWER
                    magicLeapCamera.RecenterXROriginAtStart = false;
#endif

                    // Restore
                    mainCamera.nearClipPlane = nearClipPlane;
                    mainCamera.clearFlags = clearFlags;
                    mainCamera.backgroundColor = backgroundColor;
                    mainCamera.transform.localPosition = localPosition;
                    mainCamera.transform.localRotation = localRotation;
                    mainCamera.transform.localScale = localScale;
                }

                // Obtain MagicLeapCamera focus point transform, or create one if needed.
                focusPointTransform = magicLeapCamera.StereoConvergencePoint;

                if (focusPointTransform == null)
                {
                    focusPointTransform = new GameObject("focus point").transform;
                    focusPointTransform.transform.parent = mainCamera.transform;
                    focusPointTransform.transform.localPosition = Vector3.forward * mainCamera.stereoConvergence;
                    magicLeapCamera.StereoConvergencePoint = focusPointTransform;
                }
            }
        }

        private void CreateDebugVisuals()
        {
            // Local method to create sphere primitive for use in debug visuals.
            GameObject CreateSpherePrimitive(Material material)
            {
                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                primitive.layer = this.gameObject.layer;
                primitive.transform.SetParent(transform);
                primitive.SetActive(false);

                if (material != null)
                {
                    primitive.GetComponent<Renderer>().material = material;
                }

                // Remove collider to not interfere with scene
                Collider collider = primitive.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                return primitive;
            };

            sphereCastVisual = CreateSpherePrimitive(sphereCastMaterial);
            hitPointVisual = CreateSpherePrimitive(hitPointMaterial);
        }

        private IEnumerator DetectConvergencePointCoroutine()
        {
            while (true)
            {
                if (detectionInterval > 0)
                {
                    yield return new WaitForSeconds(detectionInterval);
                }
                else
                {
                    yield return null;
                }

                if (!isActiveAndEnabled)
                {
                    continue;
                }

                bool focusPointDetected = false;
                Vector3 focusPoint = Vector3.zero;
                bool useSphereCast = true;

                // Default Headpose parameters for sphere cast
                Vector3 rayOrigin = mainCamera.transform.position;
                Vector3 rayDirection = mainCamera.transform.forward;

                // Eye Tracking option
                if (eyeTrackingOption != EyeTrackingOptions.DoNotUseEyeTracking_UseHeadpose &&
#if MAGICLEAP_UNITY_SDK_2_1_0_OR_NEWER
                    Permissions.CheckPermission(MLPermission.EyeTracking) &&
#else
                    MLPermissions.CheckPermission(MLPermission.EyeTracking).IsOk &&
#endif
                    GetEyeFixationPoint(out Vector3 fixationPoint))
                {
                    // Transform the fixation point from perception space into world space
                    if (xrOrigin != null)
                    {
                        fixationPoint = xrOrigin.CameraFloorOffsetObject.transform.TransformPoint(fixationPoint);
                    }
                    else if (mainCamera.transform.parent != null)
                    {
                        fixationPoint = mainCamera.transform.parent.TransformPoint(fixationPoint);
                    }

                    switch (eyeTrackingOption)
                    {
                        case EyeTrackingOptions.UseEyeFixationPointDirectlyAsFocusPoint:
                            focusPoint = fixationPoint;
                            focusPointDetected = true;
                            useSphereCast = false;
                            rayDirection = (focusPoint - rayOrigin).normalized;
                            break;

                        case EyeTrackingOptions.SphereCastAlongEyeGazeDirection:
                        default:
                            useSphereCast = true;
                            rayDirection = (fixationPoint - rayOrigin).normalized;
                            break;
                    }
                }

                if (useSphereCast && Physics.SphereCast(new Ray(rayOrigin, rayDirection), sphereCastRadius, out RaycastHit hitInfo, mainCamera.farClipPlane, sphereCastMask))
                {
                    focusPoint = hitInfo.point;
                    focusPointDetected = true;
                }

                if (focusPointDetected)
                {
                    // Set focus distance
                    Vector3 focusPointVector = Vector3.Project(focusPoint - rayOrigin, rayDirection);
                    float focusDistance = focusPointVector.magnitude;
                    SetFocusDistance(focusDistance);

                    // Update debug visuals
                    sphereCastVisual.transform.localScale = Vector3.one * sphereCastRadius * 2.0f;
                    Vector3 focusPointAlongRay = rayOrigin + focusPointVector;
                    sphereCastVisual.transform.position = focusPointAlongRay;

                    hitPointVisual.transform.localScale = Vector3.one * sphereCastRadius * 2.0f * .1f;
                    hitPointVisual.transform.position = focusPoint;

                    UpdateDebugVisualVisibility(showDebugVisuals);
                }
                else
                {
                    UpdateDebugVisualVisibility(false);
                }
            }
        }

        private bool GetEyeFixationPoint(out Vector3 fixationPoint)
        {
            fixationPoint = Vector3.zero;

            // MLXR
            if (MLDevice.IsMagicLeapLoaderActive())
            {
                if (!eyesDevice.isValid)
                {
                    if (mlInputs == null)
                    {
                        mlInputs = new MagicLeapInputs();
                        mlInputs.Enable();
                    }
                    eyesActions = new MagicLeapInputs.EyesActions(mlInputs);
                    eyesDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice);

                    if (!eyesDevice.isValid)
                    {
                        return false;
                    }
                }

                var eyes = eyesActions.Data.ReadValue<UnityEngine.InputSystem.XR.Eyes>();
                InputSubsystem.Extensions.TryGetEyeTrackingState(eyesDevice, out var trackingState);
                if (trackingState.FixationConfidence > .25f)
                {
                    fixationPoint = eyes.fixationPoint;
                    return true;
                }
            }

            // OpenXR
            if (MLDevice.IsOpenXRLoaderActive())
            {
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
                if (!eyesDevice.isValid)
                {
                    List<InputDevice> inputDeviceList = new();
                    InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, inputDeviceList);
                    eyesDevice = inputDeviceList.FirstOrDefault();

                    if (!eyesDevice.isValid)
                    {
                        return false;
                    }
                }

                if (eyesDevice.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked) &&
                    eyesDevice.TryGetFeatureValue(EyeTrackingUsages.gazePosition, out Vector3 position) &&
                    eyesDevice.TryGetFeatureValue(EyeTrackingUsages.gazeRotation, out Quaternion rotation))
                {
                    // Eye fixation point not currently supported under OpenXR, use a default distance for now.
                    const float OpenXRDefaultFixationDistance = 1.5f;
                    fixationPoint = position + (rotation * Vector3.forward * OpenXRDefaultFixationDistance);
                    return true;
                }
#endif
            }

            return false;
        }

        private void SetFocusDistance(float distance)
        {
            if (MLDevice.IsMagicLeapLoaderActive())
            {
                if (focusPointTransform != null)
                {
                    focusPointTransform.position = mainCamera.transform.position + mainCamera.transform.forward * distance;
                }
            }

            if (MLDevice.IsOpenXRLoaderActive())
            {
#if UNITY_OPENXR_1_9_0_OR_NEWER && MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
                mainCamera.stereoConvergence = distance;
                if (MLRenderingExtensionFeature != null)
                {
                    MLRenderingExtensionFeature.FocusDistance = mainCamera.stereoConvergence;
                }
#endif
            }
        }

        private void UpdateDebugVisualVisibility(bool visible)
        {
            if (debugVisualsVisible != visible)
            {
                debugVisualsVisible = visible;
                DisplayDebugVisuals(debugVisualsVisible);
            }
        }

        private void DisplayDebugVisuals(bool show)
        {
            if (sphereCastVisual != null)
            {
                sphereCastVisual.SetActive(show);
            }

            if (hitPointVisual != null)
            {
                hitPointVisual.SetActive(show);
            }
        }

#if UNITY_EDITOR

        // When in Editor, validate settings when using SDK 2.0.0 or greater to notify the developer of any potential issues.
        private void OnValidate()
        {

#if MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER
#if UNITY_XR_MAGICLEAP_PROVIDER
            bool mlXRProviderSelected = MagicLeapMRTK3Settings.IsXRProviderSelectedForBuildTarget<MagicLeapLoader>(BuildTargetGroup.Android);
#else
#pragma warning disable CS0219 // assigned but never used
            bool mlXRProviderSelected = false;
#pragma warning restore CS0219
#endif // UNITY_XR_MAGICLEAP_PROVIDER

#if UNITY_OPENXR_1_9_0_OR_NEWER
            bool openXRProviderSelected = MagicLeapMRTK3Settings.IsXRProviderSelectedForBuildTarget<OpenXRLoader>(BuildTargetGroup.Android);
#else
            bool openXRProviderSelected = false;
#endif // UNITY_OPENXR_1_9_0_OR_NEWER

            // Notify error if using ML SDK >= 2.0.0 and < 2.2.0 and targeting ML XR Provider as the MagicLeapCamera won't work.
#if !MAGICLEAP_UNITY_SDK_2_2_0_OR_NEWER
            if (mlXRProviderSelected)
            {
                Debug.LogError($"{GetType().Name} will not work correctly when selecting the Magic Leap XR Provider with this version " +
                    "of the Magic Leap SDK.  Please update to version 2.2.0, or later, of the Magic Leap SDK to resolve the issue, " +
                    "or target the OpenXR XR Provider instead.");
            }
#endif // !MAGICLEAP_UNITY_SDK_2_2_0_OR_NEWER

            if (openXRProviderSelected && eyeTrackingOption == EyeTrackingOptions.UseEyeFixationPointDirectlyAsFocusPoint)
            {
                Debug.LogError($"{GetType().Name}, the {eyeTrackingOption} option is not currently supported under OpenXR.");
            }

#elif UNITY_OPENXR_1_7_0_OR_NEWER && MAGICLEAP_UNITY_SDK_1_9_0_OR_NEWER
            Debug.LogError($"{GetType().Name} will not work correctly with this combination of the Magic Leap SDK package version " +
                "and OpenXR package.  Please update to version 2.2.0, or later, of the Magic Leap SDK to resolve the issue.");
#endif // MAGICLEAP_UNITY_SDK_2_0_0_OR_NEWER, elif UNITY_OPENXR_1_7_0_OR_NEWER && MAGICLEAP_UNITY_SDK_1_9_0_OR_NEWER

        }

#endif // UNITY_EDITOR

    }
}