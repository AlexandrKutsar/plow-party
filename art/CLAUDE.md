# art — model sources

Every 3D model in the game is built by a Python script run in Blender. The scripts are the source of truth; the FBX and the palette PNG under `client/Assets/_Project/Art/` are build outputs, regenerated, never edited by hand. Where outputs go and how they are named and imported is set by `client/Assets/_Project/Art/CLAUDE.md`. Decisions: ADR-0008, ADR-0009.

## Layout

| Path | What |
|---|---|
| `blender/build.py` | Entry point: resets the scene, builds the palette and the requested models, writes the outputs |
| `blender/plow_art/palette.py` | The palette (up to 64 colors on an 8×8 grid of 4-pixel cells), its 32×32 PNG, and the single `Palette` material |
| `blender/plow_art/shapes.py` | Low-poly building blocks (box, cylinder, cone, ball, extruded profile, empty) and `paint` |
| `blender/plow_art/critter.py` | Shared Critter kit: seated body, head, eyes, muzzle, arms on the steering wheel, ear and tail styles; `output(species)` |
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
- The root is an empty named after the model, its origin on the ground at the center of the footprint; `shapes.center_footprint`, called by `build.py` after every model, enforces the centering. Gameplay collision shapes are sized from this footprint, so report a changed footprint in the model's commit.
- A model that sits on another model's anchor sets `ANCHORED = True` and is not centered: its origin is the anchor point. Critters have their origin on the seat cushion at `CritterSeat`, hands on the Vehicle's steering wheel; a Vehicle change that moves the seat or the wheel needs `plow_art/critter.py` updated.
- Parts that move or get swapped (wheels, Bucket, seat anchor) are separate objects with their origin at their pivot. `L`/`R` in names are from the driver's seat.
- Colors come only from `palette.COLORS`: `paint` points a face's UVs at the color's cell, so every model shares one material and one texture. A new color goes into the palette, never into a new material.
- Shapes are chamfered boxes and low-segment cylinders, flat-shaded; a Vehicle or a Critter stays under ~1500 triangles.
- Names follow `GLOSSARY.md`: `Vehicle`, `Bucket`, `CritterSeat` (the anchor a Critter is parented to), `Critter_<Species>`.

## Models

| Model | Script | Output |
|---|---|---|
| Vehicle | `blender/models/vehicle.py` | `client/Assets/_Project/Art/Vehicles/Vehicle/SM_Vehicle.fbx` — tractor-style Vehicle with front Bucket, open seat, `CritterSeat` anchor; ~1100 triangles |
| Critter Fox | `blender/models/critter_fox.py` | `client/Assets/_Project/Art/Critters/Critter_Fox/SM_Critter_Fox.fbx` — orange, pointed black-lined ears, bushy white-tipped tail; ~630 triangles |
| Critter Bear | `blender/models/critter_bear.py` | `client/Assets/_Project/Art/Critters/Critter_Bear/SM_Critter_Bear.fbx` — brown, round ears, tan muzzle and belly; ~700 triangles |
| Critter Rabbit | `blender/models/critter_rabbit.py` | `client/Assets/_Project/Art/Critters/Critter_Rabbit/SM_Critter_Rabbit.fbx` — white, long pink-lined ears swept back; ~650 triangles |
| Critter Raccoon | `blender/models/critter_raccoon.py` | `client/Assets/_Project/Art/Critters/Critter_Raccoon/SM_Critter_Raccoon.fbx` — gray, black mask, striped tail; ~800 triangles |
| Critter Penguin | `blender/models/critter_penguin.py` | `client/Assets/_Project/Art/Critters/Critter_Penguin/SM_Critter_Penguin.fbx` — black and white, yellow beak and feet; ~530 triangles |
| Critter Beaver | `blender/models/critter_beaver.py` | `client/Assets/_Project/Art/Critters/Critter_Beaver/SM_Critter_Beaver.fbx` — wood-brown, buck teeth, flat dark tail; ~730 triangles |

## Adding a model

1. Create `blender/models/<model>.py` with `NAME`, `OUTPUT` at `client/Assets/_Project/Art/<Category>/<Asset>/SM_<Asset>.fbx` (`SK_` for rigged models), and `build`.
2. Register it in `MODELS` in `build.py`.
3. Build, check it with `look` in Blender, refresh Unity; the import postprocessor in `_Project/Editor/ArtImport/` applies settings and the palette material.
4. Follow "Adding an asset" in `client/Assets/_Project/Art/CLAUDE.md` (visual prefab, nesting) and add a row to both asset tables.
