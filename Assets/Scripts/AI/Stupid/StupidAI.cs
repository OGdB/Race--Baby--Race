using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(BaseAI))]
public class StupidAI : MonoBehaviour
{
    [Header("Speed")]
    public AnimationCurve sMagToSpeedCurve;
    public bool sendIt;

    [Header("Steering")]
    public bool normalizeSteeringDir;
    public StupidSteerMode steerMode;
    public AnimationCurve steeringCurve;
    public bool useCurve;
    public Vector3 steerProjectionOffset;
    public int nearestNodeOffset;
    public int sampleSize;
    public int spread;

    [Header("Non-navigational steering")]
    public Vector3 navRaycastOffset;
    public Vector3[] pushRaycasts;
    public LayerMask pushLayermask;
    public float pushAmount;
    public bool normalizePush;
    public Color pushColor = Color.blue;

    public Vector3[] pullRaycasts;
    public LayerMask pullLayermask;
    public float pullAmount;
    public bool normalizePull;
    public Color pullColor = Color.magenta;

    [Header("Items")]
    public Vector3 itemRaycastOffset;
    public Vector3[] itemRaycasts;
    public LayerMask itemLayermask;
    public Color itemColor = Color.green;

    [Header("Pathfinding")]
    public StupidPathfindingMode pathfindingMode;
    public StupidSearchMode searchMode;
    private int maxLoops = 10000;

    [Header("Bezier path")]
    public Color pathColor = Color.cyan;
    public float visibleRange;
    [Range(1, 16)]
    public int smoothingPasses;
    [Range(0f, 1f)]
    public float cutAmt;

    [Header("Cosmetic")]
    public string carName;
    public float nameHueSpeed;
    public float hueLetterSpace;
    private float currentNameHue;

    [Header("Debug")]
    [SerializeField, Range(-1f, 1f)]
    private float steeringDir;
    [SerializeField, Range(0f, 1f)]
    private float steeringMag;

    private BaseAI baseAI;
    private Vector3[] nodes;
    private Vector3 targetPos;
    private int lastLap;
    private Transform lastCheckPoint;


    private void Start()
    {
        baseAI = GetComponent<BaseAI>();
        baseAI.SetBody(1);

        //generate smoothed path
        ReconstructPath();
    }

    //Simple A* to convert the node system to a vector3[]
    private void ReconstructPath()
    {
        Node startNode = baseAI.GetFirstNode();

        switch (pathfindingMode)
        {
            case StupidPathfindingMode.Greedy:
                {
                    //start sets
                    List<Node> openSet = new List<Node>();
                    List<Node> closedSet = new List<Node>();

                    //add start to the open set
                    openSet.Add(startNode);

                    int loops = 0;
                    while (openSet.Count > 0)
                    {
                        //sort
                        switch (searchMode)
                        {
                            case StupidSearchMode.Random:
                                openSet = openSet.OrderBy(r => Random.value).ToList();
                                break;

                            default:
                                openSet = openSet.OrderBy(r => r.cost).ToList();
                                break;
                        }

                        //get current node and make closed
                        Node currentNode = openSet[0];
                        openSet.Remove(currentNode);
                        closedSet.Add(currentNode);

                        //We've found the path!!! Now retrace it.
                        if (currentNode == startNode && loops > 0)
                        {
                            List<Vector3> newPath = new List<Vector3>();
                            newPath.Add(currentNode.transform.position);

                            Node currentRetraceNode = currentNode.parent;

                            while (currentRetraceNode != startNode)
                            {
                                newPath.Add(currentRetraceNode.transform.position);
                                currentRetraceNode = currentRetraceNode.parent;
                            }

                            newPath.Reverse();
                            nodes = newPath.ToArray();
                            break;
                        }

                        //scroll through all neighbouring nodes
                        for (int n = 0; n < currentNode.nextNodes.Length; n++)
                        {
                            Node neighbour = currentNode.nextNodes[n];

                            //if we've seen it, ignore it.
                            if ((closedSet.Contains(neighbour) && neighbour != startNode) || neighbour == null)
                            {
                                continue;
                            }

                            //calculate the gcost from the current node to the neighbour + the current nodes gcost
                            float newMovementCostToNeighbour = currentNode.cost + Vector3.Distance(currentNode.transform.position, neighbour.transform.position);

                            //if the open set doesn't yet contain the neighbour then use it (or if newmovement gcost is less than the neighbours gcost
                            if (newMovementCostToNeighbour < neighbour.cost || !openSet.Contains(neighbour))
                            {
                                switch (searchMode)
                                {
                                    case StupidSearchMode.WorstFirst:
                                        neighbour.cost = -newMovementCostToNeighbour;
                                        break;

                                    case StupidSearchMode.BestFirstVertical:
                                        neighbour.cost = newMovementCostToNeighbour - Vector3.Distance(new Vector3(0, currentNode.transform.position.y, 0), new Vector3(0, neighbour.transform.position.y, 0)) * 10;
                                        break;

                                    default:
                                        neighbour.cost = newMovementCostToNeighbour;
                                        break;
                                }

                                neighbour.parent = currentNode;

                                if (!openSet.Contains(neighbour))
                                {
                                    openSet.Add(neighbour);
                                }
                            }
                        }

                        //ensure we don't fail
                        loops++;
                        if (loops > maxLoops)
                        {
                            Debug.LogWarning("Exceeded max loops! Aborting...");
                            return;
                        }
                    }
                }
                break;
        }


        //apply smoothing
        for (int p = 0; p < smoothingPasses; p++)
        {
            nodes = Chaikin(nodes);
        }
    }

