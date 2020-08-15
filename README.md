# ar-3dplanphoto
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


Todo:
- ARScene :
    - Handle multiple rooms
    - Generate floormap
    - Generate UV Texture : get fov
	- Take photo without markers
- ARCube : from smartphone, place a cube and project texture
- Java : convert Unity to Android Java project

Problem:
- Unity/ARScene : drift problem, locations of markers changes


https://www.youtube.com/watch?v=FGL6SffDeVU

https://docs.unity3d.com/ScriptReference/Camera.ScreenPointToRay.html

https://forum.unity.com/threads/how-to-calculate-the-correct-inverse-view-projection.384656/


https://stackoverflow.com/questions/49016071/how-to-take-save-picture-screenshot-using-unity-arcore-sdk


Solutions to take picture:
- Screenshot (2240x1080 H P20Pro ; image seems to be scale up + need to remove UI from screenshot)
- ARFoundation : TakeScreenshot arCameraManager.TryGetLatestImage (1440x1080 H P20Pro image seems to be compressed too much; quality 75 according to ImageMagick)

//TODO load/save focal

1292