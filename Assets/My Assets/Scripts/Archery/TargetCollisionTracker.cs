using UnityEngine;

public class TargetCollisionTracker : MonoBehaviour
{
    private ArcheryGameManager gameManager;

    void Start()
    {
        // Find the ArcheryGameManager in the scene
        gameManager = FindObjectOfType<ArcheryGameManager>();
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