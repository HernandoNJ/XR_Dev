using UnityEngine;

public class AppManager : MonoBehaviour
{
    [SerializeField] private GameObject healingRoom;
    [SerializeField] private GameObject openEnvRoom;

    private void Start()
    {
        EnableHealingRoom();
    }

    public void EnableHealingRoom()
    {
        healingRoom.SetActive(true);
        openEnvRoom.SetActive(false);
    }

    public void EnableOpenEnvRoom()
    {
        openEnvRoom.SetActive(true);
        healingRoom.SetActive(false);
    }
}
