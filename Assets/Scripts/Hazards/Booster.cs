using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    private GameObject[] players;
    public float force;
    public float zforce = -0.5f;
    public float xforce = -0.5f;
    public float boostTime ;
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    void Update()
    {
        if(boostTime > 0)
        {
            boostTime = boostTime - Time.deltaTime;

            for (int i = 0; i < players.Length; i++)
            {
                players[i].GetComponent<Rigidbody>().AddForce(xforce, 0f, zforce, ForceMode.Impulse); //(transform.forward * force)
            }
        }
        if (boostTime <= 0)
        {
            boostTime = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        boostTime = 0.9f;
    }

}
