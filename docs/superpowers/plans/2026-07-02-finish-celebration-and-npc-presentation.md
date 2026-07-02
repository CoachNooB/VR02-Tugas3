# Finish Celebration and NPC Presentation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add finish-beacon hover glow and one-time sound, procedural lava ambience, two victorious finish NPCs, continuous NPC player-facing, and textured URP NPC materials.

**Architecture:** Extend the existing deterministic asset/scene builders and keep runtime responsibilities separate. `T7_CourseManager` emits one completion event, `T7_FinishPresentation` handles sound and finish NPCs, `T7_TutorialNPC` owns facing/victory state, and `T7_ProceduralLavaAudio` generates one shared spatial ambience clip.

**Tech Stack:** Unity 6000.4.6f1, URP 17.4, C#, Mecanim Humanoid animation, Unity AudioSource/AudioClip, NUnit Unity Test Framework.

---

### Task 1: One-Time Finish Completion Signal and Beacon Glow

**Files:**
- Modify: `Assets/Tugas7/Scripts/T7_CourseManager.cs`
- Modify: `Assets/Tugas7/Scripts/T7_CourseInteractable.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_CourseSystemsTests.cs`

- [ ] **Step 1: Write failing completion-event test**

Add a test that subscribes to the manager event, completes all checkpoints, and calls finish twice:

```csharp
int completedCount = 0;
manager.CourseCompleted += () => completedCount++;
manager.TryActivateCheckpoint(1, root.transform);
manager.TryActivateCheckpoint(2, root.transform);
manager.TryActivateCheckpoint(3, root.transform);

Assert.That(manager.TryFinishCourse(), Is.True);
Assert.That(manager.TryFinishCourse(), Is.False);
Assert.That(completedCount, Is.EqualTo(1));
```

Add a renderer test using a temporary URP Lit material. Configure a beacon with gold glow, call `SetHighlighted(true)`, inspect `_EmissionColor` from its `MaterialPropertyBlock`, then call `SetHighlighted(false)` and assert restoration.

- [ ] **Step 2: Run focused test and confirm RED**

```bash
/home/pimio/Unity/Hub/Editor/6000.4.6f1/Editor/Unity \
  -batchmode -nographics -projectPath "$PWD" -runTests \
  -testPlatform EditMode -testFilter Tugas7.Tests.T7_CourseSystemsTests \
  -testResults TestResults/finish-signal-red.xml -logFile Logs/finish-signal-red.log
```

Expected: compilation fails because `CourseCompleted` and configurable highlight intensity do not exist.

- [ ] **Step 3: Implement minimal completion and highlight behavior**

Add to `T7_CourseManager`:

```csharp
public event Action CourseCompleted;
```

Invoke it only after setting `IsComplete = true` in the successful path of `TryFinishCourse`.

Add to `T7_CourseInteractable`:

```csharp
[SerializeField, Min(1f)] private float highlightIntensity = 4f;

public void ConfigureHighlight(Color color, float intensity)
{
    glowColor = color;
    highlightIntensity = Mathf.Max(1f, intensity);
}
```

Use `glowColor * highlightIntensity` in `SetHighlighted`. Preserve the original shared-material emission and the existing property-block approach.

- [ ] **Step 4: Run focused tests and commit**

Expected: all `T7_CourseSystemsTests` pass.

```bash
git add Assets/Tugas7/Scripts/T7_CourseManager.cs \
  Assets/Tugas7/Scripts/T7_CourseInteractable.cs \
  Assets/Tugas7/Tests/EditMode/T7_CourseSystemsTests.cs
git commit -m "feat(finish): signal completion and boost glow"
```

### Task 2: NPC Continuous Facing and Sticky Victory State

**Files:**
- Modify: `Assets/Tugas7/Scripts/T7_TutorialNPC.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_TutorialNPCTests.cs`
- Modify: `Assets/Tugas7/Tests/PlayMode/T7_TutorialNPCPlayModeTests.cs`

- [ ] **Step 1: Write failing NPC behavior tests**

Create an NPC and player more than eight meters away with a vertical offset. Configure the NPC, call a public deterministic facing method with enough delta time, and assert its forward direction matches the flattened player direction:

```csharp
player.position = new Vector3(10f, 20f, 20f);
npc.Configure(animator, null, player);
npc.UpdateFacing(1f);
Vector3 expected = Vector3.ProjectOnPlane(player.position - npc.transform.position, Vector3.up).normalized;
Assert.That(Vector3.Angle(npc.transform.forward, expected), Is.LessThan(0.1f));
Assert.That(Mathf.Abs(npc.transform.forward.y), Is.LessThan(0.001f));
```

