using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl; // Required to restart the game

public class GameManager : MonoBehaviour
{
    public int score;
    public GameObject bossPanel;
    public GameObject gameOverPanel; // Drag your Panel here in Inspector
    public GameObject winPanel;
    public GameObject lvl_1;
    public GameObject lvl_2;

    [Header("Level Prefabs")]
    public GameObject[] levelPrefabs; // Slot 0 = Level 1, Slot 1 = Level 2
    
    [Header("UI & References")]
    public Transform mazeParent; // The AR Image Target or Anchor

    public GameObject currentLevelInstance;
    private int currentLevelIndex = 0; // Tracks which level we are in

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;

        // 1. Clear the old level and player to prevent "physics jumping"
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        // 2. Spawn a fresh copy of the level we were just playing
        currentLevelInstance = Instantiate(levelPrefabs[currentLevelIndex], mazeParent.position, mazeParent.rotation, mazeParent);

        // 3. Reset the UI
        gameOverPanel.SetActive(false);
        
        Debug.Log("Restarted Level: " + (currentLevelIndex + 1));
    }

    public void ShowGameOver()
    {
        score = 0;
        gameOverPanel.SetActive(true); // Shows the screen
        Time.timeScale = 0f; // Optional: Pauses the game
    }


    public void enterBoss()
    {
        score = 0;
        bossPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void gameCompleted()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void next()
    {
        Time.timeScale = 1f;
        currentLevelIndex++;
        bossPanel.SetActive(false);
        lvl_1.SetActive(false);
        lvl_2.SetActive(true);
    }
}
