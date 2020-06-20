using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Point3GameObjects
{
    public Point3GameObjects(UnityEngine.Vector3 point, GameObject a, GameObject b, GameObject c) {
        this.point = point;
        gos = new GameObject[] { a, b, c };
    }

    public GameObject[] gos;
    public UnityEngine.Vector3 point;
}
