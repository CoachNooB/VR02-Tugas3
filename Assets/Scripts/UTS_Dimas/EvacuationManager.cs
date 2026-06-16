using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EvacuationManager : MonoBehaviour
{
    // UI References (diisi otomatis oleh builder)
    public TextMeshProUGUI txtObjectiveID;
    public TextMeshProUGUI txtObjectiveAlarm;
    public TextMeshProUGUI txtObjectiveDoor;
    public TextMeshProUGUI txtObjectiveExit;
    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI txtFeedback;
    public CanvasGroup warningCanvasGroup;
    public Image mapIDCard;
    public Image mapAlarm;
    public Image mapDoor;
    public Image mapExit;

    [Header("Color States")]
    public Color colorTodo = Color.gray;
    public Color colorActive = Color.yellow;
    public Color colorDone = Color.green;

    // State
    private bool hasIDCard = false;
    private bool isAlarmActivated = false;
    private bool isDoorOpened = false;
    private bool isEvacuationComplete = false;
    private float timeRemaining = 120f;
    private bool isTimerRunning = true;
    private Coroutine feedbackCoroutine;

    void Start()
    {
        UpdateChecklistUI();
        UpdateMapUI();
        if (warningCanvasGroup != null) warningCanvasGroup.alpha = 0f;
        TimerManager timerMgr = FindObjectOfType<TimerManager>();
        if (timerMgr != null) timerMgr.OnTimerExpiredEvent += OnTimerExpired;
    }

    void Update()
    {
        if (isTimerRunning && !isEvacuationComplete && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (txtTimer != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                txtTimer.text = $"{minutes:00}:{seconds:00}";
                if (timeRemaining < 30f)
                    txtTimer.color = Mathf.PingPong(Time.time * 2, 1) > 0.5f ? Color.red : Color.white;
            }
        }
        else if (timeRemaining <= 0 && !isEvacuationComplete)
        {
            timeRemaining = 0;
            isTimerRunning = false;
            TriggerFeedback("⛔ TIME'S UP! Evacuation failed.", true);
        }
    }

    public void OnTimerExpired()
    {
        if (isEvacuationComplete) return;
        isTimerRunning = false;
        TriggerFeedback("⛔ TIME'S UP! Evacuation failed.", true);
    }

    // ========== BUTTON METHODS ==========
    public void InteractIDCardStation()
    {
        Debug.Log("[EvacuationManager] ID Card button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!hasIDCard)
        {
            hasIDCard = true;
            TriggerFeedback("✅ ID Card Collected!", false);
            UpdateChecklistUI();
            UpdateMapUI();
        }
        else
            TriggerFeedback("ID Card already taken.", false);
    }

    public void InteractAlarmStation()
    {
        Debug.Log("[EvacuationManager] Alarm button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!hasIDCard)
        {
            TriggerFeedback("❌ Take ID Card First!", true);
            return;
        }
        if (!isAlarmActivated)
        {
            isAlarmActivated = true;
            TriggerFeedback("🚨 Alarm Activated!", false);
            UpdateChecklistUI();
            UpdateMapUI();
        }
        else
            TriggerFeedback("Alarm already active.", false);
    }

    public void InteractEmergencyDoorStation()
    {
        Debug.Log("[EvacuationManager] Door button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!hasIDCard)
        {
            TriggerFeedback("❌ Take ID Card First!", true);
            return;
        }
        if (!isAlarmActivated)
        {
            TriggerFeedback("❌ Activate Alarm First!", true);
            return;
        }
        if (!isDoorOpened)
        {
            isDoorOpened = true;
            TriggerFeedback("🚪 Emergency Door Opened!", false);
            UpdateChecklistUI();
            UpdateMapUI();
            DoorController door = FindObjectOfType<DoorController>();
            if (door != null) door.OpenDoor();
        }
        else
            TriggerFeedback("Door already open.", false);
    }

    public void InteractExitStation()
    {
        Debug.Log("[EvacuationManager] Exit button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!isDoorOpened)
        {
            TriggerFeedback("❌ Emergency Door is Locked!", true);
            return;
        }
        isEvacuationComplete = true;
        TriggerFeedback("🏆 EVACUATION COMPLETE! You are safe.", false);
        UpdateChecklistUI();
        UpdateMapUI();
        TimerManager timerMgr = FindObjectOfType<TimerManager>();
        if (timerMgr != null) timerMgr.StopTimer();
    }

    // ========== UI UPDATE ==========
    private void UpdateChecklistUI()
    {
        if (txtObjectiveID != null)
        {
            txtObjectiveID.text = (hasIDCard ? "☑" : "☐") + " 1. Take ID Card";
            txtObjectiveID.color = hasIDCard ? colorDone : colorActive;
        }
        if (txtObjectiveAlarm != null)
        {
            txtObjectiveAlarm.text = (isAlarmActivated ? "☑" : "☐") + " 2. Activate Alarm";
            txtObjectiveAlarm.color = isAlarmActivated ? colorDone : (!hasIDCard ? colorTodo : colorActive);
        }
        if (txtObjectiveDoor != null)
        {
            txtObjectiveDoor.text = (isDoorOpened ? "☑" : "☐") + " 3. Open Emergency Door";
            txtObjectiveDoor.color = isDoorOpened ? colorDone : (!isAlarmActivated ? colorTodo : colorActive);
        }
        if (txtObjectiveExit != null)
        {
            txtObjectiveExit.text = (isEvacuationComplete ? "☑" : "☐") + " 4. Go To Exit";
            txtObjectiveExit.color = isEvacuationComplete ? colorDone : (!isDoorOpened ? colorTodo : colorActive);
        }
    }

    private void UpdateMapUI()
    {
        if (mapIDCard != null) mapIDCard.color = hasIDCard ? colorDone : colorActive;
        if (mapAlarm != null) mapAlarm.color = isAlarmActivated ? colorDone : (hasIDCard ? colorActive : colorTodo);
        if (mapDoor != null) mapDoor.color = isDoorOpened ? colorDone : (isAlarmActivated ? colorActive : colorTodo);
        if (mapExit != null) mapExit.color = isEvacuationComplete ? colorDone : (isDoorOpened ? colorActive : colorTodo);
    }

    private void TriggerFeedback(string message, bool isWarning)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(ShowFeedbackRoutine(message, isWarning));
    }

    private IEnumerator ShowFeedbackRoutine(string message, bool isWarning)
    {
        if (txtFeedback != null)
        {
            txtFeedback.text = message;
            txtFeedback.color = isWarning ? Color.red : Color.green;
        }
        if (warningCanvasGroup == null) yield break;
        float duration = 0.2f, elapsed = 0f;
        while (elapsed < duration)
        {
            warningCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        warningCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(2f);
        elapsed = 0f;
        while (elapsed < duration)
        {
            warningCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        warningCanvasGroup.alpha = 0f;
    }
}