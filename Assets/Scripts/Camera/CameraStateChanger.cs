using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraStateChanger : MonoBehaviour
{
    private Collider collider;
    private CinemachineBrain mainBrain;
    private CinemachineVirtualCamera mainCam;
    public CinemachineVirtualCamera newCam;
    public bool firstOnly = true;
    // Start is called before the first frame update
    void Start()
    {
        mainBrain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineBrain>();
        collider = gameObject.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (firstOnly)
            {
                if(other.GetComponent<BaseAI>().position == 1)
                {
                    mainBrain.ActiveVirtualCamera.Priority = 0;
                    newCam.Priority = 1;
                }
            }
            else
            {
                mainBrain.ActiveVirtualCamera.Priority = 0;
                newCam.Priority = 1;
            }
        }
    }
}
