using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

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
            Assert.That(NpcType.GetProperty("IsVictorious"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("SetPlayerNearby"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("TryStartConversation"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("CancelConversation"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("EnterVictory"), Is.Not.Null);
            Assert.That(NpcType.GetMethod("UpdateFacing"), Is.Not.Null);

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
        public void UpdateFacingTracksDistantPlayerOnHorizontalPlane()
        {
            var npcGo = new GameObject("NPC");
            var playerGo = new GameObject("Player");
            try
            {
                var npc = npcGo.AddComponent<T7_TutorialNPC>();
                playerGo.transform.position = new Vector3(10f, 7f, 10f);
                npc.Configure(null, null, playerGo.transform);

                npc.UpdateFacing(1f);

                Vector3 expected = playerGo.transform.position - npcGo.transform.position;
                expected.y = 0f;
                Assert.That(Vector3.Angle(npcGo.transform.forward, expected), Is.LessThan(0.01f));
                Assert.That(npcGo.transform.forward.y, Is.EqualTo(0f).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(npcGo);
                UnityEngine.Object.DestroyImmediate(playerGo);
            }
        }

        [Test]
        public void VictoryIsIdempotentStickyAndPreventsConversation()
        {
            const string controllerPath = "Assets/Tugas7/Tests/EditMode/T7_TemporaryVictoryTest.controller";
            var go = new GameObject("NPC");
            AnimatorController controller = null;
            try
            {
                var animator = go.AddComponent<Animator>();
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
                controller.AddParameter("IsVictorious", AnimatorControllerParameterType.Bool);
                animator.runtimeAnimatorController = controller;
                var npc = go.AddComponent<T7_TutorialNPC>();
                npc.Configure(animator, null, null, 1f);
                int finishedCount = 0;
                npc.ConversationFinished += () => finishedCount++;
                npc.SetPlayerNearby(true);
                Assert.That(npc.TryStartConversation(), Is.True);

                npc.EnterVictory();
                npc.EnterVictory();
                Assert.That(animator.GetBool("IsVictorious"), Is.True);
                npc.CancelConversation();
                npc.SetPlayerNearby(false);
                go.SetActive(false);
                go.SetActive(true);
                npc.SetPlayerNearby(true);

                Assert.That(npc.IsVictorious, Is.True);
                Assert.That(npc.State, Is.EqualTo(T7_TutorialNPC.NPCState.Victorious));
                Assert.That(npc.CanInteract, Is.False);
                Assert.That(npc.TryStartConversation(), Is.False);
                Assert.That(finishedCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                AssetDatabase.DeleteAsset(controllerPath);
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator VictoryAnimationIsRestoredAfterReactivation()
        {
            const string controllerPath = "Assets/Tugas7/Tests/EditMode/T7_TemporaryReactivationTest.controller";
            var go = new GameObject("NPC");
            try
            {
                var animator = go.AddComponent<Animator>();
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("IsVictorious", AnimatorControllerParameterType.Bool);
                animator.runtimeAnimatorController = controller;
                var npc = go.AddComponent<T7_TutorialNPC>();
                npc.Configure(animator, null, null);
                npc.EnterVictory();

                go.SetActive(false);
                go.SetActive(true);
                yield return null;

                Assert.That(animator.GetBool("IsVictorious"), Is.True);
                Assert.That(npc.IsVictorious, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                AssetDatabase.DeleteAsset(controllerPath);
            }
        }

        [Test]
        public void ConfigureSynchronizesVictoryToReplacementAnimator()
        {
            const string controllerPath = "Assets/Tugas7/Tests/EditMode/T7_TemporaryReplacementTest.controller";
            var npcGo = new GameObject("NPC");
            var animatorGo = new GameObject("ReplacementAnimator");
            try
            {
                var npc = npcGo.AddComponent<T7_TutorialNPC>();
                npc.EnterVictory();
                var animator = animatorGo.AddComponent<Animator>();
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("IsVictorious", AnimatorControllerParameterType.Bool);
                animator.runtimeAnimatorController = controller;

                npc.Configure(animator, null, null);

                Assert.That(animator.GetBool("IsVictorious"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(npcGo);
                UnityEngine.Object.DestroyImmediate(animatorGo);
                AssetDatabase.DeleteAsset(controllerPath);
            }
        }

        [Test]
        public void VictorySupportsAnimatorControllerWithoutVictoryParameter()
        {
            const string controllerPath = "Assets/Tugas7/Tests/EditMode/T7_TemporaryCompatibilityTest.controller";
            var go = new GameObject("NPC");
            try
            {
                var animator = go.AddComponent<Animator>();
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
                animator.runtimeAnimatorController = controller;
                var npc = go.AddComponent<T7_TutorialNPC>();

                Assert.DoesNotThrow(() => npc.Configure(animator, null, null));
                Assert.DoesNotThrow(npc.EnterVictory);
                Assert.That(npc.State, Is.EqualTo(T7_TutorialNPC.NPCState.Victorious));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                AssetDatabase.DeleteAsset(controllerPath);
            }
        }

        [Test]
        public void VictoryHidesDialogueUiAndProximityCannotRestorePrompt()
        {
            var npcGo = new GameObject("NPC");
            var dialogueGo = new GameObject("Dialogue");
            var promptPanel = new GameObject("Prompt");
            var dialoguePanel = new GameObject("DialoguePanel");
            try
            {
                promptPanel.transform.SetParent(dialogueGo.transform);
                dialoguePanel.transform.SetParent(dialogueGo.transform);
                var dialogue = dialogueGo.AddComponent<T7_WorldSpaceDialogue>();
                dialogue.Configure(null, promptPanel, null, dialoguePanel, null, null, null);
                var npc = npcGo.AddComponent<T7_TutorialNPC>();
                npc.Configure(null, dialogue, null, 1f);
                npc.SetPlayerNearby(true);
                Assert.That(dialogue.PromptVisible, Is.True);
                Assert.That(npc.TryStartConversation(), Is.True);
                Assert.That(dialogue.DialogueVisible, Is.True);

                npc.EnterVictory();
                npc.SetPlayerNearby(true);

                Assert.That(dialogue.PromptVisible, Is.False);
                Assert.That(dialogue.DialogueVisible, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(npcGo);
                UnityEngine.Object.DestroyImmediate(dialogueGo);
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
        public void HeadHitRayRequiresDirectNpcHitWithinRange()
        {
            Type interactorType = Type.GetType("Tugas7.T7_NPCHeadHitInteractor, Tugas7.Runtime");
            Assert.That(interactorType, Is.Not.Null);
            MethodInfo configure = interactorType.GetMethod("Configure");
            MethodInfo tryHit = interactorType.GetMethod("TryHit");
            Assert.That(configure, Is.Not.Null);
            Assert.That(tryHit, Is.Not.Null);

            var player = new GameObject("Player");
            var cameraGo = new GameObject("Camera");
            var npcGo = new GameObject("NPC");
            try
            {
                cameraGo.transform.SetParent(player.transform);
                Camera camera = cameraGo.AddComponent<Camera>();
                Component interactor = player.AddComponent(interactorType);
                var npc = npcGo.AddComponent<T7_TutorialNPC>();
                npcGo.AddComponent<Animator>();
                npcGo.AddComponent<CapsuleCollider>();
                npcGo.transform.position = new Vector3(0f, 0f, 2f);
                Physics.SyncTransforms();
                configure.Invoke(interactor, new object[] { camera, 3f });

                bool nearHit = (bool)tryHit.Invoke(interactor, new object[] { new Ray(Vector3.zero, Vector3.forward) });
                npcGo.transform.position = new Vector3(0f, 0f, 4f);
                Physics.SyncTransforms();
                bool farHit = (bool)tryHit.Invoke(interactor, new object[] { new Ray(Vector3.zero, Vector3.forward) });

                Assert.That(nearHit, Is.True);
                Assert.That(farHit, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(player);
                UnityEngine.Object.DestroyImmediate(npcGo);
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
        public void BuiltSceneContainsStartAndFourSectionGuides()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/T6_T7_MainScene.unity", OpenSceneMode.Additive);
            try
            {
                T7_TutorialNPC[] guides = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<T7_TutorialNPC>(true))
                    .ToArray();
                Assert.That(guides, Has.Length.EqualTo(5));
                Assert.That(guides.Count(guide => guide.DialogueLines.Count == 1), Is.EqualTo(4));
                Assert.That(guides.SelectMany(guide => guide.GetComponentsInChildren<Canvas>(true)),
                    Has.All.Matches<Canvas>(canvas => canvas.renderMode == RenderMode.WorldSpace));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
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

        [Test]
        public void RepeatedAssetPreparationPreservesAnimatorController()
        {
            const string path = "Assets/Tugas7/Animations/T7_TutorialNPC.controller";
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            Assert.That(prepare, Is.Not.Null);

            prepare.Invoke(null, null);
            string first = File.ReadAllText(path);
            prepare.Invoke(null, null);
            string second = File.ReadAllText(path);

            Assert.That(second, Is.EqualTo(first));
        }
    }
}
