using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class HelloThereAI : MonoBehaviour
{
    #region Variables
    private BaseAI baseAI;
    private Node currentTarget;
    private Node nextTarget;
    private bool TurnWaiting = false;
    private float offTargetTimer = 0;
    private List<Node> allNodes = new List<Node>();

    private LayerMask walls;
    private LayerMask hazards;
    private LayerMask players;

    [SerializeField] private Vector2 dir;

    [Header("Variables")]
    [SerializeField] private float confirmationDistance = 5f;
    [SerializeField] private float waitingTime = 0.15f;
    [SerializeField] private float hazardSlowDown = 0.5f;
    [SerializeField] private AnimationCurve sMagToSpeedCurve;
    [SerializeField] private float pushWallMultiplier = 0.1f;

    [Header("Cosmetics")]
    [SerializeField] private CarBody carBody;

    [Space(20)]
    [SerializeField] private bool debug = false;
    #endregion

    void Start()
    {
        // AI
        baseAI = GetComponent<BaseAI>();
        baseAI.SetBody(carBody);
        baseAI.SetName("Kenobi");
        currentTarget = baseAI.GetFirstNode();
        nextTarget = currentTarget.GetComponent<Node>().nextNodes[0];
        baseAI.AimBack(true);
        // Layers
        walls = 1 << LayerMask.NameToLayer("Walls");
        hazards = 1 << LayerMask.NameToLayer("Hazards");
        players = 1 << LayerMask.NameToLayer("Players");

        // Get list of all nodes
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
        Movement();

        // Check whether the current target is still viable.
        if (!LineCastTarget(currentTarget.transform.position, walls))
        {
            offTargetTimer += Time.deltaTime;
            print(offTargetTimer);
            if (offTargetTimer > 1f)
            {
                ResetTarget();
            }
        }
        else
        {
            offTargetTimer = 0; // reset timer
        }

        // Slow down when there is a wall or hazard in front of the player to either wait for the hazard to move away or to provide space to rotate away from the wall
        if (DirectionalRay(0, hazards | walls, 5f, true, false) || DirectionalRay(-3f, hazards | walls, 5f, true, false) || DirectionalRay(3f, hazards | walls, 5f, true, false)) // Hold if there's a hazard in front of you.
        {
            float slowDownSpeed = hazardSlowDown * RelativeDirectionToTarget();
            baseAI.SetDirection(new Vector2(dir.x, slowDownSpeed));
        }

        // Vanquish my opposition when they are in front- or behind the AI.
        if (DirectionalRay(180, players, 25f, true, false) && baseAI.GetCurrentItem() != Item.None) // raycast if player is behind player &> use item
        {
            baseAI.AimBack(true);
        }
        else if (DirectionalRay(0, players, 25f, true, false) && baseAI.GetCurrentItem() != Item.None) // raycast if player is in front of player &> use item
        {
            baseAI.AimBack(false);
            if (baseAI.GetCurrentItem() != Item.None)
            {
                baseAI.UseItem();
            }
        }

        // debug movement lines
        if (debug)
        {
            Debug.DrawLine(currentTarget.transform.position, nextTarget.transform.position, Color.red);
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.blue);
            Debug.DrawLine(transform.position, nextTarget.transform.position, Color.yellow);
        }
    }

    /// <summary>
    /// 2 methods of pathfinding:
    /// 
    /// LINECAST: The AI constantly linecasts to the next node of its current target-node. 
    /// If it can do so without hitting the invisible walls, that next node will become the current target. 
    /// There is a short timebuffer until it starts steering.
    ///
    /// DISTANCE: If the AI gets close enough to the current target, it will also start targeting the next node. A cooldown will ensure it doesn't
    /// immediately 'reset' the current target due to the linecast method.
    /// 
    /// </summary>
    private void Movement()
    {
        nextTarget = currentTarget.nextNodes.Length < 2 ? currentTarget.nextNodes[0] : GetShortestPath();

        // If the AI can draw a direct line between itself and the nextTarget, make that next Target the Current Target after a small buffer.
        if (LineCastTarget(nextTarget.transform.position, walls))
        {
            if (!TurnWaiting)
            {
                StartCoroutine(WaitForTurn());
            }
        }
        else if (Vector3.Distance(transform.position, currentTarget.transform.position) < confirmationDistance)
        {
            if (currentTarget.nextNodes.Length > 1) // if a branch
            {
                currentTarget = GetShortestPath();
                nextTarget = currentTarget.nextNodes[0];
            }
            else
            {
                currentTarget = currentTarget.nextNodes[0];
                nextTarget = currentTarget.nextNodes[0];
            }
        }

        dir = transform.InverseTransformDirection((currentTarget.transform.position - transform.position).normalized);
        float forwardSpeed = sMagToSpeedCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(dir.x)));
        baseAI.SetDirection(new Vector2(dir.x + WallPrevention(), forwardSpeed) * RelativeDirectionToTarget());

        IEnumerator WaitForTurn()
        {
            TurnWaiting = true;
            yield return new WaitForSeconds(waitingTime);
            currentTarget = nextTarget;
            TurnWaiting = false;
        }
    }

    /// <summary>
    ///     /// A linecast is drawn to the current node-target. If this linecast is interrupted by a wall, the target is reset to the nearest node that is
    /// in sight (uninterrupted by walls). 
    /// </summary>
    private void ResetTarget()
    {
        print("reset");
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

    /// <summary>
    /// Linecast to a target, detecting only 1 specific layer.
    /// </summary>
    /// <param name="target"></param> WorldPosition to cast towards.
    /// <param name="layer"></param> Layer to detect.
    /// <returns> Return whether the layer was hit </returns>
    private bool LineCastTarget(Vector3 target, int layer)
    {
        return !Physics.Linecast(transform.position, target, layer);
    }

    /// <summary>
    /// Raycast in a certain angle to detect objects from a specific layer.
    /// </summary>
    /// <param name="angle"></param> The angle relative to the AI's forward direction to raycast towards.
    /// <param name="layer"></param> The layer to detect.
    /// <param name="length"></param> The length of the raycast.
    /// <param name="drawRay"></param> Do you want to visualize this raycast?
    /// <param name="checkTag"></param> Tag to filter on as well.
    /// <returns> Return whether an object of the layer was hit by the raycast. </returns>
    private bool DirectionalRay(float angle, int layer, float length, bool drawRay, bool checkTag)
    {
        RaycastHit hit;
        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward * RelativeDirectionToTarget();
        bool hitting = Physics.Raycast(transform.position, direction * length, out hit, length, layer);
        if (hitting && hit.collider.gameObject.GetInstanceID() == GetInstanceID())
        {
            hitting = false;
        }
        Color color = Color.green;
        if (hitting)
        {
            color = Color.red;
        }
        if (drawRay && debug)
        {
            Debug.DrawRay(transform.position, direction * length, color);
        }
        return checkTag ? hitting && hit.collider.CompareTag("Hazard") : hitting;
    }

    /// <summary>
    /// Draw multiple rays detecting walls. Steer in the corresponding direction if the AI gets too close to the wall.
    /// </summary>
    /// <returns> Returns a multiplier that is applied to the steering direction of the AI; a direction steering away from the walls </returns>
    private float WallPrevention()
    {
        float multiplier = 0;
        bool[] leftWallChecks = new bool[5];
        leftWallChecks[0] = DirectionalRay(-50, walls, 5f, true, false);
        leftWallChecks[1] = DirectionalRay(-40, walls, 5f, true, false);
        leftWallChecks[2] = DirectionalRay(-30, walls, 5f, true, false);
        leftWallChecks[3] = DirectionalRay(-20, walls, 5f, true, false);
        leftWallChecks[4] = DirectionalRay(-10, walls, 5f, true, false);

        bool[] rightWallChecks = new bool[5];
        rightWallChecks[0] = DirectionalRay(10, walls, 5f, true, false);
        rightWallChecks[1] = DirectionalRay(20, walls, 5f, true, false);
        rightWallChecks[2] = DirectionalRay(30, walls, 5f, true, false);
        rightWallChecks[3] = DirectionalRay(40, walls, 5f, true, false);
        rightWallChecks[4] = DirectionalRay(50, walls, 5f, true, false);

        foreach (bool wallDetected in leftWallChecks)
        {
            if (wallDetected)
            {
                multiplier += pushWallMultiplier;
            }
        }
        foreach (bool wallDetected in rightWallChecks)
        {
            if (wallDetected)
            {
                multiplier -= pushWallMultiplier;
            }
        }

        return multiplier;
    }

    /// <summary>
    /// Called when the next target node contains 2 nextnodes thus is a branch.
    /// When a branch is detected, loop through both branches until the next branch is detected as an endpoint.
    /// The next upcoming branch is chosen as an endpoint because there is no other way of detecting the endpoint of a branch, to reliably
    /// check which branch is shorter, you need the same reference point for each branch calculation, therefore the next upcoming path is chosen,
    /// which can be detected because it contains more than 1 'next nodes'.
    /// 
    /// The distance of each path towards the next branch is calculated and compared.
    /// </summary>
    /// <returns> Returns the node that leads to the shortest path. </returns>
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

    /// <summary>
    /// Detect whether the front or the back of the AI-car is closest to facing the current target node.
    /// </summary>
    /// <returns> Return a float which acts as a multiplier. The AI will drive in reverse if the back is closer facing the curren target </returns>
    private float RelativeDirectionToTarget()
    {
        return Vector3.Angle(currentTarget.transform.position - transform.position, transform.forward) > 100f ? -1f : 1f;
    }
}