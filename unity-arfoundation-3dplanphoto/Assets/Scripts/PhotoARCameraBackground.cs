using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PhotoARCameraBackground : MonoBehaviour
{
    public ARCameraBackground aRCameraBackground;
    private RenderTexture renderTexture = null;
    private Texture2D m_LastCameraTexture = null;

    // Does not work : NullReferenceException, need to check why
    // Dumb copy-paste from https://forum.unity.com/threads/how-to-get-camera-texture-in-arfoundation.543827/
    public string GetImage() {
        // Copy the camera background to a RenderTexture
        Graphics.Blit(null, renderTexture, aRCameraBackground.material);

        // Copy the RenderTexture from GPU to CPU
        var activeRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        if (m_LastCameraTexture == null)
            m_LastCameraTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, true);
        m_LastCameraTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        m_LastCameraTexture.Apply();
        RenderTexture.active = activeRenderTexture;

        // Write to file
        var bytes = m_LastCameraTexture.EncodeToPNG();

        string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_texture.png";
        File.WriteAllBytes(Application.persistentDataPath + "/" + fn, bytes);

        return fn;
    }
}
