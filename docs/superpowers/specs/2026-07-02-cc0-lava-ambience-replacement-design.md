# CC0 Lava Ambience Replacement Design

## Goal

Replace the synthetic lava ambience with a more convincing imported loop while preserving existing spatial emitters, finish audio, gameplay, and deterministic scene rebuilding.

## Sources

Use only these CC0 Freesound recordings:

- `Kilauea Lava Sounds.wav` by `e__`
  - Source: https://freesound.org/people/e__/sounds/172630/
  - License: CC0 1.0
  - Purpose: natural lava crackle, pops, and volcanic field character.
- `Bubbling, Large, A.wav` by `InspectorJ`
  - Source: https://freesound.org/people/InspectorJ/sounds/398720/
  - License: CC0 1.0
  - Purpose: close bubbling texture layered below the field recording.

Use Freesound's downloadable preview files when original-download authentication is unavailable. The retained project asset is the processed derivative, not both complete source recordings.

## Processing

Create one 20–30 second stereo or mono OGG loop under:

`Assets/Tugas7/Audio/T7_LavaAmbience.ogg`

Processing:

- Select a Kilauea segment without audible speech.
- Apply a high-pass filter to reduce wind and handling rumble.
- Apply restrained low-pass filtering only where needed to soften harsh preview compression.
- Mix the bubbling recording quietly beneath the field recording.
- Use a long equal-power overlap between loop endpoints.
- Normalize conservatively, leaving headroom for multiple overlapping 3D emitters.
- Avoid aggressive compression or exaggerated bass.

The result should sound like distant active lava, not boiling water or constant synthetic hum.

## Runtime Integration

Keep the existing `AmbientLavaAudio` group and its four spatial emitters. Replace procedural generation with one shared imported `AudioClip`.

`T7_ProceduralLavaAudio` becomes an ambience-source controller:

- accepts a configured imported clip;
- assigns the same clip to every emitter;
- retains looping, spatial blend, volume, distance, rolloff, and safe playback behavior;
- does not synthesize or cache runtime clips for scene ambience.

The deterministic builder loads `T7_LavaAmbience.ogg` and configures the controller. A missing clip logs one warning and leaves sources silent without throwing.

The existing procedural clip API may remain only if current automated tests or other callers require it. It must no longer drive facility ambience.

## Attribution

Add both source recordings, authors, URLs, CC0 license, access/import date, retained derivative filename, and processing summary to:

`Assets/Tugas7/ThirdParty/ATTRIBUTION.md`

## Verification

Automated tests verify:

- the processed OGG exists and is imported as an `AudioClip`;
- attribution lists both source recordings, authors, URLs, CC0, and derivative filename;
- the scene controller references the imported clip;
- all lava emitters share that clip;
- sources retain looped 3D settings and safe playback behavior;
- procedural synthesis is not used by the scene ambience;
- finish sound and existing gameplay tests remain unchanged.

Manual acceptance verifies:

- no obvious speech is audible;
- loop boundary has no click or sudden level change;
- bubbling supports rather than dominates the volcanic field recording;
- overlapping emitters do not clip;
- finish sound remains clearly audible.
