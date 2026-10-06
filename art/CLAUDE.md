# art — model sources

Every 3D model in the game is built by a Python script run in Blender. The scripts are the source of truth; the FBX and the palette PNG under `client/Assets/_Project/Art/` are build outputs, regenerated, never edited by hand. Where outputs go and how they are named and imported is set by `client/Assets/_Project/Art/CLAUDE.md`. Decisions: ADR-0008, ADR-0009.

## Layout

| Path | What |
|---|---|
| `blender/build.py` | Entry point: resets the scene, builds the palette and the requested models, writes the outputs |
| `blender/plow_art/palette.py` | The palette (up to 64 colors on an 8×8 grid of 4-pixel cells), its 32×32 PNG, and the single `Palette` material |
| `blender/plow_art/shapes.py` | Low-poly building blocks (box, cylinder, cone, ball, extruded profile, empty) and `paint` |
| `blender/plow_art/critter.py` | Shared Critter kit: seated body, head, eyes, muzzle, arms on the steering wheel, ear and tail styles; `output(species)` |
| `blender/plow_art/imported.py` | Brings a third-party glTF from `sources/` into the pipeline: bakes transforms, repaints every face to a palette cell (explicit material-to-color map, or nearest of listed colors for textured sources), joins it into one mesh, scales it to a footprint size and puts it on the ground |
| `blender/plow_art/export.py` | FBX export with the axis and scale settings Unity expects |
| `sources/<author>/<file>.glb` | Downloaded third-party models, untouched; each one has a row in `CREDITS.md` |
| `CREDITS.md` | Author, link, and license of every third-party source |
| `blender/models/<model>.py` | One file per model: `NAME`, `OUTPUT` (repo-relative path `client/Assets/_Project/Art/<Category>/<Asset>/SM_<Asset>.fbx`), `build(collection)` |

## Building

Headless, from the repo root:

```bash
"C:/Program Files/Blender Foundation/Blender 5.2/blender.exe" --background --factory-startup --python art/blender/build.py -- vehicle
```

Without model names it builds every entry of `MODELS` in `build.py`. In a live Blender (MCP for Blender), add `art/blender` to `sys.path`, import `build`, call `build.reload_modules()` and `build.build("vehicle", write_files=False)` to iterate, then with `write_files=True` to export. Unity picks the files up on the next asset refresh.

## Conventions