Test `EnterVictory()` twice and assert `IsVictorious` remains true, `State` becomes `Victorious`, talking cannot start, and Animator bool `IsVictorious` remains true.

- [ ] **Step 2: Run focused tests and confirm RED**

Expected: compilation fails because `UpdateFacing`, `EnterVictory`, `IsVictorious`, and `Victorious` do not exist.

- [ ] **Step 3: Implement continuous tracking and victory**

Extend the enum:

```csharp
public enum NPCState { Unavailable, Waving, Talking, Victorious }
```

Add:

```csharp
public bool IsVictorious => State == NPCState.Victorious;

public void EnterVictory()
{
    if (IsVictorious) return;
    CancelConversation();
    State = NPCState.Victorious;
    animator?.SetBool("IsVictorious", true);
    dialogue?.HidePrompt();
    dialogue?.HideDialogue();
}

public void UpdateFacing(float deltaTime)
{
    if (player == null) return;
    Vector3 direction = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up);
    if (direction.sqrMagnitude < 0.001f) return;
    transform.rotation = Quaternion.RotateTowards(
        transform.rotation, Quaternion.LookRotation(direction), turnSpeed * deltaTime);
}
```

Call `UpdateFacing(Time.deltaTime)` from `Update` for every state. Ensure `CanInteract` is false when victorious, `CancelConversation` does not clear victory, and `SetPlayerNearby` does not show prompts after victory.

- [ ] **Step 4: Run focused Edit/Play Mode tests and commit**

Expected: NPC suites pass.

```bash
git add Assets/Tugas7/Scripts/T7_TutorialNPC.cs Assets/Tugas7/Tests
git commit -m "feat(npc): track player and support victory"
```

### Task 3: Victory Clip and Textured NPC Prefab

**Files:**
- Modify: `Assets/Tugas7/Editor/T7_UpgradeAssetBuilder.cs`
- Modify: `Assets/Animations/Ch44_nonPBR@Victory Idle.fbx.meta` through importer configuration
- Modify: `Assets/Tugas7/Animations/T7_TutorialNPC.controller`
- Modify: `Assets/Tugas7/Prefabs/T7_TutorialNPC.prefab`
- Create: `Assets/Tugas7/Materials/NPC/T7_NPC_*.mat`
- Create conditionally: `Assets/Tugas7/Textures/NPC/T7_NPC_*.png`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_TutorialNPCTests.cs`

- [ ] **Step 1: Write failing asset contract tests**

After `T7_UpgradeAssetBuilder.PrepareAll`, load the Animator Controller and assert:

```csharp
Assert.That(controller.parameters.Any(p =>
    p.name == "IsVictorious" && p.type == AnimatorControllerParameterType.Bool), Is.True);
AnimatorState victory = controller.layers[0].stateMachine.states
    .Select(child => child.state).Single(state => state.name == "Victory");
Assert.That(victory.motion, Is.TypeOf<AnimationClip>());
Assert.That(((AnimationClip)victory.motion).isLooping, Is.True);
```

Load the NPC prefab. For each `SkinnedMeshRenderer`, assert every material uses `Universal Render Pipeline/Lit`, has a non-null `_BaseMap`, and belongs under `Assets/Tugas7/Materials/NPC/`.

- [ ] **Step 2: Run focused tests and confirm RED**

Expected: tests fail because Victory state/parameter and NPC materials do not exist.

- [ ] **Step 3: Configure Victory import and Animator**

Add `VictoryPath = "Assets/Animations/Ch44_nonPBR@Victory Idle.fbx"`. Configure it as Humanoid, copy the Waving Avatar, name the clip `Victory`, bake root rotation/position, set loop time, and disable root motion.

Add `IsVictorious` bool and `Victory` state. Add no-exit transitions from Waving and Talking to Victory when true. Add Head Hit exit routing to Victory when true while retaining existing Talking/Waving exits otherwise. Update `ControllerIsCurrent` to require four states and all three parameters so repeated preparation is stable.

- [ ] **Step 4: Build reusable textured URP materials**

Inspect imported renderer materials and textures first. If a usable embedded base map exists, copy/reference it. Otherwise create deterministic 256×256 fallback base maps with layered skin pores and woven clothing noise using a fixed seed, save PNG files under `Assets/Tugas7/Textures/NPC/`, import them as sRGB textures with desktop compression, and reuse them across prefab renderers.

Create URP Lit materials with:

```csharp
material.SetTexture("_BaseMap", baseMap);
material.SetFloat("_Metallic", 0f);
material.SetFloat("_Smoothness", isSkin ? 0.32f : 0.16f);
```

Preserve renderer material-slot count. Map skin-like original slots to skin material and remaining slots to clothing material. Save the prefab only when generated content changes.

- [ ] **Step 5: Run preparation twice, test idempotence, and commit**

Expected: both runs produce identical controller/prefab serialization; focused tests pass.

```bash
git add Assets/Animations/Ch44_nonPBR@Victory\\ Idle.fbx.meta \
  Assets/Tugas7/Animations Assets/Tugas7/Editor \
  Assets/Tugas7/Materials/NPC Assets/Tugas7/Textures/NPC \
  Assets/Tugas7/Prefabs Assets/Tugas7/Tests
