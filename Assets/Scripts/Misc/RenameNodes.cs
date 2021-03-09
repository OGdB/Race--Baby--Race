using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenameNodes : MonoBehaviour
{
    void OnValidate()
    {
        // If the name of the previous gameobject (if existent) is the same as the current gameobject
        for (int n = 0; n < transform.childCount; n++)
        {
            if (n - 1 != -1)
            {
                if (transform.GetChild(n).name == transform.GetChild(n - 1).name || transform.GetChild(n).name == transform.GetChild(n + 1).name)
                {
                    transform.GetChild(n).name = transform.GetChild(n).name + " (" + n + ")";
                }
            }
        }
    }
}