    private void FixedUpdate()
    {
        int targetNode = Mathf.RoundToInt(Mathf.Repeat(
            GetNearestNode(transform.position + transform.InverseTransformDirection(steerProjectionOffset)) + nearestNodeOffset,
            nodes.Length));

        targetPos = nodes[targetNode];
        for(int s = 0; s < sampleSize; s++)
        {
            targetPos += nodes[Mathf.RoundToInt(Mathf.Repeat(targetNode + s * spread, nodes.Length))];
        }
        targetPos = new Vector3(
            targetPos.x / ((float)sampleSize + 1),
            targetPos.y / ((float)sampleSize + 1),
            targetPos.z / ((float)sampleSize + 1)
            );

        //calculate direction
        Vector3 dir = transform.InverseTransformDirection((targetPos - transform.position));

        //non-directional steering
        Vector3 pushSteer = Vector3.zero;
        foreach (Vector3 push in pushRaycasts)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + (transform.rotation * navRaycastOffset), (transform.rotation * push), out hit, push.magnitude, pushLayermask, QueryTriggerInteraction.Ignore))
            {
                pushSteer -= push * ((transform.position - push) - (transform.position - hit.point)).magnitude;
            }
        }

        Vector3 pullSteer = Vector3.zero;
        foreach (Vector3 pull in pullRaycasts)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + (transform.rotation * navRaycastOffset), (transform.rotation * pull), out hit, pull.magnitude, pullLayermask, QueryTriggerInteraction.Ignore))
            {
                pullSteer += pull * ((transform.position - pull) - (transform.position - hit.point)).magnitude;
            }
        }

        dir += (normalizePush ? new Vector3(pushSteer.x, 0, 0).normalized : pushSteer) / (float)pushRaycasts.Length * pushAmount + (normalizePull ? new Vector3(pullSteer.x, 0, 0).normalized : pullSteer) / (float)pullRaycasts.Length * pullAmount;

        //normalize direction
        dir = normalizeSteeringDir ? dir.normalized : new Vector3(dir.x, 0, 0).normalized;

        //steer
        float steer = dir.x;

        if (useCurve)
        {
            steer = steer * steeringCurve.Evaluate(Mathf.Abs(steer));
        }

        switch (steerMode)
        {
            case StupidSteerMode.Binary:
                steer = (steer < 0) ? -1 : 1;
                break;

            case StupidSteerMode.Half:
                steer = steer * .5f;
                break;

            case StupidSteerMode.Full:
                break;

            case StupidSteerMode.Double:
                steer = steer * 2f;
                break;

            case StupidSteerMode.Triple:
                steer = steer * 3f;
                break;

            case StupidSteerMode.Quadruple:
                steer = steer * 4f;
                break;
        }

        //calculate intended speed
        float intendedSpeed = sendIt ? 1 : sMagToSpeedCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(steer)));

        //drive
        baseAI.SetDirection(new Vector2(steer, intendedSpeed));
        
        //use item
        if (baseAI.GetCurrentItem() != Item.None)
        {
            foreach (Vector3 itemDir in pushRaycasts)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + (transform.rotation * -itemRaycastOffset), (transform.rotation * itemDir), out hit, itemDir.magnitude, itemLayermask, QueryTriggerInteraction.Ignore))
                {
                    baseAI.UseItem();
                }
            }
        }

        if (baseAI.checkpoint != lastCheckPoint)
        {
            lastCheckPoint = baseAI.checkpoint;
            if (baseAI.GetCurrentItem() != Item.None)
            {
                baseAI.UseItem();
            }
        }

        //regenerate path each lap
        if (baseAI.lap != lastLap)
        {
            lastLap = baseAI.lap;
            ReconstructPath();
        }

        //do some cosmetic stuff
        currentNameHue = Mathf.Repeat(currentNameHue + nameHueSpeed * Time.deltaTime, 1);

        string newName = "";
        for(int c = 0; c < carName.Length; c++)
        {
            Color col = Color.HSVToRGB(Mathf.Repeat(currentNameHue + hueLetterSpace * c, 1), 1, 1);
            newName += "<color=#" + ColorUtility.ToHtmlStringRGB(col) + ">" + carName[c] + "</color>";
        }

        baseAI.SetName(newName);

        //debug
        steeringDir = Mathf.Clamp(steer, -1f, 1f);
        steeringMag = Mathf.Clamp01(Mathf.Abs(steer));
    }

    private int GetNearestNode(Vector3 position)
    {
        int nearest = 0;
        float distance = float.MaxValue;

        for(int n = 0; n < nodes.Length; n++)
        {
            float thisDist = Vector3.Distance(position, nodes[n]);
            if (thisDist < distance)
            {
                distance = thisDist;
                nearest = n;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = pathColor;

            for (int n = 0; n < nodes.Length; n++)
            {
                if(Vector3.Distance(nodes[n], transform.position) < visibleRange)
                {
                    Gizmos.DrawSphere(nodes[n], 0.25f);

                    Gizmos.DrawLine(
                        nodes[n],
                        nodes[Mathf.RoundToInt(Mathf.Repeat(n + 1, nodes.Length))]
                        );
                }
            }

            Gizmos.color = new Color(1f - pathColor.r, 1f - pathColor.g, 1f - pathColor.b);

            Gizmos.DrawSphere(targetPos, 1f);
        }

        //draw push and pull steering
        Gizmos.color = pushColor;
        foreach (Vector3 push in pushRaycasts)
        {
            Gizmos.DrawRay(transform.position + navRaycastOffset, transform.rotation * push);
        }

        Gizmos.color = pullColor;
        foreach (Vector3 pull in pullRaycasts)
        {
            Gizmos.DrawRay(transform.position + navRaycastOffset, transform.rotation * pull);
        }

        if(!Application.isPlaying)
        {
            Gizmos.color = itemColor;
            foreach (Vector3 itemDir in itemRaycasts)
            {
                Gizmos.DrawRay(transform.position + (transform.rotation * itemRaycastOffset), transform.rotation * itemDir);
            }
        }
        else if(baseAI.GetCurrentItem() != Item.None)
        {
            Gizmos.color = itemColor;
            foreach (Vector3 itemDir in itemRaycasts)
            {
                Gizmos.DrawRay(transform.position + (transform.rotation * itemRaycastOffset), transform.rotation * itemDir);
            }
        }
    }

    public Vector3[] Chaikin(Vector3[] pts)
    {
        Vector3[] newPts = new Vector3[(pts.Length - 2) * 2 + 2];
        newPts[0] = pts[0];
        newPts[newPts.Length - 1] = pts[pts.Length - 1];

        int j = 1;
        for (int i = 0; i < pts.Length - 2; i++)
        {
            newPts[j] = pts[i] + (pts[i + 1] - pts[i]) * (1 - cutAmt);
            newPts[j + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * cutAmt;
            j += 2;
        }
        return newPts;
    }
}

public enum StupidSteerMode
{
    Binary,
    Half,
    Full,
    Double,
    Triple,
    Quadruple
}

public enum StupidPathfindingMode
{
    Greedy
}

public enum StupidSearchMode
{
    BestFirst,
    BestFirstVertical, //prefers vertical paths (significantly)
    WorstFirst,
    Random
}