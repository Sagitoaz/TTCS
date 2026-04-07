# Shared VFX Shaders

Use this folder for reusable shader files for sprite VFX.

Recommended structure:

- `Core/` common utility shaders
- `Sprite/` sprite-specific VFX shaders (fill, dissolve, glow, distortion-lite)
- `Includes/` shared HLSL includes if needed

When a new effect needs a dedicated shader, generate it in this folder first, then create material variants under `Assets/Materials/VFX/Shared/`.
