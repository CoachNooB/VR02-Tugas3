using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UTS_GameController : MonoBehaviour
{
    [Header ("Doors")]
    private bool securityOpen;
    private bool corridorOpen;
    private bool officeOpen;
    private bool controlRoomOpen;
    private bool exitOpen;
    public float autoCloseDelay = 5f;
    private float openTimer = 0f;
    private Color accessColor = new Color(0f, 1f, 0f, 1f);
    private Color deniedColor = new Color(1f, 0f, 0f, 1f);

    [Header ("Game Objective")]
    private bool hasKeyCard;
    private bool alarmActive;
    private bool exitDoorActive;
    private bool isEvacuationComplete;
    private bool gameOver;

    [Header ("UI Text")]
    [SerializeField] TextMeshProUGUI _txtObjectiveIDCard;
    [SerializeField] TextMeshProUGUI _txtObjectiveAlarm;
    [SerializeField] TextMeshProUGUI _txtObjectiveExitDoor;
    [SerializeField] TextMeshProUGUI _txtObjectiveExit;
    [SerializeField] TextMeshProUGUI _txtStatus;
    [SerializeField] TextMeshProUGUI _txtTimer;
    [SerializeField] TextMeshProUGUI _txtGameOver;
    [SerializeField] CanvasGroup _warningCanvasGroup;
    [SerializeField] TextMeshProUGUI _warningPopupText;
    [SerializeField] Image _flashImage;
    [SerializeField] Image _mapIDCard;
    [SerializeField] Image _mapAlarm;
    [SerializeField] Image _mapDoor;
    [SerializeField] Image _mapExit;

    [Header("Color States")]
    public Color colorTodo = Color.gray;
    public Color colorActive = Color.yellow;
    public Color colorDone = Color.green;
    public Color colorError = Color.red;

    private Coroutine feedbackCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hasKeyCard = false;
        alarmActive = false;
        exitDoorActive = false;
        isEvacuationComplete = false;
        gameOver = false;
        _txtGameOver.color = new Color(0f, 0f, 0f, 0f);
        UpdateChecklistUI();
        UpdateMapUI();
        if (_warningCanvasGroup != null) _warningCanvasGroup.alpha = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameOver)
        {
            Time.timeScale = 0f;
            if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                RestartGame();
            }
        } 
    }

    public void ToggleIsOpen(string door)
    {
        if(door == "corridor")
        {
            corridorOpen = !corridorOpen;
        }
        if(door == "security")
        {
            if(!hasKeyCard) ShowWarningPopup("Take ID Card First!", true);
            securityOpen = (hasKeyCard) ? !securityOpen : false;
        }
        if(door == "office")
        {
            officeOpen = !officeOpen;
        }
        if(door == "control")
        {
            if(!hasKeyCard) ShowWarningPopup("Take ID Card First!", true);
            if(!alarmActive) ShowWarningPopup("Activate Alarm First!", true);
            controlRoomOpen = (hasKeyCard && alarmActive) ? !controlRoomOpen : false;
        }
        if(door == "exit")
        {
            if(!hasKeyCard) ShowWarningPopup("Take ID Card First!", true);
            if(!alarmActive) ShowWarningPopup("Activate Alarm First!", true);
            if(!exitDoorActive) ShowWarningPopup("Activate Exit First!", true);
            exitOpen = (hasKeyCard && alarmActive && exitDoorActive) ? !exitOpen : false;
        }
        StartCoroutine(closeDoor(door));
    }

    IEnumerator closeDoor(string door)
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if(door == "corridor")
        {
            corridorOpen = false;
        }
        if(door == "security")
        {
            securityOpen = false;
        }
        if(door == "office")
        {
            officeOpen = false;
        }
        if(door == "control")
        {
            controlRoomOpen = false;
        }
        if(door == "exit")
        {
            exitOpen = false;
        }
    }

    public bool GetDoorStatus(string door)
    {
        if(door == "corridor")
        {
            return corridorOpen;
        }
        if(door == "security")
        {
            return securityOpen;
        }
        if(door == "office")
        {
            return officeOpen;
        }
        if(door == "control")
        {
            return controlRoomOpen;
        }
        if(door == "exit")
        {
            return exitOpen;
        }
        return false;
    }

    public Color GetDoorColor(string door)
    {
        if(door == "corridor")
        {
            return accessColor;
        }
        if(door == "security")
        {
            return (hasKeyCard) ? accessColor : deniedColor;
        }
        if(door == "office")
        {
            return accessColor;
        }
        if(door == "control")
        {
            return (hasKeyCard && alarmActive) ? accessColor : deniedColor;
        }
        if(door == "exit")
        {
            return (hasKeyCard && alarmActive && exitDoorActive) ? accessColor : deniedColor;
        }
        return deniedColor;
    }

    public void SetHasKeyCard()
    {
        hasKeyCard = true;
        ShowSuccessFeedback("ID Card Collected!");
        UpdateChecklistUI();
        UpdateMapUI();
        StartCoroutine(UpdateStatusAfterDelay());
    }

    public void SetAlarmActive()
    {
        if(alarmActive)
        {
            ShowWarningPopup("Alarm Already Active!", true);
            return; 
        }
        alarmActive = true;
        ShowSuccessFeedback("Alarm Activated!");
        UpdateChecklistUI();
        UpdateMapUI();
        StartCoroutine(UpdateStatusAfterDelay());
    }

    public void SetEmergencyExitActive()
    {
        if(exitDoorActive)
        {
            ShowWarningPopup("Emergency Door Already Active!", true);
            return; 
        }
        exitDoorActive = true;
        ShowSuccessFeedback("Emergency Exit Activated!");
        UpdateChecklistUI();
        UpdateMapUI();
        StartCoroutine(UpdateStatusAfterDelay());
    }

    public void SetEvacFinish()
    {
        isEvacuationComplete = true;
        ShowSuccessFeedback("EVACUATION COMPLETE! You are safe.");
        UpdateChecklistUI();
        UpdateMapUI();
        StartCoroutine(UpdateStatusAfterDelay());
        StartCoroutine(SetGameOver(true));
    }

    public IEnumerator SetGameOver(bool success)
    {
        ShowPopupRoutine("Play Again? (Press Enter)", false);
        if(success)
        {
            ShowSuccessFeedback("EVACUATION COMPLETE! You are safe.");
            _txtGameOver.color = new Color(0f, 1f, 0f, 1f);
        }
        else
        {
            ShowWarningPopup("EVACUATION FAILED!", true);
            _txtGameOver.color = new Color(1f, 0f, 0f, 1f);
        }
        yield return new WaitForSeconds(5f);
        gameOver = true;
    }

    // ========== UI UPDATE ==========
    public void UpdateChecklistUI()
    {
        if (_txtObjectiveIDCard != null)
        {
            _txtObjectiveIDCard.text = (hasKeyCard ? "☑" : "☐") + " 1. Take ID Card";
            _txtObjectiveIDCard.color = hasKeyCard ? colorDone : colorActive;
            _txtObjectiveIDCard.fontStyle = hasKeyCard ? FontStyles.Strikethrough : FontStyles.Normal;
        }
        else Debug.LogWarning("txtObjectiveIDCard is null!");
        if (_txtObjectiveAlarm != null)
        {
            _txtObjectiveAlarm.text = (alarmActive ? "☑" : "☐") + " 2. Activate Alarm";
            _txtObjectiveAlarm.color = alarmActive ? colorDone : (!hasKeyCard ? colorTodo : colorActive);
            _txtObjectiveAlarm.fontStyle = alarmActive ? FontStyles.Strikethrough : FontStyles.Normal;
        }
        if (_txtObjectiveExitDoor != null)
        {
            _txtObjectiveExitDoor.text = (exitDoorActive ? "☑" : "☐") + " 3. Open Emergency Door";
            _txtObjectiveExitDoor.color = exitDoorActive ? colorDone : (!alarmActive ? colorTodo : colorActive);
            _txtObjectiveExitDoor.fontStyle = exitDoorActive ? FontStyles.Strikethrough : FontStyles.Normal;
        }
        if (_txtObjectiveExit != null)
        {
            _txtObjectiveExit.text = (isEvacuationComplete ? "☑" : "☐") + " 4. Go To Exit";
            _txtObjectiveExit.color = isEvacuationComplete ? colorDone : (!exitDoorActive ? colorTodo : colorActive);
            _txtObjectiveExit.fontStyle = isEvacuationComplete ? FontStyles.Strikethrough : FontStyles.Normal;
        }
    }

    public void UpdateMapUI()
    {
        if (_mapIDCard != null) _mapIDCard.color = hasKeyCard ? colorDone : colorActive;
        if (_mapAlarm != null) _mapAlarm.color = alarmActive ? colorDone : (hasKeyCard ? colorActive : colorTodo);
        if (_mapDoor != null) _mapDoor.color = exitDoorActive ? colorDone : (alarmActive ? colorActive : colorTodo);
        if (_mapExit != null) _mapExit.color = isEvacuationComplete ? colorDone : (exitDoorActive ? colorActive : colorTodo);
    }

    IEnumerator UpdateStatusAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (_txtStatus != null && !isEvacuationComplete)
        {
            _txtStatus.text = GetCurrentObjectiveHint();
            _txtStatus.color = Color.white;
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
        if (_flashImage != null)
            StartCoroutine(FlashColor(Color.green));
    }

    public void ShowWarningPopup(string message, bool isError)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(ShowPopupRoutine(message, isError));
        if (isError && _flashImage != null)
            StartCoroutine(FlashColor(Color.red));
    }

    public void OnTimerExpired()
    {
        if (isEvacuationComplete) return;
        ShowWarningPopup("TIME'S UP! Evacuation failed.", true);
    }

    IEnumerator ShowFeedbackRoutine(string message, bool isWarning)
    {
        if (_txtStatus != null)
        {
            _txtStatus.text = message;
            _txtStatus.color = isWarning ? colorError : colorDone;
        }
        yield return new WaitForSeconds(3f);
        if (_txtStatus != null && !isEvacuationComplete)
        {
            _txtStatus.text = GetCurrentObjectiveHint();
            _txtStatus.color = Color.white;
        }
    }

    IEnumerator ShowPopupRoutine(string message, bool isError)
    {
        if (_warningCanvasGroup != null && _warningPopupText != null)
        {
            _warningPopupText.text = message;
            _warningPopupText.color = isError ? colorError : colorDone;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                _warningCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _warningCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(2.5f);
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                _warningCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _warningCanvasGroup.alpha = 0f;
        }
    }

    IEnumerator FlashColor(Color color)
    {
        if (_flashImage == null) yield break;
        float duration = 0.3f, elapsed = 0f;
        while (elapsed < duration)
        {
            _flashImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0f, 0.5f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            _flashImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0.5f, 0f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        _flashImage.color = new Color(1, 0, 0, 0);
    }

    string GetCurrentObjectiveHint()
    {
        if (!hasKeyCard) return "Go to Office and Take ID Card!";
        if (!alarmActive) return "Go to Security and Activate Alarm!";
        if (!exitDoorActive) return "Go to Control Room and Open Exit Door!";
        if (!isEvacuationComplete) return "Go to Exit !";
        return "🏆 Evacuation Complete!";
    }

    public void RestartGame()
    {
        // PENTING: Player.cs mengubah Time.timeScale jadi 0 saat finish (game beku).
        // Tanpa baris ini, scene yang di-reload akan tetap beku.
        Time.timeScale = 1f;

        // muat ulang scene yang sedang aktif (mulai dari awal)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
