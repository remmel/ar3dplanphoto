using System;
using System.Collections;
using System.Collections.Generic;
using ToastPlugin;
using UnityEngine;
using UnityEngine.UI;

public class DrawProjector : MonoBehaviour
{
    public GameObject plane; //where to display the photo
    public GameObject spherePrefab;

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
        this.GetComponent<Projector>().aspectRatio = (float)w / h;


        float hfov = getHorizontalFov();
        Debug.Log("hfov:" + hfov);

        var halfw = getHalfWidth();
        var halfh = getHalfHeight();

        Vector3 centerplane = this.transform.position + this.transform.forward * far; //an alternative is to use go.transform.Translate
        plane.transform.position = centerplane;
        plane.transform.localScale = new Vector3(halfw * 2 * 0.1f, 0.1f, halfh * 2 * 0.1f);

        Vector3 righthalf = this.transform.right * halfw;
        Vector3 uphalf = this.transform.up * halfh;
        Vector3 ne = centerplane + righthalf + uphalf;
        Vector3 se = centerplane + righthalf - uphalf;
        Vector3 sw = centerplane - righthalf - uphalf;
        Vector3 nw = centerplane - righthalf + uphalf;       

        Debug.DrawLine(this.transform.position, centerplane, Color.blue, 99f);
        Debug.DrawLine(this.transform.position, ne, Color.white, 99f);
        Debug.DrawLine(this.transform.position, se, Color.white, 99f);
        Debug.DrawLine(this.transform.position,sw, Color.white, 99f);
        Debug.DrawLine(this.transform.position, nw, Color.white, 99f);       

        GameObject go = Instantiate(spherePrefab, ne, Quaternion.identity);
        go.transform.parent = this.transform;
        go = Instantiate(spherePrefab, se, Quaternion.identity);
        go.transform.parent = this.transform;
        go = Instantiate(spherePrefab, sw, Quaternion.identity);
        go.transform.parent = this.transform;
        go = Instantiate(spherePrefab, nw, Quaternion.identity);
        go.transform.parent = this.transform;
        
        //https://forum.unity.com/threads/solved-image-projection-shader.254196/
    }

    GameObject InstSphere(Vector3 vector3, Color color) {
        GameObject o = Instantiate(spherePrefab, vector3, Quaternion.identity);
        o.GetComponent<Renderer>().material.color = color;
        return o;
    }

    float getHorizontalFov() {
        var aspect = this.w * 1.0f / this.h;
        var vFovRad = this.vfov * Mathf.Deg2Rad;
        var radHFOV = 2 * Mathf.Atan(Mathf.Tan(vFovRad / 2) * aspect);
        var hFOV = Mathf.Rad2Deg * radHFOV;
        return hFOV;
    }

    float getHalfWidth() {
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

    Vector3 project(Vector3 v, float z) {
        float y2 = (z * v.y) / v.z;
        float x2 = (z * v.x) / v.z;
        return new Vector3(x2, y2, z);
    }
}
