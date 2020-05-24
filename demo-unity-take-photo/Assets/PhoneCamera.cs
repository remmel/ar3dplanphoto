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

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0) {
            Debug.LogError("no camera detected");
            cameraAvailable = false;
            return;
        }

        for(int i=0; i<devices.Length; i++) {
            if(!devices[i].isFrontFacing) {
                webCamTexture = new WebCamTexture(devices[i].name, Screen.height, Screen.width);
            }
        }

        if(webCamTexture == null) {
            Debug.LogError("no back camera found");
            return;
        }

        webCamTexture.Play();
        background.texture = webCamTexture;
        cameraAvailable = true;

        Button btn = takePictureBtn.GetComponent<Button>();
        btn.onClick.AddListener(TakePicture);

        Debug.Log("Initied");
        //adb logcat -s Unity PackageManager dalvikvm DEBUG //adb logcat -v time -s Unity
    }

    void Update()
    {
        if (!cameraAvailable) return;

        float ratio = (float)webCamTexture.width / (float)webCamTexture.height;
        fit.aspectRatio = ratio;

        //float scaleY = webCamTexture.videoVerticallyMirrored ? -1f : 1f;
        //background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        //int orient = -webCamTexture.videoRotationAngle;
        //background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    void TakePicture() {
        Texture2D snap = new Texture2D(webCamTexture.width, webCamTexture.height);
        snap.SetPixels(webCamTexture.GetPixels());
        snap.Apply();
        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string path = Application.persistentDataPath + "/Screenshot_" + timeStamp +".jpg";
        System.IO.File.WriteAllBytes(path, snap.EncodeToJPG());
        Debug.Log("Screenshot saved in "+ path);
        ToastHelper.ShowToast("Screenshot saved in " + path);
    }
}
