using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl; // Required to restart the game

public class GameManager : MonoBehaviour
{
    public PlayerController pc;
    
    [Header("Level Prefabs")]
    public GameObject level1Prefab; // Drag the 'Level 1' Prefab from Assets here
    public GameObject level2Prefab; // Drag the 'Level 2' Prefab from Assets here
    
    [Header("UI & References")]
    public GameObject gameOverPanel;
    public GameObject bossPanel;
    public GameObject winPanel;
    public Transform mazeParent; 
    public GameObject currentLevelInstance;

    private int currentLevel = 1;

    void Start()
    {
        // Initialize Level 1 if nothing is there
        if(currentLevelInstance == null) RestartCurrentLevel();
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f; // Always unpause first

        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        // Pick the right prefab to spawn
        GameObject prefabToSpawn = (currentLevel == 1) ? level1Prefab : level2Prefab;

        // Spawn a fresh copy from the PREFAB (not the instance)
        currentLevelInstance = Instantiate(prefabToSpawn, mazeParent.position, mazeParent.rotation, mazeParent);

        pc = currentLevelInstance.GetComponentInChildren<PlayerController>();

        gameOverPanel.SetActive(false);
    }

    // Call this from your "Next Level" button
    public void LoadLevel2()
    {
        Time.timeScale = 1f;
        bossPanel.SetActive(false);
        currentLevel = 2;
        RestartCurrentLevel();
    }

    void Update()
    {
        // Your existing trigger logic is fine, but ensure 'pc' is linked
        if (pc != null)
        {
            if (pc.ShowGameOver)
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    Time.timeScale = 0f;
                }
                pc.ShowGameOver = false;
            }
        }
    }

    public void enterBoss()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void finishGame()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}