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
    public int priority = 0;
    public bool onDollyCart = false;
    private CinemachineDollyCart cart;
    // Start is called before the first frame update
    void Start()
    {
        mainBrain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineBrain>();
        collider = gameObject.GetComponent<Collider>();
        if(onDollyCart)
        {
            cart = newCam.GetComponentInParent<CinemachineDollyCart>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (priority > mainBrain.ActiveVirtualCamera.Priority)
            {
                if (onDollyCart)
                {
                    cart.enabled = true;
                    cart.m_Position = 0;
                }
                mainBrain.ActiveVirtualCamera.Priority = 0;
                newCam.Priority = priority;
            }
            // Reserved for the first/start camera
            else if (priority == 100)
            {
                mainBrain.ActiveVirtualCamera.Priority = 0;
                newCam.Priority = 1;
            }
        }
    }
}
