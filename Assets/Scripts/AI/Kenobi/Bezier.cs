using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bezier : MonoBehaviour
{
    [SerializeField]
    private float tSeconds = 1;
    private float t;
    [SerializeField]
    List<Vector3> nodeList = new List<Vector3>();

    private void Start()
    {
        /*foreach (Transform trans in GetComponentsInChildren<Transform>())
        {
            if (trans != this.transform)
            {
                nodeList.Add(trans.position);
            }
        }*/
    }
    private void OnValidate()
    {
        tSeconds = Mathf.Clamp(tSeconds, 0.1f, 100f);
    }
    void Update()
    {
        t = (t + Time.deltaTime / tSeconds) % 1f;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Each Array is connected to the previous one.
            // Vector3[] nodes = nodeList.ToArray();
            Vector3[] nodes = GetComponent<Pathing>().shortestPath;
            Vector3[] previousArray = nodes;
            Vector3[] nextArray;
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                if (i != nodes.Length - 2)
                {
                    nextArray = DrawPath(previousArray, false, false) ;
                    previousArray = nextArray;
                }
                else
                {
                    nextArray = DrawPath(previousArray, false, true);
                    previousArray = nextArray;
                }
            }

            /*Vector3[] pArray = DrawPath(nodes, true, true);
            Vector3[] TArray = DrawPath(pArray, false, false);
            Vector3[] FArray = DrawPath(TArray, false, false);
            Vector3[] GArray = DrawPath(FArray, false, false);
            Vector3[] HArray = DrawPath(GArray, false, false);
            Vector3[] JArray = DrawPath(HArray, false, true);*/

            /*Vector3 Pa = DrawLerp(nodeArray[0], nodeArray[1], true, true, Color.yellow);
            Vector3 Pb = DrawLerp(nodeArray[1], nodeArray[2], true, true, Color.yellow);
            Vector3 Pc = DrawLerp(nodeArray[2], nodeArray[3], true, true, Color.yellow);
            Vector3 Pd = DrawLerp(nodeArray[3], nodeArray[4], true, true, Color.yellow);
            Vector3 Pe = DrawLerp(nodeArray[4], nodeArray[5], true, true, Color.yellow);

            Vector3 Ta = DrawLerp(Pa, Pb, true, false, Color.green);
            Vector3 Tb = DrawLerp(Pb, Pc, true, false, Color.green);
            Vector3 Tc = DrawLerp(Pc, Pd, true, false, Color.green);
            Vector3 Td = DrawLerp(Pd, Pe, true, false, Color.green);

            Vector3 Fa = DrawLerp(Ta, Tb, true, false, Color.red);
            Vector3 Fb = DrawLerp(Tb, Tc, true, false, Color.red);
            Vector3 Fc = DrawLerp(Tc, Td, true, false, Color.red);

            Vector3 Ga = DrawLerp(Fa, Fb, true, false, Color.blue);
            Vector3 Gb = DrawLerp(Fb, Fc, true, false, Color.blue);

            Vector3 Ha = DrawLerp(Ga, Gb, true, true, Color.white);*/
        }
    }

    private Vector3[] DrawPath(Vector3[] array, bool line, bool sphere)
    {
        Vector3[] pathArray = new Vector3[array.Length - 1];
        for (int i = 0; i < array.Length - 1; i++)
        {
            Vector3 path = DrawLerp(array[i], array[i + 1], line, sphere, Color.yellow);
            pathArray[i] = path;
        }

        return pathArray;
    }
    private Vector3 DrawLerp(Vector3 pointA, Vector3 pointB, bool drawLine, bool drawSphere, Color gizmosColor)
    {
        Gizmos.color = gizmosColor;
        Vector3 pointAB = GetLerp(pointA, pointB);

        if (drawLine)
        {
            VisualizeLine(pointA, pointB);
        }
        if (drawSphere)
        {
            VisualizeSphere(pointAB);
        }

        return pointAB;
    }
    private Vector3 GetLerp(Vector3 pointA, Vector3 pointB)
    {
        Vector3 pointAB = Vector3.Lerp(pointA, pointB, t);

        return pointAB;
    }
    private void VisualizeLine(Vector3 pointA, Vector3 pointB)
    {
        Gizmos.DrawLine(pointA, pointB);
    }
    private void VisualizeSphere(Vector3 pointAB)
    {
        Gizmos.DrawSphere(pointAB, 0.5f);
    }
}