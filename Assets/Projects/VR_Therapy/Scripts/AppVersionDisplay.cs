using TMPro;
using UnityEngine;

public class AppVersionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;

    private void Start()
    {
        var version = Application.version;
        var bundleCode = GetBundleVersionCode();
        versionText.text = $"App version {version}.{bundleCode}";
    }

    private int GetBundleVersionCode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var context = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity");

                var packageInfo = context
                    .Call<AndroidJavaObject>("getPackageManager")
                    .Call<AndroidJavaObject>("getPackageInfo",
                        context.Call<string>("getPackageName"), 0);

                return packageInfo.Get<int>("versionCode");
            }
            catch
            {
                return 123456789;
            }
#elif UNITY_EDITOR
        // Read PlayerSettings
        return int.Parse(UnityEditor.PlayerSettings.Android.bundleVersionCode.ToString());
#else
            return 123456789;
#endif
    }
}
