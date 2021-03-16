using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class HelloThereAI : MonoBehaviour
{
    private BaseAI baseAI;
    [SerializeField]
    private Node currentTargetNode;
    [SerializeField]
    private Node upcomingTargetNode;
    [SerializeField]
    private float maxConfirmationDistance = 3f;
    private float confirmationDistance = 2f;
    [SerializeField]
    private List<Node> allNodes = new List<Node>();
    [SerializeField]
    private float waitingTime = 1f;
    private bool timerRunning = false;
    [Header("Cosmetic")]
    public int body;
    void Start()
    {
        baseAI = GetComponent<BaseAI>();
        baseAI.SetBody(body);
        currentTargetNode = baseAI.GetFirstNode();
        upcomingTargetNode = currentTargetNode.GetComponent<Node>().nextNodes[0];

        foreach (Transform child in currentTargetNode.transform.parent)
        {
            if (child.TryGetComponent(out Node node))
            {
                allNodes.Add(node);
            }
        }
    }

    private void Update()
    {
        StandardMovement();
        CheckForCol();
        Debug.DrawLine(currentTargetNode.transform.position, upcomingTargetNode.transform.position, Color.red);

        Debug.DrawLine(transform.position, currentTargetNode.transform.position, Color.blue);
        Debug.DrawLine(transform.position, upcomingTargetNode.transform.position, Color.yellow);

    }

    private void StandardMovement()
    {
        // if nearing the last node in the calculated curved points
/*        if (Vector3.Distance(transform.position, currentTargetNode.transform.position) < confirmationDistance)
        {
            if (currentTargetNode.nextNodes.Length > 1)
            {
                int randomPath = Random.Range(0, currentTargetNode.nextNodes.Length);
                currentTargetNode = currentTargetNode.nextNodes[randomPath];
                upcomingTargetNode = currentTargetNode.nextNodes[0];
            }
            else
            {
                currentTargetNode = currentTargetNode.nextNodes[0];
                upcomingTargetNode = currentTargetNode.nextNodes[0];
            }
        }
*/
        // If the car can draw a line between itself and the upcoming node > Set that node as the current node
        if (LineCastNextTarget(upcomingTargetNode.transform.position, 1 << 13))
        {
            if (!timerRunning)
            {
                StartCoroutine(WaitForTurn());
            }
        }

        Vector3 dir = transform.InverseTransformDirection((currentTargetNode.transform.position - transform.position).normalized);
        Debug.DrawRay(transform.position, transform.TransformDirection(dir) * 3, Color.red);
        baseAI.SetDirection(new Vector2(dir.x, 1));
    }
    private void CheckForCol()
    {
        if (!LineCastNextTarget(currentTargetNode.transform.position, 1 << 13)) // If there's no direct line between the AI and its target
        {
            ResetTarget();
        }
    }

    private void ResetTarget()
    {
        print("reset target!");
        float closestNodeDistance = Mathf.Infinity;
        Node closestNode = null;
        for (int i = 0; i < allNodes.Count; i++)
        {
            float currentNodeDistance = Vector3.Distance(transform.position, allNodes[i].transform.position);
            if (currentNodeDistance < closestNodeDistance) // if the currently checked node is closer than the currently registered closest node, set that node as the closest.
            {
                closestNodeDistance = currentNodeDistance;
                closestNode = allNodes[i];
            }
        }
        currentTargetNode = closestNode;
        upcomingTargetNode = closestNode.nextNodes[0];
    }

    private bool LineCastNextTarget(Vector3 target, int layer)
    {
        // Bit shift the index of the layer (8) to get a bit mask
        if (!Physics.Linecast(transform.position, target, layer))     
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator WaitForTurn()
    {
        timerRunning = true;
        yield return new WaitForSeconds(waitingTime);
        currentTargetNode = upcomingTargetNode;
        upcomingTargetNode = currentTargetNode.nextNodes[0];
        timerRunning = false;

    }
}