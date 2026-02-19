using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public bool isBoss;
    public GameManager gm;
    public float speed = 10f;
    public Rigidbody rb;
    void Update()
    {
        // This reads both a physical gamepad AND your On-Screen Stick
        Vector2 input = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;

        rb.velocity = new Vector3(input.x, 0, input.y) * speed;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Power"))
        {
            collision.gameObject.SetActive(false);
            gm.score += 1;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ghost")
        {
            if (gm != null) gm.ShowGameOver();
        }
        if (gm.score == 4 && isBoss == false)
        {
            if (gm != null) gm.enterBoss();
        }
        else if (gm.score == 4 && isBoss)
        {
            if (gm != null) gm.gameCompleted();
        }
    }
}
