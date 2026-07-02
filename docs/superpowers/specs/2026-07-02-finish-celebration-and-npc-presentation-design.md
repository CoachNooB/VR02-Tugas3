# Finish Celebration and NPC Presentation Design

## Goal

Improve finish-zone feedback and character presentation without changing course progression, checkpoint requirements, guide dialogue, head-hit interaction, or repeat-finish behavior.

## Existing Assets

- Use `Assets/Animations/Ch44_nonPBR@Victory Idle.fbx` for finish celebration.
- Use `Assets/Audio/wow_2.wav` for successful finish interaction.
- Reuse the existing tutorial NPC model, Avatar, Waving, Talking, and Head Hit clips.
- Generate lava ambience procedurally at runtime. Do not import another audio asset.

## Beacon Feedback

The finish beacon remains a `T7_CourseInteractable`. Looking directly at it within the existing interaction range applies a stronger gold HDR emission through its renderer property block. Looking away restores the material's original emission.

The glow is raycast-hover feedback and works whether the beacon is locked or unlocked. Existing prompt text continues to communicate the locked state.

Successful completion raises a finish-completed event exactly once. The event plays `wow_2.wav` and starts the finish NPC celebration. Locked interactions do nothing beyond returning the existing locked message. Interacting again after completion returns the recorded completion result but does not replay sound or restart animation.

## Finish NPCs

The deterministic scene builder creates exactly two finish NPCs, placed on opposite sides of the finish platform where they do not block the beacon raycast, player route, or required colliders.

They use the same model and textured material setup as guide NPCs. Their initial animation is looping Waving. They have no proximity prompt or tutorial dialogue. On the first successful finish event, both enter a looping Victory state and remain there for the rest of the run.

The shared Animator Controller gains:

- Boolean parameter `IsVictorious`.
- Looping state `Victory`, using the `Victory Idle` clip.
- Any non-head-hit guide state transitions to Victory when `IsVictorious` is true.
- Victory has no automatic exit.
- Existing Waving, Talking, and Head Hit behavior remains intact before victory.

## NPC Facing

Every guide and finish NPC rotates its root horizontally toward the player every frame, regardless of distance. Vertical pitch is ignored so feet remain grounded and the model does not tilt.

Rotation uses the existing configurable turn speed rather than snapping. Talking is no longer required for tracking. Finish NPCs continue tracking while waving and while victorious.

## NPC Materials

The asset builder creates reusable URP Lit NPC materials and assigns them to all skinned mesh renderers in the prefab.

First preference is texture data embedded in or imported with the Waving FBX. Usable base-color and normal maps are retained and assigned. If the FBX exposes no usable maps, the builder creates small deterministic skin and clothing detail textures locally, saves them under `Assets/Tugas7/Textures/NPC/`, and assigns them to reusable materials under `Assets/Tugas7/Materials/NPC/`.

Materials use physically plausible non-metallic settings, moderate smoothness for skin, lower smoothness for clothing, and normal mapping where available. Model geometry and animation rig remain unchanged.

## Procedural Lava Ambience

A focused runtime component creates one reusable `AudioClip` containing a seamless low-frequency rumble with sparse crackle impulses. Generation is deterministic and occurs once per scene load.

The scene builder places low-volume looping 3D `AudioSource` objects near major lava regions. Sources:

- use spatial blend `1`;
- loop continuously;
- disable play-on-awake until the generated clip is assigned;
- use bounded minimum and maximum distances;
- do not add real-time processing, mixers, or extra imported assets.

The volume is low enough that finish feedback and interaction sounds remain clear.

## Components and Data Flow

- `T7_RaycastInteractor` retains target acquisition and calls existing highlight methods.
- `T7_CourseInteractable` retains emission property-block highlighting and exposes successful finish completion without firing on repeats.
- `T7_CourseManager` owns the one-time course-completion state and event.
- A finish presentation component subscribes to course completion, plays the finish clip, and tells configured finish NPCs to become victorious.
- `T7_TutorialNPC` owns player tracking and the animator's `IsVictorious` value.
- A procedural ambience component owns clip generation and source configuration.
- `T7_CourseSceneBuilder` wires all references deterministically.
- `T7_UpgradeAssetBuilder` configures Victory import, Animator Controller, NPC textures, materials, and prefab renderers.

## Error Handling

- Missing finish audio logs one warning and still triggers NPC victory.
- Missing Victory clip leaves NPCs waving and logs one builder warning; it does not break scene generation.
- Missing renderer makes highlighting a no-op.
- Missing player reference disables facing without throwing.
- Missing texture data uses deterministic generated fallback textures.
- Rebuilding assets or scene updates existing generated assets and does not duplicate sources, NPCs, states, parameters, or scene objects.

## Automated Verification

Edit Mode tests verify:

- finish completion event fires once and not for locked or repeated interactions;
- beacon highlight changes emission and restores it;
- NPC facing works outside dialogue and ignores vertical offset;
- victory state is sticky at the component level;
- Animator has `IsVictorious` and looping Victory state;
- NPC prefab renderers use textured URP Lit materials;
- generated fallback textures exist when embedded maps are unavailable;
- rebuilt scene contains exactly two non-dialogue finish NPCs;
- procedural ambience configuration is looping, spatial, and attached near lava;
- finish presentation references `wow_2.wav`.

Play Mode tests verify:

- aiming at the beacon enables glow and looking away restores it;
- first successful beacon interaction plays finish feedback and starts both victory loops;
- repeated interaction does not replay finish feedback;
- guide and finish NPCs face the player from beyond eight meters;
- existing finish, tutorial, head-hit, checkpoint, lava damage, and UI behavior remains functional.

## Acceptance Criteria

- Beacon visibly glows while targeted by the player raycast.
- First successful finish interaction plays `wow_2.wav`.
- Lava areas emit continuous low-volume procedural ambience.
- Exactly two finish-zone NPCs wave before completion and loop Victory afterward.
- All NPCs continuously face the player horizontally from any distance.
- NPC surfaces visibly use textured URP Lit materials.
- Repeated finish interaction remains safe and does not replay celebration feedback.
- Scene rebuild is deterministic and all automated tests pass.
