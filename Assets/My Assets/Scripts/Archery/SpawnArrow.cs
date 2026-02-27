using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnArrow : MonoBehaviour
{
    public GameObject arrow;
    public GameObject bow;
    //public Transform obj;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(arrow, bow.transform.position + new Vector3(0, 0, 0), bow.transform.rotation);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        arrow.transform.position = bow.transform.position + new Vector3(0, 0, 0);
        arrow.transform.eulerAngles = bow.transform.eulerAngles;
    }
}
