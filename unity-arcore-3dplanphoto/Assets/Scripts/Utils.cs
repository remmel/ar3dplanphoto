using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Utils
{

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    public static void Toast(string message) {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null) {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }

    public static void Log(string message) {
        string time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/log.txt", true);
        writer.WriteLine(time + " : " + message);
        writer.Close();
    }
}
