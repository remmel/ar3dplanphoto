using System;
using System.Collections.Generic;
using System.Linq;
using ToastPlugin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//TODO check if all pictures saved in json files, later load them, later display frame 

public class SpawnAndPhoto : MonoBehaviour
{
    public GameObject wallToSpawn;
    public GameObject arCamera;
    public PlacementIndicatorS placementIndicator;

    public List<GameObject> spawnedWalls = new List<GameObject>(); //current room spawned objs
    public List<GameObject> spawnedPhotos = new List<GameObject>();

    public List<List<GameObject>> spawnedWallsByRoom = new List<List<GameObject>>(); //spawned of all rooms
    public GameObject spawnedParentDebug; //used to load dumb rooms

    public GameObject spherePrefab; //to display corners
    public GameObject linePrefab;

    public Dropdown dropdownRoom; //menu rooms
    public Dropdown dropdownCamera; //menu cameras
    Dictionary<string, GameObject> dropdownCameraValues = new Dictionary<string, GameObject>();

    public int currentRoom = 1;

    public bool load = false;

    public Material matWall;

    public GameObject projector;

    //save points/lines/Quads drawn to easily destory them
    private List<GameObject> ui3dGOs = new List<GameObject>(); 

    Dictionary<GameObject, List<Vector3>> wallPointsList = new Dictionary<GameObject, List<Vector3>>();

