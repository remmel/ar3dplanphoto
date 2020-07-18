using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ARPlace : MonoBehaviour
{
    public GameObject arCamera;

    public PlacementIndicator placementIndicator;

    protected SpawnAndPhoto spawnAndPhoto;

    public void Start() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        spawnAndPhoto = GetComponent<SpawnAndPhoto>();
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
    }
}
