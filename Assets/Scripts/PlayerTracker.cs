using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    List<GameObject> cameras = new List<GameObject>();
    List<GameObject> players = new List<GameObject>();

    public Transform trackedObject;
    public float maxDistance = 10;
    public float updateSpeed = 10;
    [Range(0, 10)]
    public float currentDistance = 5;
    private GameObject ahead;
    public float hideDistance = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        ahead = new GameObject("ahead");
    }

    // LateUpdate is called once per frame
    void FixedUpdate()
    {
        
        // Changes the position of where the player is headed
        ahead.transform.position = trackedObject.position + trackedObject.forward * (maxDistance * 0.25f);
        // Limits the value of currentDistance so it can't be higher than maxDistance
        currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);
        // Calculates destination by a mix of the tracked object, uses currentDistance to increase height, and moves the camera back by maxDistance + currentDistance
        Vector3 destination = trackedObject.position + Vector3.up * currentDistance - trackedObject.forward * (currentDistance + maxDistance * 0.5f);
        // Actually moves the camera
        transform.position = Vector3.MoveTowards(transform.position,
                                                 destination,
                                                 updateSpeed * Time.fixedDeltaTime);
        // Looks at where the player is going
        transform.LookAt(ahead.transform);
    }
}
