using UnityEngine;

public class ball : MonoBehaviour
{
    public float throwForce = 8f;
    public float upwardForce = 2f;
    public float maxSwipeTime = 0.5f;

    [Header("Reset Settings")]
    public float resetYThreshold = -1f; // If ball falls below this Y value, it resets

    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private float swipeStartTime;

    private Rigidbody rb;
    private bool hasThrown = false;
    private Vector3 initialPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
    }

    void Update()
    {
        // Condition 1: Check if the ball has fallen below the Y threshold
        if (hasThrown && transform.position.y < resetYThreshold)
        {
            ResetBall();
        }

        if (hasThrown) return;

#if UNITY_EDITOR
        MouseSwipe();
#else
        TouchSwipe();
#endif
    }

    void TouchSwipe()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) { startTouchPos = touch.position; swipeStartTime = Time.time; }
            if (touch.phase == TouchPhase.Ended) { endTouchPos = touch.position; ThrowBall(); }
        }
    }

    void MouseSwipe()
    {
        if (Input.GetMouseButtonDown(0)) { startTouchPos = Input.mousePosition; swipeStartTime = Time.time; }
        if (Input.GetMouseButtonUp(0)) { endTouchPos = Input.mousePosition; ThrowBall(); }
    }

    void ThrowBall()
    {
        float swipeTime = Time.time - swipeStartTime;
        if (swipeTime > maxSwipeTime) return;

        Vector2 swipeDir = endTouchPos - startTouchPos;
        // Adjusted Z calculation for 3D space
        Vector3 force = new Vector3(swipeDir.x, swipeDir.y, swipeDir.y * 1.5f) * throwForce / 100f;
        force.y += upwardForce;

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
        hasThrown = true;
    }

    public void ResetBall()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        hasThrown = false;
    }
}
