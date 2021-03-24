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
    private GameObject[] players;
    private BaseAI[] AIs;
    [SerializeField]
    private int currentLap;
    [SerializeField]
    private bool execute;
    private CameraStateChanger[] cameraDetectors;
    // Start is called before the first frame update
    void Start()
    {
        mainBrain = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineBrain>();
        collider = gameObject.GetComponent<Collider>();
        if(onDollyCart)
        {
            cart = newCam.GetComponentInParent<CinemachineDollyCart>();
        }
        players = GameObject.FindGameObjectsWithTag("Player");
        System.Array.Resize(ref AIs, players.Length);
        for (var i = 0; i < players.Length; i++)
        {
            AIs[i] = players[i].GetComponent<BaseAI>();
        }
        GameObject[] helpCameras = GameObject.FindGameObjectsWithTag("CameraDetector");
        System.Array.Resize(ref cameraDetectors, helpCameras.Length);
        for (var i = 0; i < helpCameras.Length; i++)
        {
            cameraDetectors[i] = helpCameras[i].GetComponent<CameraStateChanger>();
        }
    }

    // To keep track of branching paths and not switch between cams dedicated to different paths,
    // each priority assigned has to be a multiple of 10, with branching paths adding a unique
    // number behind the priority. E.g. "142" puts the camera on branch 2
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            execute = false;
            // Makes sure the player toggling things is on the highest lap in the race.
            for (var i = 0; i < players.Length; i++)
            {
                if(AIs[i].lap > currentLap)
                {
                    foreach (CameraStateChanger camDetector in cameraDetectors)
                    {
                        camDetector.currentLap = AIs[i].lap;
                    }
                    i = 0;
                }
                if(players[i] == other.gameObject && AIs[i].lap == currentLap)
                {
                    Debug.Log(players[i] + " is on lap " + AIs[i].lap + " and the race is on lap " + currentLap);
                    execute = true;
                }
            }
            if (execute)
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
                if (priority == 100)
                {
                    mainBrain.ActiveVirtualCamera.Priority = 0;
                    newCam.Priority = 10;
                }
            }
        }
    }
}
