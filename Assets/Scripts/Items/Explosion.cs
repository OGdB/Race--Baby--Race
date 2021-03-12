using System;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public LayerMask obstacleMask;
    public float blastRadius = 5;
    public float explosionForce;
    public float upRatio;

    private void Start()
    {
        Collider[] collidersNear = Physics.OverlapSphere(transform.position, blastRadius, obstacleMask);
        foreach (Collider collider in collidersNear)
        {
            Transform target = collider.transform;
            Vector3 direction = (target.position - transform.position).normalized;

            Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
            if (targetRigidbody != null)
            {
                targetRigidbody.AddForce(direction * explosionForce + (Vector3.up * explosionForce * upRatio));
            }

            Grenade grenade = target.GetComponent<Grenade>();
            if(grenade != null)
            {
                grenade.Explode();
            }
        }

        Destroy(gameObject, 60f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(255, 0, 0, 0.1f);
        Gizmos.DrawSphere(transform.position, blastRadius);
    }
}
