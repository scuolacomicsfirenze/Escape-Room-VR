using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Casts a laser beam from this transform's forward direction, reflects it off
/// any <see cref="Mirror"/> hit by the raycast, and draws the full path with a LineRenderer.
/// Fires events when the beam reaches or loses the end target.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private int maxReflections = 20;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask hitLayers = Physics.DefaultRaycastLayers;

    [Header("Visual")]
    [SerializeField] private float beamWidth = 0.05f;
    [SerializeField] private Material beamMaterial;
    [SerializeField] private Color beamColor = Color.red;

    [Header("End Target")]
    [SerializeField] private Transform endTarget;
    [SerializeField] private float endTargetRadius = 0.25f;

    // ── Events ────────────────────────────────────────────────────────────────
    public System.Action OnTargetReached;
    public System.Action OnTargetLost;

    // ── Private ───────────────────────────────────────────────────────────────
    private LineRenderer _lr;
    private readonly List<Vector3> _points = new(32);
    private bool _targetWasReached;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        InitLineRenderer();
    }

    private void Update() => CastBeam();

    /// <summary>World-space points of the current beam path (read-only).</summary>
    public IReadOnlyList<Vector3> GetBeamPoints() => _points;

    // ── Core ──────────────────────────────────────────────────────────────────

    private void CastBeam()
    {
        _points.Clear();

        Vector3 origin     = transform.position;
        Vector3 direction  = transform.forward;
        bool targetReached = false;

        _points.Add(origin);

        for (int i = 0; i < maxReflections; i++)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitLayers))
            {
                _points.Add(origin + direction * maxDistance);
                break;
            }

            _points.Add(hit.point);

            if (endTarget != null &&
                Vector3.Distance(hit.point, endTarget.position) <= endTargetRadius)
            {
                targetReached = true;
                break;
            }

            Mirror mirror = hit.collider.GetComponentInParent<Mirror>();
            if (mirror == null) break;

            Vector3 reflected = mirror.Reflect(direction, hit.normal);
            origin    = hit.point + reflected * 0.001f;
            direction = reflected;
        }

        // Fire events only on state change
        if (targetReached && !_targetWasReached)
        {
            _targetWasReached = true;
            OnTargetReached?.Invoke();
        }
        else if (!targetReached && _targetWasReached)
        {
            _targetWasReached = false;
            OnTargetLost?.Invoke();
        }

        _lr.positionCount = _points.Count;
        _lr.SetPositions(_points.ToArray());
    }

    // ── Setup ─────────────────────────────────────────────────────────────────

    private void InitLineRenderer()
    {
        _lr.useWorldSpace      = true;
        _lr.startWidth         = beamWidth;
        _lr.endWidth           = beamWidth;
        _lr.startColor         = beamColor;
        _lr.endColor           = beamColor;
        _lr.numCapVertices     = 4;
        _lr.numCornerVertices  = 4;
        _lr.shadowCastingMode  = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lr.receiveShadows     = false;

        if (beamMaterial != null)
            _lr.material = beamMaterial;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        if (_lr != null) InitLineRenderer();
    }

    private void OnDrawGizmosSelected()
    {
        if (endTarget == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(endTarget.position, endTargetRadius);
    }
#endif
}
