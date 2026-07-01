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
    }
}
