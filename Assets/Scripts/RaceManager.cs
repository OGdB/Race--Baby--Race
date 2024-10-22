﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RaceManager : MonoBehaviour
{
    [Header("Race settings")]
    public int laps;
    public bool raceHasStarted = false;
    public bool raceHasEnded = false;

    [Header("Checkpoints")]
    public Transform[] checkpoints;
    public LayerMask checkpointLayerMask;

    [Header("Player stuff")]
    public List<PlayerScore> players = new List<PlayerScore>();

    [Header("UI")]
    public Text raceInfoText;
    public Text scoreBoardText;

    [Header("Gizmos")]
    public Color color;
    public bool alwaysVisible;

    private void Start()
    {
        this.players.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            this.players.Add(new PlayerScore(player.GetComponent<BaseAI>()));
        }

        StartCoroutine("StartRace");
    }

    public IEnumerator StartRace()
    {
        raceInfoText.text = "3";
        yield return new WaitForSeconds(1f);

        raceInfoText.text = "2";
        yield return new WaitForSeconds(1f);

        raceInfoText.text = "1";
        yield return new WaitForSeconds(1f);

        raceInfoText.text = "GO!";
        raceHasStarted = true;
        yield return new WaitForSeconds(1f);

        raceInfoText.text = "";
        yield return null;
    }

    public IEnumerator EndRace()
    {
        yield return new WaitForEndOfFrame();

        raceHasEnded = true;
        yield return new WaitForSeconds(5f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        yield return null;
    }

    private void FixedUpdate()
    {
        //check all checkpoints using an overlapbox ==('collider')
        for (int c = 0; c < checkpoints.Length; c++)
        {
            Collider[] colliders = Physics.OverlapBox(checkpoints[c].position, checkpoints[c].localScale, checkpoints[c].rotation, checkpointLayerMask, QueryTriggerInteraction.Collide);

            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    //if a collider in range of one of the checkpoints is a player
                    if (collider.CompareTag("Player"))
                    {
                        //find the player in question
                        foreach (PlayerScore player in players)
                        {
                            //found player
                            if (player.AI.gameObject == collider.gameObject)
                            {
                                if (player.currentCheckPoint == c)
                                {
                                    player.currentCheckPoint++;

                                    if (player.currentCheckPoint >= checkpoints.Length)
                                    {
                                        player.currentCheckPoint = 0;
                                        player.currentLap++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //calculate distance to next checkpoint
        foreach (PlayerScore player in players)
        {
            player.distanceToNextCheckPoint = Vector3.Distance(player.AI.transform.position, checkpoints[player.currentCheckPoint].position);
        }
    }

    private void Update()
    {
        //assign checkpoint
        for (int p = 0; p < players.Count; p++)
        {
            players[p].AI.checkpoint = checkpoints[Mathf.RoundToInt(Mathf.Repeat(players[p].currentCheckPoint - 1, checkpoints.Length))];
        }

        //sort list of players based on lap, checkpoint and distance to next checkpoint
        players = players.OrderByDescending(x => x.currentLap).ThenByDescending(y => y.currentCheckPoint).ThenBy(z => z.distanceToNextCheckPoint).ToList();

        //assign positions to the baseAI
        if (!raceHasEnded)
        {
            scoreBoardText.text = "";
        }

        for (int p = 0; p < players.Count; p++)
        {
            players[p].AI.position = p + 1;
            players[p].AI.lap = players[p].currentLap;

            //add text to scoreboard
            if (!raceHasEnded)
            {
                scoreBoardText.text += BaseAIHelper.AddOrdinal(players[p].AI.position) + " - " + players[p].AI.GetName().Replace("\n", " ") + "\n";
            }

            //check if someone finished
            if (players[p].currentLap >= laps && !raceHasEnded)
            {
                raceInfoText.text = players[p].AI.GetName().Replace("\n", " ") + " Won!";
                StartCoroutine("EndRace");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (alwaysVisible)
        {
            Draw();
        }
    }

    public void OnDrawGizmosSelected()
    {
        Draw();
    }

    public void Draw()
    {
        Gizmos.color = color;
        Matrix4x4 originalMatrix = Gizmos.matrix;

        if(checkpoints != null)
        {
            for (int c = 0; c < checkpoints.Length; c++)
            {
                Gizmos.matrix = checkpoints[c].localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2);
            }
        }

        Gizmos.matrix = originalMatrix;
    }
}

[System.Serializable]
public class PlayerScore
{
    public PlayerScore(BaseAI AI)
    {
        this.AI = AI;
    }

    public int currentLap = 0;
    public int currentCheckPoint = 0;
    public float distanceToNextCheckPoint = 0;
    public BaseAI AI;
}