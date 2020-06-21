using System;
using System.Numerics;

[Serializable]
public class Obj
{
    public const string TYPE_WALL = "Wall";
    public const string TYPE_PHOTO = "Photo";


    public string name;
    public UnityEngine.Vector3 position;
    public UnityEngine.Quaternion rotation;
    public string type;
}