- Meters, Z up, the model's front faces Blender −Y; the export maps this to Unity +Z forward with identity rotation and scale 1.
- The root is an empty named after the model, its origin on the ground at the center of the footprint; `shapes.center_footprint`, called by `build.py` after every model, enforces the centering. Gameplay collision shapes are sized from this footprint (listed in the Models table in meters, X × Y), so report a changed footprint in the model's commit.
- A model that sits on another model's anchor sets `ANCHORED = True` and is not centered: its origin is the anchor point. Critters have their origin on the seat cushion at `CritterSeat`, hands on the Vehicle's steering wheel; a Vehicle change that moves the seat or the wheel needs `plow_art/critter.py` updated.
- Parts that move or get swapped (wheels, Bucket, seat anchor) are separate objects with their origin at their pivot. `L`/`R` in names are from the driver's seat.
- A third-party model is never committed to `client/` as downloaded: its model script loads it through `imported.load`, so it leaves the build with the palette material and the project's scale like any generated model. A source that cannot be mapped to the palette (alpha textures, baked lighting) is rejected or recorded as an exception in ADR-0009.
- Colors come only from `palette.COLORS`: `paint` points a face's UVs at the color's cell, so every model shares one material and one texture. A new color goes into the palette, never into a new material.
- Shapes are chamfered boxes and low-segment cylinders, flat-shaded; a Vehicle or a Critter stays under ~1500 triangles, a building under ~5000, a field Obstacle under ~2000.
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
| Barn | `blender/models/barn.py` | `client/Assets/_Project/Art/Environment/Barn/SM_Barn.fbx` — Quaternius Big Barn; perimeter building; footprint 6.6 × 7.0 box; ~4220 triangles |
| Shed | `blender/models/shed.py` | `client/Assets/_Project/Art/Environment/Shed/SM_Shed.fbx` — Quaternius Small Barn; perimeter building; footprint 4.8 × 5.0 box; ~2176 triangles |
| ChickenCoop | `blender/models/chicken_coop.py` | `client/Assets/_Project/Art/Environment/ChickenCoop/SM_ChickenCoop.fbx` — Quaternius ChickenCoop; perimeter building; footprint 3.0 × 2.7 box; ~948 triangles |
| House | `blender/models/house.py` | `client/Assets/_Project/Art/Environment/House/SM_House.fbx` — CreativeTrio Cabin Shed, log house; perimeter building; footprint 5.0 × 3.5 box; ~2745 triangles |
| Fence | `blender/models/fence.py` | `client/Assets/_Project/Art/Environment/Fence/SM_Fence.fbx` — Quaternius Fence; one 2.2 m section, tiles along X; footprint 2.2 × 0.07 box; ~208 triangles |
| Well | `blender/models/well.py` | `client/Assets/_Project/Art/Environment/Well/SM_Well.fbx` — Quaternius Well; field Obstacle; footprint 1.1 × 1.6 box (stone ring ⌀ 1.1 circle); ~1870 triangles |
| WoodPile | `blender/models/wood_pile.py` | `client/Assets/_Project/Art/Environment/WoodPile/SM_WoodPile.fbx` — generated; stacked logs, ends face front and back; footprint 2.0 × 1.0 box; ~1068 triangles |
| HayBale | `blender/models/hay_bale.py` | `client/Assets/_Project/Art/Environment/HayBale/SM_HayBale.fbx` — generated; round bale lying along X, snow cap; footprint 1.2 × 1.2 box; ~208 triangles |
| Cart | `blender/models/cart.py` | `client/Assets/_Project/Art/Environment/Cart/SM_Cart.fbx` — generated; two-wheel farm cart with hay, shafts to the front; footprint 1.4 × 2.9 box; ~508 triangles |
| Pine | `blender/models/pine.py` | `client/Assets/_Project/Art/Environment/Pine/SM_Pine.fbx` — generated; three snowy tiers; footprint ⌀ 2.3 circle; ~160 triangles |
| DropOffZone | `blender/models/drop_off_zone.py` | `client/Assets/_Project/Art/Environment/DropOffZone/SM_DropOffZone.fbx` — Quaternius Cauldron over Bonfire with generated flames; `SteamOrigin` anchor 1.6 m up; footprint ⌀ 3.4 circle; ~1904 triangles |
| Drift | `blender/models/drift.py` | `client/Assets/_Project/Art/Environment/Drift/SM_Drift.fbx` — generated; snowed-over haystack mound; footprint ⌀ 2.6 circle; ~670 triangles |
| SteamPuff | `blender/models/steam_puff.py` | `client/Assets/_Project/Art/VFX/Steam/SM_SteamPuff.fbx` — generated; white puff mesh for the `FX_Steam` mesh particles, origin at its center; ~80 triangles |
| Loot | `blender/models/loot.py` | `client/Assets/_Project/Art/Props/Loot/SM_Loot.fbx` — CreativeTrio Present, red gift box with green ribbon; 0.7 m; ~230 triangles |
| Loot_Gadget | `blender/models/loot_gadget.py` | `client/Assets/_Project/Art/Props/Loot_Gadget/SM_Loot_Gadget.fbx` — Quaternius Cube Exclamation, blue with yellow "!" and a yellow top panel; 0.7 m; ~1100 triangles |
| Snowball | `blender/models/snowball.py` | `client/Assets/_Project/Art/Props/Snowball/SM_Snowball.fbx` — generated; ⌀ 0.36 m, origin at its center; ~80 triangles |

## Adding a model

1. Create `blender/models/<model>.py` with `NAME`, `OUTPUT` at `client/Assets/_Project/Art/<Category>/<Asset>/SM_<Asset>.fbx` (`SK_` for rigged models), and `build`.
2. Register it in `MODELS` in `build.py`.
3. Build, check it with `look` in Blender, refresh Unity; the import postprocessor in `_Project/Editor/ArtImport/` applies settings and the palette material.
4. Follow "Adding an asset" in `client/Assets/_Project/Art/CLAUDE.md` (visual prefab, nesting) and add a row to both asset tables.
