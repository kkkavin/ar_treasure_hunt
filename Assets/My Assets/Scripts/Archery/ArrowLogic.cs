using UnityEngine;
using UnityEngine.InputSystem; 

public class ArrowLogic : MonoBehaviour
{
    public BowStringVisual bowStringScript;
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

    [Header("Audio")]
    public AudioSource releaseSound; // Drag your Audio Source here in the Inspector

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

        // Safety check for input devices
        if (Pointer.current == null) return;

        // 1. TOUCH DOWN: Start the pull from ANYWHERE on the screen
        // We check '!isReleased' so you can't grab an arrow that is already flying
        if (Pointer.current.press.wasPressedThisFrame && !isReleased)
        {
            isBeingPulled = true;
            // Record exactly where the finger first touched the screen
            startTouchPos = Pointer.current.position.ReadValue(); 
            OnPullStart(bowString);
        }

        // 2. DRAGGING: Move the arrow back dynamically
        if (isBeingPulled && Pointer.current.press.isPressed)
        {
            Vector2 currentTouchPos = Pointer.current.position.ReadValue();
            
            // Calculate how far the finger moved DOWN the screen
            float dragDistance = (startTouchPos.y - currentTouchPos.y) * pullSensitivity;
            
            // Limit the pullback distance
            currentPullAmount = Mathf.Clamp(dragDistance, 0f, maxPullDistance);
            
            // Visually move the arrow backward
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

        // Tell the string to follow THIS arrow
        if (bowStringScript != null)
        {
            bowStringScript.arrowNock = this.transform;
        }
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

        // Play the twang sound effect!
        if (releaseSound != null)
        {
            releaseSound.Play();
        }
        
        Debug.Log($"Arrow released with force: {appliedForce}");

        // Tell the string to snap back to the center
        if (bowStringScript != null)
        {
            bowStringScript.arrowNock = null;
        }
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

    void OnCollisionEnter(Collision collision)
    {
        // Check if the object we hit has the "Target" tag
        if (collision.gameObject.CompareTag("Target"))
        {
            // 1. Stop all physical movement instantly
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            // 2. Parent the arrow to the target so it tracks with the AR image
            transform.SetParent(collision.transform);

            // 3. Disable this script so the player cannot drag the stuck arrow again
            this.enabled = false;

            // 4. Find the Spawner and tell it to create a new arrow
            FindObjectOfType<ArrowSpawner>().SpawnArrow();
        }
    }
}