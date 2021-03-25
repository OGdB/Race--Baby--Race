using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveAI : MonoBehaviour
{
    public bool debug = false;

    private BaseAI baseAI;

    [Header("Pathfinding")]
    [Tooltip("Should the Pelle Skip be used?")]
    public bool PelleSkip;
    [Tooltip("How many nodes should be checked in front of the current one")]
    public int nodesAhead = 3;
    [Tooltip("How close a node should be")]
    public float confirmationDistance = 5.0f;
    // Literally all the nodes in the level
    private List<Node> allNodes = new List<Node>();
    // The current node being travelled towards
    private Vector3 currentDestination;
    private float currentDistance;
    // The node being checked ahead of
    private Node currentNode;

    [Header("Movement")]
    public float maxSpeed = 1.0f;

    [Header("Raycasting")]
    [Range(0.1f, 2.0f), Tooltip("How close each angle should be, lower values for higher precision")]
    public float precision;
    [Range(2.0f, 180.0f), Tooltip("How wide should the cone be when checking the nearest node")]
    public float coneSpread;
    [Range(10f, 200f), Tooltip("How far rays should be cast")]
    public float rayDistance;
    [Tooltip("Which layers should be checked for walls & pathing")]
    public LayerMask walls;

    [Header("Looks")]
    [Tooltip("It'd better be a firetruck")]
    public CarBody carBody = CarBody.FireTruck;
    public string carName = "CaveTalesZ";

    // Start is called before the first frame update
    void Start()
    {
        baseAI = gameObject.GetComponent<BaseAI>();
        baseAI.SetName(carName);
        baseAI.SetBody(carBody);

        currentNode = baseAI.GetFirstNode();
        currentDestination = currentNode.transform.position;
        foreach(Transform child in currentNode.transform.parent)
        {
            allNodes.Add(child.GetComponent<Node>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        FindCurrentNode();
        currentDestination = FindLongest(FindFurthestNode().transform.position);
        SteerTowards(currentDestination, maxSpeed);
        baseAI.GetDirection();
    }

    public void FindCurrentNode()
    {
        // Increases to the next node if near a node
        if (Vector3.Distance(transform.position, currentNode.transform.position) < confirmationDistance)
        {
            currentNode = currentNode.nextNodes[0];
        }

        // Finds the nearest node and sets it as the current node if there is no clear line to the destination
        if (Physics.Linecast(transform.position, currentDestination, walls))
        {
            float closestDistance = Mathf.Infinity;
            Node possibleNode = null;
            // The car has probably died and so it can't find a line to the destination node
            foreach (Node tryNode in allNodes)
            {
                float tryDistance = Vector3.Distance(tryNode.transform.position, transform.position);
                if (tryDistance < closestDistance)
                {
                    closestDistance = tryDistance;
                    possibleNode = tryNode;
                }
            }
            currentNode = possibleNode;
        }
    }

    public Node FindFurthestNode()
    {
        Node checkNode = currentNode;
        Node furthestNode = checkNode;
        for (var i = 0; i < nodesAhead; i++)
        {
            checkNode = checkNode.nextNodes[0];

            if (!Physics.Linecast(transform.position, checkNode.transform.position, walls))
            {
                furthestNode = checkNode;
            }
        }
        if (debug)
        {
            Debug.DrawRay(transform.position, furthestNode.transform.position - transform.position, Color.green);
        }
        return furthestNode;
    }

    public void SteerTowards(Vector3 direction, float speed)
    {
        Vector3 dir = transform.InverseTransformDirection((direction - transform.position).normalized);
        baseAI.SetDirection(new Vector2(dir.x, speed));
    }

    public Vector3 FindLongest(Vector3 coneCenter)
    {
        Vector3 baseDirection = (coneCenter - transform.position).normalized;
        float longestDistance = 0.0f;
        Vector2 outputDirection = new Vector2();
        List<Vector3> debugRayLines = new List<Vector3>();
        Vector3 outputDestination = new Vector3();

        for (float i = 0.0f; i < coneSpread * 2; i += precision)
        {
            Vector3 direction = Quaternion.AngleAxis(i - coneSpread, transform.up) * baseDirection;
            RaycastHit hit;
            Physics.Raycast(transform.position,
                            direction, 
                            out hit,
                            rayDistance,
                            walls);
            if(hit.distance > longestDistance)
            {
                longestDistance = hit.distance;
                outputDirection = direction;
                outputDestination = hit.point;
            }
            if (debug)
            {
                debugRayLines.Add(direction);
            }
        }
        if (debug)
        {
            foreach (Vector3 ray in debugRayLines)
            {
                if ((Vector2)ray == outputDirection)
                {
                    Debug.DrawRay(transform.position, ray * rayDistance, Color.yellow);
                }
                else
                {
                    Debug.DrawRay(transform.position, ray * rayDistance, Color.cyan);
                }
            }
        }
        return outputDestination;
    }
}
