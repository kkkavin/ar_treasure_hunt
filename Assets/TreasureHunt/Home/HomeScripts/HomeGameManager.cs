using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeGameManager : MonoBehaviour
{
    public static HomeGameManager Instance;

    // This is the "Registry" slot where the Worker will plug itself in
    [Header("Registry")]
    public GameObject homepageObject; 

    void Awake()
    {
        // Singleton logic: If a boss already exists, destroy the new one
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This makes it persistent
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // This function can now be called from ANY scene to activate the object
    public void ToggleHomepage(bool status)
    {
        if (homepageObject != null)
        {
            homepageObject.SetActive(status);
        }
        else
        {
            Debug.LogWarning("Manager: No object is registered in the 'homepageObject' slot!");
        }
    }
    public void LoadScene(string sceneName)
    {
        homepage.SetActive(false);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }
}
