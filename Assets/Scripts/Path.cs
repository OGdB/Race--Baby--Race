using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public bool alwaysVisible;
    public Color lineColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (alwaysVisible)
        {
            Draw();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Draw();
    }

    private void Draw()
    {
        Gizmos.color = lineColor;

        /* SUMMARY
         * For each node in the Path object,
         * check if there's nodes manually added (done for branching)
         * else, just get the next sequential node (ordered in Inspector)
         */
        for (int n = 0; n < transform.childCount; n++)
        {
            Gizmos.DrawSphere(transform.GetChild(n).position, 0.25f);
            if (transform.GetChild(n).GetComponent<Node>().nextNodes.Length > 0)
            {
                foreach (Node nextNode in transform.GetChild(n).GetComponent<Node>().nextNodes)
                {
                    Gizmos.DrawLine(
                        transform.GetChild(n).position,
                        nextNode.transform.position
                        );
                }
            }
            else
            {
                    Gizmos.DrawLine(
                        transform.GetChild(n).position,
                        transform.GetChild(Mathf.RoundToInt(Mathf.Repeat(n + 1, transform.childCount))).position
                        );
            }
        }
    }
}
