using UnityEngine;

public class BowStringVisual : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform topAnchor;
    public Transform bottomAnchor;
    public Transform stringCenter; // The idle center of the bow

    public Transform arrowNock; // The arrow we are currently pulling (if any)

    void Update()
    {
        // Always lock the ends of the string to the top and bottom of the bow
        lineRenderer.SetPosition(0, topAnchor.position);
        lineRenderer.SetPosition(2, bottomAnchor.position);

        // If we are holding an arrow, stretch the middle of the string to it!
        if (arrowNock != null)
        {
            lineRenderer.SetPosition(1, arrowNock.position);
        }
        else
        {
            // Otherwise, keep the string resting at the center
            lineRenderer.SetPosition(1, stringCenter.position);
        }
    }
}