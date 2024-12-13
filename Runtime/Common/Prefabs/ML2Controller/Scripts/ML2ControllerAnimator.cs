// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using UnityEngine.InputSystem;

namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// Provides ML2 Controller animation to reflect user input with the device.
    /// </summary>
    public class ML2ControllerAnimator : MonoBehaviour
    {
        [Header("Trigger Animation")]
        [Space(4)]

        [SerializeField]
        [Tooltip("Whether the trigger animation is enabled.")]
        bool triggerAnimationEnabled = true;

        /// <summary>
        /// Whether the trigger animation is enabled.
        /// </summary>
        public bool TriggerAnimationEnabled
        {
            get => triggerAnimationEnabled;
            set => triggerAnimationEnabled = value;
        }

        [SerializeField]
        [Tooltip("The input action for the trigger value.")]
        InputActionProperty triggerValueInputAction;

        [SerializeField]
        [Tooltip("The Trigger object in the model hierarchy.")]
        GameObject triggerObject;

        [SerializeField]
        [Tooltip("The rotational Euler X angle of the Trigger object at full trigger press.")]
        float triggerPressedAngle = -25f;

        [SerializeField]
        [Tooltip("The animation curve for the trigger angle based on trigger value input.")]
        AnimationCurve triggerPressedCurve = new AnimationCurve();

        [Space(8)]

        [Header("Bumper Animation")]
        [Space(4)]

        [SerializeField]
        [Tooltip("Whether the bumper animation is enabled.")]
        bool bumperAnimationEnabled = true;

        /// <summary>
        /// Whether the bumper animation is enabled.
        /// </summary>
        public bool BumperAnimationEnabled
        {
            get => bumperAnimationEnabled;
            set => bumperAnimationEnabled = value;
        }

        [SerializeField]
        [Tooltip("The input action for the Bumper button.")]
        InputActionProperty bumperInputAction;

        [SerializeField]
        [Tooltip("The Bumper button object in the model hierarchy.")]
        GameObject bumperObject;

        [SerializeField]
        [Tooltip("The Y axis movement of the Bumper button when pressed.")]
        float bumperPressedYTranslation = -.0006f;

        [Space(8)]

        [Header("Menu Animation")]
        [Space(4)]

        [SerializeField]
        [Tooltip("Whether the menu animation is enabled.")]
        bool menuAnimationEnabled = true;

        /// <summary>
        /// Whether the menu animation is enabled.
        /// </summary>
        public bool MenuAnimationEnabled
        {
            get => menuAnimationEnabled;
            set => menuAnimationEnabled = value;
        }

        [SerializeField]
        [Tooltip("The input action for the Menu button.")]
        InputActionProperty menuInputAction;

        [SerializeField]
        [Tooltip("The Menu button object in the model hierarchy.")]
        GameObject menuObject;

        [SerializeField]
        [Tooltip("The Y axis movement of the Menu button when pressed.")]
        float menuPressedYTranslation = -.0006f;

        [Space(8)]

        [Header("Touchpad Animation")]
        [Space(4)]

        [SerializeField]
        [Tooltip("Whether the touchpad animation is enabled.")]
        bool touchpadAnimationEnabled = true;

        /// <summary>
        /// Whether the touchpad animation is enabled.
        /// </summary>
        public bool TouchpadAnimationEnabled
        {
            get => touchpadAnimationEnabled;
            set => touchpadAnimationEnabled = value;
        }

        [SerializeField]
        [Tooltip("The input action for the touchpad pressed.")]
        InputActionProperty touchpadTouchedInputAction;

        [SerializeField]
        [Tooltip("The input action for the touchpad position.")]
        InputActionProperty touchpadPositionInputAction;

        [SerializeField]
        [Tooltip("The input action for the touchpad force.")]
        InputActionProperty touchpadForceInputAction;

        [SerializeField]
        [Tooltip("The touchpad surface object to render force and position of touch.")]
        GameObject touchpadSurface;

        [SerializeField]
        [Tooltip("The minimum scale of the touchpad press point based on force.")]
        float touchPointScaleMin = .75f;

        [SerializeField]
        [Tooltip("The minimum scale of the touchpad press point based on force.")]
        float touchPointScaleMax = 1.25f;


        private bool triggerSetup = false;
        private bool bumperSetup = false;
        private bool menuSetup = false;
        private bool touchpadSetup = false;

        private float currentTriggerValue = 0f;
        private bool currentBumperPressed = false;
        private bool currentMenuPressed = false;
        private bool currentTouchpadTouched = false;

        private Material touchpadSurfaceMaterial;
        private bool touchpadHasMainTexture = false;
        private bool touchpadHasFalloffTexture = false;

        private void Awake()
        {
            // Validate field setup
            triggerSetup = triggerObject != null &&
                           triggerValueInputAction != null &&
                           triggerValueInputAction.action != null;

            bumperSetup = bumperObject != null &&
                          bumperInputAction != null &&
                          bumperInputAction.action != null;

            menuSetup = menuObject != null &&
                        menuInputAction != null &&
                        menuInputAction.action != null;

            touchpadSetup = touchpadSurface != null &&
                            touchpadTouchedInputAction != null && touchpadTouchedInputAction.action != null &&
                            touchpadPositionInputAction != null && touchpadPositionInputAction.action != null &&
                            touchpadForceInputAction != null && touchpadForceInputAction.action != null;

            if (touchpadSurface != null)
            {
                var meshRenderer = touchpadSurface.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer != null)
                {
                    touchpadSurfaceMaterial = meshRenderer.material;
                    touchpadHasMainTexture = touchpadSurfaceMaterial != null ? touchpadSurfaceMaterial.HasProperty("_MainTex") : false;
                    touchpadHasFalloffTexture = touchpadSurfaceMaterial != null ? touchpadSurfaceMaterial.HasProperty("_FalloffTex") : false;
                }
            }
        }

        void Update()
        {
            // Trigger
            if (triggerSetup)
            {
                float triggerValue = triggerAnimationEnabled ? triggerValueInputAction.action.ReadValue<float>() : 0.0f;
                if (triggerValue != currentTriggerValue)
                {
                    currentTriggerValue = triggerValue;
                    float triggerPivotNormalized = triggerPressedCurve?.Evaluate(currentTriggerValue) ?? currentTriggerValue;
                    float triggerPivotAngle = Mathf.Lerp(0, triggerPressedAngle, triggerPivotNormalized);
                    triggerObject.transform.localRotation = Quaternion.Euler(triggerPivotAngle, 0, 0);
                }
            }

            // Bumper
            if (bumperSetup)
            {
                bool bumperPressed = bumperAnimationEnabled ? bumperInputAction.action.ReadValue<float>() > 0 : false;
                if (bumperPressed != currentBumperPressed)
                {
                    currentBumperPressed = bumperPressed;
                    bumperObject.transform.localPosition = Vector3.up * (currentBumperPressed ? bumperPressedYTranslation : 0);
                }
            }

            // Menu
            if (menuSetup)
            {
                bool menuPressed = menuAnimationEnabled ? menuInputAction.action.ReadValue<float>() > 0 : false;
                if (menuPressed != currentMenuPressed)
                {
                    currentMenuPressed = menuPressed;
                    menuObject.transform.localPosition = Vector3.up * (currentMenuPressed ? menuPressedYTranslation : 0);
                }
            }

            // Touchpad
            if (touchpadSetup)
            {
                bool touchpadTouched = touchpadAnimationEnabled ? touchpadTouchedInputAction.action.ReadValue<float>() > 0 : false;
                if (touchpadTouched != currentTouchpadTouched)
                {
                    currentTouchpadTouched = touchpadTouched;
                    touchpadSurface.SetActive(currentTouchpadTouched);
                }
                if (currentTouchpadTouched && touchpadSurfaceMaterial != null)
                {
                    Vector2 touchpadPosition = touchpadPositionInputAction.action.ReadValue<Vector2>();
                    float touchpadForce = touchpadForceInputAction.action.ReadValue<float>();
                    float scale = Mathf.Lerp(touchPointScaleMin, touchPointScaleMax, touchpadForce);
                    float textureScale = 1f / scale;

                    Vector2 touchTextureOrigin = Vector2.one * (1f - textureScale) / 2.0f;
                    Vector2 touchTextureDelta = touchpadPosition * textureScale * .5f;
                    Vector2 touchTextureOffset = touchTextureOrigin - touchTextureDelta;
                    Vector2 touchTextureScale = Vector2.one * textureScale;

                    // The touchpad animation expects a specific material with a shader containing certain texture properties to
                    // provide the touchpad visual.  However, this method can work with other materials that have a main texture.
                    if (touchpadHasMainTexture)
                    {
                        touchpadSurfaceMaterial.mainTextureOffset = touchTextureOffset;
                        touchpadSurfaceMaterial.mainTextureScale = touchTextureScale;
                    }
                    if (touchpadHasFalloffTexture)
                    {
                        touchpadSurfaceMaterial.SetTextureOffset("_FalloffTex", touchTextureOffset);
                        touchpadSurfaceMaterial.SetTextureScale("_FalloffTex", touchTextureScale);
                    }
                }
            }
        }
    }
}
