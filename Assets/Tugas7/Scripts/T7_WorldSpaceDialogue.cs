using TMPro;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_WorldSpaceDialogue : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Min(0.1f)] private float maximumDistance = 8f;

        public bool PromptVisible => promptPanel != null && promptPanel.activeSelf;
        public bool DialogueVisible => dialoguePanel != null && dialoguePanel.activeSelf;

        public void Configure(Canvas targetCanvas, GameObject targetPromptPanel, TMP_Text targetPromptText,
            GameObject targetDialoguePanel, TMP_Text targetSpeakerText, TMP_Text targetDialogueText,
            Transform targetCamera)
        {
            canvas = targetCanvas;
            promptPanel = targetPromptPanel;
            promptText = targetPromptText;
            dialoguePanel = targetDialoguePanel;
            speakerText = targetSpeakerText;
            dialogueText = targetDialogueText;
            cameraTransform = targetCamera;
            if (canvas != null)
                canvas.renderMode = RenderMode.WorldSpace;
            HidePrompt();
            HideDialogue();
        }

        public void ShowPrompt(string text)
        {
            if (promptText != null) promptText.text = text;
            if (promptPanel != null) promptPanel.SetActive(true);
        }

        public void HidePrompt()
        {
            if (promptPanel != null) promptPanel.SetActive(false);
        }

        public void ShowDialogue(string speaker, string line)
        {
            HidePrompt();
            if (speakerText != null) speakerText.text = speaker;
            if (dialogueText != null) dialogueText.text = line;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
        }

        public void HideDialogue()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        private void LateUpdate()
        {
            Transform target = cameraTransform != null ? cameraTransform : Camera.main?.transform;
            if (target == null)
                return;

            Vector3 toCamera = target.position - transform.position;
            if (toCamera.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(-toCamera.normalized, Vector3.up);
            if (canvas != null)
                canvas.enabled = toCamera.sqrMagnitude <= maximumDistance * maximumDistance;
        }
    }
}
