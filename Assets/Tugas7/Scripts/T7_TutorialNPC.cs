using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_TutorialNPC : MonoBehaviour
    {
        public enum NPCState { Unavailable, Waving, Talking, Victorious }

        public static readonly string[] TutorialLines =
        {
            "Welcome to the Volcanic Training Facility. Reach the finish after activating all three checkpoints.",
            "Lava and red machinery damage you. Blue checkpoint zones become respawn points and restore health.",
            "Use WASD and the mouse to move, then press Space to jump over hazardous sections.",
            "Press E near highlighted controls. Left-click the yellow crate to push it onto the pressure plate."
        };

        [SerializeField] private Animator animator;
        [SerializeField] private T7_WorldSpaceDialogue dialogue;
        [SerializeField] private Transform player;
        [SerializeField, Min(0.01f)] private float secondsPerLine = 4f;
        [SerializeField, Min(0f)] private float turnSpeed = 360f;

        private Coroutine conversation;
        private bool playerNearby;
        [SerializeField] private string[] dialogueLines = TutorialLines;

        public NPCState State { get; private set; } = NPCState.Waving;
        public bool CanInteract => playerNearby && !IsTalking && !IsVictorious && isActiveAndEnabled;
        public bool IsTalking => State == NPCState.Talking;
        public bool IsVictorious => State == NPCState.Victorious;
        public IReadOnlyList<string> DialogueLines => dialogueLines;

        public event Action ConversationStarted;
        public event Action<int, string> LineChanged;
        public event Action ConversationFinished;

        public void Configure(Animator targetAnimator, T7_WorldSpaceDialogue targetDialogue, Transform targetPlayer,
            float lineDuration = 4f)
        {
            animator = targetAnimator;
            dialogue = targetDialogue;
            player = targetPlayer;
            secondsPerLine = Mathf.Max(0.01f, lineDuration);
            if (animator != null)
                animator.keepAnimatorStateOnDisable = true;
            SetTalkingAnimation(false);
            SetVictoryAnimation(IsVictorious);
        }

        public void SetPlayerNearby(bool nearby)
        {
            playerNearby = nearby;
            if (IsVictorious)
            {
                dialogue?.HidePrompt();
                dialogue?.HideDialogue();
                return;
            }

            if (!nearby)
            {
                CancelConversation();
                dialogue?.HidePrompt();
                return;
            }

            if (!IsTalking)
                dialogue?.ShowPrompt("Press E — Talk to Guide");
        }

        public void ConfigureDialogue(IReadOnlyList<string> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                dialogueLines = TutorialLines;
                return;
            }
            var configured = new List<string>(lines.Count);
            for (int i = 0; i < lines.Count; i++)
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    configured.Add(lines[i]);
            dialogueLines = configured.Count == 0 ? TutorialLines : configured.ToArray();
        }

        public bool TryStartConversation()
        {
            if (!CanInteract)
                return false;

            State = NPCState.Talking;
            dialogue?.HidePrompt();
            SetTalkingAnimation(true);
            ConversationStarted?.Invoke();
            if (!IsTalking)
                return true;
            Coroutine startedConversation = StartCoroutine(ConversationRoutine());
            if (IsTalking)
                conversation = startedConversation;
            else
            {
                if (startedConversation != null)
                    StopCoroutine(startedConversation);
                conversation = null;
            }
            return true;
        }

        public void EnterVictory()
        {
            if (IsVictorious)
            {
                SetVictoryAnimation(true);
                dialogue?.HidePrompt();
                dialogue?.HideDialogue();
                return;
            }

            bool wasTalking = IsTalking;
            if (conversation != null)
            {
                StopCoroutine(conversation);
                conversation = null;
            }

            State = NPCState.Victorious;
            SetTalkingAnimation(false);
            SetVictoryAnimation(true);
            dialogue?.HidePrompt();
            dialogue?.HideDialogue();
            if (wasTalking)
                ConversationFinished?.Invoke();
        }

        public bool TryPlayHeadHit()
        {
            if (!isActiveAndEnabled)
                return false;
            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
                return false;
            animator.SetTrigger("HeadHit");
            return true;
        }

        public void CancelConversation()
        {
            if (conversation != null)
            {
                StopCoroutine(conversation);
                conversation = null;
            }

            bool wasTalking = IsTalking;
            if (!IsVictorious)
                State = NPCState.Waving;
            SetTalkingAnimation(false);
            dialogue?.HideDialogue();
            if (wasTalking)
                ConversationFinished?.Invoke();
        }

        private IEnumerator ConversationRoutine()
        {
            for (int i = 0; i < dialogueLines.Length; i++)
            {
                if (!IsTalking)
                    yield break;

                dialogue?.ShowDialogue("FACILITY GUIDE", dialogueLines[i]);
                LineChanged?.Invoke(i, dialogueLines[i]);
                if (!IsTalking)
                    yield break;
                yield return new WaitForSeconds(secondsPerLine);
            }

            if (!IsTalking)
                yield break;

            conversation = null;
            State = NPCState.Waving;
            SetTalkingAnimation(false);
            dialogue?.HideDialogue();
            ConversationFinished?.Invoke();
            if (playerNearby && !IsVictorious)
                dialogue?.ShowPrompt("Press E — Talk to Guide");
        }

        private void Update()
        {
            UpdateFacing(Time.deltaTime);
        }

        public void UpdateFacing(float deltaTime)
        {
            if (player == null)
                return;

            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                return;
            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * deltaTime);
        }

        private void OnDisable() => CancelConversation();

        private void OnEnable() => SetVictoryAnimation(IsVictorious);

        private void SetTalkingAnimation(bool value)
        {
            SetAnimatorBool("IsTalking", value);
        }

        private void SetVictoryAnimation(bool value)
        {
            SetAnimatorBool("IsVictorious", value);
        }

        private void SetAnimatorBool(string parameterName, bool value)
        {
            if (animator == null)
                return;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName &&
                    parameter.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(parameterName, value);
                    return;
                }
            }
        }
    }
}
