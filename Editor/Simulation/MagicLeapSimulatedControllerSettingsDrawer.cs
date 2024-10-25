// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEditor;
using UnityEngine;
using MixedReality.Toolkit.Editor;

namespace MagicLeap.MRTK.Input.Simulation.Editor
{
    /// <summary>
    /// A custom property drawer for <see cref="MagicLeapSimulatedControllerSettings"/> fields.
    /// </summary>
    [CustomPropertyDrawer(typeof(MagicLeapSimulatedControllerSettings))]
    public class MagicLeapSimulatedControllerSettingsDrawer : PropertyDrawer
    {
        private readonly GUIContent neutralPositionContent = new ("Neutral position");

        private readonly GUIContent trackContent = new ("Momentary tracking");
        private readonly GUIContent toggleContent = new ("Toggle tracking");

        private readonly GUIContent jitterStrengthContent = new ("Jitter strength");
        private readonly GUIContent moveSmoothingContent = new ("Smoothed");
        private readonly GUIContent moveDepthContent = new ("Depth");
        private readonly GUIContent depthSensitivityContent = new ("Sensitivity");
        private readonly GUIContent moveHorizontalContent = new ("Horizontal");
        private readonly GUIContent moveVerticalContent = new ("Vertical");

        private readonly GUIContent pitchContent = new ("Pitch");
        private readonly GUIContent invertPitchContent = new ("Invert pitch");
        private readonly GUIContent yawContent = new ("Yaw");
        private readonly GUIContent rollContent = new ("Roll");

        private readonly GUIContent faceTheCameraContent = new ("Face the camera");

        private readonly GUIContent triggerContent = new ("Trigger");
        private readonly GUIContent gripContent = new ("Grip (Bumper)");
        private readonly GUIContent menuContent = new ("Menu");

        /// <inheritdoc />
        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            return PropertyDrawerUtilities.CalculatePropertyHeight(24);
        }

        /// <inheritdoc />
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            bool lastMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(
                position,
                GUIUtility.GetControlID(FocusType.Passive),
                label,
                EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            int rowMultiplier = 0;

            #region Core controls

            SerializedProperty defaultPosition = property.FindPropertyRelative("defaultPosition");

            SerializedProperty track = property.FindPropertyRelative("track");
            SerializedProperty toggle = property.FindPropertyRelative("toggle");

            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                defaultPosition, neutralPositionContent);

            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                track, trackContent);
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                toggle, toggleContent);

            #endregion Core controls

            #region Move controls

            SerializedProperty jitterStrength = property.FindPropertyRelative("jitterStrength");
            SerializedProperty moveSmoothing = property.FindPropertyRelative("isMovementSmoothed");
            SerializedProperty moveDepth = property.FindPropertyRelative("moveDepth");
            SerializedProperty depthSensitivity = property.FindPropertyRelative("depthSensitivity");
            SerializedProperty moveHorizontal = property.FindPropertyRelative("moveHorizontal");
            SerializedProperty moveVertical = property.FindPropertyRelative("moveVertical");

            EditorGUI.LabelField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                "Movement", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                jitterStrength, jitterStrengthContent);
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                moveSmoothing, moveSmoothingContent);
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                moveDepth, moveDepthContent);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                depthSensitivity, depthSensitivityContent);
            EditorGUI.indentLevel--;
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                moveHorizontal, moveHorizontalContent);
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                moveVertical, moveVerticalContent);
            EditorGUI.indentLevel--;

            #endregion Move controls

            #region Rotate controls

            SerializedProperty pitch = property.FindPropertyRelative("pitch");
            SerializedProperty yaw = property.FindPropertyRelative("yaw");
            SerializedProperty roll = property.FindPropertyRelative("roll");
            SerializedProperty invertPitch = property.FindPropertyRelative("invertPitch");

            EditorGUI.LabelField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                "Rotation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                pitch, pitchContent);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                invertPitch, invertPitchContent);
            EditorGUI.indentLevel--;
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                yaw, yawContent);
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                roll, rollContent);
            EditorGUI.indentLevel--;

            #endregion Rotate controls

            #region Hand pose controls

            SerializedProperty faceTheCamera = property.FindPropertyRelative("faceTheCamera");

            EditorGUI.LabelField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                "Controller pose", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                faceTheCamera, faceTheCameraContent);
            EditorGUI.indentLevel--;

            #endregion Hand pose controls

            #region Action controls

            SerializedProperty triggerAxis = property.FindPropertyRelative("triggerAxis");
            SerializedProperty triggerButton = property.FindPropertyRelative("triggerButton");
            SerializedProperty gripAxis = property.FindPropertyRelative("gripAxis");
            SerializedProperty gripButton = property.FindPropertyRelative("gripButton");
            SerializedProperty menuButton = property.FindPropertyRelative("menuButton");

            EditorGUI.LabelField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                "Actions", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.LabelField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                "Button", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                menuButton, menuContent);
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                gripButton, gripContent);
            EditorGUI.PropertyField(
                PropertyDrawerUtilities.GetPosition(
                    position,
                    PropertyDrawerUtilities.VerticalSpacing,
                    ++rowMultiplier,
                    PropertyDrawerUtilities.Height),
                triggerButton, triggerContent);
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel--;

            #endregion Action controls

            EditorGUI.indentLevel--;
            EditorGUIUtility.wideMode = lastMode;

            EditorGUI.EndProperty();
        }
    }
}

