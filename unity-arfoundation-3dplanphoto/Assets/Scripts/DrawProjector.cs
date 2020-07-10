using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DrawProjector : MonoBehaviour
{
    public GameObject plane; //where to display the photo

    public GameObject cube;

    private int w;
    private int h;
    int vfov = 60;
    float far = 1f; //how far should we place the projector for the projected plane

    public string fn = null;

    // Draw frame and put it 1m far from camera
    void Start() {
        this.name = "Projector " + fn;

        //load texture
        if (!String.IsNullOrEmpty(fn)) { //otherwise display already loaded texture projectcube.jpg
            string path = UnityEngine.Application.persistentDataPath + "/" + fn;
            
            if (!System.IO.File.Exists(path)) {
                Debug.LogError("file not found: " + path);
            }
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(fileData); //load also width/height
            plane.GetComponent<Renderer>().material.mainTexture = tex;

            //project texture
            this.GetComponent<Projector>().material = new Material(Shader.Find("Projector/Multiply"));
            Material thisProjectorMat = this.GetComponent<Projector>().material;
            thisProjectorMat.SetTexture("_ShadowTex", tex);
            thisProjectorMat.SetTexture("_FalloffTex", tex);
            this.GetComponent<Projector>().material = thisProjectorMat;

            w = tex.width;
            h = tex.height;
        } else {
            w = 4618;
            h = 3464;
        }
        
        Debug.Log("w:" + w + " h:" + h);
        this.GetComponent<Camera>().aspect = this.GetComponent<Projector>().aspectRatio = (float)w / h;

        float hfov = getHorizontalFov();
        Debug.Log("hfov:" + hfov);

        var halfw = getHalfWidth();
        var halfh = getHalfHeight();

        Vector3 centerplane = this.transform.position + this.transform.forward * far; //an alternative is to use go.transform.Translate
        plane.transform.position = centerplane;
        plane.transform.localScale = new Vector3(halfw * 2 * 0.1f, 0.1f, halfh * 2 * 0.1f);

        Vector3 ne = ProjectOnPlaneViewport(new Vector2(1, 1));
        Vector3 se = ProjectOnPlaneViewport(new Vector2(1, 0));
        Vector3 sw = ProjectOnPlaneViewport(new Vector2(0, 0));
        Vector3 nw = ProjectOnPlaneViewport(new Vector2(0, 1));

        Debug.DrawLine(this.transform.position, centerplane, Color.blue, 99f);
        Debug.DrawLine(this.transform.position, ne, Color.white, 99f);
        Debug.DrawLine(this.transform.position, se, Color.white, 99f);
        Debug.DrawLine(this.transform.position,sw, Color.white, 99f);
        Debug.DrawLine(this.transform.position, nw, Color.white, 99f);       

        Math3DUtils.CreateSphere(ne, Color.red).transform.parent = this.transform;
        Math3DUtils.CreateSphere(se, Color.red).transform.parent = this.transform;
        Math3DUtils.CreateSphere(sw, Color.red).transform.parent = this.transform;
        Math3DUtils.CreateSphere(nw, Color.red).transform.parent = this.transform;

        //https://forum.unity.com/threads/solved-image-projection-shader.254196/
    }

    Vector3 ProjectOnPlaneCenter(Vector2 uv) { //center is (0,0)
        Vector3 centerplane = plane.transform.position; //calculated in Start()
        var halfw = getHalfWidth();
        var halfh = getHalfHeight();
        Vector3 right = this.transform.right * uv.x * halfw;
        Vector3 up = this.transform.up * uv.y * halfh;
        return centerplane + right + up;
    }

    public Vector3 ProjectOnPlaneViewport(Vector3 uv) { //center is (0.5, 0.5)
        Vector3 centerplane = plane.transform.position; //calculated in Start()
        var halfw = getHalfWidth();
        var halfh = getHalfHeight();
        Vector3 right = this.transform.right * (uv.x - 0.5f) * 2 * halfw;
        Vector3 up = this.transform.up * (uv.y - 0.5f) * 2 * halfh;
        return centerplane + right + up;
    }

    float getHorizontalFov() {
        var aspect = this.w * 1.0f / this.h;
        var vFovRad = this.vfov * Mathf.Deg2Rad;
        var radHFOV = 2 * Mathf.Atan(Mathf.Tan(vFovRad / 2) * aspect);
        var hFOV = Mathf.Rad2Deg * radHFOV;
        return hFOV;
    }

    float getHalfWidth() {
        // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
        var hFovDeg = this.getHorizontalFov();
        return 1.0f * this.far * Mathf.Tan(Mathf.Deg2Rad * (hFovDeg / 2));
    }

    float getWidth() {
        return this.getHalfWidth() * 2;
    }

    float getHalfHeight() {
        return this.far * Mathf.Tan(Mathf.Deg2Rad * (this.vfov / 2));
    }

    float getHeight() {
        return this.getHalfHeight() * 2;
    }

    public Vector3 projectSimple(Vector3 v, float z) {
        float y2 = (z * v.y) / v.z;
        float x2 = (z * v.x) / v.z;
        return new Vector3(x2, y2, z);
    }
}
