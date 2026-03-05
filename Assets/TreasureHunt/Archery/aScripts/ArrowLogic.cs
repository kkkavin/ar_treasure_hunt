using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowLogic : MonoBehaviour
{
    public BowStringVisual bowStringScript;
    [Header("Shooting Settings")]
    public float minShootForce = 500f;
    public float maxShootForce = 3500f;
    public float maxPullDistance = 1.5f;
    public float pullSensitivity = 0.005f;

    [Header("References")]
    public Transform bowString;
    public Rigidbody rb;
    private ArcheryGameManager gameManager;

    [Header("Reset Settings")]
    public float fallThreshold = -5f;

    [Header("Hit Settings")]
    [Tooltip("How far the arrow should sit inside the target. Tweak until only the tip is embedded.")]
    public float embedDepth = 0.1f;

    private bool isReleased = false;
    private bool isBeingPulled = false;
    private Vector2 startTouchPos;
    private float currentPullAmount = 0f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    public bool hasHit = false;

    [Header("Audio")]
    public AudioSource arrowAudioSource;
    public AudioClip hitSoundClip;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<ArcheryGameManager>();

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        if (isReleased && !hasHit && rb != null && rb.velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    void Update()
    {
        if (gameManager != null && gameManager.IsPaused)
        {
            return;
        }

        if (transform.position.y < fallThreshold)
        {
            ResetArrow();
        }

        if (Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame && !isReleased)
        {
            isBeingPulled = true;
            startTouchPos = Pointer.current.position.ReadValue();
            OnPullStart(bowString);
        }

        if (isBeingPulled && Pointer.current.press.isPressed)
        {
            Vector2 currentTouchPos = Pointer.current.position.ReadValue();
            float dragDistance = (startTouchPos.y - currentTouchPos.y) * pullSensitivity;
            currentPullAmount = Mathf.Clamp(dragDistance, 0f, maxPullDistance);
            transform.localPosition = new Vector3(0, 0, -currentPullAmount);
        }

        if ((Pointer.current.press.wasReleasedThisFrame || !Pointer.current.press.isPressed) && isBeingPulled)
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

        // Cancel shot if barely pulled
        if (currentPullAmount < 0.1f)
        {
            ResetArrow();
            return;
        }

        isReleased = true;

        transform.SetParent(null);
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

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
        if (!collision.collider || collision.collider.isTrigger) return;

        if (collision.gameObject.CompareTag("Target"))
        {
            ContactPoint contact = collision.GetContact(0);
            Vector3 contactPoint = contact.point;
            Vector3 contactNormal = contact.normal;

            ProcessHit(collision.collider, contactPoint, contactNormal);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            if (rb != null)
                rb.interpolation = RigidbodyInterpolation.None;
        }
    }

    void ProcessHit(Collider targetCollider, Vector3 contactPoint, Vector3 contactNormal)
    {
        // LOCK 1: Immediate exit if already hit
        if (hasHit) return;
        hasHit = true; 

        // LOCK 2: Kill the collider immediately so the moving target 
        // literally cannot touch this arrow anymore.
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // LOCK 3: Total Physics Shutdown
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.None;
            // Stop the engine from even looking at this rigidbody
            rb.detectCollisions = false; 
        }

        // --- Rest of your positioning logic ---
        if (gameManager != null && Camera.main != null)
        {
            float dist = Vector3.Distance(Camera.main.transform.position, targetCollider.transform.position);
            if (dist < 1.0f) return; // Simplified proximity check
        }

        // Stick to target
        transform.position = contactPoint + (contactNormal * embedDepth);
        transform.rotation = GetStableRotation(-contactNormal);

        if (gameManager != null && gameManager.CurrentLevel == 2)
        {
            transform.SetParent(targetCollider.transform, true);
        }

        // Audio and Scoring
        if (arrowAudioSource != null && hitSoundClip != null)
            arrowAudioSource.PlayOneShot(hitSoundClip);

        gameManager?.RegisterHit();
        
        // Spawn next arrow
        FindObjectOfType<ArrowSpawner>()?.SpawnArrow();
    }
    /// <summary>
    /// Calculate a stable rotation that points the arrow forward along the contact normal,
    /// avoiding gimbal lock and flipping at edge cases.
    /// </summary>
    Quaternion GetStableRotation(Vector3 normal)
    {
        normal = normal.normalized;

        // define a stable "right" vector perpendicular to the normal
        Vector3 right = Vector3.Cross(normal, Vector3.up);

        // if normal is too close to up/down, use a different reference
        if (right.sqrMagnitude < 0.01f)
        {
            right = Vector3.Cross(normal, Vector3.right);
        }

        right = right.normalized;

        // compute a stable "up" perpendicular to both
        Vector3 up = Vector3.Cross(right, normal).normalized;

        // build rotation matrix: forward = normal, right = right, up = up
        return Quaternion.LookRotation(normal, up);
    }
}