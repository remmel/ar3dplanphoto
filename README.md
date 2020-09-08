Objective is to be able to create 3d plan of house, adding real texture using an Android device.
There are multiple possilities/framework to do that:
- Unity AR Foundation
- ARCore for Unity
- Unity Vuforia
- ARCore directly (java/kotlin)
- ARCore NDK (C++)

# unity-arfoundation-3dplanphoto
Try doing that using Unity Ar Fondation framework
Can currently put 6 markers, one per face of the room (walls, floor, ceil), take pictures and it generates the rooms (6 quads) with the texture in .obj format.
Take picture and place a representation of the picture/projector in the 3D world.
Can remove marker when clicking into it.

Unity/Scenes :
- ARScene : main application
- PCScene : debug application, no need of mobile
- ProjectorTest : in order to test the projection shader (alternative in [threejs](https://codesandbox.io/s/project-camera-gby2i)). Using [Unity Projector and Standard Asset shader](https://docs.unity3d.com/Manual/class-Projector.html)

# TODO
- ARScene :
    - Handle multiple rooms
    - Generate floormap
    - Generate UV Texture : get fov
- ARCube : from smartphone, place a cube and project texture
- Java : convert Unity to Android Java project

Problem:
- Unity/ARScene : drift problem, locations of markers changes


https://www.youtube.com/watch?v=FGL6SffDeVU Modify UV from code
https://docs.unity3d.com/ScriptReference/Camera.ScreenPointToRay.html
https://forum.unity.com/threads/how-to-calculate-the-correct-inverse-view-projection.384656/
https://stackoverflow.com/questions/49016071/how-to-take-save-picture-screenshot-using-unity-arcore-sdk


Solutions to take picture:
- Screenshot (2240x1080 H P20Pro ; image seems to be scale up + need to remove UI from screenshot)
- ARFoundation : TakeScreenshot arCameraManager.TryGetLatestImage (1440x1080 H P20Pro image seems to be compressed too much; quality 75 according to ImageMagick)


Save video : https://www.agora.io/en/blog/video-chat-with-unity3d-ar-foundation-pt2-screensharing/

https://tutorialsforar.com/accessing-and-saving-point-cloud-data-using-unity-and-ar-foundation/

## 6DOF

Possibilities, using AR+VR sdk: 
- AR :
-- ARCore XR plugin or
-- ARcore for unity
- VR (stereo view)
-- Cardboard Virtual Reality SDK (integreated and deprecated) or
-- [Google VR Unity](https://developers.google.com/vr/develop/unity/get-started-android) (deprecated) or `Window > Package > `
-- [Google Cardboard Unity](https://developers.google.com/cardboard/develop/unity/quickstart)

or AR + double screen 

Solutions : 
- Christoph VR [Github](https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr)
- http://arloopa.com/ : same view between left and right eye
- https://assetstore.unity.com/packages/tools/integration/6dof-for-carboard-173395 : (works on Unity 2019.4 LTS but image is too distorded, no need Cardboard plugin; 2020 ?)
- Basic and quickly stereo view [Youtube tutorial](https://www.youtube.com/watch?v=ceKvaQC6-kw)
- https://github.com/jondyne/ARcore_mobileVRHeadset/ : working with Unity 2019.4 LTS but same image from left and right eye
- https://github.com/rajandeepsingh13/6DOF-Mobile-VR-Using-GVR-and-ARCore : (working good with 2018.4 LTS and 2019.4 LTS)
- https://github.com/XRTK/XRTK-Core ?

Mixed Reality (AR + VR ; 6DoF & Double Screen):
[Google VR Unity](https://developers.google.com/vr/develop/unity/get-started-android)
errors with demo sample (HelloVR; KeyboardDemo) on Huawei P20 Pro (Unity 2019.4.8 LTS)
google-vr-unity project
https://developers.google.com/vr/develop/unity/3dof-to-6dof

[Google Cardboard Unity](https://developers.google.com/cardboard/develop/unity/quickstart)
google-cardboard-unity
Works only VR. Don't work with ARCore (??) and ARFoundation (Unity message that both cannot work)
- Had to restart the project to get the [Cardboard XR Plugin option in XR Plug-in Management](https://developers.google.com/cardboard/develop/unity/quickstart#xr_plug-in_management_settings) 
- Had to install iOS support even if not used
- Had to select ARMv7 AND AMR64



Viewer3dplan_6dof : when using Cardboard; cannot move the head


[ARCore](https://developers.google.com/ar/develop/unity/quickstart-android) + Virtual Reality SDKs=Cardboard : Detect planes but camera do not move with 3D world (Not Unity XR)
ARFoundation + Virtual Reality SDKs=Cardboard = 


Eyewear Vuforia

https://github.com/jondyne/ARcore_mobileVRHeadset/

https://github.com/ChristophGeske/ARCoreInsideOutTrackingGearVr


Install Vuforia 8 in Unity : packet manager
https://library.vuforia.com/articles/Training/getting-started-with-vuforia-in-unity.html
Can select Digital Eyewear, but black screen

Install Vuforia 9 in Unity : Store
https://docs.unity3d.com/Packages/com.ptc.vuforia.engine@8.5/manual/index.html

No more "Digital Eyewear" in configuration "moved". Cannot select Vuforia as SDK (deprecated?)

Vuforia Stereo Rendering for Digital Eyewear

Vuforia 9 + Cardboard : not stable (XR Settings > Virtual Reality Supported v > Virtual Reality SDK Cardboard)


https://library.vuforia.com/articles/Solution/Integrating-Cardboard-to-the-ARVR-Sample.html



On Unity 2018; to remove reflections (maybe some stuff useless) : 
- Window > Rendering > Light settings
-- Environment Lighting Source: Color Ambient Color: White
-- Intensity Multiplier 0