// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.Samples.KeywordRecognition
{
    public class PermissionStatus : MonoBehaviour
    {
        [SerializeField]
        private GameObject permissionVisual;
        [SerializeField]
        private GameObject statusMessage;

        void Start()
        {
            checkVoiceInputPermissionStatus();
            StartCoroutine(pollStatus());
        }

        private IEnumerator pollStatus()
        {
            while (true)
            {
                checkVoiceInputPermissionStatus();
                yield return new WaitForSeconds(1f);
            }
        }

        private void checkVoiceInputPermissionStatus()
        {
            MLResult voiceInputPermResult = MLPermissions.CheckPermission(MLPermission.VoiceInput);
            bool isGranted = voiceInputPermResult.IsOk ? true : false;
            setVisualColor(isGranted);
            setStatusMessage(voiceInputPermResult.Result.ToString());
        }

        private void setVisualColor(bool isGranted)
        {
            if (permissionVisual != null)
            {
                MeshRenderer meshRenderer = permissionVisual.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.material.color = isGranted ? Color.green : Color.red;
                }
            }
        }

        private void setStatusMessage(string message)
        {
            if (statusMessage != null)
            {
                TextMeshProUGUI statusText = statusMessage.GetComponent<TextMeshProUGUI>();
                statusText.text = message;
            }
        }
    }
}