    public void Start() {
        //load parent to spawnedObjs2

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

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

    public void SpawnAndTake() {

        // Create object
        GameObject obj = Instantiate(wallToSpawn, placementIndicator.transform.position, placementIndicator.transform.rotation);
        Debug.Log("Spawn&Take"); //adb logcat -s Unity PackageManager dalvikvm DEBUG //adb logcat -v time -s Unity

        String fn = takeSnapshot();
    }

    public void Wall() {
        GameObject wall = Instantiate(wallToSpawn, placementIndicator.transform.position, placementIndicator.transform.rotation);
        spawnedWalls.Add(wall);
        Debug.Log("Spawn");
        Debug.Log("cube: " + JsonUtility.ToJson(wall.transform.position));
        Debug.Log("camera: " + JsonUtility.ToJson(arCamera.transform.position));
        Save();

        //DrawPointLast();
        //DrawLineLast();

        ReDrawUI3D();
    }

    public void Photo() {
        String fn = takeSnapshot();

        // Create photo without UI
        GameObject go = new GameObject(fn);
        go.transform.SetPositionAndRotation(arCamera.transform.position, arCamera.transform.rotation);

        //arCamera.GetComponent<Camera>().fieldOfView
        spawnedPhotos.Add(go);
        Save();

        ReDrawUI3D();
    }

    private String takeSnapshot() {
        Texture2D snap = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        snap.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        snap.Apply();

        string fn = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_screenshot.jpg";

        string pathSnap = Application.persistentDataPath + "/" + fn;
        System.IO.File.WriteAllBytes(pathSnap, snap.EncodeToJPG());
        Debug.Log("Screenshot saved in " + pathSnap);
        ToastHelper.ShowToast("Screenshot saved in " + pathSnap);
        return fn;
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


    private void DrawOnePoint(GameObject p0, GameObject p1, GameObject p2) {
        Vector3 point;
        bool success = Math3DUtils.planesIntersectAtSinglePoint(p0, p1, p2, out point);
        Debug.Log("point" + point);
        if (success) {

            //pointsWithGameobjets.Add(new Point3GameObjects(point, p0, p1, p2));
            InstSphere(point, Color.yellow);
            DrawOnePointThreeLines(point, p0, p1, p2);
            DrawOnePointThreeLines(point, p1, p2, p0);
            DrawOnePointThreeLines(point, p2, p0, p1);

            AddPointToWall(p0, point);
            AddPointToWall(p1, point);
            AddPointToWall(p2, point);
        }
    }

    private void AddPointToWall(GameObject wall, Vector3 point) {
        List<Vector3> points = wallPointsList[wall] = wallPointsList.ContainsKey(wall) ? wallPointsList[wall] : new List<Vector3>();
        points.Add(point);
    }

    private void DrawOnePointThreeLines(Vector3 point, GameObject p0, GameObject p1, GameObject p2) {
        Vector3 point0;
        Vector3 dir0;
        bool success0 = Math3DUtils.planePlaneIntersection(out point0, out dir0, p0, p1);
        if (success0) {
            Vector3 dir0fixed = FixPositiveNegative(dir0, p2);
            InstLine(point + dir0fixed / 2, dir0, Color.yellow);
        }
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
                        GameObject p0 = spawnedWalls[i];
                        GameObject p1 = spawnedWalls[j];
                        GameObject p2 = spawnedWalls[k];
                        Debug.Log(p0.name + " - " + p1.name + " - " + p2.name + " - i=" + i + " j=" + j + " k="+k);
                        DrawOnePoint(p0, p1, p2);
                    }
                }
            }
        }

        //Draw lines between points
        foreach (List<Vector3> vs in wallPointsList.Values) {
            int count = vs.Count;
            for (int i=0; i< count-1; i++) {
                for (int j=i; j<count; j++) {
                    DrawOneLine2(vs[i], vs[j]);
                }
            }
        }

        //Draw quad
        foreach (List<Vector3> vs in wallPointsList.Values) {
            int count = vs.Count;
            if(count == 4) {
                CreateQuad(vs.ToArray());
            }
        }

        //Draw photo frame and add them to dropdown
        foreach (GameObject go in spawnedPhotos) {
            GameObject projector = DrawProjector(go.name, go.transform.position, go.transform.rotation);
            dropdownCameraValues.Add(go.name.Substring(14), projector);
        }

        dropdownCamera.AddOptions(dropdownCameraValues.Keys.ToList<string>());
    }

    private void DestroyUI3D() {
        // Destroy
        foreach (GameObject o in ui3dGOs) {
            Destroy(o);
        }
        ui3dGOs.Clear();
        wallPointsList.Clear();

        dropdownCamera.ClearOptions();
        dropdownCameraValues.Clear();
    }

    public void DropdownCameraChanged(int position) {
        GameObject go = dropdownCameraValues.Values.ElementAt<GameObject>(position);
        Debug.Log("choosed " + go.name);

        Camera.main.transform.position = go.transform.position;
        Camera.main.transform.rotation = go.transform.rotation;
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
        GameObject o = Instantiate(projector, position, rotation);
        o.GetComponent<DrawProjector>().fn = fn;
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

    private void DrawOneLine2(Vector3 from, Vector3 to) {
        GameObject go = new GameObject();
        go.name = "LineRenderer2pos";
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.SetWidth(0.05f, 0.05f);
        lr.SetPositions(new[] { from, to });
        ui3dGOs.Add(go);
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

    GameObject InstSphere(Vector3 vector3, Color color) {
        GameObject o = Instantiate(spherePrefab, vector3, Quaternion.identity);
        o.GetComponent<Renderer>().material.color = color;
        ui3dGOs.Add(o);
        return o;
    }

    GameObject InstLine(Vector3 point, Vector3 direction, Color color) {
        GameObject o = Instantiate(linePrefab, point, Quaternion.LookRotation(direction));
        o.GetComponent<Renderer>().material.color = color;
        o.transform.Rotate(90, 0, 0);
        //o.transform.position = o.transform.position + new Vector3(0, 1f, 0);
        ui3dGOs.Add(o);
        return o;
    }

    //https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html
    GameObject CreateQuad(Vector3[] vertices) {

        /*vertices = new Vector3[4] {
            vertices[1],
            vertices[0],
            vertices[3],
            vertices[2],
        };*/

        GameObject go = new GameObject();
        go.name = "Quad";
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        //mr.sharedMaterial = new Material(Shader.Find("Standard"));
        mr.material = matWall; //mat circles
        //mr.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
        mr.material.mainTextureScale = new Vector2(Vector3.Distance(vertices[0], vertices[1]), Vector3.Distance(vertices[1], vertices[2]));

        //mr.material.wrapMode = TextureWrapMode.Repeat;

        MeshFilter mf = go.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        //InstSphere(vertices[0], Color.cyan);
        
        mesh.vertices = vertices;

        int[] tris = new int[12]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1,

            //double sides
            3, 2, 1,
            2, 0, 1
        };
        //TODO reorganize order
        mesh.triangles = tris;

        // TODO double side quad

        /* Vector3[] normals = new Vector3[4]
         {
             -Vector3.forward,
             -Vector3.forward,
             -Vector3.forward,
             -Vector3.forward
         };
         mesh.normals = normals;*/

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;
        mf.mesh = mesh;

        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();

        ui3dGOs.Add(go);

        return go;
    }

    void DebugDrawLineVect(Vector3 point, Vector3 direction, float length, Color color, float duration) {
        Vector3 linePointa = point + direction * length;
        Vector3 linePointb = point - direction * length;
        Debug.DrawLine(linePointa, linePointb, color, duration);
    }

    public void OnDropDownChange(int val) {
        int count = dropdownRoom.options.Count;

        Debug.Log("dropdown: " + val + " nb:"+count);

        if(val == count -1) { //dropdown.options[val].text == "New Room"
            dropdownRoom.options.Insert(count - 1 , new Dropdown.OptionData() { text = "Room " + count });
        }

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

                    print(go.name);

                    spawnedWalls.Remove(go);
                    spawnedWallsByRoom.ForEach(objs => objs.Remove(go));
                    Destroy(go); //should destroy only if wall

                    ReDrawUI3D();
                }
            }
        }
    }
}
