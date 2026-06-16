using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EvacuationManager : MonoBehaviour
{
    // UI References
    public TextMeshProUGUI txtObjectiveID;
    public TextMeshProUGUI txtObjectiveAlarm;
    public TextMeshProUGUI txtObjectiveDoor;
    public TextMeshProUGUI txtObjectiveExit;
    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI txtFeedback;
    public CanvasGroup warningCanvasGroup;
    public TextMeshProUGUI warningPopupText;
    public Image flashImage;
    public Image mapIDCard;
    public Image mapAlarm;
    public Image mapDoor;
    public Image mapExit;

    // Station objects
    public GameObject stationIDCard;
    public GameObject stationAlarm;
    public GameObject stationDoor;
    public GameObject stationExit;

    [Header("Color States")]
    public Color colorTodo = Color.gray;
    public Color colorActive = Color.yellow;
    public Color colorDone = Color.green;
    public Color colorError = Color.red;

    private bool hasIDCard = false;
    private bool isAlarmActivated = false;
    private bool isDoorOpened = false;
    private bool isEvacuationComplete = false;
    private float timeRemaining = 120f;
    private bool isTimerRunning = true;
    private Coroutine feedbackCoroutine;

    void Start()
    {
        // ===== FALLBACK: Cari UI =====
        FindUIReferences();

        // ===== FALLBACK: Hubungkan tombol secara langsung di Start =====
        ConnectButtonsManually();

        UpdateChecklistUI();
        UpdateMapUI();
        if (warningCanvasGroup != null) warningCanvasGroup.alpha = 0f;

        TimerManager timerMgr = FindObjectOfType<TimerManager>();
        if (timerMgr != null) timerMgr.OnTimerExpiredEvent += OnTimerExpired;
    }

    void FindUIReferences()
    {
        if (txtObjectiveID == null) txtObjectiveID = GameObject.Find("Txt_ID")?.GetComponent<TextMeshProUGUI>();
        if (txtObjectiveAlarm == null) txtObjectiveAlarm = GameObject.Find("Txt_Alarm")?.GetComponent<TextMeshProUGUI>();
        if (txtObjectiveDoor == null) txtObjectiveDoor = GameObject.Find("Txt_Door")?.GetComponent<TextMeshProUGUI>();
        if (txtObjectiveExit == null) txtObjectiveExit = GameObject.Find("Txt_Exit")?.GetComponent<TextMeshProUGUI>();
        if (txtTimer == null) txtTimer = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        if (txtFeedback == null) txtFeedback = GameObject.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        if (warningCanvasGroup == null)
        {
            GameObject popup = GameObject.Find("WarningPopupPanel");
            if (popup != null) warningCanvasGroup = popup.GetComponent<CanvasGroup>();
        }
        if (warningPopupText == null)
        {
            GameObject popupText = GameObject.Find("PopupText");
            if (popupText != null) warningPopupText = popupText.GetComponent<TextMeshProUGUI>();
        }
        if (flashImage == null)
        {
            GameObject flash = GameObject.Find("WarningFlash");
            if (flash != null) flashImage = flash.GetComponent<Image>();
        }
        if (mapIDCard == null) mapIDCard = GameObject.Find("Light_ID")?.GetComponent<Image>();
        if (mapAlarm == null) mapAlarm = GameObject.Find("Light_Alarm")?.GetComponent<Image>();
        if (mapDoor == null) mapDoor = GameObject.Find("Light_Door")?.GetComponent<Image>();
        if (mapExit == null) mapExit = GameObject.Find("Light_Exit")?.GetComponent<Image>();
        if (stationIDCard == null) stationIDCard = GameObject.Find("IDCard_Station");
        if (stationAlarm == null) stationAlarm = GameObject.Find("Alarm_Station");
        if (stationDoor == null) stationDoor = GameObject.Find("Door_Station");
        if (stationExit == null) stationExit = GameObject.Find("Exit_Station");
    }

    void ConnectButtonsManually()
    {
        // Cari semua tombol dengan nama yang sesuai dan pasang listener
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button btn in allButtons)
        {
            string name = btn.name;
            if (name == "Btn_InteractIDCardStation")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(InteractIDCardStation);
                Debug.Log("Manually connected ID Card button");
            }
            else if (name == "Btn_InteractAlarmStation")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(InteractAlarmStation);
                Debug.Log("Manually connected Alarm button");
            }
            else if (name == "Btn_InteractEmergencyDoorStation")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(InteractEmergencyDoorStation);
                Debug.Log("Manually connected Door button");
            }
            else if (name == "Btn_InteractExitStation")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(InteractExitStation);
                Debug.Log("Manually connected Exit button");
            }
        }
    }

    void Update()
    {
        // Timer logic...
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
            ShowWarningPopup("⛔ TIME'S UP! Evacuation failed.", true);
        }
    }

    public void OnTimerExpired()
    {
        if (isEvacuationComplete) return;
        isTimerRunning = false;
        ShowWarningPopup("⛔ TIME'S UP! Evacuation failed.", true);
    }

    // ========== BUTTON METHODS ==========
    public void InteractIDCardStation()
    {
        Debug.Log("[EvacuationManager] ID Card button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!hasIDCard)
        {
            hasIDCard = true;
            ShowSuccessFeedback("✅ ID Card Collected!");
            UpdateChecklistUI();
            UpdateMapUI();
            ChangeStationColor(stationIDCard, colorDone);
            StartCoroutine(UpdateStatusAfterDelay());
        }
        else
        {
            ShowFeedback("⚠️ ID Card already taken.", false);
        }
    }

    public void InteractAlarmStation()
    {
        Debug.Log("[EvacuationManager] Alarm button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!hasIDCard)
        {
            ShowWarningPopup("❌ Take ID Card First!", true);
            return;
        }
        if (!isAlarmActivated)
        {
            isAlarmActivated = true;
            ShowSuccessFeedback("🚨 Alarm Activated!");
            UpdateChecklistUI();
            UpdateMapUI();
            ChangeStationColor(stationAlarm, colorDone);
            StartCoroutine(UpdateStatusAfterDelay());
        }
        else
        {
            ShowFeedback("⚠️ Alarm already active.", false);
        }
    }

    public void InteractEmergencyDoorStation()
    {
        Debug.Log("[EvacuationManager] Door button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!hasIDCard)
        {
            ShowWarningPopup("❌ Take ID Card First!", true);
            return;
        }
        if (!isAlarmActivated)
        {
            ShowWarningPopup("❌ Activate Alarm First!", true);
            return;
        }
        if (!isDoorOpened)
        {
            isDoorOpened = true;
            ShowSuccessFeedback("🚪 Emergency Door Opened!");
            UpdateChecklistUI();
            UpdateMapUI();
            ChangeStationColor(stationDoor, colorDone);
            DoorController door = FindObjectOfType<DoorController>();
            if (door != null) door.OpenDoor();
            StartCoroutine(UpdateStatusAfterDelay());
        }
        else
        {
            ShowFeedback("⚠️ Door already open.", false);
        }
    }

    public void InteractExitStation()
    {
        Debug.Log("[EvacuationManager] Exit button clicked!");
        if (isEvacuationComplete || timeRemaining <= 0) return;
        if (!isDoorOpened)
        {
            ShowWarningPopup("❌ Emergency Door is Locked!", true);
            return;
        }
        isEvacuationComplete = true;
        ShowSuccessFeedback("🏆 EVACUATION COMPLETE! You are safe.");
        UpdateChecklistUI();
        UpdateMapUI();
        ChangeStationColor(stationExit, colorDone);
        TimerManager timerMgr = FindObjectOfType<TimerManager>();
        if (timerMgr != null) timerMgr.StopTimer();
        StartCoroutine(UpdateStatusAfterDelay());
    }

    // ========== VISUAL EFFECTS ==========
    void ChangeStationColor(GameObject station, Color color)
    {
        if (station == null) return;
        Renderer rend = station.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(rend.material);
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.4f);
            rend.material = mat;
            Debug.Log($"Station {station.name} turned green!");
        }
    }

    // ========== UI UPDATE ==========
    public void UpdateChecklistUI()
    {
        if (txtObjectiveID != null)
        {
            txtObjectiveID.text = (hasIDCard ? "☑" : "☐") + " 1. Take ID Card";
            txtObjectiveID.color = hasIDCard ? colorDone : colorActive;
            txtObjectiveID.fontStyle = hasIDCard ? FontStyles.Strikethrough : FontStyles.Normal;
            Debug.Log($"Checklist ID updated: {txtObjectiveID.text}");
        }
        else Debug.LogWarning("txtObjectiveID is null!");
        if (txtObjectiveAlarm != null)
        {
            txtObjectiveAlarm.text = (isAlarmActivated ? "☑" : "☐") + " 2. Activate Alarm";
            txtObjectiveAlarm.color = isAlarmActivated ? colorDone : (!hasIDCard ? colorTodo : colorActive);
            txtObjectiveAlarm.fontStyle = isAlarmActivated ? FontStyles.Strikethrough : FontStyles.Normal;
        }
        if (txtObjectiveDoor != null)
        {
            txtObjectiveDoor.text = (isDoorOpened ? "☑" : "☐") + " 3. Open Emergency Door";
            txtObjectiveDoor.color = isDoorOpened ? colorDone : (!isAlarmActivated ? colorTodo : colorActive);
            txtObjectiveDoor.fontStyle = isDoorOpened ? FontStyles.Strikethrough : FontStyles.Normal;
        }
        if (txtObjectiveExit != null)
        {
            txtObjectiveExit.text = (isEvacuationComplete ? "☑" : "☐") + " 4. Go To Exit";
            txtObjectiveExit.color = isEvacuationComplete ? colorDone : (!isDoorOpened ? colorTodo : colorActive);
            txtObjectiveExit.fontStyle = isEvacuationComplete ? FontStyles.Strikethrough : FontStyles.Normal;
        }
    }

    public void UpdateMapUI()
    {
        if (mapIDCard != null) mapIDCard.color = hasIDCard ? colorDone : colorActive;
        if (mapAlarm != null) mapAlarm.color = isAlarmActivated ? colorDone : (hasIDCard ? colorActive : colorTodo);
        if (mapDoor != null) mapDoor.color = isDoorOpened ? colorDone : (isAlarmActivated ? colorActive : colorTodo);
        if (mapExit != null) mapExit.color = isEvacuationComplete ? colorDone : (isDoorOpened ? colorActive : colorTodo);
    }

    IEnumerator UpdateStatusAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (txtFeedback != null && !isEvacuationComplete)
        {
            txtFeedback.text = GetCurrentObjectiveHint();
            txtFeedback.color = Color.white;
        }
    }

    // ========== FEEDBACK ==========
    void ShowFeedback(string message, bool isWarning)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(ShowFeedbackRoutine(message, isWarning));
    }

    void ShowSuccessFeedback(string message)
    {
        ShowFeedback(message, false);
        if (flashImage != null)
            StartCoroutine(FlashColor(Color.green));
    }

    void ShowWarningPopup(string message, bool isError)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(ShowPopupRoutine(message, isError));
        if (isError && flashImage != null)
            StartCoroutine(FlashColor(Color.red));
    }

    IEnumerator ShowFeedbackRoutine(string message, bool isWarning)
    {
        if (txtFeedback != null)
        {
            txtFeedback.text = message;
            txtFeedback.color = isWarning ? colorError : colorDone;
        }
        yield return new WaitForSeconds(3f);
        if (txtFeedback != null && !isEvacuationComplete)
        {
            txtFeedback.text = GetCurrentObjectiveHint();
            txtFeedback.color = Color.white;
        }
    }

    IEnumerator ShowPopupRoutine(string message, bool isError)
    {
        if (warningCanvasGroup != null && warningPopupText != null)
        {
            warningPopupText.text = message;
            warningPopupText.color = isError ? colorError : colorDone;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                warningCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            warningCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(2.5f);
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                warningCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            warningCanvasGroup.alpha = 0f;
        }
    }

    IEnumerator FlashColor(Color color)
    {
        if (flashImage == null) yield break;
        float duration = 0.3f, elapsed = 0f;
        while (elapsed < duration)
        {
            flashImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0f, 0.5f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            flashImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0.5f, 0f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        flashImage.color = new Color(1, 0, 0, 0);
    }

    string GetCurrentObjectiveHint()
    {
        if (!hasIDCard) return "🔵 Go to BLUE station and click 'Take ID Card'!";
        if (!isAlarmActivated) return "🔴 Go to RED station and click 'Activate Alarm'!";
        if (!isDoorOpened) return "🟢 Go to GREEN station and click 'Open Door'!";
        if (!isEvacuationComplete) return "🟡 Go to YELLOW station and click 'Exit Building'!";
        return "🏆 Evacuation Complete!";
    }

    void OnDestroy()
    {
        TimerManager timerMgr = FindObjectOfType<TimerManager>();
        if (timerMgr != null) timerMgr.OnTimerExpiredEvent -= OnTimerExpired;
    }
}