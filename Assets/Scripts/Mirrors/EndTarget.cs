using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Visual feedback for the goal object.
/// Wire <see cref="LaserBeam.OnTargetReached"/> → <see cref="Activate"/>
/// and  <see cref="LaserBeam.OnTargetLost"/>    → <see cref="Deactivate"/> in the Inspector.
/// </summary>
public class EndTarget : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color inactiveEmission = Color.black;
    [SerializeField] private Color activeEmission   = new Color(0f, 2f, 0f);
    [SerializeField] private float pulseDuration    = 0.4f;

    [Header("Optional")]
    [SerializeField] private ParticleSystem activationParticles;
    [SerializeField] private AudioSource    activationAudio;

    [Header("Events")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    // ─────────────────────────────────────────────────────────────────────────

    private bool      _isActive;
    private Coroutine _pulseCoroutine;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    // ── Public API ────────────────────────────────────────────────────────────

    public void Activate()   => SetActive(true);
    public void Deactivate() => SetActive(false);

    // ─────────────────────────────────────────────────────────────────────────

    private void SetActive(bool active)
    {
        if (_isActive == active) return;
        _isActive = active;

        if (active)
        {
            activationParticles?.Play();
            activationAudio?.Play();
            OnActivated.Invoke();
            RestartPulse();
        }
        else
        {
            activationParticles?.Stop();
            OnDeactivated.Invoke();
            StopPulse();
        }
    }

    private void RestartPulse()
    {
        if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        _pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    private void StopPulse()
    {
        if (_pulseCoroutine != null) { StopCoroutine(_pulseCoroutine); _pulseCoroutine = null; }
        SetEmission(inactiveEmission);
    }

    private IEnumerator PulseRoutine()
    {
        while (_isActive)
        {
            yield return LerpEmission(inactiveEmission, activeEmission, pulseDuration * 0.5f);
            yield return LerpEmission(activeEmission, inactiveEmission, pulseDuration * 0.5f);
        }
    }

    private IEnumerator LerpEmission(Color from, Color to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            SetEmission(Color.Lerp(from, to, t / duration));
            yield return null;
        }
        SetEmission(to);
    }

    private void SetEmission(Color color)
    {
        if (targetRenderer != null && targetRenderer.material.HasProperty(EmissionColor))
            targetRenderer.material.SetColor(EmissionColor, color);
    }
}
