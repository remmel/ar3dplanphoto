using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlace : MonoBehaviour
{
    public GameObject arCamera;

    public PlacementIndicator placementIndicator;

    protected DrawRoom spawnAndPhoto;

    public ARCameraManager arCameraManager;

    public void Start() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        spawnAndPhoto = GetComponent<DrawRoom>();
    }

    public void Update() {
        UpdateClickToRemove();
    }

    private void UpdateClickToRemove() {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) { //TODO handle when clicking on button (should not remove any objects)
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f)) {
                if (hit.transform) {
                    GameObject go = hit.transform.gameObject;
                    spawnAndPhoto.RemoveGO(go);
                }
            }
        }
    }

    public void BtnWall() {
        spawnAndPhoto.AddWall(placementIndicator.transform);
    }

    public void BtnPhoto() {
        spawnAndPhoto.AddPhoto(arCamera.transform);
        //GetCameraImage();
    }

    /*public unsafe void GetCameraImage() {
        XRCameraImage image;
        if (!arCameraManager.TryGetLatestImage(out image))
            return;

        var conversionParams = new XRCameraImageConversionParams {
            // Get the entire image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Choose RGBA format
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image)
            transformation = CameraImageTransformation.MirrorY
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

        File.WriteAllBytes(Application.persistentDataPath + "/unsafecpuimage.jpg", m_Texture.EncodeToJPG());

        // Done with our temporary data
        buffer.Dispose();
    }*/
}
