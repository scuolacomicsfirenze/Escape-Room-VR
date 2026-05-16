using System.Linq.Expressions;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserEmitter : MonoBehaviour
{
    [Header("Laser")]
    public int maxBounces = 10;
    public float maxDistance = 50f;
    public LayerMask mirrorLayer;
    public LayerMask targetLayer; // layer del buco/ricevitore

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        ShootLaser(transform.position, transform.forward);
    }

    void ShootLaser(Vector3 origin, Vector3 direction)
    {
        var points = new System.Collections.Generic.List<Vector3>();
        points.Add(origin);

        for (int i = 0; i < maxBounces; i++)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
            {
                // nessun ostacolo: termina il raggio nell'aria
                points.Add(origin + direction * maxDistance);
                break;
            }

            points.Add(hit.point);

            // Ha colpito il target?
            if (((1 << hit.collider.gameObject.layer) & targetLayer) != 0)
            {
                hit.collider.GetComponent<LaserTarget>()?.OnHit();
                break;
            }

            // Ha colpito uno specchio?
            if (((1 << hit.collider.gameObject.layer) & mirrorLayer) != 0)
            {
                direction = Vector3.Reflect(direction, hit.normal);
                origin = hit.point + direction * 0.001f; // offset per non colpire lo stesso punto
            }
            else
            {
                // Ostacolo generico: il raggio si ferma
                break;
            }
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
    }
}