using System.Collections.Generic;
using System.IO;
using UnityEngine;

/**
 * Know how to load and read data. Also know the "persistentDataPath" directory where to save/load the data
 */ 
public class ObjLoader
{
    private static string FN = "3dplanphoto_objs.json";
    // windows : C:\Users\remme\AppData\LocalLow\Remmel\unityarf3dplanphoto\
    // Android : Android\data\com.remmel.unityarf3dplanphoto\files\
    private static string DATAPATH = UnityEngine.Application.persistentDataPath;

    public static Objs GenerateDumb() {

        List<Obj> list = new List<Obj>();

        list.Add(new Obj() { name = "aaa?"});
        list.Add(new Obj() { name = "bbb?"});

        Objs objs2 = new Objs() { list = list};
        return objs2;
    }

    public static void Write(string dirname, Objs objs) {
        if (!Directory.Exists(DATAPATH + "/" + dirname))
            Directory.CreateDirectory(DATAPATH + "/" + dirname); //duplicated code
        using (StreamWriter stream = new StreamWriter(DATAPATH + "/" + dirname + "/" + FN)) {
            stream.Write(JsonUtility.ToJson(objs, true));
        }
    }

    public static Objs Read(string dirname = null) {
        if(dirname == null) { //find last folder with json
            throw new System.Exception("find dir TO be implemented");
        }
        using (StreamReader stream = new StreamReader(DATAPATH + "/" + dirname + "/" + FN)) {
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
