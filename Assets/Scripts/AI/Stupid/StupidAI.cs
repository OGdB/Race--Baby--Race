using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class StupidAI : MonoBehaviour
{
    [Header("Speed")]
    public AnimationCurve sMagToSpeedCurve;
    public bool sendIt;

    [Header("Steering")]
    public SteerMode steerMode;
    public Vector3 steerProjectionOffset;

    [Header("Bezier path")]
    public int nearestNodeOffset;
    public Color pathColor = Color.cyan;
    public float visibleRange;
    [Range(1, 16)]
    public int smoothingPasses;

    private BaseAI baseAI;
    private Vector3[] nodes;
    private List<Vector3> bezierPoints;


    private void Start()
    {
        baseAI = GetComponent<BaseAI>();

        //generate smoothed path
        // nodes = baseAI.GetNodes();

        for(int p = 0; p < smoothingPasses; p++)
        {
            nodes = Chaikin(nodes);
        }
    }

    private void Update()
    {
        int targetNode = Mathf.RoundToInt(Mathf.Repeat(
            GetNearestNode(transform.position + transform.InverseTransformDirection(steerProjectionOffset)) + nearestNodeOffset,
            nodes.Length));

        Debug.DrawLine(transform.position, nodes[targetNode], pathColor);

        //calculate direction
        Vector3 dir = transform.InverseTransformDirection((nodes[targetNode] - transform.position).normalized);

        //steer
        float steer = 0;

        switch (steerMode)
        {
            case SteerMode.Binary:
                steer = (dir.x < 0) ? -1 : 1;
                break;

            case SteerMode.Half:
                steer = dir.x * .5f;
                break;

            case SteerMode.Full:
                steer = dir.x;
                break;

            case SteerMode.Double:
                steer = dir.x * 2f;
                break;

            case SteerMode.Triple:
                steer = dir.x * 3f;
                break;

            case SteerMode.Quadruple:
                steer = dir.x * 4f;
                break;
        }

        //calculate intended speed
        float intendedSpeed = sendIt ? 1 : sMagToSpeedCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(steer)));

        //drive
        baseAI.SetDirection(new Vector2(steer, intendedSpeed));

        //use item
        if (baseAI.GetCurrentItem() != Item.None)
        {
            baseAI.UseItem();
        }
    }

    private int GetNearestNode(Vector3 position)
    {
        int nearest = 0;
        float distance = float.MaxValue;

        for(int n = 0; n < nodes.Length; n++)
        {
            float thisDist = Vector3.Distance(position, nodes[n]);
            if (thisDist < distance)
            {
                distance = thisDist;
                nearest = n;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = pathColor;

            for (int n = 0; n < nodes.Length; n++)
            {
                if(Vector3.Distance(nodes[n], transform.position) < visibleRange)
                {
                    Gizmos.DrawSphere(nodes[n], 0.25f);

                    Gizmos.DrawLine(
                        nodes[n],
                        nodes[Mathf.RoundToInt(Mathf.Repeat(n + 1, nodes.Length))]
                        );
                }
            }
        }
    }

    public Vector3[] Chaikin(Vector3[] pts)
    {
        Vector3[] newPts = new Vector3[(pts.Length - 2) * 2 + 2];
        newPts[0] = pts[0];
        newPts[newPts.Length - 1] = pts[pts.Length - 1];

        int j = 1;
        for (int i = 0; i < pts.Length - 2; i++)
        {
            newPts[j] = pts[i] + (pts[i + 1] - pts[i]) * 0.75f;
            newPts[j + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * 0.25f;
            j += 2;
        }
        return newPts;
    }
}

public enum SteerMode
{
    Binary,
    Half,
    Full,
    Double,
    Triple,
    Quadruple
}