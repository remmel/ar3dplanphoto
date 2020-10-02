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
                    Debug.Log("Conf: " + conf.width + "x" + conf.height + conf);
                }

                arCameraManager.subsystem.currentConfiguration = bestConf;
                cameraConfInited = true;
                Debug.Log("Init Best conf camera");
            }
        }
    }

    public unsafe string GetImage() { //works

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

        string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_fov_"+ hfov+ "_photo.jpg";
        File.WriteAllBytes(Application.persistentDataPath + "/" + fn, m_Texture.EncodeToJPG());

        // Done with our temporary data
        buffer.Dispose();

        return fn;
    }

    public static float focalLenghToHFov(XRCameraIntrinsics intrinsics) {
        //??hfov = Math.atan2(717.1, 1104.3)*180/Math.PI*2 / vfov = Math.atan2(539.1, 1104.3)*180/Math.PI*2

        Debug.Log("Focal: " + intrinsics.focalLength + " PrincipalPoint: " + intrinsics.principalPoint + " Resolution:" + intrinsics.resolution);

        // Should get larger side?
        return Mathf.Atan2(intrinsics.focalLength.x, intrinsics.resolution.x) * Mathf.Rad2Deg * 2;
    }
}
