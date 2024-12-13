# Changelog

## 1.4.0
### Features

- Added options to the dangerous permissions section in **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings** > **Runtime Permissions Configuration**.
  - Added FACIAL_EXPRESSION as an enumerated permission.
  - Added the ability to manually specify additional permissions to be requested.
- Added an updated model for the ML2 Controller with static and input-driven animated prefabs.
  - The prefabs are located in /Runtime<wbr>/<wbr>Common<wbr>/<wbr>Prefabs<wbr>/<wbr>ML2Controller/.
  - The provided Magic Leap MRTK3 Rig prefabs have been updated to use the new animated model by default.
- Added utilities to offer automated and manual options to convert materials within the package when detecting use of URP or HDRP.
  - Please see options for automatic package material conversion in **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings**.
  - To manually invoke package material conversion see menu item(s) at **Magic Leap** > **MRTK3** when URP or HDRP is active.

### Bugfixes

- Fixed an issue with the ML2 Controller prefab, MLXR provider version, where the Far Ray interactor erroneously had "Hit Closest Only" disabled.

## 1.3.0
### Features
 
- Added the `MagicLeapInputSimulator` prefab to provide Magic Leap input simulation (e.g. Controller) when playing in the Unity Editor.
  - To use the Magic Leap input simulator, either enable the settings option to automatically add it in Editor play mode, in **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings**,
    or manually place the prefab in the scene. The prefab is located in /Runtime<wbr>/<wbr>Common<wbr>/<wbr>Simulation<wbr>/<wbr>Prefabs/.
- Updated the StereoConvergenceDetector to use the new MagicLeapEyeTrackerFeature when using OpenXR and Magic Leap Unity SDK 2.4.0 or greater.

### Bugfixes

- Fixed some minor issues in the provided samples.

## 1.2.0
### Features
 
- Provide compatibility with the MRTK3 version 4.0.0 release.
  - Package functionality and options will continue to work with either MRTK3 version 3.0.0+ or version 4.0.0+ releases.
  - The runtime rig configuration option will work with either the legacy (version 3.0.0) MRTK rig or the new (version 4.0.0) MRTK rig.

### Bugfixes

- Fixed an issue with detecting when the package systems are running on a Magic Leap 2 platform for secondary users, now enabling certain package functionality in this case where it was previously disabled.

## 1.1.0
### Features
 
- Added an option to the Runtime Rig Configuration for OpenXR to override the XROrigin's requested tracking origin mode.  The current recommended tracking origin mode for the ML2 is `Device` mode.
- Updated the OpenXR rig variant to request `Device` tracking mode.
- Made an improvement to the responsiveness of the hand and controller multimodal behavior, MagicLeapControllerHandProximityDisabler, for when the controller tracking state changes and when detecting a hand is holding the controller.
- Exposed the time threshold property of the MagicLeapControllerHandProximityDisabler component for having a consistent target before a hand switch is made.

### Misc.

- Updated LICENSE and NOTICE

## 1.0.0
### Features

- Incorporated better pinch detection and then retention when hand keypoints become occluded.
- Added a `StereoConvergenceDetector` utility prefab to assist in utilizing Magic Leap's camera focus distance feature, which provides better capture alignment along with reduction/removal of "judder" on device in some scenarios.  Please see the **Stereo Convergence Detector Examples** for a demonstration of the feature.
- The **Spatial Awareness Examples** sample was updated to work when using the OpenXR provider.

### Bugfixes

- Fixed an issue where the `Setting for XR Provider` choice dropdown in **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings** was potentially getting reset when re-visiting the page.  This didn't affect any options, just may have been undesirable.

## 1.0.0-pre.7.1
### Bugfixes

- Fixed an issue where the Magic Leap MRTK3 settings and systems were unintentionally running on platforms other than ML2 when using the OpenXR XR Provider.

### Known Issues / Limitations

- The provided **Spatial Awareness Examples** sample, demonstrating mesh reconstruction, currently only works with the Magic Leap XR Provider.  OpenXR support will be added to the sample in a later release.
- Other MagicLeap 2 apis that overlap with MRTK3 are not yet configured or hooked up.

## 1.0.0-pre.7
### Features

- Added OpenXR support when using the **com.unity.xr.openxr** package (version 1.9.1 or later), and choosing the `OpenXR` plugin under **Edit** > **Project Settings** > **XR Plug-in Management**.  New profiles, prefabs and settings have been provided to support running with OpenXR.  Please see the `OpenXR XR Provider Setup` section in the `README` for details on configuring the project to run with the OpenXR Provider.
- Added an option to choose the hand ray algorithm between stock MRTK and an alternative Magic Leap algorithm.  See the setting options in **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings** > **General Settings**.
- Added an option to choose the hand and controller multimodal behavior when using both hands and the ML controller at the same time.  For instance, if the hand holding the controller should be fully disabled, or just the hand's far ray disabled.  See the setting options in **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings** > **General Settings**.
- Added a Global and Segmented Dimmer sample to demonstrate Magic Leap's dimmer capabilities.

