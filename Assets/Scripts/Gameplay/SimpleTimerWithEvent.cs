using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTimerWithEvent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] UnityEvent onTimerEnd;
    bool _started;
    float _time;
    public void StartTimer(float time, UnityAction onComplete = null)
    {
        if (onComplete != null)
            onTimerEnd.AddListener(onComplete);

        _started = true;
        _time = time;
    }

    public void Update()
    {
        if (!_started) return;
        _time -= Time.deltaTime;

        if (_time <= 0f)
        {
            _time = 0f;
            _started = false;
            UpdateGraphics();
            onTimerEnd.Invoke();
            return;
        }

        UpdateGraphics();
    }

    private void UpdateGraphics()
    {
        TimeSpan t = TimeSpan.FromSeconds(_time);
        timerText.text = t.ToString(@"mm\:ss");
    }
}
