# art — model sources

Every 3D model in the game is built by a Python script run in Blender. The scripts are the source of truth; the FBX and the palette PNG under `client/Assets/_Project/Art/` are build outputs, regenerated, never edited by hand. Where outputs go and how they are named and imported is set by `client/Assets/_Project/Art/CLAUDE.md`. Decisions: ADR-0008, ADR-0009.

## Layout

| Path | What |
|---|---|
| `blender/build.py` | Entry point: resets the scene, builds the palette and the requested models, writes the outputs |
| `blender/plow_art/palette.py` | The 16-color palette, its 16×16 PNG, and the single `Palette` material |
| `blender/plow_art/shapes.py` | Low-poly building blocks (box, cylinder, extruded profile, empty) and `paint` |
| `blender/plow_art/export.py` | FBX export with the axis and scale settings Unity expects |
| `blender/models/<model>.py` | One file per model: `NAME`, `OUTPUT` (repo-relative path `client/Assets/_Project/Art/<Category>/<Asset>/SM_<Asset>.fbx`), `build(collection)` |

## Building

Headless, from the repo root:

```bash
"C:/Program Files/Blender Foundation/Blender 5.2/blender.exe" --background --factory-startup --python art/blender/build.py -- vehicle
```

Without model names it builds every entry of `MODELS` in `build.py`. In a live Blender (MCP for Blender), add `art/blender` to `sys.path`, import `build`, call `build.reload_modules()` and `build.build("vehicle", write_files=False)` to iterate, then with `write_files=True` to export. Unity picks the files up on the next asset refresh.

## Conventions

- Meters, Z up, the model's front faces Blender −Y; the export maps this to Unity +Z forward with identity rotation and scale 1.
- The root is an empty named after the model, its origin on the ground at the center of the footprint.
- Parts that move or get swapped (wheels, Bucket, seat anchor) are separate objects with their origin at their pivot. `L`/`R` in names are from the driver's seat.
- Colors come only from `palette.COLORS`: `paint` points a face's UVs at the color's cell, so every model shares one material and one texture. A new color goes into the palette, never into a new material.
- Shapes are chamfered boxes and low-segment cylinders, flat-shaded; a vehicle stays under ~1500 triangles.
- Names follow `GLOSSARY.md`: `Vehicle`, `Bucket`, `CritterSeat` (the anchor a Critter is parented to).

## Models

| Model | Script | Output |
|---|---|---|
| Vehicle | `blender/models/vehicle.py` | `client/Assets/_Project/Art/Vehicles/Vehicle/SM_Vehicle.fbx` — tractor-style Vehicle with front Bucket, open seat, `CritterSeat` anchor; ~1100 triangles |

## Adding a model

1. Create `blender/models/<model>.py` with `NAME`, `OUTPUT` at `client/Assets/_Project/Art/<Category>/<Asset>/SM_<Asset>.fbx` (`SK_` for rigged models), and `build`.
2. Register it in `MODELS` in `build.py`.
3. Build, check it with `look` in Blender, refresh Unity; the import postprocessor in `_Project/Editor/ArtImport/` applies settings and the palette material.
4. Follow "Adding an asset" in `client/Assets/_Project/Art/CLAUDE.md` (visual prefab, nesting) and add a row to both asset tables.
