using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform cam;

    private void Update()
    {
        if (cam == null)
        {
            if(Camera.main != null)
            {
                cam = Camera.main.transform;
            }
        }
        else
        {
            transform.LookAt(cam);
        }
    }
}
