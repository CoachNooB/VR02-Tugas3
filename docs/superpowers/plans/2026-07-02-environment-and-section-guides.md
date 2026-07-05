# Environment Textures and Section Guides Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Texture the course environment, add four section-specific guide NPCs with head-hit reactions, and make repeated finish interactions safe.

**Architecture:** Extend the existing asset and deterministic scene builders rather than hand-editing generated objects. Make NPC dialogue instance-configurable, add a focused camera-ray head-hit component, and make course completion return a stable recorded result.

**Tech Stack:** Unity 6000.4.6f1, URP 17.4, C#, Animator Controller, NUnit Unity Test Framework.

---

### Task 1: Instance-Configurable NPC Dialogue

**Files:**
- Modify: `Assets/Tugas7/Scripts/T7_TutorialNPC.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_TutorialNPCTests.cs`

- [ ] **Step 1: Write the failing dialogue configuration test**

Add a test that creates an NPC, calls:

```csharp
npc.ConfigureDialogue(new[] { "Section-specific line." });
CollectionAssert.AreEqual(new[] { "Section-specific line." }, npc.DialogueLines);
```

Also assert a new NPC exposes the existing four default tutorial lines.

- [ ] **Step 2: Run the focused Edit Mode test**

```bash
Unity -batchmode -nographics -projectPath "$PWD" -runTests \
  -testPlatform EditMode -testFilter Tugas7.Tests.T7_TutorialNPCTests \
  -testResults TestResults/dialogue-red.xml -logFile Logs/dialogue-red.log
```

Expected: FAIL because `ConfigureDialogue` and `DialogueLines` do not exist.

- [ ] **Step 3: Implement per-instance dialogue**

Add:

```csharp
private string[] dialogueLines = TutorialLines;
public IReadOnlyList<string> DialogueLines => dialogueLines;

public void ConfigureDialogue(IReadOnlyList<string> lines)
{
    dialogueLines = lines == null || lines.Count == 0
        ? TutorialLines
        : lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
}
```

Change the conversation coroutine to iterate `dialogueLines`.

- [ ] **Step 4: Run the focused test and commit**

Expected: all `T7_TutorialNPCTests` pass.

```bash
git add Assets/Tugas7/Scripts/T7_TutorialNPC.cs \
  Assets/Tugas7/Tests/EditMode/T7_TutorialNPCTests.cs
git commit -m "feat(npc): configure guide dialogue"
```

### Task 2: Head-Hit Animation and Ray Interaction

**Files:**
- Create: `Assets/Tugas7/Scripts/T7_NPCHeadHitInteractor.cs`
- Modify: `Assets/Tugas7/Scripts/T7_TutorialNPC.cs`
- Modify: `Assets/Tugas7/Editor/T7_UpgradeAssetBuilder.cs`
- Modify: `Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_TutorialNPCTests.cs`
- Create: `Assets/Tugas7/Tests/PlayMode/T7_NPCHeadHitPlayModeTests.cs`
- Modify: `Assets/Animations/Ch44_nonPBR@Head Hit.fbx.meta` through importer configuration

- [ ] **Step 1: Write failing head-hit tests**

Test that:

```csharp
Assert.That(npc.TryPlayHeadHit(), Is.True);
Assert.That(animator.GetBool("IsTalking"), Is.False);
```

For ray interaction, construct a camera, NPC capsule collider, and interactor. Assert direct hits within three meters call the NPC, while targets beyond three meters do not.

- [ ] **Step 2: Run focused Edit and Play Mode tests**

Expected: FAIL because `TryPlayHeadHit` and `T7_NPCHeadHitInteractor` do not exist.

- [ ] **Step 3: Configure the FBX and animator**

In `T7_UpgradeAssetBuilder`, import `Ch44_nonPBR@Head Hit.fbx` as Humanoid, copy the Waving avatar, bake root transforms, set clip name `Head Hit`, and disable looping. Add trigger `HeadHit`, a one-shot state, Any State transition into it, and exit transitions selected by `IsTalking`.

- [ ] **Step 4: Implement runtime interaction**

Add to `T7_TutorialNPC`:

```csharp
public bool TryPlayHeadHit()
{
    if (!isActiveAndEnabled || animator == null) return false;
    animator.SetTrigger("HeadHit");
    return true;
}
```

Implement `T7_NPCHeadHitInteractor` with configured camera/range, `Physics.Raycast`, `GetComponentInParent<T7_TutorialNPC>()`, and left-click polling. It must not disable or consume `T7_CratePusher`.

- [ ] **Step 5: Add NPC hit colliders and player wiring**

Add a capsule hit collider to the generated NPC prefab and configure one shared player ray interactor at three meters in `CreatePlayer`.

- [ ] **Step 6: Run tests and commit**

Expected: focused Edit and Play Mode tests pass.

```bash
git add Assets/Animations/Ch44_nonPBR@Head\\ Hit.fbx.meta Assets/Tugas7
git commit -m "feat(npc): add head-hit reaction"
```

### Task 3: Section Guide Placement

**Files:**
- Modify: `Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_TutorialNPCTests.cs`

- [ ] **Step 1: Write a failing scene-builder contract test**

