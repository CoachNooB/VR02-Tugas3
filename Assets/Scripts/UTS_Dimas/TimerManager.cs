using UnityEngine;
using TMPro;
using System;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float maxTime = 120f;
    private float currentTime;
    private bool isRunning = true;

    public Action OnTimerExpiredEvent;

    private void Start()
    {
        currentTime = maxTime;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isRunning) return;
        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            OnTimerExpiredEvent?.Invoke();
            UpdateTimerDisplay();
        }
        else
        {
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = maxTime;
        isRunning = true;
        UpdateTimerDisplay();
    }
}