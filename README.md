# Magic Leap MRTK3 Package

This package enables and provides utilities for MRTK3 development on the Magic Leap 2.

## Features

| Input Support | *Description* |
|--|--|
| ML2 Controller | *Support for the Magic Leap 2 Controller, with rig interactor prefab and visual model.* |
| Hand Tracking | *Support for hand tracking, with options to choose the hand ray type and multimodal mode when used in conjunction with the ML2 Controller.* |
| Eye Tracking | *Support for eye tracking and eye gaze interactions.* |
| Keyword Recognition | *Support for MRTK3's Keyword Recognition Subsystem, along with "See it, Say it" labels.* |

| Utilities | *Description* |
|--|--|
| Magic Leap Settings | *Our settings provide various options and utilities to make development easier on the Magic Leap 2.  This includes things like automatic permission requests and runtime configuration of the default MRTK rig for input compatibility and optimization on ML2.* |
| StereoConvergenceDetector | *A utility prefab that can be used in a scene to assist in detecting the user's focus point (what they are looking at) and setting Magic Leap 2's camera focus distance.  Setting the focus distance properly provides better capture alignment along with reducing "judder" on device.  Located in `/Runtime/Common/Prefabs/StereoConvergenceDetector/`.* |
| TrackedHandJointVisualizer | *A utility prefab to visualize the tracked hand joints with labeled keypoints.  Located in `/Runtime/Common/Prefabs/TrackedHandJointVisuals/`.* |


| Samples | *Description* |
|--|--|
| Hand And Controller Interaction Examples | *Demonstrates both hand and Magic Leap 2 Controller interactions at the same time.* |
| Eye Tracking Examples | *Demonstrates eye tracking and eye gaze interactions.* |
| Spatial Awareness Examples | *Demonstrates scene reconstruction with meshing, along with options for mesh visualization.* |
| Keyword Recognition Examples | *Demonstrates MRTK3's Keyword Recognition Subsystem working on the Magic Leap 2.* |
| Global and Segmented Dimmer Examples | *Demonstrates Magic Leap 2's global and segmented dimmer features, along with dynamic options for control.* |
| Stereo Convergence Detector Examples | *Demonstrates the StereoConvergenceDetector utility and the effect of focus distance.* |

## Prerequisites

- Magic Leap SDK v1.2.0 (or later)
- Magic Leap Unity SDK v1.8.0 (or later)

## Getting Started

Before importing the Magic Leap MRTK3 Package, developers will need to configure their project for MRTK3. This section provides general guidance on downloading and installing the MRTK3 packages using the Mixed Reality Feature tool (Windows Only) or using the MRTK3 Dev Template Project.

### Using the MRTK Dev Template Project

#### Ready-made ML2 port of the MRTK Dev Template Project

