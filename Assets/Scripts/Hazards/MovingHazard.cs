using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingHazard : MonoBehaviour
{

    public float xdelta = 2f;
    public float xspeed = 2f;
    public float zdelta = 2f;
    public float zspeed = 2f;
    public float ydelta = 2f;
    public float yspeed = 2f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        Vector3 v = startPos;
        v.x += xdelta * Mathf.Sin(Time.time * xspeed);
        v.z += zdelta * Mathf.Sin(Time.time * zspeed);
        v.y += ydelta * Mathf.Sin(Time.time * yspeed);
        transform.position = v;
    }
}
