using UnityEngine;

public class DebugProjector : MonoBehaviour
{
    public GameObject photo; //where to display the photo
    public GameObject spherePrefab;
    public GameObject cube; //object I want to project

    int w = 4618;
    int h = 3464;
    int vfov = 60;
    int far = 1;

    GameObject o;
    int frame = 0;

    void Start()
    {
        float hfov = getHorizontalFov();
        Debug.Log("hfov:"+hfov);

        var halfw = getHalfWidth();
        var halfh = getHalfHeight();

        var ne = new Vector3(halfw, halfh, far);
        var se = new Vector3(halfw, halfh * -1, far);
        var sw = new Vector3(halfw * -1, halfh * -1, far);
        var nw = new Vector3(halfw * -1, halfh, far);

        Debug.DrawLine(this.transform.position, this.transform.position + ne, Color.white, 99f);
        Debug.DrawLine(this.transform.position, this.transform.position + se, Color.white, 99f);
        Debug.DrawLine(this.transform.position, this.transform.position + sw, Color.white, 99f);
        Debug.DrawLine(this.transform.position, this.transform.position + nw, Color.white, 99f);

        

        //Debug.DrawLine(this.transform.position, this.transform.position + new Vector3(0,0, far), Color.black, 99f);

        photo.transform.position = new Vector3(0, 0, 1);
        //photo.transform.localScale = new Vector3(halfw * 2 * 0.1f, halfh * 2 * 0.1f, 0.1f);

        Instantiate(spherePrefab, ne, Quaternion.identity);
        Instantiate(spherePrefab, se, Quaternion.identity);
        Instantiate(spherePrefab, sw, Quaternion.identity);
        Instantiate(spherePrefab, nw, Quaternion.identity);

        //https://forum.unity.com/threads/solved-image-projection-shader.254196/



        Mesh m = cube.GetComponent<MeshFilter>().mesh;
        Vector3 localVertex = m.vertices[2]; //1 behind
     
        Vector3 worldVertex = cube.transform.TransformPoint(localVertex); //idem cube.transform.localToWorldMatrix.MultiplyPoint3x4(localVertex);
        o = InstSphere(worldVertex, Color.yellow);
        

        //projected on camera
        //Vector3 p = project(worldVertex, 1.0f);
        //InstSphere(p, Color.blue);
    }

    GameObject InstSphere(Vector3 vector3, Color color) {
        GameObject o = Instantiate(spherePrefab, vector3, Quaternion.identity);
        o.GetComponent<Renderer>().material.color = color;
        return o;
    }

    // Update is called once per frame
    void Update() // Camera.main.WorldToViewportPoint(o.transform.position)
    {
        if (frame < 10) {
            Debug.Log("Frame: " + frame + " visible:"+ o.GetComponent<Renderer>().isVisible);
            frame++;
        } else if(frame == 10) {
            Debug.Log("o.isVisible:" + o.GetComponent<Renderer>().isVisible);
            frame++;
        }
    }

    float getHorizontalFov() {
        var aspect = this.w *1.0f / this.h;
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
