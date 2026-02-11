using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public Rigidbody rb;
    void Update()
    {
        // This reads both a physical gamepad AND your On-Screen Stick
        Vector2 input = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;

        rb.velocity = new Vector3(input.x, 0, input.y) * speed;
    }
}
