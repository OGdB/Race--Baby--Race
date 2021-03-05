using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node : MonoBehaviour
{
    public Node[] nextNodes;

    private void OnValidate()
    {
        if (nextNodes.Length == 0)
        {
            System.Array.Resize(ref nextNodes, 1);
            int thisObject = transform.GetSiblingIndex();
            nextNodes[0] = transform.parent.GetChild(thisObject + 1).gameObject.GetComponent<Node>();
        }
    }
}