There is a ready-made ML2 port of the MRTK Dev Template Project provided within the `mrtk3_MagicLeap2` branch in the [Magic Leap fork of the MRTK Github repository](https://github.com/magicleap/MixedRealityToolkit-Unity/tree/mrtk3_MagicLeap2).  This is the quickest and easiest way to get an MRTK3 app up and running on the Magic Leap 2.  To use the ported Dev Template project, clone the forked MRTK GitHub repo and check out the `mrtk3_MagicLeap2` branch. [Official Microsoft Guide Here] (https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-template)

If you work with Git using the command line, you can clone the repo while specifying the mrtk3_MagicLeap2 branch: `git clone --branch mrtk3_MagicLeap2 https://github.com/magicleap/MixedRealityToolkit-Unity`

#### Original MRTK Dev Template Project

The original MRTK Dev Template project is available by downloading the MRTK project from the [MRTK Github repository](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity). To use the original Template project, clone MRTK from the GitHub repo and open the `MRTKDevTemplate` project under `UnityProjects` in Unity. [Official Microsoft Guide Here] (https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-template)

Clone the repo on the command line: `git clone  https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity.git`

Once the Github project is downloaded, follow the steps below to update the project settings to be compatible with Magic Leap 2.

##### Original Dev Template Project Setup

1. Using the Unity Hub, open the `MRTKDevTemplate` project using Unity version 2022.2.x or later.
   1. On the **Opening Project in Non-Matching Editor Installation** popup select **Continue**.
   2. On the **Script Updating Consent** popup select **Yes, for these and other files that might be found later**.
   3. On the **Enter Safe Mode?** popup select **Ignore**.
2. Clear any errors that appear as a result of a missing dependency from a prefab of XR provider.
   1. If Errors are still present, close the project and delete the project's Library folder and re-open unity to reimport the existing packages.
3. Download and Install the [Magic Leap Setup Tool](https://assetstore.unity.com/packages/tools/integration/magic-leap-setup-tool-194780) from the Unity Asset store.
4. Once installed, use the Project Setup window to configure your project settings. Complete all the steps in the project setup tool.

### Creating a new MRTK3 Project

This section assumes you have already configured your Unity Project for ML2. (https://developer.magicleap.cloud/learn/docs/guides/unity/getting-started/configure-unity-settings)

Download the MRTK3 dependencies using one of two methods: Using the [MixedReality Feature Tool](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool) (Windows Only), Expanded on below, or by manually downloading and importing the packages from [Github](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity). MRTK Input and MRTK UX Components packages are required for using with Magic Leap, please review [MRTK Package Dependencies](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/packages/packages-overview) to know which dependencies are required for those two packages.

#### Using the Mixed Reality Feature Tool

This section provides instructions on installing the MRTK3 dependencies into an existing project using the [Mixed Reality Feature tool](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool). Note this tool is only available for Windows.

1. Open the Mixed Reality Feature tool
2. Target your Unity project.
3. At a minimum install the following packages, as they are required:

- MRTK3 / MRTK Input
- MRTK3 / MRTK UX Components

*Note: If you do not see MRTK3, you may need to select the **Show preview releases** option located at the bottom of the window.*

4. After choosing the the packages to install select **Get Features**. This will display the package dependencies.
5. Finally, press **Import** then **Approve**
6. Clear any errors that appear as a result of a missing dependency from a prefab of XR provider.

To learn more see Microsoft's [Starting from a new project](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-new-project) guide.

## Importing the Magic Leap MRTK3 Package

Once the project is configured for ML2 and has the required MRTK3 packages, import the Magic Leap MRTK3 package into the project.

### Importing from the NPMJS package registry

Add the NPMJS scoped registry to your project.

- URL:  http://registry.npmjs.org
- Scope(s): com.magicleap

Import the `Magic Leap MRTK3` package with the Package Manager (**Window** > **Package Manager**) under **Packages: My Registries**, or include the package directly in the manifest.json, e.g. `"com.magicleap.mrtk3": "1.0.0",`.

### Importing from the Magic Leap Hub

Install the latest version of the Unity MRTK3 package from the Magic Leap Hub. Then from the Package Manager (**Window** > **Package Manager**), import the `com.magicleap.mrtk3.tgz` package that was installed from the Hub. Select the **＋** icon then select **Add package from tarball...** and choose the path used by the Magic Leap Hub to import the package into the project.

## Project Setup with XR Providers

There are currently two options of XR Providers that run on Magic Leap 2. Either, or both, XR Provider packages can be in the project, but only one active at a time.  See the setup instructions below to configure your project to run with either.

### Magic Leap XR Provider Setup

If the project contains the **com.unity.xr.magicleap** package, you can use the `Magic Leap` XR plugin.  Follow the below steps to setup the MRTK3 project to run properly with this XR Provider.

1. Bring up project settings (**Edit** > **Project Settings**).

2. Within project settings, go to the **XR Plug-in Management** category on the left.
   1. Select the `Android` settings tab.
   2. Select the `Magic Leap` provider in the list.  Make sure this is the only selected XR Provider for Android.

3. Within project settings, go to the **MRTK3** category on the left.
   1. Set the **Profile** to **MRTKProfile-MagicLeap**, found at `/Runtime/XRProviders/MagicLeap/Configuration/Default Profiles/`.

4. Configure the MRTK XR Rig to be compatible with Magic Leap 2 input (2 provided options).
   1. First option:  Use the Runtime MRTK XR Rig Configuration option in settings.
      - Within project settings, go to the **MRTK3** > **Magic Leap Settings** category on the left.
      - Make sure the `Magic Leap` option is selected for **Settings for XR Provider:** at the top to view specific settings for the Magic Leap XR Provider.
      - Check the `Runtime Rig Config Enabled` checkbox at the top of the **Runtime MRTK XR Rig Configuration** window.
         - Enabling this feature will allow the default MRTK XR Rig to work with ML2 input without needing to modify the scene.
   2. Second option:  In your scenes, remove and replace the default MRTK XR Rig with the `MRTK XR Rig - MagicLeap` prefab variant located in `/Runtime/XRProviders/MagicLeap/Prefabs/MRTK_Variants/`.

### OpenXR XR Provider Setup

If the project contains the **com.unity.xr.openxr** package (version 1.9.1 or later), you can use the `OpenXR` XR plugin.  Follow the below steps to setup the MRTK3 project to run properly with this XR Provider.

1. The following packages and their minimum versions are required to use OpenXR on Magic Leap 2 with MRTK3:
   - **com.magicleap.unitysdk** version 2.0.0 (or later)
   - **com.unity.xr.openxr** version 1.9.1 (or later)
   - **com.unity.xr.hands** version 1.4.0-pre.1 (or later)

2. Bring up project settings (**Edit** > **Project Settings**).

3. Within project settings, go to the **XR Plug-in Management** category on the left.
   1. Select the `Android` settings tab.
   2. Select the `OpenXR` provider in the list.  Make sure this is the only selected XR Provider for Android.
      - Select the `Magic Leap Feature Group`.
   3. Select the **XR Plug-in Management** > **OpenXR** category on the left for more options.
      - Set the **Depth Submission Mode** to `None`.
      - Make sure the following interaction profiles are added:
         - `Magic Leap 2 Controller Interaction Profile`
         - `Eye Gaze Interaction Profile`
         - `Hand Interaction Profile`
      - In the feature list, make sure the `Magic Leap 2 ...` features you want are selected, plus Unity's `Hand Tracking Subsystem` feature.
      - Recommendation to change the OpenXR blend mode to `Additive` to avoid having Segmented Dimmer active by default.  Select the `Magic Leap 2 Rendering Extension` settings icon.  Set **Blend Mode** to `Additive`.
      - Note:  If Microsoft's OpenXR package is also in the project, make sure Microsoft's `Hand Tracking` feature is **not** selected.

4. Within project settings, go to the **MRTK3** category on the left.
   1. Set the **Profile** to **MRTKProfile-MagicLeap-OpenXR**, found at `/Runtime/XRProviders/OpenXR/Configuration/Default Profiles/`.

5. Configure the MRTK XR Rig to be compatible with Magic Leap 2 input (2 provided options).
   1. First option:  Use the Runtime MRTK XR Rig Configuration option in settings.
      - Within project settings, go to the **MRTK3** > **Magic Leap Settings** category on the left.
      - Make sure the `OpenXR` option is selected for **Settings for XR Provider:** at the top to view specific settings for the OpenXR XR Provider.
      - Check the `Runtime Rig Config Enabled` checkbox at the top of the **Runtime MRTK XR Rig Configuration** window.
         - Enabling this feature will allow the default MRTK XR Rig to work with ML2 input without needing to modify the scene.
   2. Second option:  In your scenes, remove and replace the default MRTK XR Rig with the `MRTK XR Rig - MagicLeap - OpenXR` prefab variant located in `/Runtime/XRProviders/OpenXR/Prefabs/MRTK_Variants/`.

## Magic Leap Permissions

The required Magic Leap permissions for your application must be added to the application's `AndroidManifest.xml` file.  At a minimum for MRTK3 to use hand tracking, the HAND_TRACKING permission should be added to the manifest.  This can be done in (**Edit** > **Project Settings** > **Magic Leap** > **Permissions**), if MLSDK is setup, or by adding it to the manifest file manually.  Examples:

      <uses-permission android:name="com.magicleap.permission.HAND_TRACKING" />
      <uses-permission android:name="com.magicleap.permission.EYE_TRACKING" />

This package offers Runtime Permission Configuration in project settings to auto request and/or start certain permission easily without needing to add prefabs or code to your scenes to do so.  Permission can be setup to be auto requested/started in (**Edit** > **Project Settings** > **MRTK3** > **Magic Leap Settings** > **Runtime Permissions Configuration**).

For best eye tracking results, after having setup the EYE_TRACKING permission for your application, be sure to run the Custom Fit application and go through eye calibration.

Select any other permissions as needed for your application.

## FAQ

Why do I need to use the Magic Leap rig variant or configure the rig at runtime?
Replacing or configuring the rig ensures optimal bindings between Magic Leap inputs and the standard MRTK input action targets.
