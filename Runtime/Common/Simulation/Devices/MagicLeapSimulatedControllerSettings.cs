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
using UnityEngine.InputSystem;
using MixedReality.Toolkit.Input.Simulation;

namespace MagicLeap.MRTK.Input.Simulation
{
    /// <summary>
    /// Settings for the simulated Magic Leap Controller
    /// </summary>
    [Serializable]
    public class MagicLeapSimulatedControllerSettings : ControllerSimulationSettings
    {
        public bool ToggledState { get; set; } = false;

        [SerializeField]
        [Tooltip("The input action used to control the state of the menu button.")]
        private InputActionReference menuButton;

        /// <summary>
        /// The input action used to control the state of the menu button.
        /// </summary>
        public InputActionReference MenuButton
        {
            get => menuButton;
            set => menuButton = value;
        }
    }
}
