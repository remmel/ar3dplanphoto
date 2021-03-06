## unity-arfoundation-3dplanphoto

### Objective
Objective of that app is to create a 3d plan of a house with texture, placing himself the walls/ceil/floor. Thus rooms will be simple geometric shape.  

### Description
[Android apk](unity-arfoundation-3dplanphoto/build.apk)

Even if the app is still in development, you can currently put 6 markers, one per face of the room (walls, floor, ceil), take pictures and it generates the rooms (6 quads) with the texture in .obj format. (Recording on Android - Obj creation on Windows).

Take picture and place a representation of the picture/projector in the 3D world.  
Can remove marker when clicking into it.  
The pose of photos and walls/floor/ceil will be saved in `‎‏‎‎‎‎‎‏‎‏‏‏‎‎‎‎‎‎‏‎‎‏‎‎‎‎‏‏‏‏‏‏‎‏‏‎‎‎‏‏‎‎‎‏‏‏‎‏‎‏‏‎‏‎‏‎‏‎‎‏‏‏‏‏‎‎‏‎‎‏‎‎‎‏‎‎‎‏‏‎‎‎‏‏‎‎‎‏‎‏‏‏‎‏‏‎Internal storage‎‏‎‎‏‎/Android/data/com.remmel.unityarf3dplanphoto/files/yyyy-MM-dd_HHmmss/3dplanphoto.json` aside the photos.  
The app could also be used to record pictures with it pose (position + rotation).  
It uses Unity AR Foundation framework.  

Unity/Scenes :
- ARScene : main application
- PCScene : debug application, no need of mobile
- ProjectorTest : in order to test the projection shader (alternative in [threejs](https://codesandbox.io/s/project-camera-gby2i)). Using [Unity Projector and Standard Asset shader](https://docs.unity3d.com/Manual/class-Projector.html)

### TODO
- Handle multiple rooms
- Generate floormap
- Check vfov calculation in PhotoXRCameraImage.focalLenghToHFov (XRCameraIntrinsics)
- Clicking a the button, can remove marker
- Make Youtube + screenshot + upload .obj
- Save video (take photo every 0.5s, but after 150 photos, the app froze)
- Directly save obj on Android

### Problem
- Unity/ARScene : drift problem, locations of markers changes, precision is not so great.

### Others
Unity AR Foundation is used, alternatives could be :
- ARCore for Unity
- Unity Vuforia
- ARCore (java/kotlin) in Android Studio 
- ARCore NDK (C++) in Android Studio


https://www.youtube.com/watch?v=FGL6SffDeVU Modify UV from code
https://docs.unity3d.com/ScriptReference/Camera.ScreenPointToRay.html
https://forum.unity.com/threads/how-to-calculate-the-correct-inverse-view-projection.384656/
https://stackoverflow.com/questions/49016071/how-to-take-save-picture-screenshot-using-unity-arcore-sdk


Solutions to take picture:
- Screenshot (2240x1080 H P20Pro ; image seems to be scale up + need to remove UI from screenshot)
- ARFoundation : TakeScreenshot arCameraManager.TryGetLatestImage (1440x1080 H P20Pro image seems to be compressed too much; quality 75 according to ImageMagick) (used in `PhotoXRCameraImage`)


Save video : https://www.agora.io/en/blog/video-chat-with-unity3d-ar-foundation-pt2-screensharing/

https://tutorialsforar.com/accessing-and-saving-point-cloud-data-using-unity-and-ar-foundation/