### Known Issues / Limitations

- The provided **Spatial Awareness Examples** sample, demonstrating mesh reconstruction, currently only works with the Magic Leap XR Provider.  OpenXR support will be added to the sample in a later release.
- Other MagicLeap 2 apis that overlap with MRTK3 are not yet configured or hooked up.

## 1.0.0-pre.6
### Features

- Added an MRTK3 Keyword Recognition Subsystem for the ML2 platform.
  - Added a Keyword Recognition Subsystem sample to showcase the feature, combined with voice permission and settings enabled status to visualize what is required to enable the feature. 
- Added a "Keep Rendered Alpha" setting to provide better photo & video capture capabilities, along with better segmented dimmer support.  See the setting in **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings** > **General Settings**.
- The **MRTK XR Rig - MagicLeap** prefab has been updated to allow for a new possible minimum near clipping plane of 25cm, as apposed to the previous 37cm, when using Unity SDK 1.13 or greater and supporting OS.

### Known Issues / Limitations

- Other MagicLeap 2 apis that overlap with MRTK3 are not yet
  configured or hooked up.

## 1.0.0-pre.5
### Features

- Switched package dependencies to the latest org.mixedrealitytoolkit packages from the new Mixed Reality Toolkit Org repo, (https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity), where future updates to MRTK3 will come.  To use this version of the com.magicleap.mrtk3 package, be sure to update your project's MRTK3 packages to latest.
- Switched the hand ray back to the stock MRTK hand ray.

### Bugfixes
- Fixed an issue with the hand mesh not scaling well, and causing undesirable finger pose matching, during certain hand poses on the ML 2 platform.

### Known Issues / Limitations

- Other MagicLeap 2 apis that overlap with MRTK3 are not yet
  configured or hooked up.

## 1.0.0-pre.4
### Features

- Added two samples:
  - Eye Tracking Example - showcasing eye tracking on the ML2 platform.  For best results, run the Custom Fit application to calibrate eye tracking.
  - Spatial Awareness Example - showcasing scene reconstruction/meshing of your environment with several visual options.
  - Be sure to use the **Runtime Permission Configuration** to request eye tracking and spatial mapping permissions for the samples to work.
- Added a General Settings area to the **Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings** project settings.
  - Provides an option to `Observe OS Setting for Enabling Hand Navigation` to enable/disable Hand interactions within MRTK3 based on the OS setting.  This setting defaults to off so hand interactions are available for MRTK3 by default.

### Bugfixes
- Fixed an issue with near interactions and the MagicLeap Controller when the controller was added dynamically to the rig at runtime (via Runtime Rig Config).
- Fixed the auxiliary devices causing an error when playing in Editor when the ML Application Simulator was not active.
- Fixed a compile issue with an API change in `HandsSubsystem` within the core MRTK package. 

### Known Issues / Limitations

- Other MagicLeap 2 apis that overlap with MRTK3 are not yet
  configured or hooked up.

## 1.0.0-pre.3
### Features

- Added the Runtime Permission Configuration to make permission management easier on the ML2 platform.
  - Provides an option to request or start certain permissions from settings, without needing to modify any scene.
  - Also provides for instantiating a prefab at runtime to receive permission (granted, denied) callbacks for custom handling.
  - Available in (**Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings**).
- Updated compatibility to Magic Leap Unity SDK 1.8 (or later).

### Bugfixes
- Fixed an issue with the Runtime Rig Configuration possibly adding XR Controllers that already exist in the rig, causing issues.
  The name of a controller to be added must now not match any pre-existing rig controller in order to be added.

### Known Issues / Limitations

- Other MagicLeap 2 apis that overlap with MRTK3 are not yet
  configured or hooked up.

## 1.0.0-pre.2
### Features

- Support for Eye Tracking (Gaze).
- Added the Runtime MRTK XR Rig Configuration to make the default rig compatible with ML 2 input at runtime.
  - Provides an option that no longer requires modification of the scene by having had to swap in the ML rig variant.
  - Available in (**Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings**).
- Updated compatibility to Magic Leap Unity SDK 1.6 (or later).

### Bugfixes
- Fixed the issue with the MagicLeapAuxiliaryHandDevice not cleaning up after exiting Play Mode in Editor.

### Known Issues / Limitations

- Other MagicLeap 2 apis that overlap with MRTK3 are not yet
  configured or hooked up.

## 1.0.0-pre.1
### Features

- Initial support for MRTK3 on MagicLeap 2
- Support for Hand Tracking and Controller

### Known Issues / Limitations

- Other MagicLeap 2 apis that overlap with MRTK3 are not yet
  configured or hooked up.
