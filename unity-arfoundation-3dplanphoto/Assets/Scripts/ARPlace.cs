using ToastPlugin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ARPlace : MonoBehaviour
{
    public GameObject arCamera;

    public PlacementIndicator placementIndicator;

    public Text videoBtnText;
    protected bool videoStopped = true;
    public Text poseText;

    private Quaternion rotOffest = Quaternion.Euler(0, 0, 90);

    public void Start() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void Update() {
        UpdateClickToRemove();

        poseText.text = "Rot:" + (arCamera.transform.rotation * rotOffest).eulerAngles + "\n" + 
            "Pos:" + arCamera.transform.position;

    }

    private void UpdateClickToRemove() {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) { //TODO handle when clicking on button (should not remove any objects)
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f)) {
                if (hit.transform) {
                    GameObject go = hit.transform.gameObject;
                    GetComponent<DrawRoom>().RemoveGO(go);
                }
            }
        }
    }

    public void BtnWall() {
        GetComponent<DrawRoom>().AddWall(placementIndicator.transform);
    }

    public void BtnPhoto() {
        string fn = GetComponent<PhotoXRCameraImage>().GetImage();
        //string fn = GetComponent<PhotoSnapshot>().GetImage();

        //Transform t = arCamera.transform + Quaternion.Euler(0, 0, 180);
        Debug.Log("Rot: " + (arCamera.transform.rotation * rotOffest).eulerAngles);

        GetComponent<DrawRoom>().AddPhoto(arCamera.transform.position, arCamera.transform.rotation * rotOffest, fn);
        Debug.Log("Image saved in " + fn);
        //ToastHelper.ShowToast("Image saved in " + fn);
    }

    public void BtnVideo() {

        if(videoStopped) {

            InvokeRepeating("BtnPhoto", 0, 0.5f); //every 0.5sec
            videoBtnText.text = "Stop";
        } else {
            CancelInvoke("BtnPhoto");
            videoBtnText.text = "Start";
        }

        videoStopped = !videoStopped;
    }
}
