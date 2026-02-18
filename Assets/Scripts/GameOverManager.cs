using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required to restart the game

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel; // Drag your Panel here in Inspector

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
}
