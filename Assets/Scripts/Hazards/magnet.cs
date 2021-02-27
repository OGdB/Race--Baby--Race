using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class magnet : MonoBehaviour
{
    public float speed;

    private void OnTriggerStay(Collider other)
    {
        float forceFactor = speed * Time.deltaTime;
        var player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<Rigidbody>().AddForce((this.transform.position - player.transform.position) * speed * Time.smoothDeltaTime);
        //player.transform.position = Vector3.MoveTowards(player.transform.position, -this.transform.position, forceFactor);
    }

}

