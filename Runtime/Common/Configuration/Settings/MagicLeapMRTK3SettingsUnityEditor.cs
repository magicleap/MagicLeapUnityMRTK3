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
using UnityEngine.SceneManagement;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input.Simulation;
using MagicLeap.MRTK.Input.Simulation;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Provides setting that run in the Unity Editor.
    /// </summary>
    public sealed class MagicLeapMRTK3SettingsUnityEditor : MagicLeapMRTK3SettingsObject
    {
        private const uint PermissionsFileVersion = 1;

        [SerializeField]
        [HideInInspector]
        private uint version = PermissionsFileVersion;
        public uint Version => version;

        public enum AddInputSimulatorOption
        {
            Disabled,
            WhenMRTKInputSimulatorIsPresent,
            Always
        }

        [SerializeField]
        [Tooltip("Options to automatically add the MagicLeapInputSimulator prefab to the scene in Editor play mode if not already in the scene.")]
        private AddInputSimulatorOption addMagicLeapInputSimulator = AddInputSimulatorOption.Disabled;
        public AddInputSimulatorOption AddMagicLeapInputSimulator => addMagicLeapInputSimulator;

        [SerializeField]
        [Tooltip("The MagicLeapInputSimulator prefab to add.")]
        private GameObject magicLeapInputSimulatorPrefab;
        public GameObject MagicLeapInputSimulatorPrefab => magicLeapInputSimulatorPrefab;

        /// <inheritdoc/>
        public override bool RequiresML2Runtime => false;

        /// <inheritdoc/>
        public override bool DependentOnXRProvider => false;

#if UNITY_EDITOR

        /// <inheritdoc/>
        public override string SettingsWindowLabel => "";

        /// <inheritdoc/>
        public override void DrawSettingsWindowContent()
        {
            EditorGUILayout.LabelField("Magic Leap Input Simulator", EditorStyles.boldLabel);

            float originalLabelWidth = EditorGUIUtility.labelWidth;
            const string addInputSimulatorLabel = "Add simulator prefab to scenes in Editor play mode";
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(addInputSimulatorLabel)).x + 10.0f;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("addMagicLeapInputSimulator"),
                                                                        new GUIContent(addInputSimulatorLabel));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("magicLeapInputSimulatorPrefab"));
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

#endif // UNITY_EDITOR

        /// <inheritdoc/>
        public override void ProcessOnBeforeSceneLoad()
        {
            // Only runs in Unity Editor
#if UNITY_EDITOR
            SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        /// <inheritdoc/>
        public override void ProcessOnAfterSceneLoad()
        {
            // Nothing to do here, instead handle each scene loaded in OnSceneLoaded.
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            ProcessAddingMagicLeapInputSimulatorPrefab();
        }

        private void ProcessAddingMagicLeapInputSimulatorPrefab()
        {
            // The ML Input Simulator prefab does not get added if:
            // - Option is disabled
            // - No prefab specified
            // - Prefab is already in scene
            if (addMagicLeapInputSimulator == AddInputSimulatorOption.Disabled ||
                magicLeapInputSimulatorPrefab == null ||
                ComponentCache<MagicLeapInputSimulator>.FindFirstActiveInstance() != null)
            {
                return;
            }

            if (addMagicLeapInputSimulator == AddInputSimulatorOption.Always)
            {
                InstantiateSimulatorPrefab();
            }
            else if (addMagicLeapInputSimulator == AddInputSimulatorOption.WhenMRTKInputSimulatorIsPresent)
            {
                var mrtkInputSimulator = ComponentCache<InputSimulator>.FindFirstActiveInstance();
                if (mrtkInputSimulator != null)
                {
                    InstantiateSimulatorPrefab();
                }
            }

        }

        private void InstantiateSimulatorPrefab()
        {
            Instantiate(magicLeapInputSimulatorPrefab);
        }
    }
}