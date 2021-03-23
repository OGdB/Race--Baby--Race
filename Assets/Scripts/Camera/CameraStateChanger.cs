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

    // To keep track of branching paths and not switch between cams dedicated to different paths,
    // each priority assigned has to be a multiple of 10, with branching paths adding a unique
    // number behind the priority. E.g. "142" puts the camera on branch 2
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // Only switch the camera if the priority is higher, & the priorities are on the same branch or one of the priorities is on the main branch.
            if (priority / 10 > mainBrain.ActiveVirtualCamera.Priority / 10 &&
                (priority % 10 == mainBrain.ActiveVirtualCamera.Priority % 10
                || priority % 10 == 0
                || mainBrain.ActiveVirtualCamera.Priority % 10 == 0))
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
            else if (priority % 1000 == 100)
            {
                //// Increases the priority of all the Camera Detectors by 1000, so... Wait no that doesn't help anyone
                //foreach (GameObject cameraDetector in GameObject.FindGameObjectsWithTag("CameraDetector"))
                //{
                //    cameraDetector.GetComponent<CameraStateChanger>().priority += 1000;
                //}
                mainBrain.ActiveVirtualCamera.Priority = 0;
                newCam.Priority = 1;
            }
        }
    }
}
