using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastTest : MonoBehaviour
{
    private BaseAI baseAI;

    [Header("Obstacle Prevention")]
    private BoxCollider collider;
    [SerializeField] private bool avoidingObstacle = false;
    [SerializeField] private float obstaclePreventionMultiplier = 0f;

    private void Start()
    {
        collider = GetComponent<BoxCollider>();
        baseAI = GetComponent<BaseAI>();
    }
    void Update()
    {
        ObstacleSensor();
    }

    private void ObstacleSensor()
    {
        RaycastHit hit;
        int layerMask = 1 << 15;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 5f, layerMask))
        {
            print("obstacle!");
        }
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.yellow);
    }
}
