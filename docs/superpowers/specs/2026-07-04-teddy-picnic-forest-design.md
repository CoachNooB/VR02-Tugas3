# Teddy Picnic Forest Scene Design

## Goal

Create `Assets/UAS/Scenes/UAS_Harry_Forrest.unity` as a static cinematic diorama with a warm, sunny storybook teddy-picnic theme.

## Constraints

- Use Polytope Studio prefabs for the forest environment.
- Build teddy dolls and picnic accessories from Unity primitives because the asset pack has no teddy-bear model.
- Keep the scene static: no XR rig, gameplay, runtime scripts, or interactions.
- Preserve an editable hierarchy with clearly named parent objects and prefab instances.

## Composition

The focal point is a red-and-cream picnic blanket in a grassy clearing. Three seated teddy dolls form a readable triangle around picnic food and a basket:

- a large brown teddy;
- a medium honey-colored teddy;
- a small cream teddy.

Each teddy uses primitive meshes for its head, muzzle, ears, torso, paws, eyes, nose, and ribbon accent. Their poses face inward toward the picnic while remaining visible to the camera.

The clearing is framed by Polytope fruit trees and pine trees. Shrubs, rocks, poppies, mushrooms, grass, logs, and a partial wooden fence with a gate create foreground, middle-ground, and background depth without obscuring the teddy group.

## Scene Hierarchy

- `Environment`
  - ground and clearing
  - tree ring
  - shrubs and grass
  - flowers and mushrooms
  - rocks and logs
  - fence and gate
- `Picnic_Set`
  - blanket
  - basket
  - plates, cups, fruit, bread, and centerpiece
- `Teddy_Family`
  - large brown teddy
  - medium honey teddy
  - small cream teddy
- `Lighting`
  - warm directional sun
  - soft fill light
- `Cinematic_Camera`

## Lighting and Camera

Use a warm directional light angled across the clearing, soft ambient illumination, gentle shadows, and subtle distance fog. Use the Polytope skybox if it imports correctly with the project render pipeline.

Place the static camera at a low three-quarter angle aimed at the center of the blanket. Foreground foliage frames the lower edges, while trees and fence form a layered backdrop. The final view must keep all three teddy faces and the picnic arrangement readable.

## Implementation

Build the scene directly as serialized Unity scene content. Reference Polytope prefabs by their existing asset GUIDs. Use built-in primitive meshes and scene-local materials for the teddy dolls, blanket, and picnic accessories. Do not add generation scripts or modify imported Polytope assets.

## Validation

- Confirm the scene and all referenced prefabs import in Unity 6000.4.6f1.
- Confirm Unity batch mode reports no scene serialization or missing-reference errors.
- Confirm the hierarchy contains each designed group and all three teddy dolls.
- Inspect a rendered or editor screenshot when available to verify composition, lighting, and object visibility.
