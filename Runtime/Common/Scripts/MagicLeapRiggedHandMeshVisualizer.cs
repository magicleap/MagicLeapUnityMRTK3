// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// BEGIN ML CHANGE
// This file is derived from the file RiggedHandMeshVisualizer.cs of MRTK, Copyright (c) Microsoft Corporation,
// as acquired from https://github.com/microsoft/MixedRealityToolkit-Unity/blob/29a5fa536d2f31f471e63db73b0cc58657df10d1/com.microsoft.mrtk.input/Visualizers/RiggedHandVisualizer/RiggedHandMeshVisualizer.cs
// under the terms of the MIT license and modified extensively by Magic Leap, Inc.
// The file, as modified, is here provided to you under the terms of the LICENSE file appearing in the
// top-level directory of this distribution.

using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
#if MRTK_INPUT_4_0_0_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
#endif
// END ML CHANGE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

// ML CHANGE: Namespace change
namespace MagicLeap.MRTK.Input
{
    /// <summary>
    /// Hand visualizer that uses a rigged mesh/armature to render high-quality hand meshes.
    /// Not recommended for AR platforms like HoloLens, both for performance and design reasons. 
    /// ML CHANGE: Modified the joint error metric to be calculated along the parent transform forward which
    ///            seems to provide better results, as well as included more joints in that calculation to
    ///            better respond to hand scaling.  Also specific modifications for ML2 joints.
    /// </summary>
    /// <remarks>
    /// For augmented reality platforms such as HoloLens, we recommend not using any hand visualizations,
    /// as the conflict between the user's real hand and the slightly delayed holographic visualization
    /// can be more distracting than it's worth. However, for opaque platforms, this is a great solution.
    /// </remarks>
    // ML CHANGE: Menu path change
    [AddComponentMenu("MRTK/Input/Visualizers/Magic Leap Rigged Hand Mesh Visualizer")]
    // BEGIN ML CHANGE: Class name change, and support for MRTK3 v4
    public class MagicLeapRiggedHandMeshVisualizer : MonoBehaviour
#if MRTK_INPUT_4_0_0_OR_NEWER
        , ISelectInputVisualizer
#endif
    // END ML CHANGE
    {
        [SerializeField]
        [Tooltip("The XRNode on which this hand is located.")]
        private XRNode handNode = XRNode.LeftHand;

        /// <summary> The XRNode on which this hand is located. </summary>
        public XRNode HandNode { get => handNode; set => handNode = value; }

        [SerializeField]
        [Tooltip("When true, this visualizer will render rigged hands even on XR devices " +
                 "with transparent displays. When false, the rigged hands will only render " +
                 "on devices with opaque displays.")]
        private bool showHandsOnTransparentDisplays;

        /// <summary>
        /// When true, this visualizer will render rigged hands even on XR devices with transparent displays.
        /// When false, the rigged hands will only render on devices with opaque displays.
        /// Usually, it's recommended not to show hand visualization on transparent displays as it can
        /// distract from the user's real hands, and cause a "double image" effect that can be disconcerting.
        /// </summary>
        public bool ShowHandsOnTransparentDisplays
        {
            get => showHandsOnTransparentDisplays;
            set => showHandsOnTransparentDisplays = value;
        }

        [SerializeField]
        [Tooltip("The transform of the wrist joint.")]
        private Transform wrist;

        [SerializeField]
        [Tooltip("Renderer of the hand mesh")]
        private SkinnedMeshRenderer handRenderer = null;

        [SerializeField]
        [Tooltip("Name of the shader property used to drive pinch-amount-based visual effects. " +
                 "Generally, maps to something like a glow or an outline color!")]
        private string pinchAmountMaterialProperty = "_PinchAmount";

        // Automatically calculated over time, based on the accumulated error
        // between the user's actual joint locations and the armature's bones/joints.
        private float handScale = 1.0f;

        // The property block used to modify the pinch amount property on the material
        private MaterialPropertyBlock propertyBlock = null;

        // Caching local references 
        private HandsAggregatorSubsystem handsSubsystem;

        // Scratch list for checking for the presence of display subsystems.
        private List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();

        // BEGIN ML CHANGE:  Implement ISelectInputVisualizer interface when using MRTK3 version
        //                   4.0.0 or newer.  Also disable deprecated member warning for
        //                   XRBaseController.
#if MRTK_INPUT_4_0_0_OR_NEWER
        private XRInputButtonReader selectInput = null;

        #region ISelectInputVisualizer implementation

        /// <summary>
        /// Input reader used when pinch selecting an interactable.
        /// </summary>
        public XRInputButtonReader SelectInput
        {
            get => selectInput;
            set => selectInput = value;
        }

        #endregion ISelectInputVisualizer implementation
#endif

        // The XRController that is used to determine the pinch strength (i.e., select value!)
#pragma warning disable CS0618 // Type or member is obsolete
        private XRBaseController controller;
#pragma warning restore CS0618 // Type or member is obsolete
        // END ML CHANGE

        // The actual, physical, rigged joints that drive the skinned mesh.
        // Otherwise referred to as "armature". Must be in OpenXR order.
        private readonly Transform[] riggedVisualJointsArray = new Transform[(int)TrackedHandJoint.TotalJoints];

        // The substring used to determine the "leaf joint"
        // at the end of a finger, which is discarded.
        private const string endJointName = "end";

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (handRenderer == null)
            {
                handRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (handRenderer == null)
                {
                    Debug.LogWarning("RiggedHandMeshVisualizer couldn't find your rigged mesh renderer! " +
                                     "You should set it manually.");
                }
            }

            if (wrist == null)
            {
                // "Armature" is the default name that Blender assigns
                // to the root of an armature/rig. Also happens to be the wrist joint!
                wrist = transform.Find("Armature");

                if (wrist == null)
                {
                    Debug.LogWarning("RiggedHandMeshVisualizer couldn't find the wrist joint on your hand mesh. " +
                                     "You should set it manually!");

                    // Abort initialization as we don't even have a wrist joint to go off of.
                    return;
                }
            }

            // Start the depth-first-traversal at the wrist index.
            int index = (int)TrackedHandJoint.Wrist;

            // This performs a depth-first-traversal of the armature. Ensure
            // the provided armature's bones/joints are in OpenXR order.
            foreach (Transform child in wrist.GetComponentsInChildren<Transform>())
            {
                // The "leaf joints" are excluded.
                if (child.name.Contains(endJointName)) { continue; }

                riggedVisualJointsArray[index++] = child;
            }
        }

