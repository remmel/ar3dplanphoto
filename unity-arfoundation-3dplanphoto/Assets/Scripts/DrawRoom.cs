using System;
using System.Collections.Generic;
using System.Linq;
using ToastPlugin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;


//TODO check if all pictures saved in json files, later load them, later display frame 

public class DrawRoom : MonoBehaviour
{
    public GameObject wallToSpawn;

    public List<GameObject> spawnedWalls = new List<GameObject>(); //current room spawned objs
    public List<GameObject> spawnedPhotos = new List<GameObject>();

    public List<List<GameObject>> spawnedWallsByRoom = new List<List<GameObject>>(); //spawned of all rooms
    public GameObject spawnedParentDebug; //used to load dumb rooms

    public GameObject linePrefab;

    public Dropdown dropdownRoom; //menu rooms
    public Dropdown dropdownCamera; //menu cameras
    Dictionary<string, GameObject> dropdownCameraValues = new Dictionary<string, GameObject>();

    public int currentRoom = 1;

    public bool load = false;

    public Material matWall;

    public GameObject projectorPrefab;

    private List<GameObject> projectors = new List<GameObject>();

    //save points/lines/Quads drawn to easily destory them
    private List<GameObject> ui3dGOs = new List<GameObject>(); 
    private List<GameObject> wallsQuads = new List<GameObject>();

    Dictionary<GameObject, List<Vector3>> wallPointsList = new Dictionary<GameObject, List<Vector3>>();

    public void Start() {
        //load parent to spawnedObjs2

        //LoadGameObjectsFromParent();

        if(load)
            Load();

        ReDrawUI3D();
    }

    protected void LoadGameObjectsFromParent() {
        if (spawnedParentDebug && spawnedParentDebug.transform.childCount > 0) {
            Debug.Log("Load Dumb Rooms");
            for (int i = 0; i < spawnedParentDebug.transform.childCount; i++) {
                List<GameObject> roomSpawned = new List<GameObject>();
                GameObject room = spawnedParentDebug.transform.GetChild(i).gameObject;
                Debug.Log("room" + room.name + ":" + room.transform.childCount);

                dropdownRoom.options.Insert(dropdownRoom.options.Count - 1, new Dropdown.OptionData() { text = "b " + room.name });

                //spawnedParent.gameObject.Get
                for (int j = 0; j < room.transform.childCount; j++) {
                    GameObject wall = room.transform.GetChild(j).gameObject;
                    roomSpawned.Add(wall);
                }

                spawnedWallsByRoom.Add(roomSpawned);
            }

            spawnedWalls = spawnedWallsByRoom[0];
        }
    }

    public void AddWall(Transform placementIndicatorTransform) {
        GameObject wall = Instantiate(wallToSpawn, placementIndicatorTransform.position, placementIndicatorTransform.rotation);
        spawnedWalls.Add(wall);
        Save();
        ReDrawUI3D();
    }

    public void AddPhoto(Vector3 position, Quaternion rotation, string fn) {
        // Create photo without UI
        GameObject go = new GameObject(fn);
        go.transform.SetPositionAndRotation(position, rotation);
        spawnedPhotos.Add(go);
        Save();
        ReDrawUI3D();
    }

    [ContextMenu("Save")]
    private void Save() {
        ObjLoader.Write(ObjLoader.GameObjectsToObjs(spawnedWalls, spawnedPhotos));
    }

    [ContextMenu("Load")]
    private void Load() {
        Objs objs = ObjLoader.Read();

        foreach(Obj obj in objs.list) {
            switch(obj.type) {
                case Obj.TYPE_WALL:
                case "Plane":
                    GameObject go = Instantiate(wallToSpawn, obj.position, obj.rotation);
                    go.name = obj.name;
                    spawnedWalls.Add(go);
                    break;

                case Obj.TYPE_PHOTO: //photo without UI
                    GameObject go2 = new GameObject(obj.name);
                    go2.transform.SetPositionAndRotation(obj.position, obj.rotation);
                    spawnedPhotos.Add(go2);
                    //go2.SetActive(false);
                    break;
            }
        }
    }

