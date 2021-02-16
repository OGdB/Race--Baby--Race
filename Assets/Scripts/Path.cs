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

        for (int n = 0; n < transform.childCount; n++)
        {
            Gizmos.DrawSphere(transform.GetChild(n).position, 0.25f);

            Gizmos.DrawLine(
                transform.GetChild(n).position, 
                transform.GetChild(Mathf.RoundToInt(Mathf.Repeat(n + 1, transform.childCount))).position
                );
        }
    }
}
