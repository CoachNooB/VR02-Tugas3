using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tugas7.Tests
{
    public class T7_TutorialNPCPlayModeTests
    {
        [UnityTest]
        public IEnumerator ConversationAdvancesAndReturnsToWaving()
        {
            var go = new GameObject("Guide");
            var animator = go.AddComponent<Animator>();
            var npc = go.AddComponent<T7_TutorialNPC>();
            npc.Configure(animator, null, null, 0.02f);
            int lineCount = 0;
            npc.LineChanged += (_, _) => lineCount++;

            npc.SetPlayerNearby(true);
            Assert.That(npc.CanInteract, Is.True);
            Assert.That(npc.TryStartConversation(), Is.True);
            Assert.That(npc.IsTalking, Is.True);

            yield return new WaitForSeconds(0.12f);

            Assert.That(lineCount, Is.EqualTo(4));
            Assert.That(npc.State, Is.EqualTo(T7_TutorialNPC.NPCState.Waving));
            Assert.That(npc.IsTalking, Is.False);
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator LeavingRangeCancelsActiveConversation()
        {
            var go = new GameObject("Guide");
            var npc = go.AddComponent<T7_TutorialNPC>();
            npc.Configure(null, null, null, 1f);
            npc.SetPlayerNearby(true);
            npc.TryStartConversation();
            yield return null;

            npc.SetPlayerNearby(false);

            Assert.That(npc.State, Is.EqualTo(T7_TutorialNPC.NPCState.Waving));
            Assert.That(npc.CanInteract, Is.False);
            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator LineChangedVictoryStopsRemainingDialogue()
        {
            var npcGo = new GameObject("Guide");
            var dialogueGo = new GameObject("Dialogue");
            var dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(dialogueGo.transform);
            var dialogue = dialogueGo.AddComponent<T7_WorldSpaceDialogue>();
            dialogue.Configure(null, null, null, dialoguePanel, null, null, null);
            var npc = npcGo.AddComponent<T7_TutorialNPC>();
            npc.Configure(null, dialogue, null, 0.02f);
            npc.ConfigureDialogue(new[] { "First", "Second" });
            int lineCount = 0;
            int finishedCount = 0;
            npc.LineChanged += (_, _) =>
            {
                lineCount++;
                npc.EnterVictory();
            };
            npc.ConversationFinished += () => finishedCount++;
            npc.SetPlayerNearby(true);

            npc.TryStartConversation();
            yield return new WaitForSeconds(0.08f);

            Assert.That(lineCount, Is.EqualTo(1));
            Assert.That(finishedCount, Is.EqualTo(1));
            Assert.That(npc.IsVictorious, Is.True);
            Assert.That(dialogue.DialogueVisible, Is.False);
            Object.Destroy(npcGo);
            Object.Destroy(dialogueGo);
        }

        [UnityTest]
        public IEnumerator ConversationFinishedVictoryDoesNotRestorePrompt()
        {
            var npcGo = new GameObject("Guide");
            var dialogueGo = new GameObject("Dialogue");
            var promptPanel = new GameObject("Prompt");
            var dialoguePanel = new GameObject("DialoguePanel");
            promptPanel.transform.SetParent(dialogueGo.transform);
            dialoguePanel.transform.SetParent(dialogueGo.transform);
            var dialogue = dialogueGo.AddComponent<T7_WorldSpaceDialogue>();
            dialogue.Configure(null, promptPanel, null, dialoguePanel, null, null, null);
            var npc = npcGo.AddComponent<T7_TutorialNPC>();
            npc.Configure(null, dialogue, null, 0.02f);
            npc.ConfigureDialogue(new[] { "Only line" });
            int finishedCount = 0;
            npc.ConversationFinished += () =>
            {
                finishedCount++;
                npc.EnterVictory();
            };
            npc.SetPlayerNearby(true);
            npc.TryStartConversation();

            yield return new WaitForSeconds(0.05f);

            Assert.That(finishedCount, Is.EqualTo(1));
            Assert.That(npc.IsVictorious, Is.True);
            Assert.That(dialogue.PromptVisible, Is.False);
            Object.Destroy(npcGo);
            Object.Destroy(dialogueGo);
        }
    }
}
