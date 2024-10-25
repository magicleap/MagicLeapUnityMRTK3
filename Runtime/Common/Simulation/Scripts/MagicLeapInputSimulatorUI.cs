// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace MagicLeap.MRTK.Input.Simulation
{
    /// <summary>
    /// Magic Leap on screen UI for input simulator.
    /// </summary>
    public class MagicLeapInputSimulatorUI : MonoBehaviour
    {
        [Header("Active/Inactive Panels")]

        [SerializeField]
        private GameObject activeUIPanel;
        [SerializeField]
        private GameObject inactiveUIPanel;

        [SerializeField]
        private Button minimizeActivePanel;
        [SerializeField]
        private Button maximizeInactivePanel;

        [SerializeField]
        private Toggle latchedToggle;

        [Header("Touchpad")]

        [SerializeField]
        private Button touchpad;
        [SerializeField]
        private Image touchPoint;

        [SerializeField]
        private Slider touchForceSlider;
        [SerializeField]
        private Text touchForceLabel;
        [SerializeField]
        private Text touchPadPositionX;
        [SerializeField]
        private Text touchPadPositionY;

        [Header("UI Buttons")]

        [SerializeField]
        private Button triggerButton;
        [SerializeField]
        private Toggle triggerHoldToggle;
        [SerializeField]
        private Button bumperButton;
        [SerializeField]
        private Toggle bumperHoldToggle;
        [SerializeField]
        private Button menuButton;
        [SerializeField]
        private Toggle menuHoldToggle;

        [Header("Physical Buttons")]

        [SerializeField]
        private Button triggerPhysical;
        [SerializeField]
        private Button bumperPhysical;
        [SerializeField]
        private Button menuPhysical;
        [SerializeField]
        private GameObject triggerPhysicalPressed;
        [SerializeField]
        private GameObject bumperPhysicalPressed;
        [SerializeField]
        private GameObject menuPhysicalPressed;

        [Header("Transform")]

        [SerializeField]
        private GameObject transformActiveBlocker;
        [SerializeField]
        private Button transformXYButton;
        [SerializeField]
        private Button transformDepthButton;
        [SerializeField]
        private Button transformYawPitchButton;
        [SerializeField]
        private Button transformRollButton;
        [SerializeField]
        private GameObject transformXYActive;
        [SerializeField]
        private GameObject transformDepthActive;
        [SerializeField]
        private GameObject transformYawPitchActive;
        [SerializeField]
        private GameObject transformRollActive;

        /// <summary>
        /// Event when the Latched toggle state changes.
        /// </summary>
        public Action<bool> LatchChanged;

        /// <summary>
        /// Trigger pressed state in the UI.
        /// </summary>
        public static bool TriggerPressed { get; private set; } = false;

        /// <summary>
        /// Bumper pressed state in the UI.
        /// </summary>
        public static bool BumperPressed { get; private set; } = false;

        /// <summary>
        /// Menu pressed state in the UI.
        /// </summary>
        public static bool MenuPressed { get; private set; } = false;

        /// <summary>
        /// Touch pressed state in the UI.
        /// </summary>
        public static bool TouchpadPressed { get; private set; } = false;

        /// <summary>
        /// Touch position in the UI.
        /// </summary>
        public static Vector2 TouchpadPosition { get; private set; } = Vector2.zero;

        /// <summary>
        /// Touch force in the UI to simulate.
        /// </summary>
        public static float TouchpadForce { get; private set; } = 0f;

        /// <summary>
        /// Horizontal/Vertical transformation active.
        /// </summary>
        public static bool TransformingXY { get; private set; } = true;

        /// <summary>
        /// The delta horizontal/vertical transformation mouse movement.
        /// </summary>
        public static Vector2 TransformXYDelta { get; private set; } = Vector2.zero;

        /// <summary>
        /// Depth transformation active.
        /// </summary>
        public static bool TransformingDepth { get; private set; } = false;

        /// <summary>
        /// The delta depth transformation mouse movement.
        /// </summary>
        public static float TransformDepthDelta { get; private set; } = 0;

        /// <summary>
        /// Yaw/Pitch transformation active.
        /// </summary>
        public static bool TransformingYawPitch { get; private set; } = false;

        /// <summary>
        /// The delta yaw/pitch transformation mouse movement.
        /// </summary>
        public static Vector2 TransformYawPitchDelta { get; private set; } = Vector2.zero;

        /// <summary>
        /// Roll transformation active.
        /// </summary>
        public static bool TransformingRoll { get; private set; } = false;

        /// <summary>
        /// The delta roll transformation mouse movement.
        /// </summary>
        public static float TransformRollDelta { get; private set; } = 0;

        private static MagicLeapInputSimulatorUI Instance = null;

        private RectTransform touchpadRectTransform;
        private RectTransform touchPointRectTransform;
        private const float TouchpadExtentsScale = .95f; // Touchpad circle texture is slightly smaller than rect

        // The below factors match those of the default input action scale factors for consistent control
        private readonly Vector2 TransformXYFactor = new (.001f, .001f);
        private readonly Vector2 TransformYawPitchFactor = new (.1f, .4f);
        private const float TransformDepthFactor = .001f;
        private const float TransformRollFactor = .4f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("There should only be one MagicLeapInputSimulatorUI, destroying a duplicate instance.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            latchedToggle.onValueChanged.AddListener((isLatched) => LatchChanged?.Invoke(isLatched));

            minimizeActivePanel.onClick.AddListener(() =>
            {
                activeUIPanel.SetActive(false);
                inactiveUIPanel.SetActive(true);
            });
            maximizeInactivePanel.onClick.AddListener(() =>
            {
                activeUIPanel.SetActive(true);
                inactiveUIPanel.SetActive(false);
            });

            touchForceSlider.onValueChanged.AddListener((_) => TouchpadForce = touchForceSlider.value);

            SetupControllerButton(triggerButton, triggerHoldToggle, (bool pressed) => { TriggerPressed = pressed; });
            SetupControllerButton(bumperButton, bumperHoldToggle, (bool pressed) => { BumperPressed = pressed; });
            SetupControllerButton(menuButton, menuHoldToggle, (bool pressed) => { MenuPressed = pressed; });

            SetupControllerButton(triggerPhysical, triggerHoldToggle, (bool pressed) => { TriggerPressed = pressed; });
            SetupControllerButton(bumperPhysical, bumperHoldToggle, (bool pressed) => { BumperPressed = pressed; });
            SetupControllerButton(menuPhysical, menuHoldToggle, (bool pressed) => { MenuPressed = pressed; });

            SetupTouchpad(touchpad);

            SetupTransformation(transformXYButton, (bool active) => { TransformingXY = active; });
            SetupTransformation(transformDepthButton, (bool active) => { TransformingDepth = active; });
            SetupTransformation(transformYawPitchButton, (bool active) => { TransformingYawPitch = active; });
            SetupTransformation(transformRollButton, (bool active) => { TransformingRoll = active; });

            ClearState();
            UpdateVisualState();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Clear the UI state.
        /// </summary>
        public void ClearState()
        {
            // Clear UI elements
            triggerHoldToggle.isOn = false;
            bumperHoldToggle.isOn = false;
            menuHoldToggle.isOn = false;

            // Clear properties
            TriggerPressed = false;
            BumperPressed = false;
            MenuPressed = false;
            TouchpadPressed = false;
            TouchpadPosition = Vector2.zero;
            TouchpadForce = touchForceSlider.value;

            TransformingXY = false;
            TransformXYDelta = Vector2.zero;
            TransformingDepth = false;
            TransformDepthDelta = 0;
            TransformingYawPitch = false;
            TransformYawPitchDelta = Vector2.zero;
            TransformingRoll = false;
            TransformRollDelta = 0;

            UpdateVisualState();
        }

        /// <summary>
        /// Set the UI latched state.
        /// </summary>
        public void SetLatched(bool latched)
        {
            latchedToggle.isOn = latched;
        }

        private void SetupControllerButton(Button button, Toggle holdToggle, Action<bool> pressedAction)
        {
            // Setup button events
            EventTrigger eventTrigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDownEvent = new EventTrigger.Entry();
            pointerDownEvent.eventID = EventTriggerType.PointerDown;
            pointerDownEvent.callback.AddListener((x) =>
            {
                if (button.interactable)
                {
                    pressedAction(true);
                }
            });
            eventTrigger.triggers.Add(pointerDownEvent);

            EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();
            pointerUpEvent.eventID = EventTriggerType.PointerUp;
            pointerUpEvent.callback.AddListener((x) =>
            {
                if (button.interactable)
                {
                    pressedAction(false);
                }
            });
            eventTrigger.triggers.Add(pointerUpEvent);

            // Setup hold toggle events
            button.interactable = !holdToggle.isOn;
            holdToggle.onValueChanged.AddListener((x) =>
            {
                button.interactable = !holdToggle.isOn;
                pressedAction(holdToggle.isOn);
            });
        }

        private void SetupTouchpad(Button touchpadButton)
        {
            // Obtain touchpad hierarchy references
            touchpadRectTransform = touchpad.GetComponent<RectTransform>();
            touchPointRectTransform = touchPoint.GetComponent<RectTransform>();

            // Setup touchpad events
            EventTrigger eventTrigger = touchpadButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDownEvent = new EventTrigger.Entry();
            pointerDownEvent.eventID = EventTriggerType.PointerDown;
            pointerDownEvent.callback.AddListener((x) =>
            {
                TouchpadPressed = true;
                UpdateTouchPosition();
            });
            eventTrigger.triggers.Add(pointerDownEvent);

            EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();
            pointerUpEvent.eventID = EventTriggerType.PointerUp;
            pointerUpEvent.callback.AddListener((x) =>
            {
                TouchpadPressed = false;
                UpdateTouchPosition();
            });
            eventTrigger.triggers.Add(pointerUpEvent);
        }

        private void SetupTransformation(Button button, Action<bool> activeAction)
        {
            // Setup button events
            EventTrigger eventTrigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDownEvent = new EventTrigger.Entry();
            pointerDownEvent.eventID = EventTriggerType.PointerDown;
            pointerDownEvent.callback.AddListener((x) =>
            {
                activeAction(true);
            });
            eventTrigger.triggers.Add(pointerDownEvent);

            EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();
            pointerUpEvent.eventID = EventTriggerType.PointerUp;
            pointerUpEvent.callback.AddListener((x) =>
            {
                activeAction(false);
            });
            eventTrigger.triggers.Add(pointerUpEvent);
        }

        private void UpdateTouchPosition()
        {
            TouchpadPosition = TouchpadPressed ? CalculateRectTransformNormalizedPosition(touchpadRectTransform, TouchpadExtentsScale) :
                                                 Vector2.zero;
        }

        private Vector2 CalculateRectTransformNormalizedPosition(RectTransform rectTransform, float rectExtentsScale = 1.0f)
        {
            Vector2 mouseScreenPosition = new Vector2(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mouseScreenPosition, null, out Vector2 localPoint);

            Vector2 rectExtents = rectTransform.rect.max * rectExtentsScale;
            return Vector2.ClampMagnitude(localPoint / rectExtents, 1);
        }

        private void UpdateTransformation()
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            TransformXYDelta = TransformingXY ? mouseDelta * TransformXYFactor : Vector2.zero;
            TransformDepthDelta = TransformingDepth ? mouseDelta.y * TransformDepthFactor : 0f;
            TransformYawPitchDelta = TransformingYawPitch ? mouseDelta * TransformYawPitchFactor : Vector2.zero;
            TransformRollDelta = TransformingRoll ? mouseDelta.x * TransformRollFactor : 0f;
        }

        private void UpdateVisualState()
        {
            // Touchpad visuals
            touchPoint.gameObject.SetActive(TouchpadPressed);
            touchPointRectTransform.localPosition = TouchpadPosition * touchpadRectTransform.rect.max * TouchpadExtentsScale;
            touchPadPositionX.text = TouchpadPressed ? TouchpadPosition.x.ToString("0.####") : "";
            touchPadPositionY.text = TouchpadPressed ? TouchpadPosition.y.ToString("0.####") : "";
            touchForceLabel.text = touchForceSlider.value.ToString("0.####");

            // Physical button visuals
            triggerPhysicalPressed.SetActive(TriggerPressed);
            bumperPhysicalPressed.SetActive(BumperPressed);
            menuPhysicalPressed.SetActive(MenuPressed);

            // Transform visuals
            transformActiveBlocker.SetActive(TransformingXY || TransformingDepth || TransformingYawPitch || TransformingRoll);
            transformXYActive.SetActive(TransformingXY);
            transformDepthActive.SetActive(TransformingDepth);
            transformYawPitchActive.SetActive(TransformingYawPitch);
            transformRollActive.SetActive(TransformingRoll);
        }

        public void Update()
        {
            UpdateTouchPosition();
            UpdateTransformation();
            UpdateVisualState();
        }
    }
}