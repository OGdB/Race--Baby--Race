using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class HelloThereAI : MonoBehaviour
{
    private BaseAI baseAI;
    private Node firstNode;

    [SerializeField]
    private int smoothingStrength = 1;
    [SerializeField]
    private int forwardCalcNodesAmount = 3;
    [SerializeField]
    private List<Node> nextNodes;
    private int currentNodeInt;
    private Vector3 currentTargetNode;
    [SerializeField]    
    private Vector3[] curvedPoints;
    [SerializeField]
    private float confirmationDistance = 2f;
    private Vector3 playerPosCheck;

    void Start()
    {
        baseAI = GetComponent<BaseAI>();
        firstNode = baseAI.GetFirstNode();
        InvokeRepeating("UpdatePos", 0, 0.3f);  //1s delay, repeat every 1s

        currentTargetNode = curvedPoints[currentNodeInt];
    }

    private Vector3[] GetNextNodes(int amountOfNodes)
    {
        Node nodeCheck = firstNode;
        List<Vector3> nodeVectors = new List<Vector3>();

        // Add the upcoming nodes to a temporary list, then take the first node out of the NextNodes
        for (int i = 0; i < amountOfNodes; i++)
        {
            nextNodes.Add(nodeCheck);
            nodeVectors.Add(nodeCheck.transform.position);
            nodeCheck = nodeCheck.nextNodes[0];
        }

        return nodeVectors.ToArray();
    }

    private void Update()
    {
        // if nearing the last node in the calculated curved points
        if (Vector3.Distance(transform.position, currentTargetNode) < confirmationDistance)
        {
            currentNodeInt += 1;
            if (currentNodeInt < curvedPoints.Length) // if the current nodeInt is still within the calculated curvedPoints
            {
                currentTargetNode = curvedPoints[currentNodeInt];
            }
            else
            {
                currentNodeInt = 0;
                firstNode = nextNodes[nextNodes.Count - 1];
            }
        }

        Vector2 dir = transform.InverseTransformDirection((currentTargetNode - transform.position).normalized);
        Debug.DrawRay(transform.position, transform.TransformDirection(dir) * 3, Color.red);
        baseAI.SetDirection(new Vector2(dir.x, 1));

        if (Vector3.Distance(transform.position, playerPosCheck) > 10f)
        {
            print("AI DIED");
            Vector3 frontOfPlayer = new Vector3(transform.position.x, transform.position.y, transform.position.z + 15f);
        }
    }
    private void UpdatePos()
    {
            playerPosCheck = transform.position;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (Vector3 curvedPoint in curvedPoints)
        {
            DrawCurvedPoints(curvedPoint);
        }

        for (int i = 0; i < curvedPoints.Length; i++)
        {
            if (i + 1 < curvedPoints.Length)
            {
                foreach (Vector3 nextNode in curvedPoints)
                {
                    Gizmos.DrawLine(
                        curvedPoints[i],
                        curvedPoints[i + 1]
                        );
                }
            }
        }
    }
    private void DrawCurvedPoints(Vector3 curvedPoint)
    {
            Gizmos.DrawSphere(curvedPoint, 0.25f);
    }
}