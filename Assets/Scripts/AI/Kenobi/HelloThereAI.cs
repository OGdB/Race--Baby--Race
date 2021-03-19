using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class HelloThereAI : MonoBehaviour
{
    private BaseAI baseAI;
    public Node currentTarget;
    public Node nextTarget;
    [SerializeField] private float confirmationDistance = 2f;
    [SerializeField] private List<Node> allNodes = new List<Node>();
    [SerializeField] private float waitingTime = 0.2f;
    private bool TurnWaiting = false;
    private bool timerWaiting = false;
    [SerializeField] private AnimationCurve sMagToSpeedCurve;

    [Header("Cosmetic")]
    [SerializeField] private CarBody carBody;
    void Start()
    {
        baseAI = GetComponent<BaseAI>();
        baseAI.SetBody(carBody);
        baseAI.SetName("Kenobi");
        currentTarget = baseAI.GetFirstNode();
        nextTarget = currentTarget.GetComponent<Node>().nextNodes[0];

        foreach (Transform child in currentTarget.transform.parent)
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

        //debug lines
        Debug.DrawLine(currentTarget.transform.position, nextTarget.transform.position, Color.red);
        Debug.DrawLine(transform.position, currentTarget.transform.position, Color.blue);
        Debug.DrawLine(transform.position, nextTarget.transform.position, Color.yellow);
    }

    private void StandardMovement()
    {
        // If the car can draw a line between itself and the upcoming node > Set that node as the current node
        if (LineCastNextTarget(nextTarget.transform.position, 1 << LayerMask.NameToLayer("Walls")))
        {
            if (!TurnWaiting)
            {
                StartCoroutine(WaitForTurn());
            }
        }
        else if (Vector3.Distance(transform.position, currentTarget.transform.position) < confirmationDistance && !timerWaiting)
        {
            print("within distance");
            StartCoroutine(Timer(0.75f));
            if (currentTarget.nextNodes.Length > 1)
            {
                int randomPath = Random.Range(0, currentTarget.nextNodes.Length);
                currentTarget = currentTarget.nextNodes[randomPath];
                nextTarget = currentTarget.nextNodes[0];
            }
            else
            {
                currentTarget = currentTarget.nextNodes[0];
                nextTarget = currentTarget.nextNodes[0];
            }
        }

        Vector3 dir = transform.InverseTransformDirection((currentTarget.transform.position - transform.position).normalized);
        Debug.DrawRay(transform.position, transform.TransformDirection(dir) * 3, Color.red);
        float dirY = sMagToSpeedCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(dir.x)));
        baseAI.SetDirection(new Vector2(dir.x, dirY));
    }
    IEnumerator Timer(float time)
    {
        timerWaiting = true;
        yield return new WaitForSeconds(time);
        timerWaiting = false;
    }
    private void CheckForCol()
    {
        if (!LineCastNextTarget(currentTarget.transform.position, 1 << 13)) // If there's no direct line between the AI and its target
        {
            ResetTarget();
        }
    }

    private void ResetTarget()
    {
        // print("reset target!");
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
        currentTarget = closestNode;
        nextTarget = closestNode.nextNodes[0];
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
        TurnWaiting = true;
        yield return new WaitForSeconds(waitingTime);
        currentTarget = nextTarget;
        nextTarget = currentTarget.nextNodes[0];
        TurnWaiting = false;
    }
}