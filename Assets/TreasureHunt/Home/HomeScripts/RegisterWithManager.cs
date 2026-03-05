using UnityEngine;

public class RegisterWithManager : MonoBehaviour
{
    void Awake()
    {
        // Introduce yourself to the Boss
        if (HomeGameManager.Instance != null)
        {
            HomeGameManager.Instance.homepageObject = this.gameObject;
            Debug.Log(gameObject.name + " has registered with the Manager!");
        }
    }
}