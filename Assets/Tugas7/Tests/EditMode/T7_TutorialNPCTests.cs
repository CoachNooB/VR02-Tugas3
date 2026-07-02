using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Tugas7.Tests
{
    public class T7_TutorialNPCTests
    {
        private static Type NpcType => Type.GetType("Tugas7.T7_TutorialNPC, Tugas7.Runtime");

        [Test]
        public void PublicContractAndInitialStateAreAvailable()
        {
            Assert.That(NpcType, Is.Not.Null);
            Assert.That(NpcType.GetProperty("CanInteract"), Is.Not.Null);
            Assert.That(NpcType.GetProperty("IsTalking"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("SetPlayerNearby"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("TryStartConversation"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("CancelConversation"), Is.Not.Null);

            var go = new GameObject("NPC");
            try
            {
                var npc = go.AddComponent(NpcType);
                Assert.That(NpcType.GetProperty("State")?.GetValue(npc)?.ToString(), Is.EqualTo("Waving"));
                Assert.That(NpcType.GetProperty("CanInteract")?.GetValue(npc), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ProximityControlsInteractionAndLeavingCancels()
        {
            var go = new GameObject("NPC");
            try
            {
                var npc = go.AddComponent(NpcType);
                MethodInfo nearby = NpcType.GetMethod("SetPlayerNearby");
                MethodInfo start = NpcType.GetMethod("TryStartConversation");

                Assert.That(start.Invoke(npc, null), Is.False);
                nearby.Invoke(npc, new object[] { true });
                Assert.That(NpcType.GetProperty("CanInteract").GetValue(npc), Is.True);
                Assert.That(start.Invoke(npc, null), Is.True);
                Assert.That(start.Invoke(npc, null), Is.False);
                nearby.Invoke(npc, new object[] { false });
                Assert.That(NpcType.GetProperty("IsTalking").GetValue(npc), Is.False);
                Assert.That(NpcType.GetProperty("State").GetValue(npc).ToString(), Is.EqualTo("Waving"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ConversationEmitsFourTutorialLinesInOrder()
        {
            string[] expected =
            {
                "Welcome to the Volcanic Training Facility. Reach the finish after activating all three checkpoints.",
                "Lava and red machinery damage you. Blue checkpoint zones become respawn points and restore health.",
                "Use WASD and the mouse to move, then press Space to jump over hazardous sections.",
                "Press E near highlighted controls. Left-click the yellow crate to push it onto the pressure plate."
            };
            FieldInfo lines = NpcType?.GetField("TutorialLines", BindingFlags.Public | BindingFlags.Static);
            Assert.That(lines, Is.Not.Null);
            Assert.That((IReadOnlyList<string>)lines.GetValue(null), Is.EqualTo(expected));
        }

        [Test]
        public void DialogueCanBeConfiguredPerNpcInstance()
        {
            var go = new GameObject("SectionGuide");
            try
            {
                Component npc = go.AddComponent(NpcType);
                string[] sectionLine = { "Section-specific line." };
                MethodInfo configure = NpcType.GetMethod("ConfigureDialogue");
                PropertyInfo lines = NpcType.GetProperty("DialogueLines");

                Assert.That(configure, Is.Not.Null);
                Assert.That(lines, Is.Not.Null);
                configure.Invoke(npc, new object[] { sectionLine });

                Assert.That((IReadOnlyList<string>)lines.GetValue(npc), Is.EqualTo(sectionLine));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void TutorialPrefabUsesOnlyWorldSpaceCanvases()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Tugas7/Prefabs/T7_TutorialNPC.prefab");
            Assert.That(prefab, Is.Not.Null);
            Canvas[] canvases = prefab.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases, Is.Not.Empty);
            Assert.That(canvases, Has.All.Matches<Canvas>(canvas => canvas.renderMode == RenderMode.WorldSpace));
        }

        [Test]
        public void AttributionListsEveryRetainedExternalAsset()
        {
            TextAsset attribution = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/Tugas7/ThirdParty/ATTRIBUTION.md");
            Assert.That(attribution, Is.Not.Null);
            string[] required =
            {
                "Lava_01_basecolor_1K.png", "Lava_01_emissive_1K.png", "Lava_01_normal_1K.png",
                "Lava_01_height_1K.png", "Lava_01_roughness_1K.png", "Lava_01_ambientocclusion_1K.png",
                "Prop_Barrel1.fbx", "Prop_Crate_Large.fbx", "Prop_Locker.fbx",
                "Prop_SatelliteDish.fbx", "Prop_Shelves_WideTall.fbx"
            };
            foreach (string file in required)
                StringAssert.Contains(file, attribution.text, file);
        }
    }
}
