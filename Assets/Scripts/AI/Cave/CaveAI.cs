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
    private List<Node> allNodes = new List<Node>();
    // The current node being travelled towards
    public Vector3 currentDestination;
    private float currentDistance;
    public float respawnDistance = 10.0f;
    public Node currentNode;

    [Header("Movement")]
    public float maxSpeed = 1.0f;

    [Header("Raycasting")]
    [Range(0.1f, 2.0f), Tooltip("How close each angle should be, lower values for higher precision")]
    public float precision;
    [Range(2.0f, 180.0f), Tooltip("How wide should the cone be when checking the nearest node")]
    public float coneSpread;
    [Range(1.0f, 20.0f), Tooltip("How far rays should be cast")]
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
        if (Vector3.Distance(transform.position, currentNode.transform.position) < confirmationDistance)
        {
            currentNode = currentNode.nextNodes[0];
        }
        //currentDistance = Mathf.Infinity;
        //if(Vector3.Distance)
        if (Physics.Linecast(transform.position, currentDestination, walls))
        {
            float closestDistance = Mathf.Infinity;
            Node possibleNode = new Node();
            // The car has probably died and so it can't find a line to the destination node
            foreach(Node tryNode in allNodes)
            {
                float tryDistance = Vector3.Distance(tryNode.transform.position, transform.position);
                if(tryDistance < closestDistance)
                {
                    closestDistance = tryDistance;
                    possibleNode = tryNode;
                }
            }
            currentNode = possibleNode;
        }
        currentDestination = FindFurthest().transform.position;
        SteerTowards(currentDestination, maxSpeed);
        baseAI.GetDirection();

        
    }

    public Node FindFurthest()
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
        baseAI.SetDirection(new Vector2(dir.x, 1));
    }

    //public Vector2 FindLongest()
    //{
    //    Vector2 coneCenter = currentNode
    //    for (float i = 0.0f; i < coneSpread; i += precision)
    //    {
    //        Vector2 direction;
    //        Physics.Raycast(transform.position,
    //                        new Vector3(direction.x, transform.position.y, direction.y));
    //    }
    //}
}
