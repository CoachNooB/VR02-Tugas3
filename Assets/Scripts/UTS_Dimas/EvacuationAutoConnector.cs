using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class EvacuationAutoConnector : MonoBehaviour
{
    void Start()
    {
        // Only run in edit mode
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ConnectComponents();
        }
        #endif
    }

    void ConnectComponents()
    {
        EvacuationManager evac = FindObjectOfType<EvacuationManager>();
        if (evac == null) return;

        // Find all UI components
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        Image[] allImages = FindObjectsOfType<Image>();
        CanvasGroup[] allCanvasGroups = FindObjectsOfType<CanvasGroup>();

        // Connect checklist items
        foreach (var text in allTexts)
        {
            if (text.gameObject.name == "Txt_ID") evac.txtObjectiveID = text;
            if (text.gameObject.name == "Txt_Alarm") evac.txtObjectiveAlarm = text;
            if (text.gameObject.name == "Txt_Door") evac.txtObjectiveDoor = text;
            if (text.gameObject.name == "Txt_Exit") evac.txtObjectiveExit = text;
            if (text.gameObject.name == "TimerText") evac.txtTimer = text;
            if (text.gameObject.name == "StatusText") evac.txtFeedback = text;
            if (text.gameObject.name == "PopupText") evac.warningPopupText = text;
        }

        // Connect minimap
        foreach (var image in allImages)
        {
            if (image.gameObject.name == "Light_ID") evac.mapIDCard = image;
            if (image.gameObject.name == "Light_Alarm") evac.mapAlarm = image;
            if (image.gameObject.name == "Light_Door") evac.mapDoor = image;
            if (image.gameObject.name == "Light_Exit") evac.mapExit = image;
            if (image.gameObject.name == "WarningFlash") evac.flashImage = image;
        }

        // Connect CanvasGroup
        foreach (var cg in allCanvasGroups)
        {
            if (cg.gameObject.name == "WarningPopupPanel")
                evac.warningCanvasGroup = cg;
        }

        Debug.Log("EvacuationAutoConnector: Components connected successfully!");
    }
}