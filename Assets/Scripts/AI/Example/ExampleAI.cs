using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class ExampleAI : MonoBehaviour
{
    public float confirmationDistance = 2;

    private BaseAI baseAI;
    [SerializeField]
    private Node currentNode;    

    private void Start()
    {
        baseAI = GetComponent<BaseAI>();
        currentNode = baseAI.GetFirstNode();
        baseAI.AimBack(false);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, currentNode.transform.position) < confirmationDistance)
        {
            if (currentNode.nextNodes.Length > 1)
            {
                int randomPath = Random.Range(0, currentNode.nextNodes.Length);
                currentNode = currentNode.nextNodes[randomPath];
            }
            else
            {
                currentNode = currentNode.nextNodes[0];
            }
        }

        Vector3 dir = transform.InverseTransformDirection((currentNode.transform.position - transform.position).normalized);

        Debug.DrawRay(transform.position, transform.TransformDirection(dir) * 3, Color.red);

        baseAI.SetDirection(new Vector2(dir.x, 1));

        if(baseAI.GetCurrentItem() != Item.None)
        {
            baseAI.UseItem();
        }

        baseAI.SetName(this.GetType().ToString());
    }
}
