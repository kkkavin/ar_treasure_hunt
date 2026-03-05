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
        if (hasHit) return;

        if (gameManager != null && Camera.main != null)
        {
            float dist = Vector3.Distance(Camera.main.transform.position, targetCollider.transform.position);
            if (dist < ProximityWarning.SafeDistance) return;
        }

        hasHit = true;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        const float penetration = 0.02f;
        Vector3 finalPosition = contactPoint + contactNormal * penetration;

        // stable rotation: orient arrow into the surface instead of outwards
        // normals point out of the surface, so we flip it to make the arrow tip face the
        // impact direction. this prevents a 180° y‑axis flip when the solver chooses an
        // arbitrary up vector later.
        Quaternion finalRotation = GetStableRotation(-contactNormal);

        // slide the arrow forward along its forward axis so only the tip is embedded.
        // `embedDepth` is adjustable from the inspector; positive values move the arrow
        // into the surface direction (which is `finalRotation.forward`).
        finalPosition += (finalRotation * Vector3.forward) * embedDepth;

        // always detach first and set world pose
        transform.SetParent(null, false);
        transform.position = finalPosition;
        transform.rotation = finalRotation;

        // in level 2, parent with world pose preserved so arrow follows animation
        if (gameManager != null && gameManager.CurrentLevel == 2)
        {
            transform.SetParent(targetCollider.transform, true);  // worldPositionStays = true
        }

        if (arrowAudioSource != null && hitSoundClip != null)
            arrowAudioSource.PlayOneShot(hitSoundClip);

        FindObjectOfType<ArrowSpawner>()?.SpawnArrow();
        gameManager?.RegisterHit();

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
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