using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Controls heart animation, audio, particles and alpha based on socket state.
///
/// Phase 0 - Idle:          audio volume 0.5, no particles
/// Phase 1 - Healing (30s): healing particles, volume 0.5
///           At T=25s golden activates independently
/// Phase 2 - Golden:        runs indefinitely regardless of socket state
/// </summary>
public class XRHeartBehaviorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SkinnedMeshRenderer heartRenderer;

    [Header("Particle Systems")]
    [Tooltip("Healing particles — child of Pedestal_Socket")]
    [SerializeField] private ParticleSystem healingParticles;

    [Tooltip("Golden particles — child of XRHeartSocket (the heart)")]
    [SerializeField] private ParticleSystem goldenParticles;

    [Header("Socket Reference")]
    [Tooltip("The Pedestal_Socket — healing only triggers here")]
    [SerializeField] private XRSocketInteractor heartSocket1;

    [Header("BPM Settings")]
    [SerializeField] private float targetBpm = 72f;
    [SerializeField] private int beatsInAudioClip = 9;

    [Header("Fine Tuning")]
    [Range(0.5f, 2f)]
    [SerializeField] private float animSpeedMultiplier = 1f;

    [Range(0.5f, 2f)]
    [SerializeField] private float audioPitchMultiplier = 1f;

    [Header("Phase Settings")]
    [SerializeField] private float healingDuration      = 30f;
    [SerializeField] private float goldenStartTime      = 25f;
    [SerializeField] private float healingVolume        = 0.5f;
    [SerializeField] private float goldenVolume         = 1f;

    [Header("Particle Settings")]
    [SerializeField] private float healingSimulationSpeed = 0.2f;
    [SerializeField] private float goldenSimulationSpeed  = 0.3f;
    [SerializeField] private float fadeDuration           = 3f;

    [Tooltip("Max alpha for healing particles (0 = invisible, 1 = fully opaque)")]
    [Range(0f, 1f)]
    [SerializeField] private float healingMaxAlpha = 0.2f;

    [Header("Transit Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float transitAlpha = 0.3f;

    private XRGrabInteractable _grabInteractable;
    private AnimationClip _heartClip;
    private Material _heartMaterial;
    private float _baseAnimSpeed;
    private float _baseAudioPitch;
    private float _phaseTimer;
    private bool _isDocked;
    private bool _goldenActivated;
    private int _currentPhase;
    private Coroutine _healingFadeCoroutine;

    private void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        _grabInteractable.selectEntered.AddListener(OnSelectEntered);
        _grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        _grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        _grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    private void Start()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips.Length > 0)
                _heartClip = clips[0];
        }

        if (heartRenderer != null)
            _heartMaterial = heartRenderer.material;

        SyncToTargetBpm();
        SetPhase(0);
        SetHeartAlpha(1f);
    }

    private void Update()
    {
        if (!_isDocked || _currentPhase != 1) return;

        _phaseTimer += Time.deltaTime;

        // Activate golden independently at T=25s
        if (!_goldenActivated && _phaseTimer >= goldenStartTime)
        {
            _goldenActivated = true;
            StartCoroutine(ActivateGolden());
            SetVolume(goldenVolume);
            Debug.Log("[XRHeartBehaviorController] Golden activated");
        }

        // Deactivate healing at T=30s
        if (_phaseTimer >= healingDuration)
        {
            _phaseTimer = 0f;
            SetPhase(2);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor socket)
        {
            SetHeartAlpha(1f);

            if (socket == heartSocket1 && _currentPhase == 0)
            {
                _isDocked = true;
                _phaseTimer = 0f;
                _goldenActivated = false;
                SetPhase(1);
            }
        }
        else
        {
            SetHeartAlpha(transitAlpha);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor socket)
        {
            if (socket == heartSocket1)
            {
                _isDocked = false;
                _phaseTimer = 0f;

                // Only stop healing — golden keeps running
                StopHealingParticles();
            }
        }
        else
        {
            SetHeartAlpha(1f);
        }
    }

    private void SetPhase(int phase)
    {
        _currentPhase = phase;

        switch (phase)
        {
            case 0:
                SetAnimationActive(false);
                SetVolume(healingVolume);
                audioSource?.Stop();           // ← Stop explícito
                StopHealingParticles();
                break;

            case 1:
                SetAnimationActive(true);
                SetVolume(healingVolume);
                audioSource?.Play();           // ← Play explícito
                StartHealingParticles();
                Debug.Log("[XRHeartBehaviorController] Phase 1 — Healing + Audio started");
                break;

            case 2:
                SetAnimationActive(true);
                StopHealingParticles();
                Debug.Log("[XRHeartBehaviorController] Phase 2 — Healing complete");
                break;
        }
    }

    private void StartHealingParticles()
    {
        if (healingParticles == null) return;

        if (_healingFadeCoroutine != null)
            StopCoroutine(_healingFadeCoroutine);

        healingParticles.gameObject.SetActive(true);

        // Set startColor with full alpha before playing
        var main = healingParticles.main;
        main.simulationSpeed = healingSimulationSpeed;
        Color startColor = main.startColor.color;
        startColor.a = 0f;
        main.startColor = startColor;

        healingParticles.Play();
        _healingFadeCoroutine = StartCoroutine(
            FadeParticles(healingParticles, 0f, healingMaxAlpha, fadeDuration)
        );
    }

    private void StopHealingParticles()
    {
        if (healingParticles == null) return;

        if (_healingFadeCoroutine != null)
            StopCoroutine(_healingFadeCoroutine);

        _healingFadeCoroutine = StartCoroutine(
            FadeOutAndDeactivate(healingParticles, fadeDuration)
        );
    }

    /// <summary>
    /// Activates golden particles — needs one frame after SetActive(true)
    /// before Play() is called to avoid Unity's inactive GameObject issue.
    /// </summary>
    private IEnumerator ActivateGolden()
    {
        if (goldenParticles == null) yield break;

        goldenParticles.gameObject.SetActive(true);

        // Wait one frame so Unity registers the GameObject as active
        yield return null;

        var main = goldenParticles.main;
        main.simulationSpeed = goldenSimulationSpeed;
        goldenParticles.Play();

        Debug.Log("[XRHeartBehaviorController] Golden particles playing");
    }

    private void SetAnimationActive(bool active)
    {
        if (animator != null)
            animator.enabled = active;
    }

    private void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    private void SetHeartAlpha(float alpha)
    {
        if (_heartMaterial == null) return;

        Color color = _heartMaterial.color;
        color.a = alpha;
        _heartMaterial.color = color;

        // Re-enable depth write when fully opaque to prevent interior faces showing
        _heartMaterial.SetInt("_ZWrite", alpha >= 1f ? 1 : 0);
        _heartMaterial.renderQueue = alpha >= 1f ? 2000 : 3000;
    }

    private IEnumerator FadeParticles(ParticleSystem ps, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        var main = ps.main;
        Color currentColor = main.startColor.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            main.startColor = currentColor;
            yield return null;
        }

        currentColor.a = endAlpha;
        main.startColor = currentColor;
    }

    private IEnumerator FadeOutAndDeactivate(ParticleSystem ps, float duration)
    {
        var main = ps.main;
        float startAlpha = main.startColor.color.a;
        yield return FadeParticles(ps, startAlpha, 0f, duration);
        ps.Stop();
        ps.gameObject.SetActive(false);
    }

    private void SyncToTargetBpm()
    {
        if (_heartClip == null || audioSource == null || audioSource.clip == null) return;

        float secondsPerBeat = 60f / targetBpm;
        _baseAnimSpeed       = _heartClip.length / secondsPerBeat;
        float audioBpm       = (beatsInAudioClip / audioSource.clip.length) * 60f;
        _baseAudioPitch      = targetBpm / audioBpm;

        animator.speed    = _baseAnimSpeed * animSpeedMultiplier;
        audioSource.pitch = _baseAudioPitch * audioPitchMultiplier;

        Debug.Log($"[XRHeartBehaviorController] " +
                  $"Target: {targetBpm} BPM | " +
                  $"Anim speed: {animator.speed:F3} | " +
                  $"Audio pitch: {audioSource.pitch:F3}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (animator != null)
            animator.speed    = _baseAnimSpeed * animSpeedMultiplier;
        if (audioSource != null)
            audioSource.pitch = _baseAudioPitch * audioPitchMultiplier;
    }
#endif
}