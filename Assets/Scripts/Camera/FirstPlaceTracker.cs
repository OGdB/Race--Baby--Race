﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPlaceTracker : MonoBehaviour
{
    public float playerUpdateDelay = 3;
    public GameObject firstPlayer;

    private GameObject[] players;
    private BaseAI[] playerAI;

    private bool updating;

    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        // Creates an array of BaseAI components with the same index as each player in the players array
        System.Array.Resize(ref playerAI, players.Length);
        for (int i = 0; i < players.Length; i++)
        {
            playerAI[i] = players[i].GetComponent<BaseAI>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (playerAI[i].position == 1)
            {
                if(firstPlayer != playerAI[i] && !updating)
                {
                    StartCoroutine("UpdatePlayer");
                    updating = true;
                }
                break;
            }
        }

        if (firstPlayer != null)
        {
            transform.position = firstPlayer.transform.position;
        }
    }

    public IEnumerator UpdatePlayer()
    {
        yield return new WaitForSeconds(playerUpdateDelay);

        for (int i = 0; i < players.Length; i++)
        {
            if (playerAI[i].position == 1)
            {
                firstPlayer = players[i];
                break;
            }
        }

        updating = false;
        yield return null;
    }
}
