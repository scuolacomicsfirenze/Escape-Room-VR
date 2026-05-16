using UnityEngine;
using UnityEngine.Events;

public class LaserTarget : MonoBehaviour
{
    public UnityEvent onLaserHit;
    public UnityEvent onLaserLost;

    private bool wasHit = false;

    // Chiamato da LaserEmitter ogni frame in cui il raggio arriva qui
    public void OnHit()
    {
        if (!wasHit)
        {
            wasHit = true;
            onLaserHit.Invoke();
        }
    }

    // Chiamato da LaserEmitter ogni frame per resettare lo stato
    public void ResetHit()
    {
        if (wasHit)
        {
            wasHit = false;
            onLaserLost.Invoke();
        }
    }
}
