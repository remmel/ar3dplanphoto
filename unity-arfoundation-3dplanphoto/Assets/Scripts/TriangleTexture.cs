using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleTexture : MonoBehaviour
{
    TriangleTex[] vts; 

    Vector3[] worldVertices;

    static bool debug = false;

    Mesh getMesh() {
        return this.GetComponent<MeshFilter>().mesh;
    }

    void Init()
    {
        Mesh m = getMesh();
        int nbTriangles = m.triangles.Length / 3;
        // Dictionary<int, TriangleTex> vts = new Dictionary<int, TriangleTex>(nbTriangles);
        vts = new TriangleTex[nbTriangles];

        worldVertices = new Vector3[m.vertices.Length];
        for (int i = 0; i < m.vertices.Length; i++) {
            worldVertices[i] = this.transform.TransformPoint(m.vertices[i]);
        }

        for (int t=0; t<nbTriangles; t++) {
            TriangleTex tex = new TriangleTex();
            tex.center = getCenter(t);
            vts[t] = tex;
        }
    }

    Vector3 getWordVertex(int t) {
        Mesh m = getMesh();
        Vector3 lVertex = m.vertices[m.triangles[t]];
        return this.transform.TransformPoint(lVertex);
    }

    Vector3 getCenter(int t) {
        //Vector3 a = getWordVertex(i * 3 + 0);
        //Vector3 b = getWordVertex(i * 3 + 1);
        //Vector3 c = getWordVertex(i * 3 + 2);

        Mesh m = getMesh();

        int v = m.triangles[t * 3 + 0];

        Vector3 a = worldVertices[v];
        Vector3 b = worldVertices[m.triangles[t * 3 + 1]];
        Vector3 c = worldVertices[m.triangles[t * 3 + 2]];

        return (a + b + c) / 3;
    }

    static bool isVisible(Vector3 point, Transform camera) { // warning, the object must have a collider
        Vector3 dir = (point - camera.transform.position).normalized;
        //Vector3 raycastOrigin = wVertex - dir * 0.1f;
        //bool raycast = Physics.Raycast(raycastOrigin, dir, 10); //transform.TransformDirection(Vector3.forward)

        float distance = Vector3.Distance(camera.transform.position, point) - 0.01f; //remove some 1cm to avoid touching the object (excluding)
        bool raycast = Physics.Raycast(camera.transform.position, dir, distance);

        if (debug)
            Debug.DrawRay(camera.transform.position, dir * distance, raycast ? Color.red : Color.green, 100); //Debug.DrawLine(wVertex, wVertex - dir * 10, Color.red, Mathf.Infinity);

        return !raycast;
    }

    bool OutOfViewportTriangle(Vector2[] uvs, int numtriangle) {
        //Vector3 a = getWordVertex(numtriangle * 3 + 0);
        //Vector3 b = getWordVertex(numtriangle * 3 + 1);
        //Vector3 c = getWordVertex(numtriangle * 3 + 2);

        Mesh m = getMesh();

        Vector3 a = uvs[m.triangles[numtriangle * 3 + 0]];
        Vector3 b = uvs[m.triangles[numtriangle * 3 + 1]];
        Vector3 c = uvs[m.triangles[numtriangle * 3 + 2]];

        return Math3DUtils.OutOfViewportUV(a) || Math3DUtils.OutOfViewportUV(b) || Math3DUtils.OutOfViewportUV(c);
    }

    float getAngle(int t) {
        Mesh m = getMesh();

        Vector3 a = worldVertices[m.triangles[t * 3 + 0]];
        Vector3 b = worldVertices[m.triangles[t * 3 + 1]];
        Vector3 c = worldVertices[m.triangles[t * 3 + 2]];

        Plane plane = new Plane(a, b, c);

        Vector3 norm = plane.normal;
        Debug.Log("normal:" + norm);

        Math3DUtils.CreateSphere(a, Color.gray);
        Debug.DrawRay(a, norm, Color.white, 100);
        Debug.Log("dir:" + this.transform.forward);

        Vector3 dirProjector = this.transform.forward;

        Debug.DrawRay(a, -dirProjector, Color.grey, 100);

        float angle = Vector3.Angle(norm, -dirProjector);
        Debug.Log("Angle:" + angle); // mod 90

        return angle;
    }

    public void CalculateUV(Camera camera) {
        Mesh m = this.GetComponent<MeshFilter>().mesh;

        Init();

        // precalculate all uv (quick to calculate worldToViewPoint or raycast?
        Vector2[] uvs = new Vector2[m.vertices.Length];
        for(int i = 0; i<m.vertices.Length; i++) {
            Vector2 uv = camera.WorldToViewportPoint(this.worldVertices[i]);
            uvs[i] = uv;

            /*
            if (debug) {
                Vector3 projected = ProjectOnPlaneViewport(uv);
                GameObject pgo = Math3DUtils.CreateSphere(projected, Color.cyan);
                pgo.transform.rotation = this.transform.rotation;
                pgo.name = "Projected";
            }
            */
        }


        for (int t = 0; t < m.triangles.Length / 3; t++) {
            TriangleTex vt = vts[t];

            bool outOfView = this.OutOfViewportTriangle(uvs, t);
            bool visible = isVisible(vt.center, camera.transform);

            if(!outOfView && visible) {
                Vector2 a = uvs[m.triangles[t * 3 + 0]];
                Vector3 b = uvs[m.triangles[t * 3 + 1]];
                Vector3 c = uvs[m.triangles[t * 3 + 2]];

                vt.uvs3 = new Vector2[] { a, b, c };
                vt.photo = "cube";
                vt.distance = Vector3.Distance(camera.transform.position, vt.center);
                vt.angle = this.getAngle(t);
            }
        }
    }

    public string Export(List<Camera> cameras, ref int offsetV, ref int offsetVT) {
        Init();

        foreach(Camera camera in cameras)
            CalculateUV(camera); //for each projection


        Mesh m = this.GetComponent<MeshFilter>().mesh;

        // cube: 6 faces, 36 triangles, 24 vertices
        Debug.Log("vertices: " + m.vertices.Length + " triangles: " + m.triangles.Length + " offsetV:"+offsetV+ " offsetVT:"+offsetVT);

        string wavefrontV = "";
        string wavefrontVT = "";
        string wavefrontF = "";

        for (int i = 0; i < m.vertices.Length; i++) {
            Vector3 wVertex = this.transform.TransformPoint(m.vertices[i]);
            wavefrontV += "v " + wVertex.x + " " + wVertex.y + " " + wVertex.z + " 1.0\n";
        }

        int vt = 0;
        for (int t = 0; t < m.triangles.Length / 3; t++) {
            TriangleTex ttex = this.vts[t];

            int va = m.triangles[t * 3 + 0] + offsetV;
            int vb = m.triangles[t * 3 + 1] + offsetV;
            int vc = m.triangles[t * 3 + 2] + offsetV;

            if(ttex.uvs3 != null) {
                foreach (Vector2 uv in ttex.uvs3) { //do not handle when same uv twice (duplicate date) //should group by texture (ttex['cube'] = [])
                    wavefrontVT += "vt " + uv.x + " " + uv.y + "\n";
                }
                wavefrontF += "f " + (va + 1) + "/" + (offsetVT + vt + 1) + " " + (vb + 1) + "/" + (offsetVT + vt+2) + " " + (vc + 1) + "/" + (offsetVT + vt+3) + "\n";
                vt += 3;
            } else {
                wavefrontF += "f " + (va + 1) + " " + (vb + 1) + " " + (vc + 1) + "\n";
            }
        }



        offsetV += m.vertices.Length;
        offsetVT += vt;

        string n = "cube";
        return
            "o " + this.name + "\n\n" +
            "mtllib ./" + n + ".mtl\n\n" +
            wavefrontV + "\n" +
            wavefrontVT + "\n" +
            wavefrontF +
            "#" + System.DateTime.Now.ToLongDateString() + " " + System.DateTime.Now.ToLongTimeString() + " v=" + m.vertices.Length + "; vt=" + vt + "; f=" + m.triangles.Length + ";" + "\n";
    }
}