    // Draw a corner if the 3 spawnedWalls intersect at a single point
    private void DrawOneCorner(GameObject s0, GameObject s1, GameObject s2) {
        Vector3 point;
        bool success = Math3DUtils.planesIntersectAtSinglePoint(s0, s1, s2, out point);
        Debug.Log("point" + point);
        if (success) {
            GameObject go = Math3DUtils.CreateSphere(point, Color.yellow, 0.1f);
            ui3dGOs.Add(go);
            go.name = "Corner";

            GameObject line0 = DrawCornerLine(point, s0, s1, s2);
            GameObject line1 = DrawCornerLine(point, s1, s2, s0);
            GameObject line2 = DrawCornerLine(point, s2, s0, s1);

            if (line0) line0.transform.parent = go.transform;
            if (line1) line1.transform.parent = go.transform;
            if (line2) line2.transform.parent = go.transform;

            AddPointToWall(s0, point);
            AddPointToWall(s1, point);
            AddPointToWall(s2, point);
        }
    }

    private void AddPointToWall(GameObject spawedWall, Vector3 point) {
        List<Vector3> points = wallPointsList[spawedWall] = wallPointsList.ContainsKey(spawedWall) ? wallPointsList[spawedWall] : new List<Vector3>();
        points.Add(point);
    }

    // Draw a lines 
    private GameObject DrawCornerLine(Vector3 point, GameObject s0, GameObject s1, GameObject s2) {
        Vector3 point0;
        Vector3 dir0;
        bool success0 = Math3DUtils.planePlaneIntersection(out point0, out dir0, s0, s1);
        if (success0) {
            Vector3 dir0fixed = FixPositiveNegative(dir0, s2);
            return InstLine(point + dir0fixed / 2, dir0, Color.yellow);
        }
        return null;
    }

    // The direction of the line must be fixed, as a line can have 2 directions. Using the 3rd plane, we know what is its direction
    private Vector3 FixPositiveNegative(Vector3 dir, GameObject plane) {
        Vector3 normal = plane.transform.up;
        float product = Vector3.Dot(dir, normal); //1 or -1 means parallel
        return product > 0 ? dir : -1 * dir;
    }

    [ContextMenu("DrawUI3D")]
    private void ReDrawUI3D() {
        DestroyUI3D();

        //Draw points with "axis"
        if (spawnedWalls.Count >= 3) {
            for (int i = 0; i < spawnedWalls.Count - 2; i++) {
                for (int j = i + 1; j < spawnedWalls.Count - 1; j++) {
                    for (int k = j + 1; k < spawnedWalls.Count; k++) {
                        DrawOneCorner(spawnedWalls[i], spawnedWalls[j], spawnedWalls[k]);
                    }
                }
            }
        }
       
        foreach (List<Vector3> vs in wallPointsList.Values) {
            GameObject go = new GameObject("Wall");

            //Draw lines between points 
            int count = vs.Count;
            for (int i=0; i< count-1; i++) {
                for (int j=i; j<count; j++) {
                    Math3DUtils.CreateLine(vs[i], vs[j]).transform.parent = go.transform;
                }
            }

            //Draw quad
            if (vs.Count == 4) {
                GameObject goQuad = Math3DUtils.CreateQuad(vs.ToArray(), matWall, true);
                wallsQuads.Add(goQuad);
                goQuad.transform.parent = go.transform;
            }

            ui3dGOs.Add(go);
        }

        //Draw photo frame from photo taken // also load fov ?
        foreach (GameObject go in spawnedPhotos) {
            projectors.Add(DrawProjector(go.name, go.transform.position, go.transform.rotation));
        }

        // Add Projector to dropdown
        if (dropdownCamera) {
            //Draw photo frame and add them to dropdown
            dropdownCameraValues.Add("All", null);
            foreach (GameObject projector in projectors) {
                dropdownCameraValues.Add(projector.name.Substring(14), projector);
            }
            dropdownCamera.AddOptions(dropdownCameraValues.Keys.ToList<string>());
        }  
    }

    [ContextMenu("GenerateObj")]
    void GenerateObj() {
        Math3DUtils.MeshDivide(wallsQuads, 7);

        List<Camera> cameras = new List<Camera>();
        foreach (GameObject go in projectors)
            if(go.active) //to be able to disable some projection to debug
                cameras.Add(go.GetComponent<Camera>());

        ObjExportUtils.Export(cameras, this.wallsQuads);
    }

