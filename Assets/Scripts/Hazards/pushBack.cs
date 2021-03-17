using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pushBack : MonoBehaviour
{
    public float force;
    public float angle;
    public ForceMode forceMode = ForceMode.Impulse;

    private void OnTriggerStay(Collider other)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
        var player = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < player.Length; i++)
        {
            if(other.gameObject == player[i])
            {
                player[i].GetComponent<Rigidbody>().AddForce(-player[i].transform.forward * force * Time.deltaTime, forceMode);
               // player[i].GetComponent<Rigidbody>().AddForce(force * dir  * Time.deltaTime , ForceMode.VelocityChange);

            }

        }

        
    }

}

