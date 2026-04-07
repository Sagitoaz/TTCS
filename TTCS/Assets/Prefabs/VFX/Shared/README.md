# Shared VFX Prefabs

This folder stores reusable sprite VFX prefabs that can be dragged into any character prefab.

Suggested structure:

- `Sprite/` for sprite-based effects (`SpriteVFXAutoPlayer`)
- `Composite/` for layered effects containing multiple sprite VFX children

Workflow:

1. Create a VFX prefab here.
2. Add `SpriteVFXAutoPlayer`.
3. Tune duration, fill/scale/color/shader tracks.
4. Drag prefab as child under a character prefab.
5. In Animation Record, keyframe the child GameObject `Active` on/off.
