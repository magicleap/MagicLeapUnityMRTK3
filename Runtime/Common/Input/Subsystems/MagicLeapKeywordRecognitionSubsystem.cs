// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.MagicLeap;
using UnityEngine.Events;
using System.Collections.Concurrent;

using MagicLeap.MRTK.Settings;
#if UNITY_ANDROID && !UNITY_EDITOR

#if MAGICLEAP_UNITY_SDK_2_1_0_OR_NEWER
using MagicLeap.Android;
#endif

using System.Threading;
#endif

namespace MagicLeap.MRTK.Input
{
    [Preserve]
    [MRTKSubsystem(
        Name = "com.magicleap.xr.keywordrecognition",
        DisplayName = "MagicLeap Subsystem for Keyword Recognition API",
        Author = "MagicLeap",
        ProviderType = typeof(MagicLeapVoiceIntentProvider),
        SubsystemTypeOverride = typeof(MagicLeapKeywordRecognitionSubsystem),
        ConfigType = typeof(BaseSubsystemConfig))]
    public class MagicLeapKeywordRecognitionSubsystem : KeywordRecognitionSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            if (!MagicLeapMRTK3Settings.DeviceIsCompatible())
            {
                return;
            }

            // Fetch subsystem metadata from the attribute.
            var cinfo = XRSubsystemHelpers.ConstructCinfo<MagicLeapKeywordRecognitionSubsystem, KeywordRecognitionSubsystemCinfo>();

