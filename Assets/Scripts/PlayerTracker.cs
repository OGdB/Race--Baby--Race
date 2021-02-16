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

    public Collider[] environment;
    public float environmentRadius = 5f;
    public float objectAvoidance = 1f;
    public LayerMask environmentMask;
    // Start is called before the first frame update
    void Start()
    {
        //players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        //// Runs through all the players and finds their camera component tagged MainCamera
        //foreach (GameObject player in players)
        //{
        //    foreach (Transform child in transform)
        //    {
        //        if (child.tag == "MainCamera")
        //            cameras.Add(child.gameObject);
        //    }
        //}
        environmentMask = LayerMask.GetMask("Environment", "Road");
        ahead = new GameObject("ahead");
    }

    // LateUpdate is called once per frame
    void FixedUpdate()
    {
        
        // Changes the position of where the player is headed
        ahead.transform.position = trackedObject.position + trackedObject.forward * (maxDistance * 0.25f);
        // Limits the value of currentDistance so it can't be higher than maxDistance
        currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);
        
        Vector3 destination = trackedObject.position + Vector3.up * currentDistance - trackedObject.forward * (currentDistance + maxDistance * 0.5f);
        // Find all colliders in environmentRadius from the camera (that match the mask)
        environment = Physics.OverlapSphere(transform.position, environmentRadius, environmentMask);
        // Run through the environment colliders
        foreach (Collider wall in environment)
        {
            Vector3 distance = destination - wall.bounds.ClosestPoint(transform.position);
            // If the destination is within objectAvoidance radius of the closest point on an obstacle
            if (distance.magnitude < objectAvoidance)
            {
                // Push destination away so it distance.magnitude equals objectAvoidance
                Vector3 push = distance.normalized * objectAvoidance - distance;
                destination += push;
                Debug.Log("Avoided " + wall.name);
            }
        }
        // Actually moves the camera
        transform.position = Vector3.MoveTowards(transform.position,
                                                 destination,
                                                 updateSpeed * Time.fixedDeltaTime);
        //environment = Physics.OverlapSphere(transform.position, environmentRadius, environmentMask);
        //environmentPush = new Vector3(0, 0, 0);
        //foreach (Collider wall in environment)
        //{
        //    Vector3 push = transform.position - wall.bounds.ClosestPoint(transform.position);
        //    if (push.magnitude < environmentDistance)
        //    {
        //        push = push.normalized * Mathf.Sqrt(push.magnitude) * objectAvoidance;
        //        environmentPush += push;
        //        Debug.Log("Pushed away camera by " + environmentPush);
        //    }
        //}
        transform.LookAt(ahead.transform);
    }
}