After rebuilding, load `Assets/Scenes/T6_T7_MainScene.unity` and assert exactly five `T7_TutorialNPC` components: one start guide and four section guides. Assert their configured lines match the spec and every child canvas uses `RenderMode.WorldSpace`.

- [ ] **Step 2: Run the test**

Expected: FAIL with one NPC found.

- [ ] **Step 3: Generalize NPC construction**

Replace the single-purpose method with:

```csharp
private static T7_TutorialNPC BuildTutorialNPC(
    Transform parent, string name, Vector3 position,
    Transform player, Camera camera, IReadOnlyList<string> lines)
```

Use it for the existing start guide and four section guides positioned against side walls outside required jumps and interaction rays.

- [ ] **Step 4: Rebuild, test, and commit**

```bash
Unity -batchmode -nographics -projectPath "$PWD" \
  -executeMethod Tugas7.Editor.T7_CourseSceneBuilder.RebuildBatch \
  -logFile Logs/section-guides-rebuild.log -quit
```

Expected: rebuild succeeds and the scene contract test passes.

```bash
git add Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs \
  Assets/Scenes/T6_T7_MainScene.unity Assets/Tugas7/Tests
git commit -m "feat(scene): add section guides"
```

### Task 4: Textured Gameplay Environment

**Files:**
- Modify: `Assets/Tugas7/Editor/T7_UpgradeAssetBuilder.cs`
- Modify: `Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_LavaMaterialTests.cs`

- [ ] **Step 1: Write failing material assignment tests**

Rebuild and assert representative renderers use shared materials:

```csharp
Assert.AreEqual("T7_ReinforcedConcrete", platform.sharedMaterial.name);
Assert.AreEqual("T7_DarkConcreteWall", wall.sharedMaterial.name);
Assert.AreEqual("T7_DangerMetal", sweeper.sharedMaterial.name);
Assert.AreEqual("T7_WeatheredMetal", gate.sharedMaterial.name);
Assert.AreEqual("T7_InteractableMetal", terminal.sharedMaterial.name);
```

Assert all materials have `_BaseMap`; concrete and metal variants also have `_BumpMap`.

- [ ] **Step 2: Run the material tests**

Expected: FAIL because gameplay primitives still use flat-color materials.

- [ ] **Step 3: Generate shared variants**

Create `T7_DarkConcreteWall`, `T7_DangerMetal`, and `T7_InteractableMetal` URP Lit materials using existing CC0 base/normal/ORM maps. Preserve red emission on danger metal and cyan/gold via renderer property blocks or dedicated shared variants.

- [ ] **Step 4: Assign materials by category**

Update `CreateFloor`, `CreateEnvironment`, `CreateSideWalls`, `CreateGate`, `CreateSweeper`, moving-platform construction, checkpoint floors, and interactable creation to use the new shared textured variants without changing transforms, colliders, or gameplay scripts.

- [ ] **Step 5: Rebuild, test, and commit**

Expected: material tests and existing gameplay tests pass.

```bash
git add Assets/Tugas7/Editor Assets/Tugas7/Materials \
  Assets/Tugas7/Tests Assets/Scenes/T6_T7_MainScene.unity
git commit -m "feat(environment): texture gameplay surfaces"
```

### Task 5: Idempotent Finish Beacon

**Files:**
- Modify: `Assets/Tugas7/Scripts/T7_CourseManager.cs`
- Modify: `Assets/Tugas7/Scripts/T7_CourseInteractable.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_CourseSystemsTests.cs`

- [ ] **Step 1: Write the failing repeated-finish test**

Complete all checkpoints, finish once, call `Interact()` again, and assert both responses equal the same `Run complete — MM:SS.s` message while `IsComplete` remains true.

- [ ] **Step 2: Run the focused test**

Expected: FAIL because the second interaction reports locked.

- [ ] **Step 3: Store and expose the result**

Add:

```csharp
public string CompletionMessage { get; private set; }
```

Set it once in `TryFinishCourse`. In `T7_CourseInteractable.Interact`, return the stored message when the course is already complete; do not invoke completion events again.

- [ ] **Step 4: Run tests and commit**

Expected: repeated finish and existing course tests pass.

```bash
git add Assets/Tugas7/Scripts/T7_CourseManager.cs \
  Assets/Tugas7/Scripts/T7_CourseInteractable.cs \
  Assets/Tugas7/Tests/EditMode/T7_CourseSystemsTests.cs
git commit -m "fix(finish): make beacon interaction idempotent"
```

### Task 6: Final Verification

**Files:**
- Modify generated assets only if the deterministic rebuild changes them.

- [ ] **Step 1: Rebuild the scene twice**

Run `T7_CourseSceneBuilder.RebuildBatch` twice and confirm the second run produces no unexpected duplicate objects or asset churn.

- [ ] **Step 2: Run compilation and all tests**

Run Unity batch compilation, then the complete Edit Mode and Play Mode suites. Expected: zero compile errors and all tests pass.

- [ ] **Step 3: Inspect final scene contracts**

Confirm five NPCs, all world-space canvases, the Head Hit animator state, three-meter hit range, textured representative surfaces, unchanged gameplay colliders, and stable repeated finish responses.

- [ ] **Step 4: Commit generated scene changes**

```bash
git add Assets/Scenes/T6_T7_MainScene.unity Assets/Tugas7
git commit -m "chore(scene): rebuild textured facility"
```
