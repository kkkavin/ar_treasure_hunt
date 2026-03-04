using UnityEngine;

public class ProximityWarning : MonoBehaviour
{
    [Header("UI References")]
    public GameObject warningPanel; // Drag your UI Panel here
    public GameObject aim;

    [Header("Settings")]
    public float minimumSafeDistance = 1.5f; // Distance in meters (Unity units)

    private Transform archeryTarget;
    private Collider arrowCollider;

    // make the value available to other scripts
    public static float SafeDistance { get; private set; }

    void Awake()
    {
        SafeDistance = minimumSafeDistance;
    }

    void Update()
    {
        // keep the static value up to date in case you tweak it at runtime
        SafeDistance = minimumSafeDistance;

        // Always try to find the active target if we don't have one
        if (archeryTarget == null || !archeryTarget.gameObject.activeInHierarchy)
        {
            GameObject[] foundTargets = GameObject.FindGameObjectsWithTag("Target");
            if (foundTargets.Length > 0)
            {
                archeryTarget = foundTargets[0].transform;
            }
            return;
        }

        if (arrowCollider == null || !arrowCollider.gameObject.activeInHierarchy)
        {
            GameObject a = GameObject.FindGameObjectWithTag("Arrow");
            if (a != null)
                arrowCollider = a.GetComponent<Collider>();
            return;
        }

        float currentDistance = Vector3.Distance(transform.position, archeryTarget.position);
        bool tooClose = currentDistance < minimumSafeDistance;

        if (tooClose)
        {
            if (!warningPanel.activeSelf) warningPanel.SetActive(true);
            if (aim.activeSelf) aim.SetActive(false);
        }
        else
        {
            if (warningPanel.activeSelf) warningPanel.SetActive(false);
            if (!aim.activeSelf) aim.SetActive(true);
        }

        // disable / enable every collider on the target, not just the mesh
        if (archeryTarget != null)
        {
            Collider[] cols = archeryTarget.GetComponents<Collider>();
            foreach (var c in cols)
            {
                if (c != null) c.enabled = !tooClose;
            }
        }

        if (arrowCollider != null)
            arrowCollider.enabled = !tooClose;
    }
}