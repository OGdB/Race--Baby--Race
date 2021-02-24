﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class BaseAI : MonoBehaviour
{
    //serialized variables
    [Header("Locomotion")]
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float speedAcceleration;

    [SerializeField]
    private float maxSteeringAngle;
    [SerializeField]
    private float steeringAcceleration;

    [SerializeField]
    private Vector2 direction;
    // direction.x = steering;
    // direction.y = speed;

    [SerializeField]
    private float uprightForce;


    [Header("State")]
    [SerializeField]
    private bool isGrounded;
    [SerializeField]
    private Transform groundedCheckPos;
    [SerializeField]
    private float groundCheckDistance;
    [SerializeField]
    private LayerMask groundMask;


    [Header("Items")]
    [SerializeField]
    private Item currentItem = Item.None;
    [SerializeField]
    private Transform itemCheckPos;
    [SerializeField]
    private float itemCheckDistance;

    [Header("Scoring")]
    public int position;
    [SerializeField]
    private Text positionText;

    [Header("Misc")]
    [SerializeField]
    private bool overrideControl;

    //fully private
    private float currentSpeed;
    private float currentSteeringAngle;
    private Vector3 velocity;

    private Transform path;


    //references
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (overrideControl)
        {
            direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }

    private void FixedUpdate()
    {
        //move and shit
        Locomotion();

        //shoot and shit
        Items();

        //display the AI's position
        positionText.text = BaseAIHelper.AddOrdinal(position);
    }

    private void Locomotion()
    {
        //check if grounded
        Collider[] colliders = Physics.OverlapSphere(groundedCheckPos.position, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
        isGrounded = colliders.Length > 0;

        //set speed based on input (input = direction.y);
        currentSpeed = Mathf.Lerp(currentSpeed, isGrounded ? (Mathf.Clamp(direction.y, -1f, 1f) * maxSpeed) : 0, speedAcceleration * Time.fixedDeltaTime);

        //add speed to velocity (we don't actually modify the rigidbody's velocity here so we can set the x and y to 0)
        velocity = new Vector3(0, 0, currentSpeed * Time.fixedDeltaTime);

        //change current steering angle based on input
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * direction.x, steeringAcceleration * Time.fixedDeltaTime);

        //move
        rb.MovePosition(transform.position + (rb.rotation * velocity) * Time.fixedDeltaTime);

        //steer
        float actualSteeringAngle = Mathf.Lerp(0, currentSteeringAngle, Mathf.Abs(currentSpeed / maxSpeed)); //only steer when driving
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, actualSteeringAngle * Time.fixedDeltaTime, 0));

        //stay upright
        Quaternion deltaUpward = Quaternion.FromToRotation(transform.up, Vector3.up);
        rb.AddTorque(new Vector3(deltaUpward.x, deltaUpward.y, deltaUpward.z) * uprightForce * Time.fixedDeltaTime);
    }

    private void Items()
    {
        //check for items
        Collider[] colliders = Physics.OverlapSphere(itemCheckPos.position, itemCheckDistance, ~0, QueryTriggerInteraction.Collide);

        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("ItemBox"))
                {
                    Item newItem = collider.GetComponent<ItemBox>().GetItem();
                    currentItem = (currentItem == Item.None) ? newItem : currentItem;
                }
            }
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        //we need to make sure that the new directions don't exeed a magnitude of 1 per axis...
        //otherwise you can exeed max speed and steering angles
        direction = new Vector2(
            Mathf.Clamp(newDirection.x, -1, 1),
            Mathf.Clamp(newDirection.y, -1, 1)
            );
    }

    public Vector3[] GetNodes()
    {
        //if no path, found path
        if(path == null)
        {
            path = GameObject.FindGameObjectWithTag("Path").transform;
        }

        //collect node positions
        Vector3[] nodes = new Vector3[path.childCount];

        for(int n = 0; n < nodes.Length; n++)
        {
            nodes[n] = path.GetChild(n).position;
        }

        return nodes;
    }

    public Vector3[] GetPlayerPositions()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<Vector3> playerPositions = new List<Vector3>();

        for (int p = 0; p < players.Length; p++)
        {
            if(players[p].transform.position == transform.position)
            {
                continue;
            }

            playerPositions.Add(players[p].transform.position);
        }

        return playerPositions.ToArray();
    }

    public Item GetCurrentItem()
    {
        return currentItem;
    }

    public void UseItem()
    {
        currentItem = Item.None;
    }

    public int GetPosition()
    {
        return position;
    }
}

public static class BaseAIHelper
{
    public static string AddOrdinal(int num)
    {
        if (num <= 0) return num.ToString();

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }

        switch (num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }
    }
}