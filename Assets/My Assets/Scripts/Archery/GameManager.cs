using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Level Containers")]
    public GameObject level1Container;
    public GameObject level2Container;

    [Header("Game Rules")]
    public int hitsToAdvance = 3;
    private int currentHits = 0;
    private int currentLevel = 1;

    [Header("Physics Settings")]
    public float hitCooldown = 0.5f; // Minimum time between counted hits
    private float lastHitTime = -1f;

    [Header("UI Elements")]
    public GameObject finishedLevel1;
    public GameObject finishedLevel2;

    [Header("Bow Setup")]
    public GameObject level1BowObject; // The child object with the Lvl 1 SkinnedMesh
    public GameObject level2BowObject; // The child object with the Lvl 2 SkinnedMesh

    // pause‑management fields
    bool isPaused;
    float baseFixedDeltaTime;

    public bool IsPaused => isPaused;          // read‑only property for other scripts

    void Awake()
    {
        baseFixedDeltaTime = Time.fixedDeltaTime; // cache default fixed timestep
    }

    void Start()
    {
        LoadLevel(1);
    }

    public void RegisterHit()
    {
        if (isPaused) return;                  // ignore input when paused

        if (Time.time < lastHitTime + hitCooldown) return;

        currentHits++;
        lastHitTime = Time.time;

        Debug.Log($"Hit Registered! Total: {currentHits}");

        if (currentHits >= hitsToAdvance)
        {
            // Delay pause so the hit sound finishes playing (~0.5 sec)
            Invoke(nameof(AdvanceLevel), 0.5f);
        }
    }

    void AdvanceLevel()
    {
        currentHits = 0;

        if (currentLevel == 1)
        {
            Debug.Log("Advancing to Level 2!");
            finishedLevel1.SetActive(true);
            PauseGame();                       // pause when showing panel
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
        level1Container.SetActive(level == 1);
        level1BowObject.SetActive(level == 1);

        level2Container.SetActive(level == 2);
        level2BowObject.SetActive(level == 2);

        ClearOldArrows();
    }

    void ClearOldArrows()
    {
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject arrow in arrows)
        {
            if (arrow.transform.parent != null)
            {
                Destroy(arrow);
            }
        }
    }

    // pause helpers
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

    // callbacks for the level‑complete UI buttons
    public void ResumeFromFinishedLevel1()
    {
        finishedLevel1.SetActive(false);
        LoadLevel(2);
        ResumeGame();
    }

    public void ResumeFromFinishedLevel2()
    {
        finishedLevel2.SetActive(false);
        LoadLevel(1);               // restart or whatever you want
        ResumeGame();
    }
}