using UnityEngine;
using UnityEngine.UI; // Required if you want to show the score on screen

public class HoopScore : MonoBehaviour
{
    public int score = 0;
    public int winCondition = 3;
    public Text scoreText; // Optional: Drag a UI Text element here
    public GameObject victoryUI; // Optional: A "You Win" screen or effect

    private bool isGameOver = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only count if the game isn't over and the object is the Ball
        if (!isGameOver && other.CompareTag("ball"))
        {
            score++;
            UpdateScoreUI();
            Debug.Log("Score: " + score);

            if (score >= winCondition)
            {
                WinGame();
            }
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    void WinGame()
    {
        isGameOver = true;
        Debug.Log("Victory! You've reached 3 points.");
        
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
        }

        // To stop the ball loop, you can find the Ball Spawner 
        // and disable its script or stop the Coroutine.
        // Example: FindObjectOfType<BallSpawner>().enabled = false;
    }
}