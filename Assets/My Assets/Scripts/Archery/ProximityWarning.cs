using UnityEngine;

public class ProximityWarning : MonoBehaviour
{
    [Header("UI References")]
    public GameObject warningPanel; // Drag your UI Panel here
    public GameObject aim;

    [Header("Settings")]
    public float minimumSafeDistance = 1.5f; // Distance in meters (Unity units)

    private Transform archeryTarget;
    private Collider targetCollider;

    void Update()
    {
        // Always try to find the active target if we don't have one
        if (archeryTarget == null || !archeryTarget.gameObject.activeInHierarchy)
        {
             // FindGameObjectsWithTag returns active objects. We grab the first one.
            GameObject[] foundTargets = GameObject.FindGameObjectsWithTag("Target");
            
            if (foundTargets.Length > 0)
            {
                archeryTarget = foundTargets[0].transform;
                targetCollider = foundTargets[0].GetComponent<MeshCollider>();
            }
            return; 
        }

        // 2. Measure the physical distance between the phone (this script) and the target
        float currentDistance = Vector3.Distance(transform.position, archeryTarget.position);

        // 3. Logic: Are we too close?
        if (currentDistance < minimumSafeDistance)
        {
            // Show the warning text
            if (!warningPanel.activeSelf) warningPanel.SetActive(true);
            if (aim.activeSelf) aim.SetActive(false);

            // Turn off the target's hitbox so arrows can't stick to it
            if (targetCollider != null) targetCollider.enabled = false;
        }
        else
        {
            // Hide the warning text
            if (warningPanel.activeSelf) warningPanel.SetActive(false);
            if (!aim.activeSelf) aim.SetActive(true);

            // Turn the hitbox back on so the game is playable
            if (targetCollider != null) targetCollider.enabled = true;
        }
    }
}