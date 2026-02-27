using UnityEngine;
using UnityEngine.InputSystem; 

public class ArrowLogic : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float minShootForce = 500f;       // Force if you barely tap it
    public float maxShootForce = 3500f;      // Force if you pull it all the way back
    public float maxPullDistance = 1.5f;     // How far back the arrow can physically move
    public float pullSensitivity = 0.005f;   // Converts screen pixels to world 3D distance
    
    [Header("References")]
    public Transform bowString; 
    public Rigidbody rb;
    
    [Header("Reset Settings")]
    public float fallThreshold = -5f;
    
    private bool isReleased = false;
    private bool isBeingPulled = false;
    private Vector2 startTouchPos;
    private float currentPullAmount = 0f;
    
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; 
        
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        // Check if it fell out of bounds
        if (transform.position.y < fallThreshold)
        {
            ResetArrow();
        }

        if (Pointer.current == null) return;

        // 1. TOUCH DOWN: Start the pull
        if (Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isBeingPulled = true;
                    startTouchPos = screenPos; // Remember exactly where the finger started
                    OnPullStart(bowString);
                }
            }
        }

        // 2. DRAGGING: Move the arrow back dynamically
        if (isBeingPulled && Pointer.current.press.isPressed)
        {
            Vector2 currentTouchPos = Pointer.current.position.ReadValue();
            
            // Calculate how far the finger moved DOWN the screen (Y axis)
            float dragDistance = (startTouchPos.y - currentTouchPos.y) * pullSensitivity;
            
            // Clamp the value so you can't pull it back infinitely
            currentPullAmount = Mathf.Clamp(dragDistance, 0f, maxPullDistance);
            
            // Move the arrow backward visually (Assuming -Z is "back" on your bow model)
            transform.localPosition = new Vector3(0, 0, -currentPullAmount);
        }

        // 3. TOUCH UP: Release the arrow
        if (Pointer.current.press.wasReleasedThisFrame && isBeingPulled)
        {
            isBeingPulled = false;
            OnRelease();
        }
    }

    public void OnPullStart(Transform stringTarget)
    {
        if (stringTarget == null) return;
        
        isReleased = false;
        rb.isKinematic = true;
        transform.SetParent(stringTarget); 
        transform.localPosition = Vector3.zero; 
        currentPullAmount = 0f;
    }

    public void OnRelease()
    {
        if (isReleased) return;
        isReleased = true;
        transform.SetParent(null); 
        rb.isKinematic = false; 
        
        // Calculate the dynamic force using the ratio of how far it was pulled
        float pullRatio = currentPullAmount / maxPullDistance; 
        float appliedForce = Mathf.Lerp(minShootForce, maxShootForce, pullRatio);
        
        rb.AddForce(transform.forward * appliedForce); 
        
        Debug.Log($"Arrow released with force: {appliedForce}");
    }

    public void ResetArrow()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetParent(bowString); 
        transform.position = bowString.position;
        transform.rotation = bowString.rotation;

        isReleased = false;
        isBeingPulled = false;
        currentPullAmount = 0f;
    }
}