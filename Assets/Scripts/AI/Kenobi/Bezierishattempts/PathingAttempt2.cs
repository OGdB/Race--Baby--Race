using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingAttempt2 : MonoBehaviour
{
    [SerializeField]
    private List<Transform> nodeList = new List<Transform>();
    [SerializeField]
    private float t;
    [SerializeField]
    private float speed = 1f;

    private void Update()
    {
        t = (t + Time.deltaTime / speed) % 1f;
    }
    private Vector3 QuadraticLerp(Vector3 pointA, Vector3 pointB, Vector3 pointC, float t)
    {
        Vector3 pointAB = Vector3.Lerp(pointA, pointB, t);
        Vector3 pointBC = Vector3.Lerp(pointB, pointC, t);

        return Vector3.Lerp(pointAB, pointBC, t);
    }
    private Vector3 CubicLerp(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, float t)
    {
        Vector3 pointABC = QuadraticLerp(pointA, pointB, pointC, t);
        Vector3 pointBCD = QuadraticLerp(pointB, pointC, pointD, t);

        return Vector3.Lerp(pointABC, pointBCD, t);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector3 cubicLerp = CubicLerp(nodeList[0].position, nodeList[1].position, nodeList[2].position, nodeList[3].position, t);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(cubicLerp, 0.5f);
        }
    }
}