git commit -m "feat(npc): add victory and textured materials"
```

### Task 4: Finish Presentation and Two Finish NPCs

**Files:**
- Create: `Assets/Tugas7/Scripts/T7_FinishPresentation.cs`
- Modify: `Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_CourseSystemsTests.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_TutorialNPCTests.cs`
- Modify: `Assets/Tugas7/Tests/PlayMode/T7_GameplayPlayModeTests.cs`

- [ ] **Step 1: Write failing presentation tests**

Create a manager, two NPCs, an `AudioSource`, and presentation component. Configure them, complete the course, and assert:

```csharp
Assert.That(leftNpc.IsVictorious, Is.True);
Assert.That(rightNpc.IsVictorious, Is.True);
Assert.That(presentation.PlayCount, Is.EqualTo(1));
manager.TryFinishCourse();
Assert.That(presentation.PlayCount, Is.EqualTo(1));
```

Add a scene contract test asserting exactly two NPCs under `FinishArea/FinishCelebrationNPCs`, neither has an enabled proximity-prompt object, and the presentation AudioSource references `Assets/Audio/wow_2.wav`.

- [ ] **Step 2: Run focused tests and confirm RED**

Expected: compilation fails because `T7_FinishPresentation` does not exist.

- [ ] **Step 3: Implement presentation component**

Create:

```csharp
public sealed class T7_FinishPresentation : MonoBehaviour
{
    public int PlayCount { get; private set; }
    public void Configure(T7_CourseManager manager, AudioSource source,
        IReadOnlyList<T7_TutorialNPC> finishNpcs);
}
```

Subscribe/unsubscribe to `CourseCompleted` in `OnEnable`/`OnDisable`. On first event, increment `PlayCount`, call `source.PlayOneShot(source.clip)` when clip exists, and call `EnterVictory()` on each configured NPC. Guard with a private `presented` bool.

- [ ] **Step 4: Build finish NPCs and wire sound**

Change `BuildTutorialNPC` to return its NPC and accept `bool enableInteraction = true`. For finish NPCs, disable the `InteractionRange` child and hide dialogue UI. Place exactly two under a `FinishCelebrationNPCs` group at safe opposite-side positions. Create one non-spatial finish AudioSource using `wow_2.wav`, `playOnAwake = false`, and configure presentation with manager and both NPCs.

Set beacon highlight to a strong gold value via `ConfigureHighlight(new Color(1f, 0.45f, 0.03f), 6f)`.

- [ ] **Step 5: Rebuild, run focused tests, and commit**

```bash
/home/pimio/Unity/Hub/Editor/6000.4.6f1/Editor/Unity \
  -batchmode -nographics -projectPath "$PWD" \
  -executeMethod Tugas7.Editor.T7_CourseSceneBuilder.RebuildBatch \
  -logFile Logs/finish-presentation-rebuild.log -quit
```

Expected: scene contains seven total NPCs: five guides plus two finish NPCs. Tests pass.

```bash
git add Assets/Tugas7/Scripts/T7_FinishPresentation.cs Assets/Tugas7/Scripts/T7_FinishPresentation.cs.meta \
  Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs Assets/Tugas7/Tests \
  Assets/Scenes/T6_T7_MainScene.unity
