using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class magnet : MonoBehaviour
{
    public float force;
    public float angle;

    private void OnTriggerStay(Collider other)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
        var player = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < player.Length; i++)
        {
            if(other.gameObject == player[i])
            {
                player[i].GetComponent<Rigidbody>().AddForce(force * dir);
               // player[i].GetComponent<Rigidbody>().AddForce((this.transform.position - player[i].transform.position) * force * Time.smoothDeltaTime);
                //player[i].transform.position = Vector3.MoveTowards(player[i].transform.position, -this.transform.position, forceFactor);
            }

        }

        
    }

}

