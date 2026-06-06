using UnityEngine;

public class ProximityWarning : MonoBehaviour
{
    [Header("UI References")]
    public GameObject warningPanel;
    public GameObject aim;

    [Header("Settings")]
    public float minimumSafeDistance = 1.5f;

    private Transform archeryTarget;
    private Collider arrowCollider;
    private GameManager gameManager;

    // kept so that other scripts (ArrowLogic) can read the value
    public static float SafeDistance { get; private set; }

    // state helpers
    private bool wasJustTooClose = false;      // previous–frame proximity
    private bool pausedByProximity = false;    // true while we have paused ourselves

    void Awake()
    {
        SafeDistance = minimumSafeDistance;
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        SafeDistance = minimumSafeDistance; // allow runtime tweaking

        // attempt to locate the current target/arrow if we lost them
        if (archeryTarget == null || !archeryTarget.gameObject.activeInHierarchy)
        {
            var found = GameObject.FindGameObjectsWithTag("Target");
            if (found.Length > 0)
                archeryTarget = found[0].transform;
        }

        if (arrowCollider == null || !arrowCollider.gameObject.activeInHierarchy)
        {
            var a = GameObject.FindGameObjectWithTag("Arrow");
            if (a != null)
                arrowCollider = a.GetComponent<Collider>();
        }

        // if we still don't have a target there is nothing to do
        if (archeryTarget == null) return;

        float currentDistance = Vector3.Distance(transform.position, archeryTarget.position);
        bool tooClose = currentDistance < minimumSafeDistance;

        bool currentlyPaused = gameManager != null && gameManager.IsPaused;

        // --- automatic pause/resume --------------------------------
        if (tooClose && !wasJustTooClose)
        {
            if (gameManager != null && !currentlyPaused)
            {
                gameManager.PauseGame();
                pausedByProximity = true;
            }
        }
        else if (!tooClose && wasJustTooClose)
        {
            if (gameManager != null && pausedByProximity)
            {
                // only resume if we're not showing a level‑complete panel
                if (!gameManager.finishedLevel1.activeSelf &&
                    !gameManager.finishedLevel2.activeSelf)
                {
                    gameManager.ResumeGame();
                }
                pausedByProximity = false;
            }
        }

        wasJustTooClose = tooClose;

        // --- warning panel / aim ------------------------------------
        // show the warning if we're too close *and* either the game is
        // not paused or the pause was caused by proximity itself
        bool showWarning = tooClose && (!currentlyPaused || pausedByProximity);

        if (warningPanel != null)
            warningPanel.SetActive(showWarning);
        if (aim != null)
            aim.SetActive(!tooClose);

        // --- collider enabling/disabling ----------------------------
        Collider[] cols = archeryTarget.GetComponents<Collider>();
        foreach (var c in cols)
            if (c != null) c.enabled = !tooClose;

        if (arrowCollider != null)
            arrowCollider.enabled = !tooClose;
    }
}