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
        public void VictoriousNpcRejectsHeadHit()
        {
            const string controllerPath = "Assets/Tugas7/Tests/EditMode/T7_TemporaryVictoryHeadHitTest.controller";
            var go = new GameObject("NPC");
            try
            {
                var animator = go.AddComponent<Animator>();
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("IsVictorious", AnimatorControllerParameterType.Bool);
                controller.AddParameter("HeadHit", AnimatorControllerParameterType.Trigger);
                animator.runtimeAnimatorController = controller;
                var npc = go.AddComponent<T7_TutorialNPC>();
                npc.Configure(animator, null, null);
                npc.EnterVictory();

                Assert.That(npc.TryPlayHeadHit(), Is.False);
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
        public void ConversationStartedVictoryPreventsDialogueRoutineStartup()
        {
            var npcGo = new GameObject("NPC");
            var dialogueGo = new GameObject("Dialogue");
            var dialoguePanel = new GameObject("DialoguePanel");
            try
            {
                dialoguePanel.transform.SetParent(dialogueGo.transform);
                var dialogue = dialogueGo.AddComponent<T7_WorldSpaceDialogue>();
                dialogue.Configure(null, null, null, dialoguePanel, null, null, null);
                var npc = npcGo.AddComponent<T7_TutorialNPC>();
                npc.Configure(null, dialogue, null, 1f);
                npc.ConfigureDialogue(new[] { "This line must not start." });
                int changedCount = 0;
                int finishedCount = 0;
                npc.LineChanged += (_, _) => changedCount++;
                npc.ConversationFinished += () => finishedCount++;
                npc.ConversationStarted += npc.EnterVictory;
                npc.SetPlayerNearby(true);

                Assert.That(npc.TryStartConversation(), Is.True);

                Assert.That(npc.IsVictorious, Is.True);
                Assert.That(changedCount, Is.Zero);
                Assert.That(dialogue.DialogueVisible, Is.False);
                Assert.That(finishedCount, Is.EqualTo(1));
                npc.CancelConversation();
                Assert.That(finishedCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(npcGo);
                UnityEngine.Object.DestroyImmediate(dialogueGo);
            }
        }

        [Test]
        public void TalkingAnimationIgnoresControllerWithOnlyVictoryBool()
        {
            const string controllerPath = "Assets/Tugas7/Tests/EditMode/T7_TemporaryVictoryOnlyTest.controller";
            var go = new GameObject("NPC");
            try
            {
                var animator = go.AddComponent<Animator>();
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                controller.AddParameter("IsVictorious", AnimatorControllerParameterType.Bool);
                animator.runtimeAnimatorController = controller;
                var npc = go.AddComponent<T7_TutorialNPC>();

                npc.Configure(animator, null, null);
                npc.SetPlayerNearby(true);
                npc.TryStartConversation();
                npc.CancelConversation();

                UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                AssetDatabase.DeleteAsset(controllerPath);
            }
        }

        [Test]
        public void AnimatorWritesIgnoreControllerWithoutNpcBools()
        {
            const string controllerPath = "Assets/Tugas7/Tests/EditMode/T7_TemporaryNoBoolsTest.controller";
            var go = new GameObject("NPC");
            try
            {
                var animator = go.AddComponent<Animator>();
                animator.runtimeAnimatorController =
                    AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                var npc = go.AddComponent<T7_TutorialNPC>();

                npc.Configure(animator, null, null);
                npc.EnterVictory();

                Assert.That(npc.IsVictorious, Is.True);
                UnityEngine.TestTools.LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                AssetDatabase.DeleteAsset(controllerPath);
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
        public void PreparedTutorialNpcHasLoopingVictoryAndTexturedUrpMaterials()
        {
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            Assert.That(prepare, Is.Not.Null);
            prepare.Invoke(null, null);

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/Tugas7/Animations/T7_TutorialNPC.controller");
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.parameters.Any(parameter =>
                parameter.name == "IsVictorious" &&
                parameter.type == AnimatorControllerParameterType.Bool), Is.True);
            AnimatorState[] states = controller.layers[0].stateMachine.states
                .Select(child => child.state).ToArray();
            Assert.That(states.Select(state => state.name),
                Is.EquivalentTo(new[] { "Waving", "Talking", "Head Hit", "Victory" }));
            Assert.That(controller.parameters.Any(parameter =>
                parameter.name == "IsTalking" &&
                parameter.type == AnimatorControllerParameterType.Bool), Is.True);
            Assert.That(controller.parameters.Any(parameter =>
                parameter.name == "HeadHit" &&
                parameter.type == AnimatorControllerParameterType.Trigger), Is.True);
            AnimatorState victory = states.Single(state => state.name == "Victory");
            Assert.That(victory.motion, Is.TypeOf<AnimationClip>());
            Assert.That(((AnimationClip)victory.motion).isLooping, Is.True);
            Assert.That(controller.layers[0].stateMachine.defaultState.name, Is.EqualTo("Waving"));
            Assert.That(victory.transitions, Is.Empty);
            AssertVictoryRoute(states, "Waving", false);
            AssertVictoryRoute(states, "Talking", false);
            AssertVictoryRoute(states, "Head Hit", true);
            AssertTalkRoute(states, "Waving", "Talking", AnimatorConditionMode.If);
            AssertTalkRoute(states, "Talking", "Waving", AnimatorConditionMode.IfNot);
            AssertTalkRoute(states, "Head Hit", "Talking", AnimatorConditionMode.If);
            AssertTalkRoute(states, "Head Hit", "Waving", AnimatorConditionMode.IfNot);
            AnimatorStateTransition hitEntry = controller.layers[0].stateMachine.anyStateTransitions
                .Single(transition => transition.destinationState.name == "Head Hit");
            Assert.That(hitEntry.hasExitTime, Is.False);
            Assert.That(hitEntry.canTransitionToSelf, Is.False);
            Assert.That(hitEntry.duration, Is.EqualTo(0.15f).Within(0.001f));
            Assert.That(hitEntry.conditions, Has.Length.EqualTo(2));
            Assert.That(hitEntry.conditions.Any(condition =>
                condition.parameter == "HeadHit" && condition.mode == AnimatorConditionMode.If), Is.True);
            Assert.That(hitEntry.conditions.Any(condition =>
                condition.parameter == "IsVictorious" && condition.mode == AnimatorConditionMode.IfNot), Is.True);

            ModelImporter wavingImporter = (ModelImporter)AssetImporter.GetAtPath(
                "Assets/Animations/Ch44_nonPBR@Waving.fbx");
            ModelImporter victoryImporter = (ModelImporter)AssetImporter.GetAtPath(
                "Assets/Animations/Ch44_nonPBR@Victory Idle.fbx");
            Assert.That(victoryImporter.animationType, Is.EqualTo(ModelImporterAnimationType.Human));
            Assert.That(victoryImporter.avatarSetup, Is.EqualTo(ModelImporterAvatarSetup.CopyFromOther));
            Assert.That(victoryImporter.sourceAvatar,
                Is.EqualTo(AssetDatabase.LoadAllAssetsAtPath(wavingImporter.assetPath).OfType<Avatar>().First()));
            ModelImporterClipAnimation importedVictory = victoryImporter.clipAnimations.Single();
            Assert.That(importedVictory.name, Is.EqualTo("Victory"));
            Assert.That(importedVictory.loopTime, Is.True);
            Assert.That(importedVictory.lockRootRotation, Is.True);
            Assert.That(importedVictory.lockRootPositionXZ, Is.True);
            Assert.That(importedVictory.lockRootHeightY, Is.True);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Tugas7/Prefabs/T7_TutorialNPC.prefab");
            Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
            Assert.That(animators, Has.Length.EqualTo(1));
            Assert.That(animators[0].gameObject, Is.EqualTo(prefab));
            Assert.That(animators[0].applyRootMotion, Is.False);
            Assert.That(animators[0].keepAnimatorStateOnDisable, Is.True);
            Assert.That(animators[0].avatar, Is.EqualTo(victoryImporter.sourceAvatar));
            Assert.That(animators[0].runtimeAnimatorController, Is.EqualTo(controller));
            SkinnedMeshRenderer[] renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.That(renderers, Is.Not.Empty);
            Assert.That(renderers.Sum(renderer => renderer.sharedMaterials.Length), Is.EqualTo(2));
            Assert.That(renderers, Has.All.Matches<SkinnedMeshRenderer>(renderer =>
                renderer.sharedMesh != null && renderer.bones.Length > 0));
            foreach (SkinnedMeshRenderer renderer in renderers)
            foreach (Material material in renderer.sharedMaterials)
            {
                Assert.That(material, Is.Not.Null, renderer.name);
                Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"), renderer.name);
                Assert.That(material.GetTexture("_BaseMap"), Is.Not.Null, renderer.name);
                Assert.That(material.GetFloat("_Metallic"), Is.Zero.Within(0.001f), material.name);
                Assert.That(material.GetFloat("_Smoothness"),
                    Is.EqualTo(material.name.Contains("Skin") ? 0.32f : 0.16f).Within(0.001f), material.name);
                StringAssert.StartsWith("Assets/Tugas7/Materials/NPC/",
                    AssetDatabase.GetAssetPath(material), renderer.name);
                Texture2D texture = (Texture2D)material.GetTexture("_BaseMap");
                if (AssetDatabase.GetAssetPath(texture).StartsWith("Assets/Tugas7/Textures/NPC/"))
                {
                    Assert.That(texture.width, Is.EqualTo(256));
                    Assert.That(texture.height, Is.EqualTo(256));
                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(
                        AssetDatabase.GetAssetPath(texture));
                    Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Default));
                    Assert.That(importer.sRGBTexture, Is.True);
                    Assert.That(importer.mipmapEnabled, Is.True);
                    Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Compressed));
                    Assert.That(importer.maxTextureSize, Is.EqualTo(256));
                }
            }
        }

        [Test]
        public void PrepareAllRepairsMalformedNpcController()
        {
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/Tugas7/Animations/T7_TutorialNPC.controller");
            prepare.Invoke(null, null);
            try
            {
                AnimatorState waving = controller.layers[0].stateMachine.states.Select(child => child.state)
                    .Single(state => state.name == "Waving");
                AnimatorStateTransition victoryRoute = waving.transitions.Single(transition =>
                    transition.destinationState != null && transition.destinationState.name == "Victory");
                waving.RemoveTransition(victoryRoute);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                prepare.Invoke(null, null);
                waving = controller.layers[0].stateMachine.states.Select(child => child.state)
                    .Single(state => state.name == "Waving");
                Assert.That(waving.transitions.Any(transition =>
                    transition.destinationState != null && transition.destinationState.name == "Victory"), Is.True);
            }
            finally
            {
                prepare.Invoke(null, null);
            }
        }

        [Test]
        public void PrepareAllRepairsWrongTypedAndDuplicateNpcParametersIdempotently()
        {
            const string path = "Assets/Tugas7/Animations/T7_TutorialNPC.controller";
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            prepare.Invoke(null, null);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            try
            {
                foreach (AnimatorControllerParameter parameter in controller.parameters.ToArray())
                    controller.RemoveParameter(parameter);
                controller.AddParameter("IsTalking", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("IsVictorious", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("HeadHit", AnimatorControllerParameterType.Bool);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                prepare.Invoke(null, null);
                AssertRequiredParameter(controller, "IsTalking", AnimatorControllerParameterType.Bool);
                AssertRequiredParameter(controller, "IsVictorious", AnimatorControllerParameterType.Bool);
                AssertRequiredParameter(controller, "HeadHit", AnimatorControllerParameterType.Trigger);
                Assert.That(controller.parameters, Has.Length.EqualTo(3));
                string repaired = File.ReadAllText(path);
                prepare.Invoke(null, null);
                Assert.That(File.ReadAllText(path), Is.EqualTo(repaired));
            }
            finally
            {
                prepare.Invoke(null, null);
            }
        }

        [Test]
        public void PrepareAllRepairsRequiredTransitionWithExtraCondition()
        {
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            prepare.Invoke(null, null);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/Tugas7/Animations/T7_TutorialNPC.controller");
            try
            {
                AnimatorState waving = controller.layers[0].stateMachine.states.Select(child => child.state)
                    .Single(state => state.name == "Waving");
                AnimatorStateTransition victoryRoute = waving.transitions.Single(transition =>
                    transition.destinationState != null && transition.destinationState.name == "Victory");
                victoryRoute.AddCondition(AnimatorConditionMode.If, 0f, "IsTalking");
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                prepare.Invoke(null, null);
                waving = controller.layers[0].stateMachine.states.Select(child => child.state)
                    .Single(state => state.name == "Waving");
                victoryRoute = waving.transitions.Single(transition =>
                    transition.destinationState != null && transition.destinationState.name == "Victory");
                Assert.That(victoryRoute.conditions, Has.Length.EqualTo(1));
                Assert.That(victoryRoute.conditions[0].parameter, Is.EqualTo("IsVictorious"));
            }
            finally
            {
                prepare.Invoke(null, null);
            }
        }

        [Test]
        public void PrepareAllRemovesExtraNpcTransitionsIdempotently()
        {
            const string path = "Assets/Tugas7/Animations/T7_TutorialNPC.controller";
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            prepare.Invoke(null, null);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            try
            {
                AnimatorStateMachine machine = controller.layers[0].stateMachine;
                AnimatorState[] states = machine.states.Select(child => child.state).ToArray();
                AnimatorState waving = states.Single(state => state.name == "Waving");
                AnimatorState headHit = states.Single(state => state.name == "Head Hit");
                waving.AddTransition(headHit);
                machine.AddAnyStateTransition(waving);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                prepare.Invoke(null, null);
                states = machine.states.Select(child => child.state).ToArray();
                Assert.That(states.Single(state => state.name == "Waving").transitions, Has.Length.EqualTo(2));
                Assert.That(states.Single(state => state.name == "Talking").transitions, Has.Length.EqualTo(2));
                Assert.That(states.Single(state => state.name == "Head Hit").transitions, Has.Length.EqualTo(3));
                Assert.That(states.Single(state => state.name == "Victory").transitions, Is.Empty);
                Assert.That(machine.anyStateTransitions, Has.Length.EqualTo(1));
                string repaired = File.ReadAllText(path);
                prepare.Invoke(null, null);
                Assert.That(File.ReadAllText(path), Is.EqualTo(repaired));
            }
            finally
            {
                prepare.Invoke(null, null);
            }
        }

        [Test]
        public void PrepareAllRepairsZeroAndExtraControllerLayersIdempotently()
        {
            const string path = "Assets/Tugas7/Animations/T7_TutorialNPC.controller";
            string backup = File.ReadAllText(path);
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            try
            {
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                controller.AddLayer("Unexpected");
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                prepare.Invoke(null, null);
                Assert.That(controller.layers, Has.Length.EqualTo(1));
                Assert.That(controller.layers[0].name, Is.EqualTo("Base Layer"));

                controller.layers = Array.Empty<AnimatorControllerLayer>();
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                prepare.Invoke(null, null);
                Assert.That(controller.layers, Has.Length.EqualTo(1));
                Assert.That(controller.layers[0].name, Is.EqualTo("Base Layer"));
                string repaired = File.ReadAllText(path);
                prepare.Invoke(null, null);
                Assert.That(File.ReadAllText(path), Is.EqualTo(repaired));
            }
            finally
            {
                File.WriteAllText(path, backup);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                prepare.Invoke(null, null);
            }
        }

        private static void AssertRequiredParameter(AnimatorController controller, string name,
            AnimatorControllerParameterType type)
        {
            AnimatorControllerParameter[] matches = controller.parameters
                .Where(parameter => parameter.name == name).ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), name);
            Assert.That(matches[0].type, Is.EqualTo(type), name);
        }

        private static void AssertVictoryRoute(IEnumerable<AnimatorState> states, string from, bool exitTime)
        {
            AnimatorStateTransition transition = states.Single(state => state.name == from).transitions
                .Single(item => item.destinationState != null && item.destinationState.name == "Victory");
            Assert.That(transition.hasExitTime, Is.EqualTo(exitTime), from);
            Assert.That(transition.duration, Is.EqualTo(0.15f).Within(0.001f), from);
            Assert.That(transition.conditions.Any(condition =>
                condition.parameter == "IsVictorious" && condition.mode == AnimatorConditionMode.If), Is.True, from);
        }

        private static void AssertTalkRoute(IEnumerable<AnimatorState> states, string from, string to,
            AnimatorConditionMode mode)
        {
            AnimatorStateTransition transition = states.Single(state => state.name == from).transitions
                .Single(item => item.destinationState != null && item.destinationState.name == to);
            Assert.That(transition.duration, Is.EqualTo(0.15f).Within(0.001f), $"{from}->{to}");
            Assert.That(transition.conditions.Any(condition =>
                condition.parameter == "IsTalking" && condition.mode == mode), Is.True, $"{from}->{to}");
            Assert.That(transition.conditions.Any(condition =>
                condition.parameter == "IsVictorious" && condition.mode == AnimatorConditionMode.IfNot),
                Is.True, $"{from}->{to}");
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
                    .Where(npc => npc.transform.parent == null ||
                        npc.transform.parent.name != "FinishCelebrationNPCs")
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
        public void BuiltSceneContainsConfiguredFinishCelebration()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/T6_T7_MainScene.unity", OpenSceneMode.Additive);
            try
            {
                GameObject gauntlet = scene.GetRootGameObjects()
                    .Single(root => root.name == "T7_GauntletRoot");
                Transform finish = gauntlet.transform.Find("FinishArea/FinishCelebrationNPCs");
                Assert.That(finish, Is.Not.Null);
                T7_TutorialNPC[] finishNpcs =
                    finish.GetComponentsInChildren<T7_TutorialNPC>(true);
                Assert.That(finishNpcs, Has.Length.EqualTo(2));
                Assert.That(gauntlet.GetComponentsInChildren<T7_TutorialNPC>(true),
                    Has.Length.EqualTo(7));
                Assert.That(finishNpcs.Select(npc => Mathf.Sign(npc.transform.position.x)),
                    Is.EquivalentTo(new[] { -1f, 1f }));
                foreach (T7_TutorialNPC npc in finishNpcs)
                {
                    Assert.That(Mathf.Abs(npc.transform.position.x), Is.GreaterThanOrEqualTo(3f));
                    Assert.That(npc.transform.position.z, Is.GreaterThan(187f));
                    Transform interaction = npc.transform.Find("InteractionRange");
                    Assert.That(interaction, Is.Not.Null);
                    Assert.That(interaction.gameObject.activeSelf, Is.False);
                    Assert.That(interaction.GetComponent<T7_NPCProximityPrompt>().enabled, Is.False);
                    Transform dialogue = npc.transform.Find("WorldSpaceDialogue");
                    Assert.That(dialogue, Is.Not.Null);
                    Assert.That(dialogue.gameObject.activeSelf, Is.False);
                }

                T7_FinishPresentation presentation =
                    finish.GetComponent<T7_FinishPresentation>();
                Assert.That(presentation, Is.Not.Null);
                AudioSource source = finish.GetComponent<AudioSource>();
                Assert.That(source, Is.Not.Null);
                Assert.That(source.playOnAwake, Is.False);
                Assert.That(source.loop, Is.False);
                Assert.That(source.spatialBlend, Is.Zero);
                Assert.That(AssetDatabase.GetAssetPath(source.clip),
                    Is.EqualTo("Assets/Audio/wow_2.wav"));

                T7_CourseInteractable beacon = gauntlet.transform
                    .Find("FinishArea/FinishBeacon").GetComponent<T7_CourseInteractable>();
                var serializedBeacon = new SerializedObject(beacon);
                Assert.That(serializedBeacon.FindProperty("highlightIntensity").floatValue,
                    Is.EqualTo(6f));
                Assert.That(serializedBeacon.FindProperty("glowColor").colorValue,
                    Is.EqualTo(new Color(1f, 0.45f, 0.03f, 1f)));
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
            const string prefabPath = "Assets/Tugas7/Prefabs/T7_TutorialNPC.prefab";
            Type builder = Type.GetType("Tugas7.Editor.T7_UpgradeAssetBuilder, Tugas7.Editor");
            MethodInfo prepare = builder?.GetMethod("PrepareAll");
            Assert.That(prepare, Is.Not.Null);

            prepare.Invoke(null, null);
            string first = File.ReadAllText(path);
            string firstPrefab = File.ReadAllText(prefabPath);
            prepare.Invoke(null, null);
            string second = File.ReadAllText(path);
            string secondPrefab = File.ReadAllText(prefabPath);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(secondPrefab, Is.EqualTo(firstPrefab));
        }
    }
}
