# CC0 Lava Ambience Replacement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace scene ambience synthesis with one processed CC0 lava loop made from authentic Kilauea and bubbling recordings.

**Architecture:** Process source previews offline into one normalized seamless OGG, import only the derivative, and configure the existing four spatial emitters with that shared clip. Retain procedural generation only for compatibility tests; facility ambience uses the imported asset.

**Tech Stack:** Unity 6000.4.6f1, C#, Unity AudioSource, FFmpeg, OGG Vorbis, NUnit Unity Test Framework.

---

### Task 1: Download, Process, and Document CC0 Audio

**Files:**
- Create: `Assets/Tugas7/Audio/T7_LavaAmbience.ogg`
- Create: `Assets/Tugas7/Audio/T7_LavaAmbience.ogg.meta`
- Modify: `Assets/Tugas7/ThirdParty/ATTRIBUTION.md`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_ProceduralLavaAudioTests.cs`

- [ ] **Step 1: Write failing asset and attribution tests**

Add tests asserting:

```csharp
AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
    "Assets/Tugas7/Audio/T7_LavaAmbience.ogg");
Assert.That(clip, Is.Not.Null);
Assert.That(clip.length, Is.InRange(20f, 30f));
```

Load `ATTRIBUTION.md` and require both source titles, `e__`, `InspectorJ`, both Freesound URLs, `CC0`, and `T7_LavaAmbience.ogg`.

- [ ] **Step 2: Run focused Edit Mode test and confirm RED**

```bash
/home/pimio/Unity/Hub/Editor/6000.4.6f1/Editor/Unity \
  -batchmode -nographics -projectPath "$PWD" -runTests \
  -testPlatform EditMode -testFilter Tugas7.Tests.T7_ProceduralLavaAudioTests \
  -testResults TestResults/lava-import-red.xml -logFile Logs/lava-import-red.log
```

Expected: asset and attribution assertions fail.

- [ ] **Step 3: Download source previews outside Assets**

Resolve preview URLs from the two Freesound pages and download to `/tmp/t7-lava-source/`. Verify the page metadata reports CC0 before processing. Do not retain source recordings in the repository.

- [ ] **Step 4: Build the loop with FFmpeg**

Select a speech-free 20–30 second Kilauea region. Filter wind, mix bubbling at a lower level, create a long endpoint overlap, and normalize with headroom. Use commands equivalent to:

```bash
ffmpeg -i kilauea.mp3 -i bubbling.mp3 -filter_complex \
  "[0:a]atrim=start=START:duration=30,highpass=f=90,lowpass=f=10000,volume=0.85[k]; \
   [1:a]aloop=loop=-1:size=MAX,atrim=duration=30,highpass=f=70,volume=0.18[b]; \
   [k][b]amix=inputs=2:normalize=0,alimiter=limit=0.70[m]" \
  -map "[m]" -ar 44100 -ac 1 /tmp/t7-lava-source/mix.wav
```

Create a 24-second result with a three-second equal-power overlap between tail and head, then encode:

```bash
ffmpeg -i seamless.wav -c:a libvorbis -q:a 5 \
  Assets/Tugas7/Audio/T7_LavaAmbience.ogg
```

Measure duration, peak, integrated loudness, and endpoint sample deltas with `ffprobe`/FFmpeg. Reject audible speech, clipping, and abrupt seam.

- [ ] **Step 5: Configure Unity import and attribution**

Import as compressed OGG suitable for looping desktop ambience. Enable load in background where available. Record title, author, URL, CC0, import date, derivative filename, and processing summary.

- [ ] **Step 6: Run focused tests and commit asset import**

Expected: asset/attribution tests pass.

```bash
git add Assets/Tugas7/Audio Assets/Tugas7/ThirdParty/ATTRIBUTION.md \
  Assets/Tugas7/Tests/EditMode/T7_ProceduralLavaAudioTests.cs
git commit -m "chore(audio): import CC0 lava ambience"
```

### Task 2: Use Imported Clip for Facility Ambience

**Files:**
- Modify: `Assets/Tugas7/Scripts/T7_ProceduralLavaAudio.cs`
- Modify: `Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs`
- Modify: `Assets/Tugas7/Tests/EditMode/T7_ProceduralLavaAudioTests.cs`
- Modify: `Assets/Tugas7/Tests/PlayMode/T7_GameplayPlayModeTests.cs`
- Modify: `Assets/Scenes/T6_T7_MainScene.unity`

- [ ] **Step 1: Write failing imported-clip integration tests**

Require:

```csharp
controller.Configure(importedClip, sources);
Assert.That(sources, Has.All.Matches<AudioSource>(source => source.clip == importedClip));
```

Load the built scene and assert the ambience controller's serialized clip is the imported OGG. Assert no scene emitter receives a clip whose name starts with `ProceduralLava_`.

- [ ] **Step 2: Run focused Edit and Play Mode tests and confirm RED**

Expected: compile fails because the imported-clip `Configure` overload/property does not exist, or scene still has no serialized ambience clip.

- [ ] **Step 3: Add imported clip configuration**

Add:

```csharp
[SerializeField] private AudioClip ambienceClip;
public AudioClip AmbienceClip => ambienceClip;

public void Configure(AudioClip clip, IReadOnlyList<AudioSource> newTargets)
{
    ambienceClip = clip;
    // copy targets and apply existing spatial configuration
}
```

`ApplyConfiguration` assigns `ambienceClip`, not `CreateClip()`. If missing, warn once and remain silent. Keep the old static synthesis API for compatibility tests only.

- [ ] **Step 4: Wire builder to imported OGG**

Load:

```csharp
AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
    "Assets/Tugas7/Audio/T7_LavaAmbience.ogg");
```

Pass it into the controller while retaining four existing emitters and all spatial settings.

- [ ] **Step 5: Rebuild twice and run focused tests**

Expected: exactly one ambience controller, four sources sharing `T7_LavaAmbience`, no procedural scene clips, and no duplicate groups.

- [ ] **Step 6: Commit integration**

```bash
git add Assets/Tugas7/Scripts/T7_ProceduralLavaAudio.cs \
  Assets/Tugas7/Editor/T7_CourseSceneBuilder.cs Assets/Tugas7/Tests \
  Assets/Scenes/T6_T7_MainScene.unity
git commit -m "feat(audio): use imported lava ambience"
```

### Task 3: Full Verification

**Files:**
- Modify generated scene only if rebuild produces required semantic changes.

- [ ] **Step 1: Run batch compile**

Expected: exit code `0`, no compiler errors.

- [ ] **Step 2: Run complete Edit Mode suite**

Expected: zero failures.

- [ ] **Step 3: Run complete Play Mode suite**

Expected: zero failures.

- [ ] **Step 4: Inspect audio and scene contracts**

Confirm imported OGG duration and peak, loop seam continuity, attribution completeness, four shared 3D sources, no procedural ambience in scene, one finish sound source, and no missing clips.

- [ ] **Step 5: Perform manual listening acceptance**

Listen near at least two lava regions. Confirm no audible speech, no loop click, bubbling remains subordinate, overlapping sources do not clip, and `wow_2.wav` remains clear.
