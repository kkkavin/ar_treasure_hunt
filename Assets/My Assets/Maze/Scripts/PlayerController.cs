using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public int score = 0;
    public bool isBoss;
    public bool ShowGameOver = false, enterBoss = false, gameCompleted = false;
    public float speed = 10f;
    public Rigidbody rb;
    private GameManager gm;

    void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }
    
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
            score += 1;
        }
        if (score == 4 && isBoss == false)
        {
            gm.enterBoss();
        }
        if (score == 4 && isBoss)
        {
            gm.finishGame();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ghost")
        {
            ShowGameOver = true;
            Debug.Log("Collision");
        }
    }
}
