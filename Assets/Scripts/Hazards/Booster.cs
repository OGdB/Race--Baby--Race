using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    public float force;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Rigidbody>().AddForce(other.transform.forward * force * Time.fixedDeltaTime * other.GetComponent<BaseAI>().GetDirection().y, ForceMode.Acceleration);
        }
    }
}
