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
    private GameManager gameManager;

    [Header("Reset Settings")]
    public float fallThreshold = -5f;

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
        gameManager = FindObjectOfType<GameManager>(); // cache

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        // Check if game is paused – block all input if so
        if (gameManager != null && gameManager.IsPaused)
        {
            return;
        }

        // Check if it fell out of bounds
        if (transform.position.y < fallThreshold)
        {
            ResetArrow();
        }

        if (Pointer.current == null) return;

        // 1. TOUCH DOWN
        if (Pointer.current.press.wasPressedThisFrame && !isReleased)
        {
            isBeingPulled = true;
            startTouchPos = Pointer.current.position.ReadValue();
            OnPullStart(bowString);
        }

        // 2. DRAGGING
        if (isBeingPulled && Pointer.current.press.isPressed)
        {
            Vector2 currentTouchPos = Pointer.current.position.ReadValue();
            float dragDistance = (startTouchPos.y - currentTouchPos.y) * pullSensitivity;
            currentPullAmount = Mathf.Clamp(dragDistance, 0f, maxPullDistance);
            transform.localPosition = new Vector3(0, 0, -currentPullAmount);
        }

        // 3. TOUCH UP
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

    void ProcessHit(Collider targetCollider)
    {
        if (hasHit)
            return;

        // distance check – ignore hits while camera is too close
        if (Camera.main != null)
        {
            float dist = Vector3.Distance(Camera.main.transform.position,
                                          targetCollider.transform.position);
            if (dist < ProximityWarning.SafeDistance)
                return;        // bail out without parenting/spawning
        }

        hasHit = true;

        if (arrowAudioSource != null && hitSoundClip != null)
            arrowAudioSource.PlayOneShot(hitSoundClip);

        gameManager?.RegisterHit();

        transform.SetParent(targetCollider.transform);
        rb.isKinematic = true;

        // spawn next arrow only for real collisions
        ArrowSpawner sp = FindObjectOfType<ArrowSpawner>();
        if (sp != null)
            sp.SpawnArrow();

        GetComponent<Collider>().enabled = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target") &&
            !collision.collider.isTrigger)          // ignore triggers here
        {
            ProcessHit(collision.collider);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            // the spherical “proximity” trigger – only use it to disable
            // interpolation, do *not* count it as a hit or parent the arrow
            if (rb != null)
                rb.interpolation = RigidbodyInterpolation.None;
        }
    }
}