using UnityEngine;

public class VrAppHandler : MonoBehaviour
{
    public void StopApp()
    {
 #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif 
    } 
}