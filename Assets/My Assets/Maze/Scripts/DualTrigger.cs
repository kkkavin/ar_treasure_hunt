using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DualTrigger : MonoBehaviour
{
    private bool isPlayerInside = false;
    private bool isGhostInside = false;
    public Animator ghostAnim;
    public UnityEvent Triggered;

    public void StopGhostAnimation()
    {
        if (ghostAnim != null)
        {
            ghostAnim.enabled = false; // Freezes the animation entirely
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
        if (other.CompareTag("Ghost"))
        {
            isGhostInside = true;
        }

        CheckConditions();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
        if (other.CompareTag("Ghost"))
        {
            isGhostInside = false;
        }
    }

    void CheckConditions()
    {
        if (isPlayerInside && isGhostInside)
        {
            Debug.Log("Both Player and Ghost are in the zone!");
            StopGhostAnimation();
            Triggered.Invoke();
        }
    }
}
