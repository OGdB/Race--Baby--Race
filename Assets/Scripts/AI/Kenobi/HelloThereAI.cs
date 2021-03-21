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
    private bool turnCooldown = false;
    [SerializeField] private AnimationCurve sMagToSpeedCurve;

    [Header("Cosmetic")]
    [SerializeField] private CarBody carBody;

    [Header("Layers")]
    [SerializeField] private LayerMask walls;
    [SerializeField] private LayerMask hazards;
    [SerializeField] private LayerMask items;

    [Space(20)]
    [SerializeField] private bool debug = false;
    void Start()
    {
        // AI
        baseAI = GetComponent<BaseAI>();
        baseAI.SetBody(carBody);
        baseAI.SetName("Kenobi");
        currentTarget = baseAI.GetFirstNode();
        nextTarget = currentTarget.GetComponent<Node>().nextNodes[0];
        // Layers
        walls = 1 << LayerMask.NameToLayer("Walls");
        items = 1 << LayerMask.NameToLayer("Items");
        hazards = 1 << LayerMask.NameToLayer("Hazards");

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
        float wallPreventionMultiplier = WallPrevention();
        StandardMovement(wallPreventionMultiplier);
        CheckLineCastCurrentTarget();

        if (FrontDirectionalRay(0, hazards, 5f, true, true) ) // Hold if there's a hazard in front of you.
        {
            float slowDownSpeed = 0.4f * Direction();
            print("slowing down");
            baseAI.SetDirection(new Vector2(0, slowDownSpeed));
        }

        //debug lines
        if (debug)
        {
            Debug.DrawLine(currentTarget.transform.position, nextTarget.transform.position, Color.red);
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.blue);
            Debug.DrawLine(transform.position, nextTarget.transform.position, Color.yellow);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            print(Vector3.Angle(currentTarget.transform.position - transform.position, transform.forward));
        }
    }

    private void StandardMovement(float wallPreventionMultiplier)
    {
        if (currentTarget.nextNodes.Length < 2)
        {
            nextTarget = currentTarget.nextNodes[0];
        }
        else
        {
            nextTarget = GetShortestPath();
        }
        // If the AI can draw a direct line between itself and the nextTarget, make that next Target the Current Target after a small buffer.
        if (LineCastNextTarget(nextTarget.transform.position, 1 << LayerMask.NameToLayer("Walls")))
        {
            if (!TurnWaiting)
            {
                StartCoroutine(WaitForTurn());
            }
        }

        else if (Vector3.Distance(transform.position, currentTarget.transform.position) < confirmationDistance && !turnCooldown)
        {
            StartCoroutine(TurningCooldown(0.75f));
            if (currentTarget.nextNodes.Length > 1) // if a branch
            {
                /*int randomPath = Random.Range(0, currentTarget.nextNodes.Length);
                currentTarget = currentTarget.nextNodes[randomPath];
                nextTarget = currentTarget.nextNodes[0];*/
                currentTarget = GetShortestPath();
                nextTarget = currentTarget.nextNodes[0];
            }
            else
            {
                currentTarget = currentTarget.nextNodes[0];
                nextTarget = currentTarget.nextNodes[0];
            }
        }

        Vector2 dir = transform.InverseTransformDirection((currentTarget.transform.position - transform.position).normalized);
        float forwardSpeed = sMagToSpeedCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(dir.x)));
        baseAI.SetDirection(new Vector2(dir.x * Direction() + wallPreventionMultiplier, forwardSpeed * Direction()));

        IEnumerator WaitForTurn()
        {
            TurnWaiting = true;
            yield return new WaitForSeconds(waitingTime);
            currentTarget = nextTarget;
            TurnWaiting = false;
        }
    }
    IEnumerator TurningCooldown(float time)
    {
        turnCooldown = true;
        yield return new WaitForSeconds(time);
        turnCooldown = false;
    }
    private void CheckLineCastCurrentTarget()
    {
        if (!LineCastNextTarget(currentTarget.transform.position, 1 << 13)) // If there's no direct line between the AI and its target, the AI probably died.
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
        if (!Physics.Linecast(transform.position, target, layer))     
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool FrontDirectionalRay(float angle, int layer, float length, bool drawRay, bool checkTag)
    {
        RaycastHit hit;
        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward * Direction();
        bool hitting = Physics.Raycast(transform.position, direction * length, out hit, length, layer);

        Color color = Color.green;
        if (hitting)
        {
            color = Color.red;
        }
        if (drawRay)
        {
            Debug.DrawRay(transform.position, direction * length, color);
        }
        if (checkTag)
        {
            return hitting && hit.collider.CompareTag("Hazard");
        }
        else
        {
            return hitting;
        }
    }

    private float WallPrevention()
    {
        float multiplier = 0;
        bool[] leftWallChecks = new bool[3];
        leftWallChecks[0] = FrontDirectionalRay(-25, walls, 5f, true, false);
        leftWallChecks[1] = FrontDirectionalRay(-20, walls, 5f, true, false);
        leftWallChecks[2] = FrontDirectionalRay(-10, walls, 5f, true, false);
        bool[] rightWallChecks = new bool[3];
        rightWallChecks[0] = FrontDirectionalRay(20, walls, 5f, true, false);
        rightWallChecks[1] = FrontDirectionalRay(25, walls, 5f, true, false);
        rightWallChecks[2] = FrontDirectionalRay(30, walls, 5f, true, false);

        foreach (bool wallDetected in leftWallChecks)
        {
            if (wallDetected)
            {
                multiplier += 0.2f;
            }
        }
        foreach (bool wallDetected in rightWallChecks)
        {
            if (wallDetected)
            {
                multiplier -= 0.2f;
            }
        }

        return multiplier;
    }

    private Node GetShortestPath()
    {
        // The node which will send the AI on the shortest path
        Node nodeToPick = null;
        float shortestPathLength = Mathf.Infinity;

        for (int i = 0; i < currentTarget.nextNodes.Length; i++) // For each branch of the upcoming node.
        {
            Node thisBranch = currentTarget.nextNodes[i];
            Node currentNode = currentTarget.nextNodes[i]; // The branchnode we're currently checking. (moves on until another branch is reached).
            Node nextNode = currentNode.nextNodes[0]; // The upcoming node.
            float totalPathLength = 0f;

            // Makes a path from the start of the branch until the next upcoming branch.
            while (currentNode.nextNodes.Length < 2) // While no other branch has been reached...
            {
                totalPathLength += Vector3.Distance(currentNode.transform.position, nextNode.transform.position);
                currentNode = currentNode.nextNodes[0];
                nextNode = currentNode.nextNodes[0];
            }

            if (totalPathLength < shortestPathLength)
            {
                nodeToPick = thisBranch;
                shortestPathLength = totalPathLength;
            }
        }

        return nodeToPick;
    }

    private float Direction()
    {
        return Vector3.Angle(currentTarget.transform.position - transform.position, transform.forward) > 95f ? -1f : 1f;
    }
}