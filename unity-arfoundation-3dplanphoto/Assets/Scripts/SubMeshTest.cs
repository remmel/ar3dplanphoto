using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubMeshTest : MonoBehaviour
{
    public GameObject go;

    void Start()
    {
        Math3DUtils.MeshDivide(go);
        Math3DUtils.MeshDivide(go);
        Math3DUtils.MeshDivide(go);
        Math3DUtils.MeshDivide(go);
        Math3DUtils.MeshDivide(go);
        Math3DUtils.MeshDivide(go);
        Math3DUtils.MeshDivide(go);
        Math3DUtils.MeshDivide(go);
    }
}
