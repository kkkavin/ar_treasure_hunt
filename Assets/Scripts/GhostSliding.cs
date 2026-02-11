using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSliding : MonoBehaviour
{
    public Transform player; // Drag the Player object here in the Inspector
    public float speed = 2.0f;
    private Rigidbody rb;
    public bool caught = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() // Use FixedUpdate for smooth physics sliding
    {
        if (player != null && caught)
        {
            // Calculate the position to slide towards
            Vector3 targetPosition = Vector3.MoveTowards(transform.position, player.position, speed * Time.fixedDeltaTime);
            
            // Move the Rigidbody so it interacts with walls/colliders
            if (player != transform)
            {
                rb.MovePosition(targetPosition);
            }
        }
    }
    public void caughtfunc()
    {
        caught = true;
    }
}