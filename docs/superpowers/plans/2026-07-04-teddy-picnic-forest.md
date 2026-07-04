# Teddy Picnic Forest Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build `Assets/UAS/Scenes/UAS_Harry_Forrest.unity` as the approved static teddy-picnic forest diorama.

**Architecture:** A temporary Unity editor builder creates the scene through supported Unity APIs, preserving Polytope prefab links and generating scene-local primitive objects and materials. The builder validates required assets, saves the scene, writes a completion marker, and is removed afterward so the deliverable contains no runtime or generation dependency.

**Tech Stack:** Unity 6000.4.6f1, C# editor scripting, Universal Render Pipeline, Polytope Studio prefabs.

---

### Task 1: Create the scene builder

**Files:**
- Create temporarily: `Assets/Editor/TeddyPicnicSceneBuilder.cs`
- Create: `Assets/UAS/Scenes/UAS_Harry_Forrest.unity`
- Create: `Assets/UAS/Materials/TeddyPicnic/*.mat`

- [ ] **Step 1: Add an editor-only builder**

Implement `TeddyPicnicSceneBuilder` with an initialization hook that runs once, only when the target scene and completion marker do not match. It must:

- create a new empty scene;
- configure warm sunny render settings, fog, and skybox;
- create named `Environment`, `Picnic_Set`, `Teddy_Family`, `Lighting`, and `Cinematic_Camera` roots;
- create a broad ground disc and clearing;
- instantiate fruit trees, pine trees, shrubs, grass, flowers, mushrooms, rocks, logs, fences, and a gate from their exact Polytope prefab paths;
- build three seated bears from primitive meshes, with distinct sizes and colors;
- build a gingham blanket, basket, cups, plates, fruit, bread, and centerpiece from primitive meshes;
- create persistent URP/Lit material assets under `Assets/UAS/Materials/TeddyPicnic`;
- save the scene and write `Temp/teddy-picnic-scene-built.txt`.

- [ ] **Step 2: Trigger compilation in the open Unity editor**

Run:

```bash
touch Assets/Editor/TeddyPicnicSceneBuilder.cs
```

Expected: Unity imports and compiles the editor script, then the initialization hook creates and saves the target scene.

- [ ] **Step 3: Verify the completion marker**

Run:

```bash
test -f Temp/teddy-picnic-scene-built.txt
```

Expected: exit code 0.

### Task 2: Remove temporary tooling and validate the scene

**Files:**
- Delete: `Assets/Editor/TeddyPicnicSceneBuilder.cs`
- Delete if generated: `Assets/Editor/TeddyPicnicSceneBuilder.cs.meta`
- Verify: `Assets/UAS/Scenes/UAS_Harry_Forrest.unity`

- [ ] **Step 1: Remove the temporary editor builder**

Delete the builder and its metadata after Unity saves the scene. The scene must remain self-contained and retain Polytope prefab references.

- [ ] **Step 2: Check serialized scene structure**

Run:

```bash
rg -n "m_Name: (Environment|Picnic_Set|Teddy_Family|Teddy_Brown|Teddy_Honey|Teddy_Cream|Lighting|Cinematic_Camera)" Assets/UAS/Scenes/UAS_Harry_Forrest.unity
```

Expected: every required root and teddy name is present.

- [ ] **Step 3: Check Polytope prefab references**

Run:

```bash
rg -c "m_SourcePrefab:" Assets/UAS/Scenes/UAS_Harry_Forrest.unity
```

Expected: at least 30 prefab instances.

- [ ] **Step 4: Check serialization and editor logs**

Confirm the scene has a Unity YAML header, contains no missing-script entries, and the current editor log contains the builder success message without compiler or scene-save errors.

- [ ] **Step 5: Review the final diff**

Run:

```bash
git diff --check
git status --short
```

Expected: no whitespace errors; only the requested scene, its existing metadata, documentation, and pre-existing untracked Polytope assets are present.
