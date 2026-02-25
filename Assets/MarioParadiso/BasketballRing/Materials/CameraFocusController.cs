using UnityEngine;
using Vuforia;

public class CameraFocusController : MonoBehaviour
{
    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaPaused += OnVuforiaPaused;
    }

    private void OnVuforiaStarted()
    {
        VuforiaBehaviour.Instance.CameraDevice.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    private void OnVuforiaPaused(bool paused)
    {
        if (!paused)
        {
            VuforiaBehaviour.Instance.CameraDevice.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }
}