using UnityEngine;

public class ArrowSpawner : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform stringAnchor;
    public BowStringVisual bowStringVisual; // The script that draws the string

    void Awake()
    {
        // Force the Android device to run the game at 60 Frames Per Second
        Application.targetFrameRate = 60;
    }
    void Start()
    {
        SpawnArrow();
    }

    public void SpawnArrow()
    {
        // 1. Spawn the new arrow
        GameObject newArrow = Instantiate(arrowPrefab, stringAnchor);
        newArrow.transform.localPosition = Vector3.zero;
        newArrow.transform.localRotation = Quaternion.identity;

        // 2. Grab the ArrowLogic script from the new arrow
        ArrowLogic arrowScript = newArrow.GetComponent<ArrowLogic>();

        if (arrowScript != null)
        {
            // Give the arrow its anchor point
            arrowScript.bowString = stringAnchor;

            // Give the arrow the string script so it knows what to stretch when pulled
            arrowScript.bowStringScript = bowStringVisual;
        }

        // 3. Tell the BowStringVisual that THIS new arrow is the one it should attach to
        if (bowStringVisual != null)
        {
            bowStringVisual.arrowNock = newArrow.transform;
        }
    }
}