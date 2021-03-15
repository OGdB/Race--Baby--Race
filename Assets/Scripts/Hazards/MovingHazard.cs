using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingHazard : MonoBehaviour
{
    public float speed = 2f;
    public float xdelta = 2f;
    public float zdelta = 2f;
    public float ydelta = 2f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        Vector3 v = startPos;
        v.x += xdelta * Mathf.Sin(Time.time * speed);
        v.z += zdelta * Mathf.Sin(Time.time * speed);
        v.y += ydelta * Mathf.Sin(Time.time * speed);
        transform.position = v;
    }
}
