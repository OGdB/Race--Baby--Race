using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    public GameObject[] players;
    public float force;
    public float zforce = -0.5f;
    public float xforce = -0.5f;
    public float[] boostTime ;
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        boostTime = new float[players.Length];
    }

    void Update()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (boostTime[i] > 0)
            {
                boostTime[i] = boostTime[i] - Time.deltaTime;

                players[i].GetComponent<Rigidbody>().AddForce(xforce, 0f, zforce, ForceMode.Impulse); //(transform.forward * force)
            }
            if (boostTime[i] <= 0)
            {
                boostTime[i] = 0f;
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (collision.gameObject == players[i])
            {
                boostTime[i] = 1f;
            }
        }
            
    }

}
