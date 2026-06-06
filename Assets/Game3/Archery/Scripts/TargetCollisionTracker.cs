using UnityEngine;

public class TargetCollisionTracker : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        // Find the GameManager in the scene
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // If the colliding object is an arrow...
        if (collision.gameObject.CompareTag("Arrow"))
        {
            if (gameManager != null)
            {
                gameManager.RegisterHit();
            }
        }
    }
}