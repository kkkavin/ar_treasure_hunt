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
    public AudioSource arrowAudioSource; // The single "speaker" attached to the arrow
    public AudioClip hitSoundClip;       // The actual "thud" audio file

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        // FIX 1: Turn off interpolation while docked in the bow
        rb.interpolation = RigidbodyInterpolation.None;

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
        // FIX 2: Keep it off while we manually move the arrow backward
        rb.interpolation = RigidbodyInterpolation.None;

        transform.SetParent(stringTarget);
        transform.localPosition = Vector3.zero;
        currentPullAmount = 0f;

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

        // Change this line inside your OnRelease() function
        rb.interpolation = RigidbodyInterpolation.Extrapolate;

        float pullRatio = currentPullAmount / maxPullDistance;
        float appliedForce = Mathf.Lerp(minShootForce, maxShootForce, pullRatio);

        rb.AddForce(transform.forward * appliedForce);

        if (arrowAudioSource != null)
        {
            arrowAudioSource.PlayOneShot(arrowAudioSource.clip);
        }

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
        rb.interpolation = RigidbodyInterpolation.None;

        transform.SetParent(bowString);
        transform.position = bowString.position;
        transform.rotation = bowString.rotation;

        isReleased = false;
        isBeingPulled = false;
        currentPullAmount = 0f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            // 1. Tell the single speaker to play the thud clip once!
            if (arrowAudioSource != null && hitSoundClip != null)
            {
                arrowAudioSource.PlayOneShot(hitSoundClip);
            }

            // 2. Stop all physical movement
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            // 3. Parent the arrow to the target
            transform.SetParent(collision.transform);
            this.enabled = false;

            // 4. Spawn a new arrow
            FindObjectOfType<ArrowSpawner>().SpawnArrow();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. If we hit the bubble...
        if (other.gameObject.CompareTag("Target"))
        {
             // 2. Turn off the smoothing BEFORE the real crash!
             if (rb != null)
             {
                 rb.interpolation = RigidbodyInterpolation.None;
             }
        }
    }
}