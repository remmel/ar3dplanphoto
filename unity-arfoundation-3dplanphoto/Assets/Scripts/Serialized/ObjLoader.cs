using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ObjLoader
{
    // windows : C:\Users\Comparabus\AppData\LocalLow\Remmel\unityarf3dplanphoto\
    // Android : Android\data\com.remmel.unityarf3dplanphoto\files\
    private static string path = UnityEngine.Application.persistentDataPath + "/3dplanphoto_objs.json";

    public static Objs GenerateDumb() {

        List<Obj> list = new List<Obj>();

        list.Add(new Obj() { name = "aaa?"});
        list.Add(new Obj() { name = "bbb?"});

        Objs objs2 = new Objs() { list = list};
        return objs2;
    }

    public static void Write(Objs objs) {
        using (StreamWriter stream = new StreamWriter(path)) {
            stream.Write(JsonUtility.ToJson(objs, true));
        }
    }

    public static Objs Read() {
        using (StreamReader stream = new StreamReader(path)) {
            return JsonUtility.FromJson<Objs>(stream.ReadToEnd());
        }
    }

    public static Objs GameObjectsToObjs(List<GameObject> walls, List<GameObject> photos) {
        Objs objs = new Objs();
        foreach (GameObject o in walls) {
            Obj obj = new Obj() { name = o.name, position = o.transform.position, rotation = o.transform.rotation, eulerAngles=o.transform.rotation.eulerAngles, type=Obj.TYPE_WALL }; //todo add ro
            objs.list.Add(obj);
        }

        foreach (GameObject o in photos) {
            Obj obj = new Obj() { name = o.name, position = o.transform.position, rotation = o.transform.rotation, eulerAngles = o.transform.rotation.eulerAngles, type = Obj.TYPE_PHOTO };
            objs.list.Add(obj);
        }

        return objs;
    }
}
