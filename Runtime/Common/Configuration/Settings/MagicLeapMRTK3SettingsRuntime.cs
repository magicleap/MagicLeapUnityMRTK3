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

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Coordinates the runtime handling of the MagicLeapMRTK3Settings
    /// </summary>
    public class MagicLeapMRTK3SettingsRuntime
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            ProcessSettings(RuntimeInitializeLoadType.BeforeSceneLoad);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            ProcessSettings(RuntimeInitializeLoadType.AfterSceneLoad);
        }

        private static void ProcessSettings(RuntimeInitializeLoadType runtimeInitType)
        {
            // Settings that don't require ML2 Runtime
            foreach (var settingsObject in MagicLeapMRTK3Settings.Instance.SettingsObjects)
            {
                if (!settingsObject.RequiresML2Runtime)
                {
                    ProcessSetting(settingsObject, runtimeInitType);
                }
            }

            if (!MagicLeapMRTK3Settings.RuntimeIsCompatible())
            {
                return;
            }

            // Settings that require ML2 runtime but are not dependent on XR Providers
            foreach (var settingsObject in MagicLeapMRTK3Settings.Instance.SettingsObjects)
            {
                if (settingsObject.RequiresML2Runtime &&
                    !settingsObject.DependentOnXRProvider)
                {
                    ProcessSetting(settingsObject, runtimeInitType);
                }
            }

            // Settings that require ML2 runtime and are dependent & compatible with XR Provider
            foreach (var settingsObject in MagicLeapMRTK3Settings.Instance.SettingsObjects)
            {
                if (settingsObject.RequiresML2Runtime &&
                    settingsObject.DependentOnXRProvider &&
                    settingsObject.CompatibleWithActiveXRLoader)
                {
                    ProcessSetting(settingsObject, runtimeInitType);
                }
            }
        }

        private static void ProcessSetting(
            MagicLeapMRTK3SettingsObject settingsObject,
            RuntimeInitializeLoadType runtimeInitType)
        {
            switch (runtimeInitType)
            {
                case RuntimeInitializeLoadType.BeforeSceneLoad:
                    settingsObject.ProcessOnBeforeSceneLoad();
                    break;
                case RuntimeInitializeLoadType.AfterSceneLoad:
                    settingsObject.ProcessOnAfterSceneLoad();
                    break;
                default:
                    break;
            }
        }
    }
}