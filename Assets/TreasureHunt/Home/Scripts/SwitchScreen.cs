using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScreen : MonoBehaviour
{
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;
    public void Switch()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in objectsToDeactivate)
        {
            obj.SetActive(false);
        }
    }
}