            if (!Register(cinfo))
            {
                Debug.LogError($"Failed to register the {cinfo.Name} subsystem.");
            }
        }

        [Preserve]
        private class MagicLeapVoiceIntentProvider : Provider
        {
            private ConcurrentQueue<UnityEvent> eventQueue;
            private MLVoiceIntentsConfiguration voiceConfig;
            private bool voiceConfigDirty;
            private bool mlVoicePermissionGranted;
            private bool mlVoiceEnabled;
            private bool MLVoiceReady => mlVoiceEnabled && mlVoicePermissionGranted;
            private static uint intentID;

            public MagicLeapVoiceIntentProvider()
            {
                eventQueue = new ConcurrentQueue<UnityEvent>();
                intentID = 1;
                voiceConfigDirty = false;

#if UNITY_ANDROID && !UNITY_EDITOR
#if MAGICLEAP_UNITY_SDK_2_1_0_OR_NEWER
                mlVoicePermissionGranted = Permissions.CheckPermission(MLPermission.VoiceInput);
#else
                mlVoicePermissionGranted = MLPermissions.CheckPermission(MLPermission.VoiceInput).IsOk;
#endif
                mlVoiceEnabled = JavaUtils.GetSystemSetting<int>("getInt", "enable_voice_cmds") == 1;
                if (!MLVoiceReady)
                {
                    // Poll for the permission and check granted status every second.
                    SynchronizationContext mainSyncContext = SynchronizationContext.Current;
                    System.Timers.Timer timer = new System.Timers.Timer(1000);
                    timer.Start();
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
                    {
                        mainSyncContext.Post(_ =>
                        {
#if MAGICLEAP_UNITY_SDK_2_1_0_OR_NEWER
                            mlVoicePermissionGranted = Permissions.CheckPermission(MLPermission.VoiceInput);
#else
                            mlVoicePermissionGranted = MLPermissions.CheckPermission(MLPermission.VoiceInput).IsOk;
#endif
                            mlVoiceEnabled = JavaUtils.GetSystemSetting<int>("getInt", "enable_voice_cmds") == 1;
                            if (MLVoiceReady)
                            {
                                timer.Stop();
                            }
                        }, null);
                    };
                }
#endif

            }

            /// <inheritdoc/>
            public override void Start()
            {
                base.Start();
                if (voiceConfig != null)
                {
                    SetupVoiceIntents();
                }
            }

            /// <inheritdoc/>
            public override void Update()
            {
                if (MLVoiceReady)
                {
                    if (voiceConfigDirty)
                    {
                        // The voice intent configuration does not fully support dynamic updates.
                        // You must call the API to setup voice intents and resubscribe to events.
                        // This allows us to register multiple added/removed keywords at once.
                        // A similar method is used in WindowsKeywordRecognitionSystem.
                        voiceConfigDirty = false;
                        Destroy();
                        UpdateVoiceConfig();
                    }
                }
                while (eventQueue.TryDequeue(out UnityEvent unityEvent))
                {
                    unityEvent.Invoke();
                }
            }

            /// <inheritdoc/>
            public override void Stop()
            {
                if (voiceConfig != null)
                {
                    MLVoice.Stop();
                    MLVoice.OnVoiceEvent -= OnMLVoiceEvent;
                }
                base.Stop();
            }

            /// <inheritdoc/>
            public override void Destroy()
            {
                Stop();
            }

            #region IKeywordRecognitionSubsystem implementation

            /// <inheritdoc/>
            public override IReadOnlyDictionary<string, UnityEvent> GetAllKeywords()
            {
                return keywordDictionary;
            }

            /// <inheritdoc/>
            public override void RemoveAllKeywords()
            {
                keywordDictionary.Clear();
                voiceConfigDirty = true;
            }

            /// <inheritdoc/>
            public override void RemoveKeyword(string keyword)
            {
                keywordDictionary.Remove(keyword);
                voiceConfigDirty = true;
            }

            /// <inheritdoc/>
            public override UnityEvent CreateOrGetEventForKeyword(string keyword)
            {
                if (keywordDictionary.TryGetValue(keyword, out UnityEvent e))
                {
                    return e;
                }
                else
                {
                    UnityEvent unityEvent = new UnityEvent();
                    keywordDictionary.Add(keyword, unityEvent);
                    voiceConfigDirty = true;
                    return unityEvent;
                }
            }

            #endregion IKeywordRecognitionSubsystem implementation

            private void InitializeVoiceConfigurationAsset()
            {
                // [NOTE] MLVoiceIntentConfiguration doesn't initialize public members,
                //        and will fail without this.
                voiceConfig = ScriptableObject.CreateInstance<MLVoiceIntentsConfiguration>();
                voiceConfig.VoiceCommandsToAdd = new List<MLVoiceIntentsConfiguration.CustomVoiceIntents>();
                voiceConfig.AutoAllowAllSystemIntents = false;
                voiceConfig.SlotsForVoiceCommands = new List<MLVoiceIntentsConfiguration.SlotData>();
                voiceConfig.AllVoiceIntents = new List<MLVoiceIntentsConfiguration.JSONData>();
            }

            private void AddCustomIntent(string keyword)
            {
                MLVoiceIntentsConfiguration.CustomVoiceIntents newIntent;
                newIntent.Id = intentID++;
                newIntent.Value = keyword;
                voiceConfig.VoiceCommandsToAdd.Add(newIntent);
            }

            private void UpdateVoiceConfig()
            {
                if (voiceConfig == null)
                {
                    InitializeVoiceConfigurationAsset();
                }

                if (voiceConfig != null) {
                    // Keep our voice config consistent with internal keywordDictionary
                    voiceConfig.VoiceCommandsToAdd.Clear();
                    foreach (string keyword in keywordDictionary.Keys)
                    {
                        AddCustomIntent(keyword);
                    }
                    SetupVoiceIntents();
                }
            }

            private void SetupVoiceIntents()
            {
                MLResult result = MLVoice.SetupVoiceIntents(voiceConfig);
                if (result.IsOk)
                {
                    MLVoice.OnVoiceEvent += OnMLVoiceEvent;
                }
                else
                {
                    Debug.LogError("MagicLeapVoiceIntentProvider failed to setup voice intents: " + result.ToString());
                }
            }

            private void OnMLVoiceEvent(in bool wasSuccessful, in MLVoice.IntentEvent voiceEvent)
            {
                if (wasSuccessful)
                {
                    if (keywordDictionary.TryGetValue(voiceEvent.EventName, out UnityEvent e))
                    {
                        eventQueue.Enqueue(e);
                    }
                }
            }
        }
    }
}