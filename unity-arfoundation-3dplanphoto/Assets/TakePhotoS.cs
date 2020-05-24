using System.Collections;
using System.Collections.Generic;
using ToastPlugin;
using UnityEngine;

public class TakePhotoS : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Take() {
        //yield return new WaitForEndOfFrame();

        Texture2D snap = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        snap.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        snap.Apply();
        //System.IO.File.WriteAllBytes(filename, texture.EncodeToPNG());

        string timeStamp = System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
        string path = Application.persistentDataPath + "/Screenshot_" + timeStamp + ".jpg";
        System.IO.File.WriteAllBytes(path, snap.EncodeToJPG());
        Debug.Log("Screenshot saved in " + path);
        ToastHelper.ShowToast("Screenshot saved in " + path);

    }
}
