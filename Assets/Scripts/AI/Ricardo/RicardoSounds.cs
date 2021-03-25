using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RicardoSounds : MonoBehaviour
{
    public AudioClip deathS;
    public AudioClip boosterS;
    public AudioClip itemUsedS;
    public AudioClip gotItemS;
    public AudioClip hazardHitS;

    public AudioSource audioS;
    private RicardoAI ai;
    private Item currItem;

    // Start is called before the first frame update
    void Start()
    {
        ai = gameObject.GetComponent<RicardoAI>();
        ai.baseAI = GetComponent<BaseAI>();
        currItem = ai.baseAI.GetCurrentItem();
    }
    private void Update()
    {
        //play sound on item used
        if (currItem != ai.baseAI.GetCurrentItem() && currItem != Item.None)
        {
            audioS.clip = itemUsedS;
            audioS.Play();
        }
        currItem = ai.baseAI.GetCurrentItem();
    }
    //play sound on death
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "grass")
        {
            audioS.clip = deathS;
            audioS.Play();
        }

    }
    //play sound on collision with boosters, hazards, item boxes
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Booster") && Random.value < 0.1f)//have a 50% chance of playing so it's not too irritating
        {
            audioS.clip = boosterS;
            audioS.Play();
        }
        if (ai.baseAI.GetCurrentItem() == Item.None && other.gameObject.CompareTag("ItemBox") )
        {
            audioS.clip = gotItemS;
            audioS.Play();
        }
        if (other.gameObject.CompareTag("Hazard") && !audioS.isPlaying)
        {
            audioS.clip = hazardHitS;
            audioS.Play();
        }
    }
}
