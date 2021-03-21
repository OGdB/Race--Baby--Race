using System.Collections;
using UnityEngine;

public class LookAtAI : MonoBehaviour
{
    [SerializeField] private BaseAI baseAI;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float rotationSpeed = 5f;
    private bool isPlaying = false;
    private Vector3[] playerPositions;
    private void FixedUpdate()
    {
        playerPositions = baseAI.GetPlayerPositions();
        float closestDistance = Vector3.Distance(transform.position, playerPositions[0]);

        if (playerPositions.Length > 0)
        {
            Vector3 closestPlayer = playerPositions[0];
            foreach (Vector3 playerPos in playerPositions)
            {
                float thisDistance = Vector3.Distance(transform.position, playerPos);
                if (thisDistance < closestDistance)
                {
                    closestPlayer = playerPos;
                    closestDistance = thisDistance;
                }
            }

            Vector3 dir = closestPlayer - transform.position;
            dir *= -1;
            dir.y = 0;
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }

        // hello ;)
        if (closestDistance < 10f && !isPlaying)
        {
            StartCoroutine(HelloThere());
        }
        IEnumerator HelloThere()
        {
            isPlaying = true;
            audioSource.Play();
            yield return new WaitForSeconds(10f);
            isPlaying = false;
        }
    }
}
