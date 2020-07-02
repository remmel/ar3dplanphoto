using System;
using System.Collections;
using System.Collections.Generic;
using ToastPlugin;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCamera : MonoBehaviour
{

    // https://github.com/sseasycode/SSTools
    private bool cameraAvailable;
    private WebCamTexture webCamTexture;

    public RawImage background;
    public AspectRatioFitter fit;

    public Button takePictureBtn;
    public Dropdown dropdown;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0) {
            Debug.LogError("no camera detected");
            cameraAvailable = false;
            return;
        }

        for(int i=0; i<devices.Length; i++) {
            WebCamDevice device = devices[i];

            String resolutionStr = "";
            Array.ForEach(device.availableResolutions, r => resolutionStr+= " "+r.width+"x"+r.height);

            dropdown.options.Add(new Dropdown.OptionData() { text = device.name + " f:" + (device.isFrontFacing ? 1 : 0) + resolutionStr + " k:"+ device.kind + " dcm:" +device.depthCameraName});
        }

        ChangeCamera(4);

        Button btn = takePictureBtn.GetComponent<Button>();
        btn.onClick.AddListener(TakePicture);

        dropdown.onValueChanged.AddListener(ChangeCamera);

        Debug.Log("Initied");
        //adb logcat -s Unity PackageManager dalvikvm DEBUG //adb logcat -v time -s Unity
    }

    void ChangeCamera(int camera) {
        if(webCamTexture != null)
            webCamTexture.Stop();
        WebCamDevice device = WebCamTexture.devices[camera];
        Resolution res = device.availableResolutions[0];
        webCamTexture = new WebCamTexture(device.name, res.width, res.height);
        webCamTexture.Play();
        background.texture = webCamTexture;
    }

    void Update()
    {
        //if (!cameraAvailable) return;

        //float ratio = (float)webCamTexture.width / (float)webCamTexture.height;
        //fit.aspectRatio = ratio;
    }

    void TakePicture() {
        Texture2D tex = new Texture2D(webCamTexture.width, webCamTexture.height);
        tex.SetPixels(webCamTexture.GetPixels());
        tex.Apply();
        string timeStamp = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string path = Application.persistentDataPath + "/Photo_" + timeStamp +".jpg";
        System.IO.File.WriteAllBytes(path, tex.EncodeToJPG());
        Debug.Log("Photo saved in "+ path);
        ToastHelper.ShowToast("Photo saved in " + path);
    }
}
