using UnityEngine;
using UnityEngine.UIElements;

public class ButtonsHandler : MonoBehaviour
{
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var heartBtn = root.Q<Button>("HeartBtn");
        heartBtn.text = "Corazon";
        
        var uterusBtn = root.Q<Button>("UterusBtn");
        uterusBtn.text = "Utero";
        
        var quitBtn = root.Q<Button>("QuitBtn");
        quitBtn.text = "Salir";
    }
}