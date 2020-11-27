using UnityEngine;
using GoogleARCore;
using UnityEngine.EventSystems;
using System.IO;
using GoogleARCore.Examples.ComputerVision;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARPlace : MonoBehaviour {

    public Camera FirstPersonCamera; // The first-person camera being used to render the passthrough camera image (i.e. A Rbackground).
    public GameObject GameObjectPointPrefab ;// A prefab to place when a raycast from a user touch hits a feature point
    private const float k_PrefabRotation = 180.0f;// The rotation in degrees need to apply to prefab when it is placed.
    private bool m_IsQuitting = false;// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.

    private int m_HighestResolutionConfigIndex = 0;
    private bool m_Resolutioninitialized = false;
    public ARCoreSession ARSessionManager;

    private ARCoreSession.OnChooseCameraConfigurationDelegate m_OnChoseCameraConfiguration = null;

    private TextureReader TextureReaderComponent;
    private Texture2D m_TextureToRender = null;
    private byte[] m_EdgeImage = null;

    public void Awake() {
        // Enable ARCore to target 60fps camera capture frame rate on supported devices.
        // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
        Application.targetFrameRate = 60;

        // Register the callback to set camera config before arcore session is enabled.
        //m_OnChoseCameraConfiguration = _ChooseCameraConfiguration;
        //ARSessionManager.RegisterChooseCameraConfigurationCallback(m_OnChoseCameraConfiguration);
    }

    public void BtnWall() {
        //spawnAndPhoto.AddWall(placementIndicator.transform);
    }

    public void BtnPhoto() {
        //spawnAndPhoto.AddPhoto(arCamera.transform);
        takePhotoGrayscale640x480();
       // takePhoto3();
    }

    private int _ChooseCameraConfiguration(List<CameraConfig> supportedConfigurations) {

        if (!m_Resolutioninitialized) {

            string debug = "CameraConfig:\n";

            m_HighestResolutionConfigIndex = 0;
            CameraConfig maximalConfig = supportedConfigurations[0];
            for (int index = 1; index < supportedConfigurations.Count; index++) {
                CameraConfig config = supportedConfigurations[index];
            
                debug += "config: size:" + config.ImageSize + " maxFPS:" + config.MaxFPS + " minFPS:" + config.MinFPS + " textureSize:" + config.TextureSize + " depthSensorUsage:" + config.DepthSensorUsage + "\n";

                if ((config.ImageSize.x > maximalConfig.ImageSize.x &&
                     config.ImageSize.y > maximalConfig.ImageSize.y) ||
                    (config.ImageSize.x == maximalConfig.ImageSize.x &&
                     config.ImageSize.y == maximalConfig.ImageSize.y &&
                     config.MaxFPS > maximalConfig.MaxFPS)) {
                    m_HighestResolutionConfigIndex = index;
                    maximalConfig = config;
                }
            }
            m_Resolutioninitialized = true;

            {
                CameraConfig config = supportedConfigurations[m_HighestResolutionConfigIndex];

                string info = "Config #" + m_HighestResolutionConfigIndex + "config: size:" + config.ImageSize + " maxFPS:" + config.MaxFPS + " minFPS:" + config.MinFPS + " textureSize:" + config.TextureSize + " depthSensorUsage:" + config.DepthSensorUsage;
                Utils.Toast(info);
                Utils.Log(info);
            }            
        }

        return m_HighestResolutionConfigIndex;
    }

    private string takeSnapshot() {

        TextureReaderApi api = new TextureReaderApi();
        //api.AcquireFrame(0, )
        
        return "d";
    }

    private string takeSnapshot2() {
        string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_screenshot.jpg";
        string pathSnap = Application.persistentDataPath + "/" + fn;

        //ScreenCapture.CaptureScreenshot(fn);


        Utils.Toast("Photo " + pathSnap);
        CameraImageBytes image = Frame.CameraImage.AcquireCameraImageBytes(); //using

        int width = image.Width;
        int height = image.Height;

        byte[] m_EdgeImage = new byte[image.Width * image.Height * 4];
        //System.Runtime.InteropServices.Marshal.Copy(image.Y, m_EdgeImage, 0, image.Width * image.Height);
        Texture2D tx = new Texture2D(width, height, TextureFormat.R8, false, false);

        byte[] rgbaBuffer = new byte[width * height * 4];
        System.Runtime.InteropServices.Marshal.Copy(image.Y, rgbaBuffer, 0, width * height * 4);

        // Copy the red channel to the grayscale image buffer, since the shader only reads the red channel.
        for (int i = 0; i < width * height; i++) {
            m_EdgeImage[i] = rgbaBuffer[i * 4 + 0];
        }

        tx.LoadRawTextureData(m_EdgeImage);
        tx.Apply();

        var encodedJpg = tx.EncodeToJPG();
        File.WriteAllBytes(pathSnap, encodedJpg);

        return fn;
    }

   /* public Texture2D CameraToTexture() {
        // Create the object for the result - this has to be done before the 
        // using {} clause.
        Texture2D result;

        // Use using to make sure that C# disposes of the CameraImageBytes afterwards
        using (CameraImageBytes camBytes = Frame.CameraImage.AcquireCameraImageBytes()) {

            // If acquiring failed, return null
            if (!camBytes.IsAvailable) {
                Debug.LogWarning("camBytes not available");
                return null;
            }

            // To save a YUV_420_888 image, you need 1.5*pixelCount bytes.
            // I will explain later, why.

            byte[] YUVimage = new byte[(int)(camBytes.Width * camBytes.Height * 1.5f)];

            // As CameraImageBytes keep the Y, U and V data in three separate
            // arrays, we need to put them in a single array. This is done using
            // native pointers, which are considered unsafe in C#.
            unsafe {
                for (int i = 0; i < camBytes.Width * camBytes.Height; i++) {
                    YUVimage[i] = *((byte*)camBytes.Y.ToPointer() + (i * sizeof(byte)));
                }

                for (int i = 0; i < camBytes.Width * camBytes.Height / 4; i++) {
                    YUVimage[(camBytes.Width * camBytes.Height) + 2 * i] = *((byte*)camBytes.U.ToPointer() + (i * camBytes.UVPixelStride * sizeof(byte)));
                    YUVimage[(camBytes.Width * camBytes.Height) + 2 * i + 1] = *((byte*)camBytes.V.ToPointer() + (i * camBytes.UVPixelStride * sizeof(byte)));
                }
            }

            // Create the output byte array. RGB is three channels, therefore
            // we need 3 times the pixel count
            byte[] RGBimage = new byte[camBytes.Width * camBytes.Height * 3];

            // GCHandles help us "pin" the arrays in the memory, so that we can
            // pass them to the C++ code.
            GCHandle YUVhandle = GCHandle.Alloc(YUVimage, GCHandleType.Pinned);
            GCHandle RGBhandle = GCHandle.Alloc(RGBimage, GCHandleType.Pinned);

            // Call the C++ function that we created.
            int k = ConvertYUV2RGBA(YUVhandle.AddrOfPinnedObject(), RGBhandle.AddrOfPinnedObject(), camBytes.Width, camBytes.Height);

            // If OpenCV conversion failed, return null
            if (k != 0) {
                Debug.LogWarning("Color conversion - k != 0");
                return null;
            }

            // Create a new texture object
            result = new Texture2D(camBytes.Width, camBytes.Height, TextureFormat.RGB24, false);

            // Load the RGB array to the texture, send it to GPU
            result.LoadRawTextureData(RGBimage);
            result.Apply();

            // Save the texture as an PNG file. End the using {} clause to
            // dispose of the CameraImageBytes.
            File.WriteAllBytes(Application.persistentDataPath + "/tex.png", result.EncodeToPNG());
        }

        // Return the texture.
        return result;
    }*/

    void takePhotoGrayscale640x480() {

        Utils.Log(_CameraIntrinsicsToString(Frame.CameraImage.ImageIntrinsics, "ff"));

        

        using (CameraImageBytes image = Frame.CameraImage.AcquireCameraImageBytes()) {
            if (!image.IsAvailable) {
                Utils.Toast("not available");
                return;
            }
            OnImageAvailableBW(image.Width, image.Height, image.YRowStride, image.Y, image.Width * image.Height); //works but sometime camera not availble and only in B&W
            //OnImageAvailableC(image.Width, image.Height, image, image.Width * image.Height);
        }


    void OnImageAvailableBW(int width, int height, int rowStride, IntPtr pixelBuffer, int bufferSize) {
            byte[] bufferY = new byte[bufferSize];
            
            System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, bufferY, 0, bufferSize);

            Texture2D m_TextureRender = new Texture2D(width, height, TextureFormat.RGBA32, false, false);

            Color c = new Color();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float Y = bufferY[y * width + x];
   
                    c.r = Y;
                    c.g = Y;
                    c.b = Y;

                    c.r /= 255.0f;
                    c.g /= 255.0f;
                    c.b /= 255.0f;

                    MinMaxColor(ref c);

                    c.a = 1.0f;
                    m_TextureRender.SetPixel(width - 1 - x, y, c);
                }
            }

            string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_photo_bw.jpg";
            string path = Application.persistentDataPath + "/" + fn;
            Utils.Toast("Image " + width + "x" + height + " " + fn);
            File.WriteAllBytes(path, m_TextureRender.EncodeToJPG());
        }
    }


    void OnImageAvailableC(int width, int height, CameraImageBytes image, int bufferSize) {
        byte[] bufferY = new byte[bufferSize];
        byte[] bufferU = new byte[bufferSize];
        byte[] bufferV = new byte[bufferSize];

        Marshal.Copy(image.Y, bufferY, 0, bufferSize);
        Marshal.Copy(image.U, bufferU, 0, bufferSize);
        Marshal.Copy(image.V, bufferV, 0, bufferSize);

        int bufferSizeYUV = width * height * 3 / 2;
        byte[] bufferYUV = new byte[bufferSizeYUV];
        Marshal.Copy(image.Y, bufferYUV, 0, bufferSizeYUV);

        Texture2D m_TextureRender = new Texture2D(width, height, TextureFormat.RGBA32, false, false);

        Color c = new Color();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float Y = bufferY[y * width + x];
                float U = bufferU[y * width + x];
                float V = bufferV[y * width + x];

                /*
                c.r = Y; // + 1.4075f * (V - 128);
                c.g = 0f; //Y - 0.3455f * (U - 128) - (0.7169f * (V - 128));
                c.b = 0f; // Y + 1.7790f * (U - 128);*/

                /*float Yvalue = bufferYUV[y * width + x];
                float Uvalue = bufferYUV[(y / 2) * (width / 2) + x / 2 + (width * height)];
                float Vvalue = bufferYUV[(y / 2) * (width / 2) + x / 2 + (width * height) + (width * height) / 4];*/
                c.r = Y + (float)(1.370705 * (V - 128.0f));
                c.g = Y - (float)(0.698001 * (V - 128.0f)) - (float)(0.337633 * (U - 128.0f));
                c.b = Y + (float)(1.732446 * (U - 128.0f));


                c.r /= 255.0f;
                c.g /= 255.0f;
                c.b /= 255.0f;

                MinMaxColor(ref c);

                c.a = 1.0f;
                m_TextureRender.SetPixel(width - 1 - x, y, c);
            }
        }

        string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_photo_bw.jpg";
        string path = Application.persistentDataPath + "/" + fn;
        Utils.Toast("Image " + width + "x" + height + " " + fn);
        File.WriteAllBytes(path, m_TextureRender.EncodeToJPG());
    }

    private void MinMaxColor(ref Color c) {
        if (c.r < 0.0f) c.r = 0.0f;
        if (c.g < 0.0f) c.g = 0.0f;
        if (c.b < 0.0f) c.b = 0.0f;

        if (c.r > 1.0f) c.r = 1.0f;
        if (c.g > 1.0f) c.g = 1.0f;
        if (c.b > 1.0f) c.b = 1.0f;
    }

    private void OnImageAvailable(int width, int height, IntPtr pixelBuffer, int bufferSize) {
        if (m_TextureToRender == null || m_EdgeImage == null || m_TextureToRender.width != width || m_TextureToRender.height != height) {

            m_TextureToRender = new Texture2D(width, height, TextureFormat.R8, false, false);
            m_EdgeImage = new byte[width * height * 4];
        }

        System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, m_EdgeImage, 0, bufferSize * 4);

        // Update the rendering texture with the sampled image.
        m_TextureToRender.LoadRawTextureData(m_EdgeImage);
        m_TextureToRender.Apply();
        //EdgeDetectionBackgroundImage.material.SetTexture("_ImageTex", m_TextureToRender);
        File.WriteAllBytes(Application.persistentDataPath + "/testImage.jpg", m_TextureToRender.EncodeToJPG());

    }
   /* void OnDisable() {
        TextureReaderComponent.enabled = false;
        TextureReaderComponent.OnImageAvailableCallback -= OnImageAvailable;
    }*/


    public void Update() {
        _UpdateApplicationLifecycle();

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) {
            return;
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) {
            return;
        }

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
            // Use hit pose and camera pose to check if hittest is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0) {
                Debug.Log("Hit at back of the current DetectedPlane");
            } else {
                // Instantiate prefab at the hit pose.
                var gameObject = Instantiate(GameObjectPointPrefab, hit.Pose.position, hit.Pose.rotation);

                // Compensate for the hitPose rotation facing away from the raycast (i.e.
                // camera).
                gameObject.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);

                // Create an anchor to allow ARCore to track the hitpoint as understanding of
                // the physical world evolves.
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                // Make game object a child of the anchor.
                gameObject.transform.parent = anchor.transform;
            }
        }
    }

    private void _UpdateApplicationLifecycle() {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking) {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        } else {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting) {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
            Utils.Toast("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        } else if (Session.Status.IsError()) {
            Utils.Toast("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    private void _DoQuit() {
        Application.Quit();
    }

    private string _CameraIntrinsicsToString(CameraIntrinsics intrinsics, string intrinsicsType) {
        float fovX = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
            intrinsics.ImageDimensions.x, 2 * intrinsics.FocalLength.x);
        float fovY = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(
            intrinsics.ImageDimensions.y, 2 * intrinsics.FocalLength.y);

/*        string frameRateTime = m_RenderingFrameRate < 1 ? "Calculating..." :
            string.Format("{0}ms ({1}fps)", m_RenderingFrameTime.ToString("0.0"),
                m_RenderingFrameRate.ToString("0.0"));*/

        string frameRateTime = "00";

        string message = string.Format(
            "Unrotated Camera {4} Intrinsics:{0}  Focal Length: {1}{0}  " +
            "Principal Point: {2}{0}  Image Dimensions: {3}{0}  " +
            "Unrotated Field of View: ({5}°, {6}°){0}" +
            "Render Frame Time: {7}",
            Environment.NewLine, intrinsics.FocalLength.ToString(),
            intrinsics.PrincipalPoint.ToString(), intrinsics.ImageDimensions.ToString(),
            intrinsicsType, fovX, fovY, frameRateTime);
        return message;
    }
}
