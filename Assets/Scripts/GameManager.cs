using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required to restart the game

public class GameManager : MonoBehaviour
{
    public GameObject bossPanel;
    public GameObject gameOverPanel; // Drag your Panel here in Inspector
    public GameObject winPanel;
    public GameObject lvl_1;
    public GameObject lvl_2;

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true); // Shows the screen
        Time.timeScale = 0f; // Optional: Pauses the game
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reloads the current scene
    }

    public void enterBoss()
    {
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
        bossPanel.SetActive(false);
        lvl_1.SetActive(false);
        lvl_2.SetActive(true);
    }
}
