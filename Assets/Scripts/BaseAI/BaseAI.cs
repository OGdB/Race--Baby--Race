using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BaseAI : MonoBehaviour
{
    //serialized variables
    [Header("Locomotion")]
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float speedAcceleration;

    [SerializeField]
    private float maxSteeringAngle;
    [SerializeField]
    private float steeringAcceleration;

    [SerializeField]
    private Vector2 direction;
        // direction.x = steering;
        // direction.y = speed;

    [Header("Pathfinding")]
    public Transform path;

    //fully private
    private float currentSpeed;
    private float currentSteeringAngle;
    private Vector3 velocity;

    //references
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //set speed based on input (input = direction.y);
        currentSpeed = Mathf.Lerp(currentSpeed, Mathf.Clamp(direction.y, -1f, 1f) * maxSpeed, speedAcceleration * Time.fixedDeltaTime);

        //add speed to velocity (we don't actually modify the rigidbody's velocity here so we can set the x and y to 0)
        velocity = new Vector3(0, 0, currentSpeed * Time.fixedDeltaTime);

        //change current steering angle based on input
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * direction.x, steeringAcceleration * Time.fixedDeltaTime);

        //move
        rb.MovePosition(transform.position + (rb.rotation * velocity) * Time.fixedDeltaTime);

        //steer
        float actualSteeringAngle = Mathf.Lerp(0, currentSteeringAngle, Mathf.Abs(direction.y)); //only steer when driving
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, actualSteeringAngle * Time.fixedDeltaTime, 0));
    }

    public void SetDirection(Vector2 newDirection)
    {
        //we need to make sure that the new directions don't exeed a magnitude of 1 per axis...
        //otherwise you can exeed max speed and steering angles
        direction = new Vector2(
            Mathf.Clamp(newDirection.x, -1, 1),
            Mathf.Clamp(newDirection.y, -1, 1)
            );
    }

    public Vector3[] GetNodes()
    {
        Vector3[] nodes = new Vector3[path.childCount];

        for(int n = 0; n < nodes.Length; n++)
        {
            nodes[n] = path.GetChild(n).position;
        }

        return nodes;
    }
}