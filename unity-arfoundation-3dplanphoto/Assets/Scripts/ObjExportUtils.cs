using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ObjExportUtils
{
    /**
     * Create a textured .obj
     * From list of walls generate mesh. From list of cameras generates it texture
     * return the path of the .obj created
     * //TODO use script https://wiki.unity3d.com/index.php/ExportOBJ#:~:text=Select%20an%20object%20in%20the,OBJ%20file.
     */
    public static string Export(List<Camera> cameras, List<GameObject> gos, string objname = "export") {

        // Calculate UV
        Debug.Log("Calculate UV"); 
        foreach (GameObject go in gos) {
            TriangleTexture tt = go.GetComponent<TriangleTexture>() ?? go.AddComponent<TriangleTexture>();
            tt.CalculateUV(cameras);
        }

        // Export Mat & Objs
        Debug.Log("Export material");
        ExportMaterial(objname, cameras);
        Debug.Log("Export obj");
        return ExportObjs(objname, gos);
    }

    private static void ExportMaterial(string objname, List<Camera> cameras) {
        string mtl = "";
        foreach (Camera camera in cameras) {
            string fn = camera.GetComponent<DrawProjector>().fn;
            string n = Path.GetFileNameWithoutExtension(fn);

            mtl += "newmtl material_" + n + "\n" +
                "Ka 0.200000 0.200000 0.200000\n" +
                "Kd 1.000000 1.000000 1.000000\n" +
                "Ks 1.000000 1.000000 1.000000\n" +
                "Tr 1.000000\n" +
                "illum 2\n" +
                "Ns 0.000000\n" +
                "map_Kd " + fn + " \n\n";
        }
        Write(Application.persistentDataPath + "/" + objname + ".mtl", mtl);
    }

    private static string ExportObjs(string objname, List<GameObject> gos) {
        string export = "";
        int offsetV = 0;
        int offsetVT = 0;
        foreach (GameObject go in gos)
            export += ExportOneGameObject(go, ref offsetV, ref offsetVT) + "\n";

        string path = Application.persistentDataPath + "/" + objname + ".obj";
        Write(path, export);
        return path;
    }

    private static string ExportOneGameObject(GameObject go, ref int offsetV, ref int offsetVT) {
        Mesh m = go.GetComponent<MeshFilter>().mesh;
        TriangleTexture tt = go.GetComponent<TriangleTexture>();

        // cube: 6 faces, 36 triangles, 24 vertices
        //Debug.Log("vertices: " + m.vertices.Length + " triangles: " + m.triangles.Length + " offsetV:" + offsetV + " offsetVT:" + offsetVT);

        string wavefrontV = "";
        string wavefrontVT = "";
        string wavefrontF = "";

        for (int i = 0; i < m.vertices.Length; i++) {
            Vector3 wVertex = go.transform.TransformPoint(m.vertices[i]);
            wavefrontV += "v " + wVertex.x + " " + wVertex.y + " " + wVertex.z + " 1.0\n";
        }

        int vt = 0;
        string matname = null;
        for (int t = 0; t < m.triangles.Length / 3; t++) {
            TriangleTextureData ttex = tt.vts[t];

            int va = m.triangles[t * 3 + 0] + offsetV;
            int vb = m.triangles[t * 3 + 1] + offsetV;
            int vc = m.triangles[t * 3 + 2] + offsetV;

            if (ttex.uvs3 != null) {
                foreach (Vector2 uv in ttex.uvs3) { //do not handle when same uv twice (duplicate date) //should group by texture (ttex['cube'] = [])
                    wavefrontVT += "vt " + uv.x + " " + uv.y + " # angle="+ttex.angle+ " distance=" + ttex.distance +"\n";
                }

                string matnamecur = Path.GetFileNameWithoutExtension(ttex.photo);
                if (matname == null || matname != matnamecur) {
                    matname = matnamecur;
                    wavefrontF += "usemtl material_" + matname + "\n";
                }

                wavefrontF += "f " + (va + 1) + "/" + (offsetVT + vt + 1) + " " + (vb + 1) + "/" + (offsetVT + vt + 2) + " " + (vc + 1) + "/" + (offsetVT + vt + 3) + "\n";
                vt += 3;
            } else {
                wavefrontF += "f " + (va + 1) + " " + (vb + 1) + " " + (vc + 1) + "\n";
            }
        }

        offsetV += m.vertices.Length;
        offsetVT += vt;

        return
            "o " + go.name + "\n\n" +
            wavefrontV + "\n" +
            wavefrontVT + "\n" +
            wavefrontF +
            "#" + System.DateTime.Now.ToLongDateString() + " " + System.DateTime.Now.ToLongTimeString() + " v=" + m.vertices.Length + "; vt=" + vt + "; f=" + m.triangles.Length + ";" + "\n";
    }

    public static void Write(string path, string text) {
        StreamWriter writer = new StreamWriter(path);
        writer.Write(text);
        writer.Close();
    }
}
