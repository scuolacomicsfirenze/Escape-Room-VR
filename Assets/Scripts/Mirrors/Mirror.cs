using UnityEngine;

/// <summary>
/// Reflects the laser beam. Attach to any GameObject that acts as a mirror.
/// Requires a BoxCollider so <see cref="LaserBeam"/> can detect it via raycast.
/// The mirror's rotation (driven externally by your VR system) determines the reflection angle.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class Mirror : MonoBehaviour
{
    public enum NormalMode
    {
        /// <summary>Use the surface normal from the RaycastHit (correct for any orientation).</summary>
        HitNormal,
        /// <summary>Always use this transform's forward vector as the mirror normal.</summary>
        TransformForward,
    }

    [Tooltip("Strategy used to determine the surface normal for reflection.")]
    [SerializeField] private NormalMode normalMode = NormalMode.HitNormal;

    [Tooltip("When enabled, both sides of the mirror reflect the beam.")]
    [SerializeField] private bool doubleSided = true;

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by <see cref="LaserBeam"/> on each hit.
    /// Returns the reflected direction.
    /// </summary>
    /// <param name="incomingDirection">Normalised incoming ray direction.</param>
    /// <param name="hitNormal">Surface normal at the hit point (from RaycastHit).</param>
    public Vector3 Reflect(Vector3 incomingDirection, Vector3 hitNormal)
    {
        Vector3 normal = normalMode == NormalMode.TransformForward
            ? transform.forward
            : hitNormal;

        if (doubleSided && Vector3.Dot(incomingDirection, normal) > 0f)
            normal = -normal;

        return Vector3.Reflect(incomingDirection, normal).normalized;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos    = transform.position;
        Vector3 normal = normalMode == NormalMode.TransformForward
            ? transform.forward
            : transform.forward; // fallback for editor preview
        Gizmos.DrawLine(pos, pos + normal * 0.5f);
        Gizmos.DrawSphere(pos + normal * 0.5f, 0.04f);
    }
#endif
}
