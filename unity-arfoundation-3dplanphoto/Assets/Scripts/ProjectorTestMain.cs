﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorTestMain : MonoBehaviour
{
    public GameObject projectorPrefab;

    void Start()
    {
        Debug.Log(this.transform.position);
        GameObject o = Instantiate(projectorPrefab, Vector3.zero, Quaternion.identity);
        o.GetComponent<DrawProjector>().fn = "cube.jpg";
    }
}
