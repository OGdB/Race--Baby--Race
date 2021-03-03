using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class magnet : MonoBehaviour
{
    public float speed;

    private void OnTriggerStay(Collider other)
    {
        float forceFactor = speed * Time.deltaTime;
        var player = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < player.Length; i++)
        {
            if(other.gameObject == player[i])
            {
                player[i].GetComponent<Rigidbody>().AddForce((this.transform.position - player[i].transform.position) * speed * Time.smoothDeltaTime);
                //player[i].transform.position = Vector3.MoveTowards(player[i].transform.position, -this.transform.position, forceFactor);
            }

        }

        
    }

}

