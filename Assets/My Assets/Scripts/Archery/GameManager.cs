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

    void Start()
    {
        // Start the game by loading Level 1
        LoadLevel(1);
    }

    public void RegisterHit()
    {
        // 1. Check if enough time has passed since the last hit
        // This prevents "double-tapping" from the same arrow in a single frame
        if (Time.time < lastHitTime + hitCooldown) return;

        currentHits++;
        lastHitTime = Time.time; // Update the timestamp
        
        Debug.Log($"Hit Registered! Total: {currentHits}");

        if (currentHits >= hitsToAdvance)
        {
            AdvanceLevel();
        }
    }

    void AdvanceLevel()
    {
        currentHits = 0; // Reset hits for the new level

        if (currentLevel == 1)
        {
            Debug.Log("Advancing to Level 2!");
            LoadLevel(2);
        }
        else if (currentLevel == 2)
        {
            Debug.Log("Game Completed!");
            // Implement win logic here (e.g., show a victory UI)
        }
    }

    void LoadLevel(int level)
    {
        currentLevel = level;

        // Toggle the visibility of the level containers
        if (level == 1)
        {
            level1Container.SetActive(true);
            level2Container.SetActive(false);
        }
        else if (level == 2)
        {
            level1Container.SetActive(false);
            level2Container.SetActive(true);
        }

        ClearOldArrows();
    }

    void ClearOldArrows()
    {
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject arrow in arrows)
        {
            // Only destroy the arrow if it is NOT attached to something (like the bow)
            if (arrow.transform.parent == null)
            {
                Destroy(arrow);
            }
        }
    }
}