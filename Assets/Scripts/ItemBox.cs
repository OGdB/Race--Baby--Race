﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public float cooldown;

    [Header("Visuals")]
    public GameObject visual;
    public Vector3 rotateDirection;
    public float floatSpeed;

    private float cooldownTimer;
    private BoxCollider collider;

    private void Start()
    {
        collider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (!visual.activeInHierarchy)
        {
            cooldownTimer += Time.deltaTime;

            if(cooldownTimer > cooldown)
            {
                visual.SetActive(true);
                collider.enabled = true;
            }
        }

        // do rotating and floating
        visual.transform.Rotate(rotateDirection * Time.deltaTime);
        visual.transform.localPosition = new Vector3(
            visual.transform.localPosition.x, 
            0.25f + Mathf.PingPong(Time.time * floatSpeed, 0.5f), 
            visual.transform.localPosition.z
            );
    }

    public Item GetItem()
    {
        visual.SetActive(false);
        collider.enabled = false;
        cooldownTimer = 0;

        return (Item)Random.Range(1, System.Enum.GetNames(typeof(Item)).Length);
    }
}

public enum Item
{
    None,
    RLauncher,
    GLauncher,
    MachineGun
}