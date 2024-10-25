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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Base class for all MagicLeap MRTK3 Settings ScriptableObjects
    /// </summary>
    public abstract class MagicLeapMRTK3SettingsObject : ScriptableObject
    {
        /// <summary>
        /// Process settings early within <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/>.
        /// </summary>
        public abstract void ProcessOnBeforeSceneLoad();

        /// <summary>
        /// Process settings after the first scene is loaded within <see cref="RuntimeInitializeLoadType.AfterSceneLoad"/>.
        /// </summary>
        public abstract void ProcessOnAfterSceneLoad();

        /// <summary>
        /// Whether the settings object is compatible with the active XR loader.
        /// </summary>
        public abstract bool CompatibleWithActiveXRLoader { get; }

#if UNITY_EDITOR

        /// <summary>
        /// The string to use for display of the settings window label.
        /// </summary>
        public abstract string SettingsWindowLabel { get; }

        /// <summary>
        /// The string to use for the display of the XR provider label this settings object is valid for.
        /// Leave as empty string if valid for any.
        /// </summary>
        public virtual string SettingsXRProviderLabel => string.Empty;

        /// <summary>
        /// Whether the settings object is compatible with the selected XR loader in Editor.
        /// </summary>
        public abstract bool CompatibleWithSelectedXRProviderInEditor(MagicLeapMRTK3Settings.XRProviderOption selectedXRProvider);

        /// <summary>
        /// SerializedObject representation in Editor.
        /// </summary>
        protected SerializedObject serializedObject;

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
        }

        /// <summary>
        /// Draw settings in Inspector.
        /// </summary>
        public virtual void OnGUI()
        {
            serializedObject.Update();

            string xrProviderLabel = SettingsXRProviderLabel.Length > 0 ?
                $"    <color=#888888>[XR Provider: <color=#888840>{SettingsXRProviderLabel}</color>]</color>" : "";
            string windowLabel = $"<b>{SettingsWindowLabel}</b>{xrProviderLabel}\n";

            // Begin Settings UI window
            GUILayout.BeginVertical(windowLabel, new GUIStyle("Window") { richText = true });
            {
                EditorGUILayout.Space(8);

                DrawSettingsWindowContent();

                EditorGUILayout.Space(8);
            }
            GUILayout.EndVertical();
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        public abstract void DrawSettingsWindowContent();

        /// <summary>
        /// Helper method to draw a line separator in GUI.
        /// </summary>
        /// <param name="height">Height of the line.</param>
        /// <param name="width">Width of the line.</param>
        protected static void DrawGUILineSeparator(float height = 2, float width = 0)
        {
            Rect lineSeparator = EditorGUILayout.GetControlRect(false, height);
            lineSeparator.width = width > 0 ? width : lineSeparator.width;
            EditorGUI.DrawRect(lineSeparator, new Color(.35f, .35f, .35f));
        }
#endif
    }
}