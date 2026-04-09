using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Shows a silhouette mesh on the socket when the correct interactable
/// (identified by tag) is hovering within the socket radius.
/// Silhouette renderer must be assigned — it lives on the socket as a
/// preview of the object that belongs there.
/// </summary>
public class XRSocketSilhouette : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The MeshRenderer used as silhouette preview on this socket")]
    [SerializeField] private MeshRenderer silhouetteRenderer;

    [Header("Tag Filter")]
    [Tooltip("Only show silhouette when an interactable with this tag is hovering")]
    [SerializeField] private string targetTag = "Heart";

    [Header("Silhouette Settings")]
    [Tooltip("Alpha of the silhouette when visible")]
    [Range(0f, 1f)]
    [SerializeField] private float silhouetteAlpha = 0.4f;

    private XRSocketInteractor _socket;
    private Material _silhouetteMaterial;

    private void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        _socket.hoverEntered.AddListener(OnHoverEntered);
        _socket.hoverExited.AddListener(OnHoverExited);
    }

    private void OnDisable()
    {
        _socket.hoverEntered.RemoveListener(OnHoverEntered);
        _socket.hoverExited.RemoveListener(OnHoverExited);
    }

    private void Start()
    {
        if (silhouetteRenderer == null)
        {
            Debug.LogError("[XRSocketSilhouette] Silhouette renderer not assigned!", this);
            return;
        }

        // Cache material instance
        _silhouetteMaterial = silhouetteRenderer.material;

        // Set initial alpha
        SetSilhouetteAlpha(0f);
        silhouetteRenderer.gameObject.SetActive(false);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        // Check tag on the hovering interactable
        if (!args.interactableObject.transform.CompareTag(targetTag)) return;

        silhouetteRenderer.gameObject.SetActive(true);
        SetSilhouetteAlpha(silhouetteAlpha);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (!args.interactableObject.transform.CompareTag(targetTag)) return;

        SetSilhouetteAlpha(0f);
        silhouetteRenderer.gameObject.SetActive(false);
    }

    private void SetSilhouetteAlpha(float alpha)
    {
        if (_silhouetteMaterial == null) return;

        Color color = _silhouetteMaterial.color;
        color.a = alpha;
        _silhouetteMaterial.color = color;
    }
}
