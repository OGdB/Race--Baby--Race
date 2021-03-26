using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(BaseAI))]
public class RicardoAI : MonoBehaviour
{

    public BaseAI baseAI;

    public bool dodging;
    public bool seesItem;
    public bool died = false;

    [Header("Pathfinding")]
    public List<Node> NodeList = new List<Node>();
    [SerializeField]
    private Node currentTargetNode;
    [SerializeField]
    private Node nextTargetNode;
    public float confirmationDistance = 7f;

    [Header("Steering")]
    [SerializeField]
    private float maxDodgingAngle = 1f;
    private float turnSpeed = 5f;
    public Vector3 direction;
    private float targetSteerAngle = 0f;

    [Header("Raycasting")]
    public Vector3 frontRaycastPos = new Vector3(0f, 0.2f, 0.5f);
    public float sensorsLength = 6f;
    public float frontSideSensorPos = 0.2f;
    public float frontAngleSensor1 = 30f;
    public float frontAngleSensor2 = 15f;
    public bool wallOnLeft = false;
    public bool wallOnRight = false;
    public bool wallInFront = false;

    [Header("CarDecor")]
    [SerializeField]private CarBody carBody;
    [SerializeField]private string carName;

    private void Start()
    {
        baseAI = GetComponent<BaseAI>();
        currentTargetNode = baseAI.GetFirstNode();
        nextTargetNode = currentTargetNode.nextNodes[0];
        baseAI.SetName(carName);
        baseAI.SetBody(carBody);
    }
    private void Update()
    {
        if (dodging)
        {
            print("dodging");
        }
    }
    private void FixedUpdate()
    {
        Pathfinding();
        BasicDirection();
        ItemUse();
        CheckWalls();
        SensorCheck();

        if (died)
        {
            TargetClosestNode();
        }
    }

    private void Pathfinding()
    {
        if (Vector3.Distance(transform.position, currentTargetNode.transform.position) < confirmationDistance)
        {
            if (currentTargetNode.nextNodes.Length > 1)
            {
                int randomPath = Random.Range(0, currentTargetNode.nextNodes.Length);
                currentTargetNode = currentTargetNode.nextNodes[randomPath];
                nextTargetNode = currentTargetNode.nextNodes[0];
            }
            else
            {
                currentTargetNode = currentTargetNode.nextNodes[0];
                nextTargetNode = currentTargetNode.nextNodes[0];
            }
        }
    }

    private void BasicDirection()
    {
        direction = transform.InverseTransformDirection((currentTargetNode.transform.position - transform.position).normalized);

        direction.y = 1.4f - Mathf.Abs(direction.x);//the speed will go down the more you steer
        if (direction.y > 1)
        {
            direction.y = 1;
        }
        if (!dodging)
        {
            baseAI.SetDirection(new Vector2(direction.x, direction.y));
        }
    }

    //use an item if the car sees a player in front or back or if he is dodging
    private void ItemUse()
    {
        Vector3 raycastStartPos = transform.position;
        raycastStartPos += transform.up * 0.2f;
        raycastStartPos += transform.forward * -0.8f;

        bool[] playersCheckBack = new bool[100];
        //creates 3 backwards raycasts that check for players in the back to throw items backwards
        playersCheckBack[0] = CheckPlayersRaycasts(raycastStartPos, 180, 6, false, true);
        playersCheckBack[1] = CheckPlayersRaycasts(raycastStartPos, 160, 6, false, true);
        playersCheckBack[2] = CheckPlayersRaycasts(raycastStartPos, 200, 6, false, true);

        bool[] playersCheckFront = new bool[100];
        raycastStartPos += transform.forward * 1.6f;
        //creates 3 front raycasts that check for players in front to throw items forward
        playersCheckFront[0] = CheckPlayersRaycasts(raycastStartPos, 0, 6, false, false);
        playersCheckFront[1] = CheckPlayersRaycasts(raycastStartPos, 20, 6, false, false);
        playersCheckFront[2] = CheckPlayersRaycasts(raycastStartPos, 340, 6, false, false);

        if (baseAI.GetCurrentItem() != Item.None)
        {
            if (dodging)
            {
                baseAI.AimBack(true);
                baseAI.UseItem();
            }
        }
    }


