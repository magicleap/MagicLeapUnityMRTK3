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
        static void OnBeforeSceneLoad()
        {
            if (!MagicLeapMRTK3Settings.RuntimeIsCompatible())
            {
                return;
            }

            foreach (var settingsObject in MagicLeapMRTK3Settings.Instance.SettingsObjects)
            {
                if (settingsObject.CompatibleWithActiveXRLoader)
                {
                    settingsObject.ProcessOnBeforeSceneLoad();
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            if (!MagicLeapMRTK3Settings.RuntimeIsCompatible())
            {
                return;
            }

            foreach (var settingsObject in MagicLeapMRTK3Settings.Instance.SettingsObjects)
            {
                if (settingsObject.CompatibleWithActiveXRLoader)
                {
                    settingsObject.ProcessOnAfterSceneLoad();
                }
            }
        }
    }
}