git commit -m "feat(finish): add sound and victory NPCs"
```

### Task 5: Procedural Spatial Lava Ambience

**Files:**
- Create: `Assets/Tugas7/Scripts/T7_ProceduralLavaAudio.cs`
- Modify: `Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs`
- Create: `Assets/Tugas7/Tests/EditMode/T7_ProceduralLavaAudioTests.cs`
- Modify: `Assets/Tugas7/Tests/PlayMode/T7_GameplayPlayModeTests.cs`

- [ ] **Step 1: Write failing procedural-audio tests**

Assert deterministic generated sample data:

```csharp
AudioClip first = T7_ProceduralLavaAudio.CreateClip(22050, 4f, 73421);
AudioClip second = T7_ProceduralLavaAudio.CreateClip(22050, 4f, 73421);
float[] a = new float[first.samples];
float[] b = new float[second.samples];
first.GetData(a, 0);
second.GetData(b, 0);
CollectionAssert.AreEqual(a, b);
Assert.That(a.Max(), Is.GreaterThan(0.05f));
Assert.That(a.All(sample => Mathf.Abs(sample) <= 1f), Is.True);
```

Test configured sources use the same clip, `loop = true`, `spatialBlend = 1f`, `playOnAwake = false`, and bounded distance values.

- [ ] **Step 2: Run focused tests and confirm RED**

Expected: compilation fails because `T7_ProceduralLavaAudio` does not exist.

- [ ] **Step 3: Implement deterministic ambience**

Implement:

```csharp
public static AudioClip CreateClip(int sampleRate = 22050, float duration = 4f, int seed = 73421);
public void Configure(IReadOnlyList<AudioSource> targets);
```

Generate mono samples from filtered seeded noise, two low sine components, sparse exponentially decaying crackle impulses, and short equal-power seam blending at the loop boundary. Normalize peak below `0.8f`. Cache one static clip per sample-rate/duration configuration. Assign the same clip to all sources, set volume around `0.12f`, loop/spatial settings, and start them during play mode.

- [ ] **Step 4: Wire deterministic lava sources**

Create an `AmbientLavaAudio` group with a small fixed set of sources near the major lava regions. Use `minDistance = 4f`, `maxDistance = 22f`, logarithmic rolloff, no doppler, and no real-time shadows or unrelated components.

- [ ] **Step 5: Rebuild, run tests, and commit**

Expected: procedural tests and gameplay tests pass; generated scene contains one ambience controller and spatial looping sources.

```bash
git add Assets/Tugas7/Scripts/T7_ProceduralLavaAudio.cs \
  Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs Assets/Tugas7/Tests \
  Assets/Scenes/T6_T7_MainScene.unity
git commit -m "feat(audio): add procedural lava ambience"
```

### Task 6: Full Rebuild and Regression Verification

**Files:**
- Modify generated assets only if deterministic rebuild updates them.

- [ ] **Step 1: Run batch compilation**

```bash
/home/pimio/Unity/Hub/Editor/6000.4.6f1/Editor/Unity \
  -batchmode -nographics -projectPath "$PWD" -quit \
  -logFile Logs/final-compile.log
```

Expected: exit code `0`, no compiler errors.

- [ ] **Step 2: Rebuild scene twice**

Run `T7_CourseSceneBuilder.RebuildBatch` twice. Record `git diff` after each run and confirm the second run adds no object duplication or serialization churn.

- [ ] **Step 3: Run complete Edit Mode suite**

```bash
/home/pimio/Unity/Hub/Editor/6000.4.6f1/Editor/Unity \
  -batchmode -nographics -projectPath "$PWD" -runTests \
  -testPlatform EditMode -testResults TestResults/edit-final.xml \
  -logFile Logs/edit-final.log
```

Expected: zero failures.

- [ ] **Step 4: Run complete Play Mode suite**

```bash
/home/pimio/Unity/Hub/Editor/6000.4.6f1/Editor/Unity \
  -batchmode -nographics -projectPath "$PWD" -runTests \
  -testPlatform PlayMode -testResults TestResults/play-final.xml \
  -logFile Logs/play-final.log
```

Expected: zero failures.

- [ ] **Step 5: Inspect acceptance contracts**

Confirm beacon hover emission and restoration, one finish sound trigger, two looping victory NPCs, seven total NPCs, distant horizontal facing, URP textured NPC renderers, looping spatial ambience, stable repeated finish message, no Screen Space canvas, and unchanged checkpoint/lava/head-hit behavior.

- [ ] **Step 6: Commit final generated state**

```bash
git add Assets/Scenes/T6_T7_MainScene.unity Assets/Tugas7
git commit -m "chore(scene): rebuild finish presentation"
```
