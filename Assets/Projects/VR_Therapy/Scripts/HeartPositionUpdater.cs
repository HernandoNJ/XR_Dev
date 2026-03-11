using UnityEngine;

public class HeartPositionUpdater : MonoBehaviour
{
    [SerializeField] private Transform camOffset;
    [SerializeField] private Vector3 offSet;

    private void Update()
    {
        transform.position = camOffset.position + offSet;
    }
}