    [ContextMenu("DestroyUI3D")]
    private void DestroyUI3D() {
        // Destroy
        foreach (GameObject o in ui3dGOs) {
            Destroy(o);
        }
        ui3dGOs.Clear();
        wallsQuads.Clear();
        wallPointsList.Clear();

        if(dropdownCamera) {
            dropdownCamera.ClearOptions();
            dropdownCameraValues.Clear();
        }
    }

    public void DropdownCameraChanged(int position) {
        GameObject curProjector = dropdownCameraValues.Values.ElementAt<GameObject>(position);

        if(curProjector == null) {
            foreach(GameObject projector in projectors) {
                projector.SetActive(true);
            }
        } else {
            foreach (GameObject projector in projectors) {
                projector.SetActive(false);
            }
            Debug.Log("choosed " + curProjector.name);

            curProjector.SetActive(true);
            Camera.main.transform.position = curProjector.transform.position;
            Camera.main.transform.rotation = curProjector.transform.rotation;
        }
    }

    private void DrawPhotoPlane(string fn, Vector3 position, Quaternion rotation) {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetPositionAndRotation(position, rotation);
        plane.transform.Rotate(90, 0, 180);
        plane.transform.localScale *= 0.1f;

        var e = Resources.Load(UnityEngine.Application.persistentDataPath + "/" + fn);

        Texture t = Resources.Load(UnityEngine.Application.persistentDataPath+"/"+fn) as Texture;

        plane.GetComponent<Renderer>().material.SetTexture("df", t);

        ui3dGOs.Add(plane);
    }

    private GameObject DrawProjector(string fn, Vector3 position, Quaternion rotation) {
        GameObject o = Instantiate(projectorPrefab, position, rotation);
        o.GetComponent<DrawProjector>().fn = fn;
        // o.GetComponent<DrawProjector>().vfov = 63;
        o.name = "Projector " + fn;
        ui3dGOs.Add(o);
        return o;
    }

    private void DrawOneLine(GameObject spawnedA, GameObject spawnedB) {
        Vector3 point = Vector3.zero;
        Vector3 direction = Vector3.zero;
        bool success = Math3DUtils.planePlaneIntersection(out point, out direction, spawnedA, spawnedB);
        if (success) {
            DebugDrawLineVect(point, direction, 0.5f, Color.red, 10f);
            InstLine(point, direction, Color.yellow);
        }
    }

    private void DrawLineAll() {
        if (spawnedWalls.Count >= 2) {
            for (int i = 0; i < spawnedWalls.Count - 1; i++) {
                for (int j = i + 1; j < spawnedWalls.Count; j++) {
                    GameObject spawnedA = spawnedWalls[i];
                    GameObject spawnedB = spawnedWalls[j];
                    Debug.Log(spawnedA.name + " - " + spawnedB.name + " - i=" + i + " j=" + j);
                    DrawOneLine(spawnedA, spawnedB);
                }
            }
        }
    }

    private void DrawLineLast() {
        if (spawnedWalls.Count >= 2) {
            GameObject spawnedA = spawnedWalls[spawnedWalls.Count - 1]; //new item added
            for (int i = 0; i < spawnedWalls.Count - 1; i++) {
                DrawOneLine(spawnedA, spawnedWalls[i]);
            }
        }
    }

    GameObject InstLine(Vector3 point, Vector3 direction, Color color) {
        //return Math3DUtils.CreateLine(point, direction, color);
        GameObject o = Instantiate(linePrefab, point, Quaternion.LookRotation(direction));
        o.GetComponent<Renderer>().material.color = color;
        o.transform.Rotate(90, 0, 0);
        return o;
    }

    void DebugDrawLineVect(Vector3 point, Vector3 direction, float length, Color color, float duration) {
        Vector3 linePointa = point + direction * length;
        Vector3 linePointb = point - direction * length;
        Debug.DrawLine(linePointa, linePointb, color, duration);
        //TODO use Debug.DrawRay
    }

    public void OnDropDownChange(int val) {
        int count = dropdownRoom.options.Count;

        Debug.Log("dropdown: " + val + " nb:"+count);

        if(val == count -1) { //dropdown.options[val].text == "New Room"
            dropdownRoom.options.Insert(count - 1 , new Dropdown.OptionData() { text = "Room " + count });
        }
    }

    public void RemoveGO(GameObject go) {
        spawnedWalls.Remove(go);
        spawnedWallsByRoom.ForEach(objs => objs.Remove(go));
        Destroy(go); //should destroy only if wall
        ReDrawUI3D();
    }
}
