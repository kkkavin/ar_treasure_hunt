using UnityEngine;

public class ball : MonoBehaviour
{
    public float throwForce = 8f;
    public float upwardForce = 2f;
    public float maxSwipeTime = 0.5f;

    [Header("AR Setup")]
    [Tooltip("Leave empty to just drop the ball into the world root, or assign an Environment container object.")]
    public Transform environmentParent; 

    [Header("Reset Settings")]
    public float resetYThreshold = -1f; // If ball falls below this Y value, it resets

    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private float swipeStartTime;

    private Rigidbody rb;
    private bool hasThrown = false;
    
    // Updated to handle AR Camera parenting
    private Transform initialParent;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Save the AR Camera (or whatever the initial parent is) and the local transforms
        initialParent = transform.parent;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
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

        // --- NEW: Unparent the ball from the AR Camera ---
        transform.SetParent(environmentParent); 
        // If environmentParent is unassigned (null), it just becomes a top-level world object, which is perfect for AR.

        Vector2 swipeDir = endTouchPos - startTouchPos;
        // Adjusted Z calculation for 3D space
        Vector3 force = new Vector3(swipeDir.x, swipeDir.y, swipeDir.y * 1.5f) * throwForce / 100f;
        
        // Make sure the force is applied relative to the camera's current rotation
        // so it always shoots "forward" from where you are looking
        Vector3 worldForce = initialParent.TransformDirection(force);
        worldForce.y += upwardForce; // Keep the upward arc absolute

        rb.isKinematic = false;
        rb.AddForce(worldForce, ForceMode.Impulse);
        hasThrown = true;
    }

    public void ResetBall()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // --- NEW: Re-parent to the AR Camera ---
        transform.SetParent(initialParent);
        
        // Reset to original local position/rotation so it sits exactly where it started on screen
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        
        hasThrown = false;
    }
}