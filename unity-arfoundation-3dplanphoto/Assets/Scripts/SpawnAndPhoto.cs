using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using ToastPlugin;
using UnityEngine;
using UnityEngine.UI;

public class SpawnAndPhoto : MonoBehaviour
{
    public GameObject objectToSpawn;
    public GameObject arCamera;
    public PlacementIndicatorS placementIndicator;

    public List<GameObject> spawnedObjs = new List<GameObject>(); //current room spawned objs

    public List<List<GameObject>> spawnedObjsByRoom = new List<List<GameObject>>(); //spawned of all rooms
    public GameObject spawnedParent;

    public GameObject spherePrefab;
    public GameObject linePrefab;

    public Dropdown dropdown;
    public int currentRoom = 1;

    public Material mat;

    //save points and lines drawn to easily destory them
    private List<GameObject> pointsAndLines = new List<GameObject>(); 
    //private List<Point3GameObjects> pointsWithGameobjets = new List<Point3GameObjects>();

    Dictionary<GameObject, List<Vector3>> wallPointsList = new Dictionary<GameObject, List<Vector3>>();

    public void Start() {
        //load parent to spawnedObjs2

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        LoadGameObjectsFromParent();
        DrawLinesAndPoints();

        Debug.Log(spawnedObjs);

        //DrawOneLine2(new Vector3(0f, 0f, 1f), new Vector3(2f, 2f, 2f));
    }

    protected void LoadGameObjectsFromParent() {
        if (spawnedParent && spawnedParent.transform.childCount > 0) {
            Debug.Log("Load Dumb Rooms");
            for (int i = 0; i < spawnedParent.transform.childCount; i++) {
                List<GameObject> roomSpawned = new List<GameObject>();
                GameObject room = spawnedParent.transform.GetChild(i).gameObject;
                Debug.Log("room" + room.name + ":" + room.transform.childCount);

                dropdown.options.Insert(dropdown.options.Count - 1, new Dropdown.OptionData() { text = "b " + room.name });

                //spawnedParent.gameObject.Get
                for (int j = 0; j < room.transform.childCount; j++) {
                    GameObject wall = room.transform.GetChild(j).gameObject;
                    roomSpawned.Add(wall);
                }

                spawnedObjsByRoom.Add(roomSpawned);
            }

            spawnedObjs = spawnedObjsByRoom[0];
        }
    }

    public void SpawnAndTake() {

        // Create object
        GameObject obj = Instantiate(objectToSpawn, placementIndicator.transform.position, placementIndicator.transform.rotation);
        Debug.Log("Spawn&Take"); //adb logcat -s Unity PackageManager dalvikvm DEBUG //adb logcat -v time -s Unity

        string timeStamp = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        takeSnapshot(timeStamp);
        savePosition(timeStamp);
    }

    public void Spawn() {
        GameObject obj = Instantiate(objectToSpawn, placementIndicator.transform.position, placementIndicator.transform.rotation);
        spawnedObjs.Add(obj);
        Debug.Log("Spawn");
        Debug.Log("cube: " + JsonUtility.ToJson(obj.transform.position));
        Debug.Log("camera: " + JsonUtility.ToJson(arCamera.transform.position));
        WriteSpawned();

        //DrawPointLast();
        //DrawLineLast();

        DestroyAndDrawAllPointsAndLines();
    }

    public void Photo() {
        string timeStamp = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        takeSnapshot(timeStamp);
        savePosition(timeStamp);
    }

    void takeSnapshot(string timeStamp) {
        Texture2D snap = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        snap.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        snap.Apply();

        
        string pathSnap = Application.persistentDataPath + "/" + timeStamp + "_screenshot_sdp.jpg";
        System.IO.File.WriteAllBytes(pathSnap, snap.EncodeToJPG());
        Debug.Log("Screenshot saved in " + pathSnap);
        ToastHelper.ShowToast("Screenshot saved in " + pathSnap);
    }

    void savePosition(string timeStamp) {
        Debug.Log("placement pos: " + JsonUtility.ToJson(placementIndicator.transform.position));
        Debug.Log("placement rot: " + JsonUtility.ToJson(placementIndicator.transform.rotation));
        Debug.Log("camera: " + JsonUtility.ToJson(arCamera.transform.position));

        string pathInfo = Application.persistentDataPath + "/" + timeStamp + "_info.txt";

        StreamWriter writer = new StreamWriter(pathInfo, true);
        writer.WriteLine("placement pos: " + JsonUtility.ToJson(placementIndicator.transform.position));
        writer.WriteLine("placement rot: " + JsonUtility.ToJson(placementIndicator.transform.rotation));
        writer.WriteLine("camera: " + JsonUtility.ToJson(arCamera.transform.position));
        writer.Close();
    }

