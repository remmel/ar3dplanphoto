using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ObjLoader: MonoBehaviour
{
    private string path;

    public void Start() {
        // windows : C:\Users\Comparabus\AppData\LocalLow\Remmel\unityarf3dplanphoto\
        // Android : Android\data\com.remmel.unityarf3dplanphoto\files\
        path = UnityEngine.Application.persistentDataPath + "/3dplanphoto_objs.json";
    }

    public Objs GenerateDumb() {

        List<Obj> list = new List<Obj>();

        list.Add(new Obj() { name = "aaa?"});
        list.Add(new Obj() { name = "bbb?"});

        Objs objs2 = new Objs() { list = list};
        return objs2;
    }

    public void Write(Objs objs) {
        using (StreamWriter stream = new StreamWriter(path)) {
            stream.Write(JsonUtility.ToJson(objs, true));
        }
    }

    public Objs Read() {
        using (StreamReader stream = new StreamReader(path)) {
            return JsonUtility.FromJson<Objs>(stream.ReadToEnd());
        }
    }

    public static Objs GameObjectsToObjs(List<GameObject> gobjs) {
        Objs objs = new Objs();
        foreach (GameObject o in gobjs) {
            Obj obj = new Obj() { name = o.name, position = o.transform.position, rotation = o.transform.rotation };
            objs.list.Add(obj);
        }
        return objs;
    }
}
