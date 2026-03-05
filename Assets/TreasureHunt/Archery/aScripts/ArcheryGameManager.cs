using UnityEngine;

public class ArcheryGameManager : MonoBehaviour
{
    [Header("Level Containers")]
    public GameObject level1Container;
    public GameObject level2Container;

    [Header("Game Rules")]
    public int hitsToAdvanceLevel1 = 3;
    public int hitsToAdvanceLevel2 = 3;
    private int currentHits = 0;
    private int currentLevel = 1;

    [Header("Physics Settings")]
    public float hitCooldown = 0.1f;  // reduced cooldown
    private float lastHitTime = -Mathf.Infinity;  // init to far past so first hit always counts

    [Header("UI Elements")]
    public GameObject finishedLevel1;
    public GameObject finishedLevel2;

    [Header("Bow Setup")]
    public GameObject level1BowObject;
    public GameObject level2BowObject;

    // pause‑management fields
    bool isPaused;
    float baseFixedDeltaTime;

    public bool IsPaused => isPaused;

    void Awake()
    {
        baseFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        LoadLevel(1);
    }

    public void RegisterHit()
    {
        if (isPaused) return;

        // only reject if cooldown hasn't elapsed since the *last hit*
        if (Time.time < lastHitTime + hitCooldown)
        {
            Debug.Log("Hit rejected – cooldown active");
            return;
        }

        currentHits++;
        lastHitTime = Time.time;

        Debug.Log($"Hit Registered! Total: {currentHits}");

        int hitsRequired = (currentLevel == 1) ? hitsToAdvanceLevel1 : hitsToAdvanceLevel2;

        if (currentHits >= hitsRequired)
        {
            Invoke(nameof(AdvanceLevel), 0.5f);
        }
    }

    void AdvanceLevel()
    {
        currentHits = 0;
        lastHitTime = -Mathf.Infinity;  // reset cooldown for new level

        if (currentLevel == 1)
        {
            Debug.Log("Advancing to Level 2!");
            finishedLevel1.SetActive(true);
            PauseGame();
        }
        else if (currentLevel == 2)
        {
            Debug.Log("Game Completed!");
            finishedLevel2.SetActive(true);
            PauseGame();
        }
    }

    void LoadLevel(int level)
    {
        currentLevel = level;
        currentHits = 0;
        lastHitTime = -Mathf.Infinity;  // reset cooldown when loading level

        level1Container.SetActive(level == 1);
        level1BowObject.SetActive(level == 1);

        level2Container.SetActive(level == 2);
        level2BowObject.SetActive(level == 2);

        ClearOldArrows();        // now destroys all arrows unconditionally
    }

    void ClearOldArrows()
    {
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject arrow in arrows)
        {
            Destroy(arrow);
        }
    }

    void SetPaused(bool paused)
    {
        if (isPaused == paused) return;
        isPaused = paused;

        Time.timeScale = paused ? 0f : 1f;
        Time.fixedDeltaTime = baseFixedDeltaTime * Time.timeScale;
        AudioListener.pause = paused;
    }

    public void PauseGame() => SetPaused(true);
    public void ResumeGame() => SetPaused(false);

    public void ResumeFromFinishedLevel1()
    {
        finishedLevel1.SetActive(false);
        LoadLevel(2);
        ResumeGame();
    }

    public void ResumeFromFinishedLevel2()
    {
        finishedLevel2.SetActive(false);
        LoadLevel(1);
        ResumeGame();
    }

    // these two methods are for the ImageTargetBehaviour events
    public void OnImageTargetFound()
    {
        // only enable the bow that belongs to the active level
        if (currentLevel == 1)
        {
            level1BowObject.SetActive(true);
            level2BowObject.SetActive(false);
        }
        else // level 2
        {
            level1BowObject.SetActive(false);
            level2BowObject.SetActive(true);
        }
    }

    public void OnImageTargetLost()
    {
        // hide both bows when the target disappears
        level1BowObject.SetActive(false);
        level2BowObject.SetActive(false);
    }

    // expose for other scripts
    public int CurrentLevel => currentLevel;

    public void goHome()
    {
        Time.timeScale = 1f; // unpause before going home
        HomeGameManager.Instance.ToggleHomepage(true);
    }
}