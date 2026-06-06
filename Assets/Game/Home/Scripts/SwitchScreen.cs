using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScreen : MonoBehaviour
{
    public GameObject[] objectsToInstantiate;
    public GameObject[] objectsToDestroy;
    public void Switch()
    {
        foreach (GameObject obj in objectsToInstantiate)
        {
            Instantiate(obj);
        }

        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }
    }
}
