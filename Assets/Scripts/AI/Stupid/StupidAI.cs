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

    [Header("Pathfinding")]
    public StupidSearchMode searchMode;
    private int maxLoops = 10000;

    [Header("Bezier path")]
    public Color pathColor = Color.cyan;
    public float visibleRange;
    [Range(1, 16)]
    public int smoothingPasses;

    private BaseAI baseAI;
    private Vector3[] nodes;
    private Vector3 targetPos;
    private int lastLap;


    private void Start()
    {
        baseAI = GetComponent<BaseAI>();

        //generate smoothed path
        ReconstructPath();
    }

    //Simple A* to convert the node system to a vector3[]
    private void ReconstructPath()
    {
        Node startNode = baseAI.GetFirstNode();

        //start sets
        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        //add start to the open set
        openSet.Add(startNode);

        int loops = 0;
        while(openSet.Count > 0)
        {
            //sort
            switch (searchMode)
            {
                case StupidSearchMode.BestFirst:
                    openSet = openSet.OrderBy(r => r.cost).ToList();
                    break;

                case StupidSearchMode.WorstFirst:
                    openSet = openSet.OrderByDescending(r => r.cost).ToList();
                    break;

                case StupidSearchMode.Random:
                    openSet = openSet.OrderBy(r => Random.value).ToList();
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
                    neighbour.cost = newMovementCostToNeighbour;
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }

            //ensure we don't fail
            loops++;
            if(loops > maxLoops)
            {
                Debug.LogWarning("Exceeded max loops! Aborting...");
                return;
            }
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
            baseAI.UseItem();
        }

        //regenerate path each lap
        if(baseAI.lap != lastLap)
        {
            lastLap = baseAI.lap;
            ReconstructPath();
        }
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
    }

    public Vector3[] Chaikin(Vector3[] pts)
    {
        Vector3[] newPts = new Vector3[(pts.Length - 2) * 2 + 2];
        newPts[0] = pts[0];
        newPts[newPts.Length - 1] = pts[pts.Length - 1];

        int j = 1;
        for (int i = 0; i < pts.Length - 2; i++)
        {
            newPts[j] = pts[i] + (pts[i + 1] - pts[i]) * 0.75f;
            newPts[j + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * 0.25f;
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

public enum StupidSearchMode
{
    BestFirst,
    WorstFirst,
    Random
}