using UnityEngine;

/// <summary>
/// Global application initialization.
/// Attach to a persistent GameObject in the main scene.
/// </summary>
public class AppInit : MonoBehaviour
{
    [Header("Frame Rate")]
    [Tooltip("72 = Quest 2 default. Use 90 only if GPU budget allows.")]
    public int targetFrameRate = 72;

    private void Awake()
    {
        // Set explicit frame rate — Quest 2 requires this to be fixed
        Application.targetFrameRate = targetFrameRate;

        // Ensure VSync does not interfere with the Quest compositor
        QualitySettings.vSyncCount = 0;

        Debug.Log($"[AppInit] targetFrameRate = {targetFrameRate} | vSyncCount = 0");
    }
}