using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrowPrefab; 
    public Transform stringAnchor;
    public BowStringVisual bowStringVisual;

    void Start()
    {
        SpawnArrow();
    }

    public void SpawnArrow()
    {
        // 1. Spawn the arrow as a child of the anchor
        GameObject newArrow = Instantiate(arrowPrefab, stringAnchor);
        newArrow.transform.localPosition = Vector3.zero;
        newArrow.transform.localRotation = Quaternion.identity;

        // 2. Grab the ArrowLogic script attached to the new arrow
        ArrowLogic arrowScript = newArrow.GetComponent<ArrowLogic>();

        // 3. Hand the scene reference directly to the prefab instance!
        if (arrowScript != null)
        {
            arrowScript.bowString = stringAnchor;
            arrowScript.bowStringScript = bowStringVisual;
        }
    }
}