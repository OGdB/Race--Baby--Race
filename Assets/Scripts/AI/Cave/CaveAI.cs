using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveAI : MonoBehaviour
{
    public bool debug = false;

    private BaseAI baseAI;

    [Header("Pathfinding")]
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
    [Tooltip("Where the path splits")]
    public Node[] pathSplits;
    [Tooltip("Which direction to take when the path splits")]
    public int[] splitDirections;



    [Header("Movement")]
    public float maxSpeed = 1.0f;
    public float speedFallOffDistance = 40.0f;
    [Range(0.0f, 1.0f)]
    public float speedFallOffAmount;
    public speedFallOffMethods speedFallOffMethod;

    [Header("Raycasting")]
    [Range(0.1f, 5.0f), Tooltip("How close each angle should be, lower values for higher precision")]
    public float precision;
    [Range(5.0f, 180.0f), Tooltip("How wide should the cone be when checking the nearest node")]
    public float coneSpread;
    [Range(10f, 200f), Tooltip("How far rays should be cast")]
    public float rayDistance;
    [Tooltip("Which layers should be checked for walls & pathing")]
    public LayerMask walls;
    [MinAttribute(1), Tooltip("How many rays away from the wall should we navigate")]
    public int rayOffset = 1;

    [Header("Looks")]
    // Items are aesthetic and I'm treating them that way too.
    private Item currentItem;
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
        //currentDestination = FindLongest(FindFurthestNode().transform.position);
        currentDestination = FindNthLongest(FindFurthestNode().transform.position, rayOffset);
        SteerTowards(currentDestination, maxSpeed);
        baseAI.GetDirection();
        LookCuteWithCone();
    }

    public void FindCurrentNode()
    {
        // Increases to the next node if near a node
        if (Vector3.Distance(transform.position, currentNode.transform.position) < confirmationDistance)
        {
            int path = 0;
            // Checks to find the right path
            for (int i = 0; i < pathSplits.Length; i++)
            {
                if(currentNode == pathSplits[i])
                {
                    path = splitDirections[i];
                }
            }
            currentNode = currentNode.nextNodes[path];
        }

        // Finds the nearest node and sets it as the current node if there is no clear line to the destination
        if (Physics.Linecast(transform.position, currentNode.transform.position, walls))
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
            int path = 0;
            // Checks to find the right path
            for (int j = 0; j < pathSplits.Length; j++)
            {
                if (checkNode == pathSplits[j])
                {
                    path = splitDirections[j];
                }
            }
            checkNode = checkNode.nextNodes[path];

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
        float distanceToTarget = Vector3.Distance(direction, transform.position);
        if (distanceToTarget < speedFallOffDistance)
        {
            Debug.Log("Slowing down");
            switch(speedFallOffMethod)
            {
                case speedFallOffMethods.Linear:
                    speed = speed * distanceToTarget / speedFallOffDistance * speedFallOffAmount;
                    break;
                case speedFallOffMethods.Multiplier:
                default:
                    speed = speed * speedFallOffAmount;
                    break;
                
            }
            
        }
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

    public Vector3 FindNthLongest(Vector3 coneCenter, int n)
    {
        Vector3 baseDirection = (coneCenter - transform.position).normalized;
        Vector2[] outputDirections = new Vector2[n];
        List<Vector3> debugRayLines = new List<Vector3>();
        Vector3[] outputDestinations = new Vector3[n];

        for (float i = 0.0f; i < coneSpread * 2; i += precision)
        {
            Vector3 direction = Quaternion.AngleAxis(i - coneSpread, Vector3.up) * baseDirection;
            RaycastHit hit;
            Physics.Raycast(transform.position,
                            direction,
                            out hit,
                            rayDistance,
                            walls);
            
            // Loops through all the possible outputDestinations
            for (int j = n - 1; j >= 0; j--)
            {
                
                if (hit.distance > Vector3.Distance(outputDestinations[j], transform.position)
                    || outputDestinations[j] == Vector3.zero)
                {
                    // If this distance is larger than the next checked instance (or its the highest anyway), loop to that iteration
                    if (j != 0 
                        && (hit.distance > Vector3.Distance(outputDestinations[j - 1], transform.position) 
                           || outputDestinations[j - 1] == Vector3.zero))
                    {
                        // Moves the checked destination down by one
                        outputDestinations[j] = outputDestinations[j - 1];
                        if(debug)
                        {
                            outputDirections[j] = outputDirections[j - 1];
                        }
                        continue;
                    }
                    else
                    {
                        outputDestinations[j] = hit.point;
                        
                        if(debug)
                        {
                            outputDirections[j] = direction;
                        }
                    }
                }
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
                foreach (Vector2 outputDirection in outputDirections)
                {
                    if ((Vector2)ray == outputDirections[n - 1])
                    {
                        Debug.DrawRay(transform.position, ray * rayDistance, Color.red);
                    }
                    else if ((Vector2)ray == outputDirection)
                    {
                        Debug.DrawRay(transform.position, ray * rayDistance, Color.yellow);
                    }
                    else
                    {
                        Debug.DrawRay(transform.position, ray * rayDistance, Color.cyan);
                    }
                }
            }
        }
        return outputDestinations[n - 1];
    }

    public void LookCuteWithCone()
    {
        currentItem = baseAI.GetCurrentItem();
        if(currentItem == Item.Grenade)
        {
            baseAI.AimBack(true);
            baseAI.UseItem();
        }
        else
        {
            baseAI.AimBack(false);
        }
    }
}

public enum speedFallOffMethods
{
    Multiplier,
    Quadratic,
    Linear
}