    //if the car sensors collide with anything (hazards(pillars and trampoulines) || placed items)
    //>>try avoiding them by steering left/right 
    private void SensorCheck()
    {
        RaycastHit hit;
        Vector3 raycastStartPos = transform.position;
        raycastStartPos += transform.forward * frontRaycastPos.z;
        raycastStartPos += transform.up * 0.2f;
        float dodgeMultiplier = 0; 
        dodging = false;
        seesItem = false;
        wallOnLeft = false;
        wallOnRight = false;
        wallInFront = false;

        Debug.DrawLine(raycastStartPos, currentTargetNode.transform.position, Color.red);


            Debug.DrawLine(raycastStartPos, nextTargetNode.transform.position, Color.yellow);

        //front middle sensor
        if (Physics.Raycast(raycastStartPos, transform.forward, out hit, sensorsLength))
        {
            if (hit.collider.CompareTag("Hazard") || hit.collider.gameObject.layer == 14)
            {
                dodging = true;
                dodgeMultiplier -= 0f;

                Debug.DrawLine(raycastStartPos, hit.point, Color.blue);
            }
            if (hit.collider.CompareTag("ItemBox") && baseAI.GetCurrentItem() == Item.None)
            {
                seesItem = true;
                dodgeMultiplier += 0f;
                Debug.DrawLine(raycastStartPos, hit.point, Color.cyan);
            }
        }
        //front right sensor 
        //this raycast will look for an item box if it has no items
        //if it finds one he will move towards it
        raycastStartPos += transform.right * frontSideSensorPos;
        if (Physics.Raycast(raycastStartPos, Quaternion.AngleAxis(0, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.collider.CompareTag("ItemBox") && baseAI.GetCurrentItem() == Item.None)
            {
                seesItem = true;
                dodgeMultiplier += 0f;
                Debug.DrawLine(raycastStartPos, hit.point, Color.cyan);
            }
        }

        //first front right angle sensor
        else if (Physics.Raycast(raycastStartPos, Quaternion.AngleAxis(frontAngleSensor2, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.collider.CompareTag("Hazard") || hit.collider.gameObject.layer == 14)
            {
                dodging = true;
                dodgeMultiplier -= 0.75f;

                Debug.DrawLine(raycastStartPos, hit.point, Color.blue);
            }

            if (hit.collider.CompareTag("ItemBox") && baseAI.GetCurrentItem() == Item.None)
            {
                seesItem = true;
                dodgeMultiplier += 0.5f;
                Debug.DrawLine(raycastStartPos, hit.point, Color.cyan);
            }
        }

        //second front right angle sensor
        else if (Physics.Raycast(raycastStartPos, Quaternion.AngleAxis(frontAngleSensor1, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.collider.CompareTag("Hazard") || hit.collider.gameObject.layer == 14)
            {
                dodging = true;
                dodgeMultiplier -= 0.5f;

                Debug.DrawLine(raycastStartPos, hit.point, Color.blue);
            }

            if (hit.collider.CompareTag("ItemBox") && baseAI.GetCurrentItem() == Item.None)
            {
                seesItem = true;
                dodgeMultiplier += 1f;
                Debug.DrawLine(raycastStartPos, hit.point, Color.cyan);
            }
        }


        //front left sensor
        raycastStartPos -= transform.right * frontSideSensorPos * 2;
        if (Physics.Raycast(raycastStartPos, Quaternion.AngleAxis(-4, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.collider.CompareTag("ItemBox") && baseAI.GetCurrentItem() == Item.None)
            {
                seesItem = true;
                dodgeMultiplier -= 0f;
                Debug.DrawLine(raycastStartPos, hit.point, Color.cyan);
            }
        }

        //first front left angle sensor
        else if (Physics.Raycast(raycastStartPos, Quaternion.AngleAxis(-frontAngleSensor2, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.collider.CompareTag("Hazard") || hit.collider.gameObject.layer == 14)
            {
                dodging = true;
                dodgeMultiplier += 0.75f;

                Debug.DrawLine(raycastStartPos, hit.point, Color.blue);
            }

            if (hit.collider.CompareTag("ItemBox") && baseAI.GetCurrentItem() == Item.None)
            {
                seesItem = true;
                dodgeMultiplier -= 0.5f;
                Debug.DrawLine(raycastStartPos, hit.point, Color.cyan);
            }
        }

        //second front left angle sensor
        else if (Physics.Raycast(raycastStartPos, Quaternion.AngleAxis(-frontAngleSensor1, transform.up) * transform.forward, out hit, sensorsLength))
        {
            if (hit.collider.CompareTag("Hazard") || hit.collider.gameObject.layer == 14)
            {
                dodging = true;
                dodgeMultiplier += 0.5f;

                Debug.DrawLine(raycastStartPos, hit.point, Color.blue);
            }

            if (hit.collider.CompareTag("ItemBox") && baseAI.GetCurrentItem() == Item.None)
            {
                seesItem = true;
                dodgeMultiplier -= 1f;
                Debug.DrawLine(raycastStartPos, hit.point, Color.cyan);
            }
        }


        CheckWalls();

        if (dodging)
        {
            if(dodgeMultiplier == 0)
            {
                float[] dodgingMult = { -1, 1 };
                int randomNr = Random.Range(0, dodgingMult.Length); ;

                //check if there are any walls on the right then go -1f
                //check if any walls on the left then go 1f
                //if no walls in any direction choose randomly between -1f and 1f
                if (wallOnRight)
                {
                    //direction.y -= 0.4f;
                    dodgeMultiplier -= 1f;
                }
                if (wallOnLeft)
                {
                    //direction.y -= 0.4f;
                    dodgeMultiplier += 1f;
                }
                if (!wallOnLeft && !wallOnRight)
                {
                    //direction.y -= 0.4f;
                    dodgeMultiplier += 1f;
                }

            }
            //    targetSteerAngle = maxDodgingAngle * dodgeMultiplier;
            direction.x = maxDodgingAngle * dodgeMultiplier;
            baseAI.SetDirection(new Vector2(direction.x, direction.y));
        }

        if (seesItem)
        {

            //    targetSteerAngle = maxDodgingAngle * dodgeMultiplier;
            direction.x = maxDodgingAngle * dodgeMultiplier;
            baseAI.SetDirection(new Vector2(direction.x, direction.y));
        }


    }

    //create 3 raycasts that check for walls
    private void CheckWalls()
    {
        Vector3 raycastStartPos = transform.position;
        raycastStartPos += transform.up * 0.2f;
        raycastStartPos += transform.forward * 0.2f;

        bool[] wallRaycasts = new bool[100];
        wallRaycasts[0] = CheckWallsRaycasts(raycastStartPos, 0, 3, false , false, false, true);//front
        wallRaycasts[1] = CheckWallsRaycasts(raycastStartPos, 90, 2.5f, false, true, false, false);//right
        wallRaycasts[2] = CheckWallsRaycasts(raycastStartPos, 270, 2.5f, false, false, true, false);//left

    }
    //this function creates raycasts that uses items if it sees other players
    private bool CheckPlayersRaycasts(Vector3 startRay, float angleDir, float length, bool showRay, bool aimBack)
    {
        RaycastHit hit;
        Vector3 rayDir = Quaternion.AngleAxis(angleDir, transform.up) * transform.forward;
        bool raycasting = Physics.Raycast(startRay, rayDir, out hit, length, 8);
        if (showRay)
        {
            Debug.DrawRay(startRay, rayDir * length, Color.cyan);
        }
        if (raycasting)
        {
            if (hit.transform.gameObject.layer == 8 && baseAI.GetCurrentItem() != Item.None)
            {
                baseAI.AimBack(aimBack);
                baseAI.UseItem();
            }
        }
        return raycasting;
    }

    //this function creates raycasts that check for walls in front, left and right
    private bool CheckWallsRaycasts(Vector3 startRay, float angleDir, float length, bool showRay, bool right, bool left, bool front)
    {
        RaycastHit hit;
        Vector3 rayDir = Quaternion.AngleAxis(angleDir, transform.up) * transform.forward;
        bool raycasting = Physics.Raycast(startRay, rayDir, out hit, length);
        if (showRay)
        {
            Debug.DrawRay(startRay, rayDir * length, Color.green);
        }
        if (raycasting)
        {
            if (hit.transform.gameObject.layer == 13)
            {
                wallInFront = front;
                wallOnRight = right;
                wallOnLeft = left;
            }
        }
        return raycasting;
    }

    //if the car has collided with the grass then turn died bool true for 1 sec
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "grass")
        {
            died = true;
            StartCoroutine("DeathTimer");
        }
    }

    IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(0.7f);
        died = false;
    }

    //this function targets the closest node to the car
    //
    //is called whenever the player has died
    private void TargetClosestNode()
    {
        float closestNodeDis = float.MaxValue;
        Node closestNode;
            foreach (Node node in NodeList)
            {
                float currCheckedDis = Vector3.Distance(transform.position, node.transform.position);
                if (currCheckedDis < closestNodeDis)
                {
                    closestNodeDis = currCheckedDis;
                    closestNode = node;
                    currentTargetNode = closestNode;
                }
            }

        if (!Physics.Linecast(transform.position, nextTargetNode.transform.position, 1 << 13))
        {
            currentTargetNode = nextTargetNode;

        }
    }
}
