using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using System;
using System.IO;
using ToastPlugin;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARFoundation;

// https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/cpu-camera-image.html
// https://github.com/Unity-Technologies/arfoundation-samples/blob/latest-preview/Assets/Scenes/CpuImages.unity
public class PhotoXRCameraImage : MonoBehaviour
{
    public ARCameraManager arCameraManager;

    protected bool cameraConfInited = false;

    void OnEnable() {
        //ToastHelper.ShowToast("On Enable");
        StartCoroutine(InitCameraConf());
    }

    protected IEnumerator InitCameraConf() {
        //https://forum.unity.com/threads/ar-foundation-camera-resolution.866743/
        yield return new WaitForSeconds(2); //Dirty way : Wait for 2s to let time to camera to be enabled 
        if (!cameraConfInited) {
            NativeArray<XRCameraConfiguration> confs = arCameraManager.GetConfigurations(Allocator.Temp);

            if(confs.Length == 0) {
                Debug.LogError("No Camera config found - Are you using an Android device?");
            } else {
                XRCameraConfiguration bestConf = confs[0];
                int bestPixels = bestConf.width * bestConf.height;

                foreach (XRCameraConfiguration conf in confs) { //1 loop useless
                    int curPixels = conf.width * conf.height;
                    if (curPixels > bestPixels) {
                        bestPixels = curPixels;
                        bestConf = conf;
                    }
                    Debug.Log("Conf: " + conf);
                }

                arCameraManager.subsystem.currentConfiguration = bestConf;
                cameraConfInited = true;
                Debug.Log("Init Best conf camera");
            }
        }
    }

    /**
     * Get image and save it
     */
    public unsafe string GetImage(string dirname) { //works

        XRCameraImage image;
        if (!arCameraManager.TryGetLatestImage(out image)) {
            ToastHelper.ShowToast("Error getting lastest image");
            return null;
        }

        XRCameraIntrinsics intrinsics;
        if(!arCameraManager.TryGetIntrinsics(out intrinsics)) {
            ToastHelper.ShowToast("Error getting intrinsics");
        }

        float hfov = focalLenghToHFov(intrinsics);

        Debug.Log("Take picture " + image.width + "x" + image.height + " hfov: " + hfov);

        var conversionParams = new XRCameraImageConversionParams {
            // Get the entire image
            inputRect = new RectInt(0, 0, image.width, image.height),

            outputDimensions = new Vector2Int(image.width, image.height),

            // Choose RGBA format
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image)
            transformation = CameraImageTransformation.MirrorY,
        };

        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr<byte>()), buffer.Length);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so we can dispose of the CameraImage. We must do this or it will leak resources.
        image.Dispose();

        // At this point, we could process the image, pass it to a computer vision algorithm, etc.
        // In this example, we'll just apply it to a texture to visualize it.

        // We've got the data; let's put it into a texture so we can visualize it.
        Texture2D m_Texture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        m_Texture.LoadRawTextureData(buffer);
        m_Texture.Apply();

        if (!Directory.Exists(Application.persistentDataPath + "/" + dirname))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + dirname); //duplicated code
        string fn = System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + "_fov_"+ hfov+ "_photo.jpg";
        File.WriteAllBytes(Application.persistentDataPath + "/" + dirname + "/" + fn, m_Texture.EncodeToJPG());

        // Done with our temporary data
        buffer.Dispose();

        return fn;
    }

    /**
     * Get the the Horizontal FOV or width FOV or x FOV. Normally it must be the largest side.
     * That value changes everytime a new session is started (right?)
     */
    public static float focalLenghToHFov(XRCameraIntrinsics intrinsics) {
        //Huawei P20 Pro: focal(1104.3,1104.3) principalPoint(717.1,539.1) resolution(1440,1080)
        //dia=Math.sqrt(Math.pow(1080,2)+Math.pow(1440,2))=1800px
        //hfov=Math.atan2(717.1, 1104.3)*180/Math.PI*2=66 | vfov=Math.atan2(539.1, 1104.3)*180/Math.PI*2=52
        //diafov=Math.atan2(1800/2, 1104.3)*180/Math.PI*2=78
        //Focal: (1105.6, 1107.9) PrincipalPoint: (720.6, 543.5) Resolution:(1440, 1080) HFov (width _ x): 66.19562 VFov (heigh _ y): 52.26654 Diagonal: 1800 Diagonal Fov : 78.29585

        //Honor View 20: focal(1076.2,1076.2) principalPoint(720.1,540.2) resolution(1440,1080)
        //dia=Math.sqrt(Math.pow(1080,2)+Math.pow(1440,2))=1800px
        //hfov=Math.atan2(720.1, 1076.2)*180/Math.PI*2=67.6 | vfov=Math.atan2(540.2, 1076.2)*180/Math.PI*2=53.3
        //diafov=Math.atan2(1800/2, 1076.2)*180/Math.PI*2=79.8
        //Focal: (1091.5, 1090.9) PrincipalPoint: (718.6, 550.1) Resolution:(1440, 1080) HFov (width _ x): 66.71733 VFov (heigh _ y): 53.5204 Diagonal: 1800 Diagonal Fov : 79.0171

        float diagonal = Mathf.Sqrt(Mathf.Pow(intrinsics.resolution.x, 2) + Mathf.Pow(intrinsics.resolution.y, 2));

        Debug.Log("Focal: " + intrinsics.focalLength +
        " PrincipalPoint: " + intrinsics.principalPoint +
        " Resolution:" + intrinsics.resolution +
        " HFov (width _ x): " + Mathf.Atan2(intrinsics.principalPoint.x, intrinsics.focalLength.x) *180/Mathf.PI * 2 +
        " VFov (heigh _ y): " + Mathf.Atan2(intrinsics.principalPoint.y, intrinsics.focalLength.y) *180/Mathf.PI * 2 +
        " Diagonal: "+  diagonal +
        " Diagonal Fov : " + Mathf.Atan2(diagonal/2, intrinsics.focalLength.x) *180/Mathf.PI * 2);

        // Should get larger side?  intrinsics.resolution.x/2 or intrinsics.principalPoint.x ?
        return (Mathf.Atan2(intrinsics.principalPoint.x, intrinsics.focalLength.x) *180/Mathf.PI) * 2;
    }
}
