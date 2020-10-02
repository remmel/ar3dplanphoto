using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadSceneARScene()
    {
        SceneManager.LoadScene("ARScene");
    }

    public void LoadSceneGenrateScene()
    {
        // only available on PC : PCScene > GameController > Draw Room (script) > right clic > GenerateObj
    }
}
