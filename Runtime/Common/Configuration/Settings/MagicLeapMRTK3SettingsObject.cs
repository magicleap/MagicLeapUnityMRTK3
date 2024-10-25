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
        /// Whether the settings object should operate only when detecting an ML2 runtime.
        /// </summary>
        /// <remarks>
        /// On device, the settings object will operate only when the device is determined to be
        /// an ML2.  In Editor play mode, the settings object will operate if a simulated ML2
        /// runtime is detected.
        /// </remarks>
        public abstract bool RequiresML2Runtime { get; }

        /// <summary>
        /// Whether the settings object is dependent on a specific XR provider being active or not.
        /// </summary>
        public abstract bool DependentOnXRProvider { get; }

        /// <summary>
        /// Whether the settings object is compatible with the active XR loader.
        /// </summary>
        /// <remarks>
        /// This property is used at runtime when the <see cref="DependentOnXRProvider"/> is true.
        /// </remarks>
        public virtual bool CompatibleWithActiveXRLoader => false;

#if UNITY_EDITOR

        [SerializeField]
        [HideInInspector]
        private bool windowedGUIExpanded = true;

        private bool WindowedGUIExpanded
        {
            get => windowedGUIExpanded;
            set
            {
                if (windowedGUIExpanded != value)
                {
                    windowedGUIExpanded = value;
                    // Manually setting the field dirty to
                    // ensure expand/collapse changes cause serialization.
                    EditorUtility.SetDirty(this);
                }
            }
        }

        /// <summary>
        /// The string to use for display of the settings window label.
        /// </summary>
        /// <remarks>
        /// Return an empty string if the settings GUI does not need to be windowed.
        /// </remarks>
        public abstract string SettingsWindowLabel { get; }

        /// <summary>
        /// The string to use for the display of the XR provider label this settings object is valid for.
        /// Leave as empty string if valid for any.
        /// </summary>
        /// <remarks>
        /// This property is used in Editor when the <see cref="DependentOnXRProvider"/> is true.
        /// </remarks>
        public virtual string SettingsXRProviderLabel => string.Empty;

        /// <summary>
        /// Whether the settings object is compatible with the selected XR loader in Editor.
        /// </summary>
        /// <remarks>
        /// This method is used in Editor when the <see cref="DependentOnXRProvider"/> is true.
        /// </remarks>
        public virtual bool CompatibleWithSelectedXRProviderInEditor(MagicLeapMRTK3Settings.XRProviderOption selectedXRProvider) => false;

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

            // Display windowed content
            if (SettingsWindowLabel.Length > 0)
            {
                // Begin Settings UI window
                GUILayout.BeginVertical(new GUIStyle("Window") { padding = new RectOffset(8, 8, 8, 8) });

                string xrProviderLabel = SettingsXRProviderLabel.Length > 0 ?
                    $"    <color=#888888>[XR Provider: <color=#888840>{SettingsXRProviderLabel}</color>]</color>" : "";
                string windowLabel = $"<b>{SettingsWindowLabel}</b>{xrProviderLabel}";

                // Foldout with label (rich text)
                EditorGUILayout.BeginHorizontal();
                WindowedGUIExpanded = EditorGUILayout.Foldout(WindowedGUIExpanded, "", true);

                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(windowLabel)).x + 10.0f;
                EditorGUILayout.LabelField(windowLabel, new GUIStyle(EditorStyles.label) { richText = true, alignment = TextAnchor.UpperLeft });
                EditorGUIUtility.labelWidth = originalLabelWidth;

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // Display expanded content
                if (WindowedGUIExpanded)
                {
                    DrawSettingsGUI();
                }

                GUILayout.EndVertical();
            }
            // Display non-windowed content
            else
            {
                DrawSettingsGUI();
            }

            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettingsGUI()
        {
            EditorGUILayout.Space(8);
            DrawSettingsWindowContent();
            EditorGUILayout.Space(8);
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