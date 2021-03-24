using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This code is messy and shit but I didn't want to clutter my AI and this is just cosmetic so...
 * I don't really care.
 * 
 * - Pelle
 */

[RequireComponent(typeof(StupidAI), typeof(AudioSource))]
public class StupidAIAudio : MonoBehaviour
{
    public GameObject hitmarker;
    public float hitmarkerTreshold;
    public AudioClip itemUseSound;
    public AudioClip reverseSound;

    private StupidAI ai;
    private AudioSource source;

    private Item lastItem;
    private bool lastGoBackwards;

    private void Start()
    {
        ai = GetComponent<StupidAI>();
        source = GetComponent<AudioSource>();

        if (ai.baseAI == null)
        {
            ai.baseAI = GetComponent<BaseAI>();
        }

        lastItem = ai.baseAI.GetCurrentItem();
        lastGoBackwards = ai.goBackwards;
    }

    private void Update()
    {
        //item throw sound
        if(lastItem != ai.baseAI.GetCurrentItem() && lastItem != Item.None)
        {
            source.PlayOneShot(itemUseSound);
        }
        lastItem = ai.baseAI.GetCurrentItem();

        if(lastGoBackwards != ai.goBackwards)
        {
            if(lastGoBackwards == false)
            {
                source.clip = reverseSound;
                source.Play();
            }
            else
            {
                source.Stop();
            }
        }
        lastGoBackwards = ai.goBackwards;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.relativeVelocity.magnitude > hitmarkerTreshold)
        {
            Destroy(Instantiate(hitmarker, 
                collision.contacts[0].point + Vector3.up * 0.5f + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 0.5f, 
                Quaternion.identity), 0.25f);
        }
    }
}