        /// <summary>
        /// A Unity event function that is called when the script component has been enabled.
        /// </summary>
        protected void OnEnable()
        {
            // Ensure hand is not visible until we can update position first time.
            handRenderer.enabled = false;

            Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand,
                         $"HandVisualizer has an invalid XRNode ({handNode})!");

            handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

            if (handsSubsystem == null)
            {
                StartCoroutine(EnableWhenSubsystemAvailable());
            }
        }

        /// <summary>
        /// A Unity event function that is called when the script component has been disabled.
        /// </summary>
        protected void OnDisable()
        {
            // Disable the rigged hand renderer when this component is disabled
            handRenderer.enabled = false;
        }

        /// <summary>
        /// Coroutine to wait until subsystem becomes available.
        /// </summary>
        private IEnumerator EnableWhenSubsystemAvailable()
        {
            yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
            OnEnable();
        }

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        private void Update()
        {
            // Query all joints in the hand.
            if (!ShouldRenderHand() ||
                !handsSubsystem.TryGetEntireHand(handNode, out IReadOnlyList<HandJointPose> joints))
            {
                // Hide the hand and abort if we shouldn't be
                // showing the hand, for whatever reason.
                // (Missing joint data, no subsystem, additive
                // display, etc!)
                handRenderer.enabled = false;
                return;
            }

            handRenderer.enabled = true;

            // We'll accumulate joint error as we iterate over each joint
            // and compare it to the user's actual joint data.
            float error = 0.0f;

            for (int i = 0; i < joints.Count; i++)
            {
                HandJointPose jointPose = joints[i];

                // The actual, physical, rigged joint on the armature.
                // This actually corresponds to the "base" of the bone;
                // as an example, riggedVisualJointsArray[IndexMetacarpal] actually
                // corresponds to a transform that is located at the wrist joint,
                // but points towards the metacarpal joint location.
                // This discrepancy is because OpenXR uses joint locations/rotations,
                // whereas armatures/Unity/Blender use *bones*.
                Transform jointTransform = riggedVisualJointsArray[i];
                // ML CHANGE: Obtain preceding joint transform.  In most cases (non-MCP or Wrist joints),
                //            this corresponds to the parent joint within the hand bone hierarchy.
                //            The parent joint is used in the error calculation for certain joints.
                Transform preJointTransform = i > 0 ? riggedVisualJointsArray[i - 1] : null;

                if (jointTransform != null)
                {
                    switch ((TrackedHandJoint)i)
                    {
                        case TrackedHandJoint.Palm:
                            // Don't track the palm. The hand mesh shouldn't have a "palm bone".
                            break;
                        case TrackedHandJoint.Wrist:
                            // Set the wrist directly from the joint data.
                            jointTransform.position = jointPose.Position;
                            jointTransform.rotation = jointPose.Rotation;
                            break;
                        case TrackedHandJoint.ThumbTip:
                        case TrackedHandJoint.IndexTip:
                        case TrackedHandJoint.MiddleTip:
                        case TrackedHandJoint.RingTip:
                        case TrackedHandJoint.LittleTip:
                            // The tip bone uses the joint rotation directly.
                            // BEGIN ML CHANGE:  Use the joint rotation, but clean it up for fingers (not thumb) so that
                            //                   there aren't any "funny" finger bends at the distal to tip.
                            //                   Also modify the joint error metric to be calculated along the parent forward
                            //                   instead of the joint's own forward.  This provided better scaling results.
                            if ((TrackedHandJoint)i == TrackedHandJoint.ThumbTip)
                            {
                                jointTransform.rotation = joints[i - 1].Rotation;
                            }
                            else
                            {
                                Vector3 localForward = Quaternion.Inverse(preJointTransform.rotation) * joints[i - 1].Forward;
                                // No bending sideways
                                localForward.x = 0;
                                // No bending backwards
                                if (localForward.y > 0)
                                {
                                    localForward.y = 0;
                                }
                                jointTransform.localRotation = localForward.sqrMagnitude > Mathf.Epsilon ?
                                    Quaternion.LookRotation(localForward) : Quaternion.identity;
                            }
                            // Compute and accumulate the error between the hand mesh and the user's joint data along parent joint forward.
                            error += JointError(jointTransform.position, joints[i - 1].Position, preJointTransform.forward);
                            // END ML CHANGE
                            break;
                        // BEGIN ML CHANGE:  Only set the rotation for the ThumbMetacarpal on ML2 as our finger MCP joints are too
                        //                   tightly clustered and would cause the hand mesh to be pinched near the wrist.
                        case TrackedHandJoint.ThumbMetacarpal:
                            jointTransform.rotation = Quaternion.LookRotation(jointPose.Position - joints[(int)TrackedHandJoint.Wrist].Position, jointPose.Up);
                            break;
                        case TrackedHandJoint.IndexMetacarpal:
                        case TrackedHandJoint.MiddleMetacarpal:
                        case TrackedHandJoint.RingMetacarpal:
                        case TrackedHandJoint.LittleMetacarpal:
                            // Special case metacarpals, because Wrist is not always i-1.
                            // This is the same "simple IK" as the default case, but with special index logic.
                            // ML CHANGE:  Do not set rotation for finger MCP joints, leave default finger MCP location as is within hand mesh.
                            break;
                        // END ML CHANGE
                        default:
                            // For all other bones, do a simple "IK" from the rigged joint to the joint data's position.
                            jointTransform.rotation = Quaternion.LookRotation(jointPose.Position - jointTransform.position, joints[i - 1].Up);
                            // ML CHANGE:  Include other joints in the error calculation and use parent joint transform's forward.
                            error += JointError(jointTransform.position, joints[i - 1].Position, preJointTransform.forward);
                            break;
                    }
                }
            }

            // Compute and apply the adjusted scale of the hand.
            // Over time, we'll grow or shrink the rigged hand
            // to more accurately fit the actual size of the
            // user's hand.

            // How quickly the hand will grow or shrink
            // to fit the user's hand size.
            const float errorGainFactor = 0.1f;

            // Reasonable minimum and maximum for how much
            // the hand mesh is allowed to stretch to fit the user.
            const float minScale = 0.8f;
            const float maxScale = 1.1f;

            // Apply.
            handScale += -error * errorGainFactor;
            handScale = Mathf.Clamp(handScale, minScale, maxScale);
            transform.localScale = new Vector3(handNode == XRNode.LeftHand ? -handScale : handScale, handScale, handScale);

            // Update the hand material based on selectedness value
            UpdateHandMaterial();
        }

        // Computes the error between the rig's joint position and
        // the user's joint position along the finger vector.
        private float JointError(Vector3 armatureJointPosition, Vector3 userJointPosition, Vector3 fingerVector)
        {
            // The computed error between the rigged mesh's joints and the user's joints
            // is essentially the distance between the mesh and user joints, projected
            // along the forward axis of the finger itself; i.e., the "length error" of the finger.
            return Vector3.Dot((armatureJointPosition - userJointPosition), fingerVector);
        }

        private bool ShouldRenderHand()
        {
            // If we're missing anything, don't render the hand.
            if (handsSubsystem == null || wrist == null || handRenderer == null)
            {
                return false;
            }

            if (displaySubsystems.Count == 0)
            {
                SubsystemManager.GetSubsystems(displaySubsystems);
            }

            // Are we running on an XR display and it happens to be transparent?
            // Probably shouldn't be showing rigged hands! (Users can
            // specify showHandsOnTransparentDisplays if they disagree.)
            if (displaySubsystems.Count > 0 &&
                displaySubsystems[0].running &&
                !displaySubsystems[0].displayOpaque &&
                !showHandsOnTransparentDisplays)
            {
                return false;
            }

            // All checks out!
            return true;
        }

        private void UpdateHandMaterial()
        {
            // BEGIN ML CHANGE:  Get pinch amount depending on which version of the rig this hand model
            //                   is in, legacy or new XRI 3+ rig.
            if (handRenderer == null)
            {
                return;
            }

            float selectValue = 0.0f;

#if MRTK_INPUT_4_0_0_OR_NEWER
            if (selectInput == null || !selectInput.TryReadValue(out selectValue))
            {
#else
            {
#endif
                if (controller == null)
                {
#pragma warning disable CS0618 // XRBaseController is obsolete
                    controller = GetComponentInParent<XRBaseController>();
#pragma warning restore CS0618 // XRBaseController is obsolete
                }

                if (controller != null)
                {
                    selectValue = controller.selectInteractionState.value;
                }
            }

            // Update the hand material
            float pinchAmount = Mathf.Pow(selectValue, 2.0f);
            handRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(pinchAmountMaterialProperty, pinchAmount);
            handRenderer.SetPropertyBlock(propertyBlock);
            // END ML CHANGE
        }
    }
}
