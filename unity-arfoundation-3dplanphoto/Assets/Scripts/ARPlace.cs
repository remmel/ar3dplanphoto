﻿using ToastPlugin;
using UnityEngine;
using UnityEngine.EventSystems;


public class ARPlace : MonoBehaviour
{
    public GameObject arCamera;

    public PlacementIndicator placementIndicator;

    public void Start() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
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

        GetComponent<DrawRoom>().AddPhoto(arCamera.transform.position, arCamera.transform.rotation * Quaternion.Euler(0, 0, 90), fn);

        ToastHelper.ShowToast("Image saved in " + fn);
    }
}