    [ContextMenu("Write")]
    private void WriteSpawned() {
        ObjLoader loader = FindObjectOfType<ObjLoader>();
        loader.Write(ObjLoader.GameObjectsToObjs(spawnedObjs));
    }

    [ContextMenu("Read")]
    private void ReadSpawned() {
        ObjLoader loader = FindObjectOfType<ObjLoader>();
        Objs objs = loader.Read();

        foreach(Obj obj in objs.list) {
            GameObject go = Instantiate(objectToSpawn, obj.position, obj.rotation);
            go.name = obj.name;
            spawnedObjs.Add(go);
        }
    }

    [ContextMenu("DrawLinesAndPoints")]
    private void DrawLinesAndPoints() {
        DrawPointAll();
        //DrawLineAll();
        //DrawPointLast();

        // TODO draw better room
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

    private void DrawPointAll() {
        //Draw points with "axis"
        if(spawnedObjs.Count >= 3) {
            for (int i = 0; i < spawnedObjs.Count - 2; i++) {
                for (int j = i + 1; j < spawnedObjs.Count - 1; j++) {
                    for (int k = j + 1; k < spawnedObjs.Count; k++) {
                        GameObject p0 = spawnedObjs[i];
                        GameObject p1 = spawnedObjs[j];
                        GameObject p2 = spawnedObjs[k];
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

    }

    private void DrawPointLast() {
        if (spawnedObjs.Count >= 3) {
            GameObject spawnedA = spawnedObjs[spawnedObjs.Count - 1]; //new item added

            for (int i = 0; i < spawnedObjs.Count - 2; i++) {
                for (int j = i + 1; j < spawnedObjs.Count - 1; j++) {
                    DrawOnePoint(spawnedA, spawnedObjs[i], spawnedObjs[j]);
                }
            }
        }
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
        pointsAndLines.Add(go);
    }

    private void DrawLineAll() {
        if (spawnedObjs.Count >= 2) {
            for (int i = 0; i < spawnedObjs.Count - 1; i++) {
                for (int j = i + 1; j < spawnedObjs.Count; j++) {
                    GameObject spawnedA = spawnedObjs[i];
                    GameObject spawnedB = spawnedObjs[j];
                    Debug.Log(spawnedA.name + " - " + spawnedB.name + " - i=" + i + " j=" + j);
                    DrawOneLine(spawnedA, spawnedB);
                }
            }
        }
    }

    private void DrawLineLast() {
        if (spawnedObjs.Count >= 2) {
            GameObject spawnedA = spawnedObjs[spawnedObjs.Count - 1]; //new item added
            for (int i = 0; i < spawnedObjs.Count - 1; i++) {
                DrawOneLine(spawnedA, spawnedObjs[i]);
            }
        }
    }

    GameObject InstSphere(Vector3 vector3, Color color) {
        GameObject o = Instantiate(spherePrefab, vector3, Quaternion.identity);
        o.GetComponent<Renderer>().material.color = color;
        pointsAndLines.Add(o);
        return o;
    }

    GameObject InstLine(Vector3 point, Vector3 direction, Color color) {
        GameObject o = Instantiate(linePrefab, point, Quaternion.LookRotation(direction));
        o.GetComponent<Renderer>().material.color = color;
        o.transform.Rotate(90, 0, 0);
        //o.transform.position = o.transform.position + new Vector3(0, 1f, 0);
        pointsAndLines.Add(o);
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
        mr.material = mat;
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

            //doble sides
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

        return go;
    }

    void DebugDrawLineVect(Vector3 point, Vector3 direction, float length, Color color, float duration) {
        Vector3 linePointa = point + direction * length;
        Vector3 linePointb = point - direction * length;
        Debug.DrawLine(linePointa, linePointb, color, duration);
    }

    [ContextMenu("Destroy")]
    void DestroyAllDrawnPointsAndLines() {
        foreach(GameObject o in pointsAndLines) {
            Destroy(o);
        }
        pointsAndLines.Clear();
        wallPointsList.Clear();
    }

    void DestroyAndDrawAllPointsAndLines() {
        DestroyAllDrawnPointsAndLines();
        DrawPointAll();
    }

    public void OnDropDownChange(int val) {
        int count = dropdown.options.Count;

        Debug.Log("dropdown: " + val + " nb:"+count);

        if(val == count -1) { //dropdown.options[val].text == "New Room"
            dropdown.options.Insert(count - 1 , new Dropdown.OptionData() { text = "Room " + count });
        }

    }

    public void Update() {
        UpdateDestroy();
    }

    private void UpdateDestroy() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f)) {
                if (hit.transform) {
                    GameObject go = hit.transform.gameObject;

                    print(go.name);

                    spawnedObjs.Remove(go);
                    spawnedObjsByRoom.ForEach(objs => objs.Remove(go));
                    Destroy(go);

                    DestroyAndDrawAllPointsAndLines();
                }
            }
        }
    }
}
