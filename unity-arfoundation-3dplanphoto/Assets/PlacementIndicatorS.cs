using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementIndicatorS : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private GameObject visual;

    void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        visual = transform.GetChild(0).gameObject;

        visual.SetActive(false);
    }

    void Update()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        rayManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);

        if(hits.Count > 0) {
            //placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            Pose pose = hits[0].pose;
            transform.position = pose.position;
            transform.rotation = pose.rotation;

            //if (!visual.activeInHierarchy) {
                visual.SetActive(true);
            //}
        }
    }
}
