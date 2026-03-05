using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeGameManager : MonoBehaviour
{
    public GameObject homepage;
    public void LoadScene(string sceneName)
    {
        homepage.SetActive(false);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }
}
