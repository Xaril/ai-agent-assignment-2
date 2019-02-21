using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class FollowPoint : MonoBehaviour
{
    //Circle
    Vector3 center = new Vector3(100, 0, 100);
    float r = 25;
    Vector3 radius;
    float t;

    //Square
    float v = 8;

    // Start is called before the first frame update
    void Start()
    {
        radius = new Vector3(0, 0, r);
        center = GameObject.FindWithTag("Player").transform.position;
        transform.position = center + radius;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //DriveInCircle();
        DriveControls();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 2.5f);
    }

    private void DriveControls()
    {
        float x = CrossPlatformInputManager.GetAxis("Horizontal");
        float z = CrossPlatformInputManager.GetAxis("Vertical");
        if(Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
            transform.position += new Vector3(v*Mathf.Cos(Mathf.Atan2(z, x)), 0, v*Mathf.Sin(Mathf.Atan2(z, x)))*Time.deltaTime;
    }

    private void DriveInCircle()
    {
        t += Time.deltaTime / 3;
        if (t >= 2 * Mathf.PI)
        {
            t = 0;
        }
        radius = new Vector3(r * Mathf.Sin(t), 0, r * Mathf.Cos(t));
        transform.position = center + radius;
    }
}
