using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathReferencePoint : MonoBehaviour
{
    public float? maxSpeed;
    public int pathIndex;
    public GameObject obj;
    public bool isActive;
    public PathReferencePoint(GameObject obj, int pathIndex)
    {
        this.obj = obj;
        this.pathIndex = pathIndex;
        this.isActive = true;
    }
}
