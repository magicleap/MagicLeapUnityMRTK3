
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
using MixedReality.Toolkit.UX;
using TMPro;

namespace MagicLeap.MRTK.Samples.StereoConvergenceDetector
{
    public class UiControls : MonoBehaviour
    {
        [SerializeField]
        private GameObject stereoConvergenceDetectorPrefab;
        [SerializeField]
        private PressableButton debugVisualToggle;
        [SerializeField]
        private PressableButton stereoConvergenceDetectorToggle;
        [SerializeField]
        private GameObject nearFieldParent;
        [SerializeField]
        private GameObject farFieldParent;
        [SerializeField]
        private TextMeshProUGUI dynamicDescriptionText;


        private Utilities.StereoConvergenceDetector stereoConvergenceDetector;

        // Start is called before the first frame update
        void Start()
        {
            if (stereoConvergenceDetectorPrefab == null ||
                debugVisualToggle == null ||
                stereoConvergenceDetectorToggle == null ||
                nearFieldParent == null ||
                farFieldParent == null ||
                dynamicDescriptionText == null)
            {
                enabled = false;
                Debug.LogError("MRTK3 StereoConvergenceDetectorSample: UiControls object is null, disabling script.");
            }

            stereoConvergenceDetector = stereoConvergenceDetectorPrefab.GetComponent<Utilities.StereoConvergenceDetector>();
        }

        public void OnDebugVisualToggleSelect()
        {
            bool toggled = debugVisualToggle.IsToggled;
            if (stereoConvergenceDetector != null)
            {
                stereoConvergenceDetector.ShowDebugVisuals = toggled;
            }
        }

        public void OnStereoConvergenceDetectorToggleSelect()
        {
            bool toggled = stereoConvergenceDetectorToggle.IsToggled;
            stereoConvergenceDetectorPrefab.SetActive(toggled);
        }

        public void OnSpherecastLayerToggleSelected(int selectedToggle)
        {
            if (selectedToggle == 0)
            {
                setLayerCurrentHierarchy(nearFieldParent, LayerMask.NameToLayer("Default"));
                setLayerCurrentHierarchy(farFieldParent, LayerMask.NameToLayer("Default"));
                dynamicDescriptionText.text = "Focus on either the near or far objects and notice " +
                                              "the object being looked at will have no blur when moving " +
                                              "the headset from side-to-side.";
            }
            else if (selectedToggle == 1)
            {
                setLayerCurrentHierarchy(nearFieldParent, LayerMask.NameToLayer("Default"));
                setLayerCurrentHierarchy(farFieldParent, LayerMask.NameToLayer("Ignore Raycast"));
                dynamicDescriptionText.text = "Only near objects will be detected for focus. Look at the far " +
                                              "objects, while near objects are focused, and notice the far " +
                                              "object blur when moving the headset from side-to-side due " +
                                              "to incorrect focus distance. ";
            }
            else
            {
                setLayerCurrentHierarchy(nearFieldParent, LayerMask.NameToLayer("Ignore Raycast"));
                setLayerCurrentHierarchy(farFieldParent, LayerMask.NameToLayer("Default"));
                dynamicDescriptionText.text = "Only far objects will be detected for focus. Look at the near " +
                                              "objects, while far objects are focused, and notice the near " +
                                              "object blur when moving the headset from side-to-side due " +
                                              "to incorrect focus distance.";

            }
        }

        private void setLayerCurrentHierarchy(GameObject root, int layer)
        {
            root.layer = layer;
            var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = layer;
            }
        }
    }
}