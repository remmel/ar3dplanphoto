using ToastPlugin;
using UnityEngine;

public class PhotoSnapshot : MonoBehaviour
{

    public string GetImage1() { //not blocking, screenshot will be done later
        float hfov = calculateFov();

        string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_fov_" + hfov + "_screenshot.jpg";
        ScreenCapture.CaptureScreenshot(fn); //doesnt not block image, screenshot will be done/written in few ms later
        Math3DUtils.Log("Focal: " + hfov + " Resolution: " + Screen.width+"x"+Screen.height + "Fn: "+fn);
        return fn;
    }

    private static float calculateFov() { // ?!?
        // Create a ray along the upper edge of the view frustum, centered on X
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height, 0));

        // Find the angle between this ray and the camera forward direction
        float angle = Vector3.Angle(Camera.main.transform.forward, ray.direction);

        // Multiply by two and you have the fov!
        float fov = angle * 2.0f;
        ToastHelper.ShowToast("fov: " + fov);
        return fov;
    }

    public string GetImage() { //blocking until screenshot is done
        float hfov = calculateFov();
        Texture2D snap = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        snap.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        snap.Apply();

        string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_fov_" + hfov + "_tscreenshot.jpg";

        string pathSnap = Application.persistentDataPath + "/" + fn;
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + fn, snap.EncodeToJPG());
        Math3DUtils.Log("Focal: " + hfov + " Resolution: " + Screen.width + "x" + Screen.height + "Fn: " + fn);
        return fn;
    }
}
