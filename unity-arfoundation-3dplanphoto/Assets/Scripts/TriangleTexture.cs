using System;
using System.Collections.Generic;
using UnityEngine;

public class TriangleTexture : MonoBehaviour
{
    public TriangleTextureData[] vts; //store the relation between each triangle and uv texture

    Vector3[] worldVertices; //store that to avoid recalculating it

    static bool debug = false;

    Mesh getMesh() {
        return this.GetComponent<MeshFilter>().mesh;
    }

    public void Init()
    {
        Mesh m = getMesh();
        int nbTriangles = m.triangles.Length / 3;
        vts = new TriangleTextureData[nbTriangles];

        worldVertices = new Vector3[m.vertices.Length];
        for (int i = 0; i < m.vertices.Length; i++) {
            worldVertices[i] = this.transform.TransformPoint(m.vertices[i]);
        }

        for (int t=0; t<nbTriangles; t++) {
            TriangleTextureData tex = new TriangleTextureData();
            tex.center = getCenter(t);
            vts[t] = tex;
        }
    }

    Vector3 getCenter(int t) {
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
        Mesh m = getMesh();

        Vector3 a = uvs[m.triangles[numtriangle * 3 + 0]];
        Vector3 b = uvs[m.triangles[numtriangle * 3 + 1]];
        Vector3 c = uvs[m.triangles[numtriangle * 3 + 2]];

        return OutOfViewportUV(a) || OutOfViewportUV(b) || OutOfViewportUV(c);
    }

    float getAngle(int t, Camera camera) {
        Mesh m = getMesh();

        Vector3 a = worldVertices[m.triangles[t * 3 + 0]];
        Vector3 b = worldVertices[m.triangles[t * 3 + 1]];
        Vector3 c = worldVertices[m.triangles[t * 3 + 2]];

        Plane plane = new Plane(a, b, c);
        Vector3 norm = plane.normal;

        Vector3 dirProjector = camera.transform.forward;
        float angle = Vector3.Angle(norm, -dirProjector);

        if (debug) {
            Debug.Log("normal:" + norm);
            Math3DUtils.CreateSphere(a, Color.gray);
            Debug.DrawRay(a, norm, Color.white, 100);
            Debug.DrawRay(a, -dirProjector, Color.black, 100);
            Debug.Log("dir:" + this.transform.forward);
            Debug.Log("Angle:" + angle); // mod 90
        }

        return angle;
    }

    public void CalculateUV(List<Camera> cameras) {
        Init();
        foreach (Camera camera in cameras)
            this.CalculateUV(camera); //for each projection
    }

    protected void CalculateUV(Camera camera) {
        Mesh m = this.GetComponent<MeshFilter>().mesh;

        // precalculate all uv (quicker to calculate worldToViewPoint or raycast?)
        Vector2[] uvs = new Vector2[m.vertices.Length];
        for(int i = 0; i<m.vertices.Length; i++) {
            Vector2 uv = camera.WorldToViewportPoint(this.worldVertices[i]); //fov must be properly set
            uvs[i] = uv;
            
            /*if (true || debug) {
                Vector3 projected = camera.GetComponent<DrawProjector>().ProjectOnPlaneViewport(uv);
                GameObject pgo = Math3DUtils.CreateSphere(projected, Color.cyan);
                pgo.transform.rotation = this.transform.rotation;
                pgo.name = "Projected";
            }*/
        }


        for (int t = 0; t < m.triangles.Length / 3; t++) {
            TriangleTextureData vt = vts[t];

            bool outOfView = this.OutOfViewportTriangle(uvs, t);
            bool visible = isVisible(vt.center, camera.transform);

            if(!outOfView && visible) {
                Vector2 a = uvs[m.triangles[t * 3 + 0]];
                Vector3 b = uvs[m.triangles[t * 3 + 1]];
                Vector3 c = uvs[m.triangles[t * 3 + 2]];

                float curAngle = this.getAngle(t, camera);

                if (vt.uvs3 == null || Math.Abs(curAngle % 90) < Math.Abs(vt.angle % 90)) { //not set OR new angle is better / smaller
                    vt.uvs3 = new Vector2[] { a, b, c };
                    vt.photo = camera.GetComponent<DrawProjector>().fn;
                    vt.distance = Vector3.Distance(camera.transform.position, vt.center);
                    vt.angle = curAngle;
                    //Debug.Log("angle: " + vt.angle);
                }
            }
        }
    }

    public static bool OutOfViewportUV(Vector2 uv) {
        return uv.x < 0.0f || uv.x > 1.0f || uv.y < 0.0f || uv.y > 1.0f;
    }
}
