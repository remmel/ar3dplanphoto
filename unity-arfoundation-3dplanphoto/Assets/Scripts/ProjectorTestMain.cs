using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorTestMain : MonoBehaviour
{
    public GameObject projectorPrefab;
    public List<GameObject> toProjectList;

    protected GameObject projector;

    void Start()
    {
        Debug.Log(this.transform.position);
        projector = Instantiate(projectorPrefab, Vector3.zero, Quaternion.identity);

        DrawProjector dp = projector.GetComponent<DrawProjector>();
        dp.fn = "cube.jpg";
    }


    [ContextMenu("GenerateObj")]
    void GenerateObj() {
        Camera camera = projector.GetComponent<Camera>();
        ObjExportUtils.Export(new List<Camera> { camera }, this.toProjectList);
    }
}
