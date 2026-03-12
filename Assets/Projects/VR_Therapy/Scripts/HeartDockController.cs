using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// HeartDockPoint — chest position estimated via Lerp between
/// CharacterController center (stable body) and Main Camera (head).
///
/// Key insight: LateUpdate runs ALWAYS regardless of dock state.
/// XRIT moves the anchored heart TO the socket each frame.
/// Moving the socket moves the heart with it — no conflict.
/// </summary>
public class HeartDockController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("XR Player root — must have CharacterController")]
    [SerializeField] private CharacterController _characterController;

    [Tooltip("Main Camera (head position)")]
    [SerializeField] private Transform _xrCamera;

    [Header("Chest Estimation")]
    [Tooltip("0 = body center, 1 = head. 0.7 recommended based on testing.")]
    [Range(0f, 1f)]
    [SerializeField] private float _bodyToHeadRatio = 0.7f;

    [Header("Dock Offset from Estimated Chest")]
    [Tooltip("Forward offset from chest toward front of body")]
    [SerializeField] private float _offsetZ = 0.15f;

    [Tooltip("Vertical offset from chest")]
    [SerializeField] private float _offsetY = 0.05f;

    [Tooltip("Horizontal offset from chest center")]
    [SerializeField] private float _offsetX = 0f;

    [Header("Visual Reference (optional)")]
    [SerializeField] private Renderer _dockVisual;

    private XRSocketInteractor _socket;
    private bool _isDocked = false;
    private Transform _playerTransform;

    private void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
        _playerTransform = _characterController.transform;
    }

    private void OnEnable()
    {
        _socket.selectEntered.AddListener(OnHeartDocked);
        _socket.selectExited.AddListener(OnHeartUndocked);
    }

    private void OnDisable()
    {
        _socket.selectEntered.RemoveListener(OnHeartDocked);
        _socket.selectExited.RemoveListener(OnHeartUndocked);
    }

    private void Start()
    {
        if (_characterController == null || _xrCamera == null)
        {
            Debug.LogError("[HeartDockController] Missing references!", this);
            return;
        }

        transform.SetParent(null, worldPositionStays: true);
        transform.rotation = Quaternion.identity;

        UpdateDockPosition();
        SetDockVisual(true);
    }

    /// <summary>
    /// Always runs — docked or not.
    /// When docked, XRIT repositions the heart to match the socket each frame.
    /// Moving the socket moves the heart naturally with it.
    /// </summary>
    private void LateUpdate()
    {
        UpdateDockPosition();
    }

    private void OnHeartDocked(SelectEnterEventArgs args)
    {
        _isDocked = true;
        SetDockVisual(false);
    }

    private void OnHeartUndocked(SelectExitEventArgs args)
    {
        _isDocked = false;
        SetDockVisual(true);
    }

    private void UpdateDockPosition()
    {
        // Body center in world space — stable, unaffected by head movement
        Vector3 bodyCenter = _playerTransform.TransformPoint(_characterController.center);

        // Lerp toward head to estimate chest/sternum
        Vector3 estimatedChest = Vector3.Lerp(bodyCenter, _xrCamera.position, _bodyToHeadRatio);

        // Horizontal player forward only — ignores head tilt completely
        Vector3 bodyForward = new Vector3(_playerTransform.forward.x, 0f, _playerTransform.forward.z).normalized;
        Vector3 bodyRight   = new Vector3(_playerTransform.right.x,   0f, _playerTransform.right.z).normalized;

        transform.position = estimatedChest
            + Vector3.up  * _offsetY
            + bodyForward * _offsetZ
            + bodyRight   * _offsetX;

        transform.rotation = Quaternion.identity;
    }

    private void SetDockVisual(bool visible)
    {
        if (_dockVisual != null)
            _dockVisual.enabled = visible;
    }

// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         if (_characterController == null || _xrCamera == null) return;
//
//         Vector3 bodyCenter     = _playerTransform.TransformPoint(_characterController.center);
//         Vector3 estimatedChest = Vector3.Lerp(bodyCenter, _xrCamera.position, _bodyToHeadRatio);
//         Vector3 dockPos        = estimatedChest
//                                + Vector3.up * _offsetY
//                                + _playerTransform.forward * _offsetZ;
//
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(bodyCenter, 0.04f);
//
//         Gizmos.color = Color.magenta;
//         Gizmos.DrawWireSphere(estimatedChest, 0.04f);
//         Gizmos.DrawLine(bodyCenter, _xrCamera.position);
//
//         Gizmos.color = _isDocked ? Color.green : Color.cyan;
//         Gizmos.DrawWireSphere(dockPos, 0.06f);
//         Gizmos.DrawLine(estimatedChest, dockPos);
//     }
// #endif
}