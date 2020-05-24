using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnerS : MonoBehaviour
{
    public GameObject objectToSpawn;
    private PlacementIndicatorS placementIndicator;


    void Start()
    {
        placementIndicator = FindObjectOfType<PlacementIndicatorS>();
    }

    // Update is called once per frame
    /*void Update()
    { 
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) {
            GameObject obj = Instantiate(objectToSpawn, placementIndicator.transform.position,
                placementIndicator.transform.rotation);
        }
    }*/

    public void Place() {
            GameObject obj = Instantiate(objectToSpawn, placementIndicator.transform.position,
                placementIndicator.transform.rotation);
    }
}
