using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRHeartBehaviorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;

    [Header("BPM Settings")]
    [Tooltip("Target heartbeat rate. 72 = resting adult.")]
    [SerializeField] private float _targetBPM = 72f;

    [Tooltip("How many heartbeats are in the audio clip")]
    [SerializeField] private int _beatsInAudioClip = 9;

    [Header("Fine Tuning")]
    [Tooltip("Multiplier on top of calculated animation speed. 1 = no change.")]
    [Range(0.5f, 2f)]
    [SerializeField] private float _animSpeedMultiplier = 1f;

    [Tooltip("Multiplier on top of calculated audio pitch. 1 = no change.")]
    [Range(0.5f, 2f)]
    [SerializeField] private float _audioPitchMultiplier = 1f;

    private XRGrabInteractable _grabInteractable;
    private AnimationClip _heartClip;
    private float _baseAnimSpeed;
    private float _baseAudioPitch;

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
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
            if (clips.Length > 0)
                _heartClip = clips[0];
        }

        SyncToTargetBPM();
        SetHeartActive(false);
    }

    private void SyncToTargetBPM()
    {
        if (_heartClip == null || _audioSource == null || _audioSource.clip == null) return;

        float secondsPerBeat = 60f / _targetBPM;

        // Base values from BPM calculation
        _baseAnimSpeed  = _heartClip.length / secondsPerBeat;
        float audioBPM  = (_beatsInAudioClip / _audioSource.clip.length) * 60f;
        _baseAudioPitch = _targetBPM / audioBPM;

        // Apply with fine tuning multipliers
        _animator.speed      = _baseAnimSpeed  * _animSpeedMultiplier;
        _audioSource.pitch   = _baseAudioPitch * _audioPitchMultiplier;

        Debug.Log($"[XRHeartBehaviorController] " +
                  $"Target: {_targetBPM} BPM | " +
                  $"Anim speed: {_animator.speed:F3} (base: {_baseAnimSpeed:F3}) | " +
                  $"Audio pitch: {_audioSource.pitch:F3} (base: {_baseAudioPitch:F3})");
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor)
            SetHeartActive(true);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor)
            SetHeartActive(false);
    }

    private void SetHeartActive(bool active)
    {
        if (_animator != null)
            _animator.enabled = active;

        if (_audioSource != null)
        {
            if (active) _audioSource.Play();
            else        _audioSource.Stop();
        }
    }

#if UNITY_EDITOR
    // Allows tweaking multipliers in Play Mode and seeing results immediately
    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (_animator != null)
            _animator.speed    = _baseAnimSpeed  * _animSpeedMultiplier;
        if (_audioSource != null)
            _audioSource.pitch = _baseAudioPitch * _audioPitchMultiplier;
    }
#endif
}