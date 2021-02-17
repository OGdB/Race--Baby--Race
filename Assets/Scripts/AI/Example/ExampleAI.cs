using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class ExampleAI : MonoBehaviour
{
    public float confirmationDistance = 2;

    private BaseAI baseAI;
    private Vector3[] nodes;
    private int currentNode = 0;
    

    private void Start()
    {
        baseAI = GetComponent<BaseAI>();
        nodes = baseAI.GetNodes();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, nodes[currentNode]) < confirmationDistance)
        {
            currentNode = Mathf.RoundToInt(Mathf.Repeat(++currentNode, nodes.Length));
        }

        Vector3 dir = transform.InverseTransformDirection((nodes[currentNode] - transform.position).normalized);

        Debug.DrawRay(transform.position, transform.TransformDirection(dir) * 3, Color.red);

        baseAI.SetDirection(new Vector2(dir.x, 1));
    }
}
