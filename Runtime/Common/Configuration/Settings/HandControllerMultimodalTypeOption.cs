// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace MagicLeap.MRTK.Settings
{
    /// <summary>
    /// Options for the type of hand and controller multimodal behavior.
    /// </summary>
    public enum HandControllerMultimodalTypeOption
    {
        /// <summary>
        /// The hand holding the controller is fully disabled.
        /// </summary>
        HandHoldingControllerFullyDisabled,

        /// <summary>
        /// The hand holding the controller is enabled, but the
        /// far hand ray is disabled.
        /// </summary>
        HandHoldingControllerRayDisabled,

        /// <summary>
        /// Both hands are disabled while the controller is active (tracking).
        /// </summary>
        HandsDisabledWhileControllerActive,

        /// <summary>
        /// Hands and controller are always active, and apps can use
        /// their own logic to handle the interaction.
        /// </summary>
        HandsAndControllerAlwaysActive,
    };
}
