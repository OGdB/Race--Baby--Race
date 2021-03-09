using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* TO-DO
 * If the next normalized vector is right/left => smooth curve. else => no smoothcurve
 * hopefully helps to keep the calculated route on the road.
 */
public class Pathing : MonoBehaviour
{
    private BaseAI baseAI;
    [SerializeField]
    private Vector3[] shortestPath;
    private Vector3[] smoothPath;

    private Vector3 forwardDirection;

    [SerializeField]
    List<Vector3> curving = new List<Vector3>();

    [SerializeField]
    private int directionInt = 0;
    [SerializeField]
    private int curveSmoothness = 1;
  
    // There are 5 branching paths
    private void Start()
    {
        baseAI = GetComponent<BaseAI>();

        shortestPath = FindShortestPath(baseAI.GetFirstNode());
    }

    /* TO-DO
     * Actually calculate the shortest path, only gets the first path now.
     */
    private Vector3[] FindShortestPath(Node firstNode)
    {
        List<Vector3> shortestPathList = new List<Vector3>();
        shortestPathList.Add(firstNode.transform.position);

        Node checkingNode = firstNode.nextNodes[0];

        // starting from the first node, get the nextNode, and continue to access the nextNodes.
        while (checkingNode != firstNode)
        {
            shortestPathList.Add(checkingNode.transform.position);
            // Eventually, the nextnode should be the checkingnode == end of loop.
            checkingNode = checkingNode.nextNodes[0];
        }

        Vector3[] shortestPathArray = shortestPathList.ToArray();
        return shortestPathArray;
    }

        /* SUMMARY
         * 
         */
    private void GetCurveDir(Vector3 firstNode, Vector3 secondNode)
    {
        Vector3 relativePos = secondNode - firstNode;
        Vector3 relativeDir = relativePos.normalized;

        if (relativeDir.x < 0)
            print("right");
        if (relativeDir.x > 0)
            print("left");
    }

    private void OnDrawGizmos()
    {
        // Normal path visualization
        PathVisualization(shortestPath, Color.blue);
    }
    private void PathVisualization(Vector3[] path, Color pathColor)
    {
        if (path != null && path.Length > 0)
        {
            Gizmos.color = pathColor;

            foreach (Vector3 point in path)
            {
                Gizmos.DrawSphere(point, 0.5f);
            }
            Gizmos.DrawLine(path[directionInt], path[directionInt] + (forwardDirection * Vector3.Distance(path[directionInt], path[directionInt + 1])));
            Gizmos.DrawSphere(path[directionInt], 0.5f);
        }
    }

    private void OnValidate()
    {
        if ((shortestPath != null && shortestPath.Length > 0))
        {

        }
    }
}
