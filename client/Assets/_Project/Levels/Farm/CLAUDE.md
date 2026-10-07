# Farm

The first and only map: a snowed-in farmyard, 40 × 40 m, with the Drop-Off Zone's cauldron in the centre. No code; this folder holds the map's obstacle prefabs and this note on how `Scenes/Match.unity` is composed. Spec: `.scratch/level/spec.md`; missing art: `.scratch/level/art-requests.md`.

## Scene composition (`Scenes/Match.unity`)

| Root | Role | Read by |
|---|---|---|
| `Arena` | Everything a Vehicle can hit, plus the ground the camera frames: `Floor` (40 × 40 m plane), `Perimeter` (four wall `BoxCollider`s just outside ±20 m with `V_Fence` rows on the line), `Obstacles` (instances of the prefabs below), and `DropOffZone` at the origin. | `VehicleArenaReader` (colliders → Obstacles), `SnowGridDriver` (masks Cells), `CameraDirector` (renderer bounds → Overview framing and Follow clamping) |
| `Scenery` | Decor outside the walls: a 140 m ground plane, the buildings standing on the wall line (Barn north, House and Shed south, Shed east, House west), farm props, about 190 Pines, and a few far houses. No colliders. | nothing; kept out of `Arena` so it never widens the camera's Arena bounds or adds Obstacles |
| `SnowGrid` | Snow Grid driver and surface; `SnowConfig` covers origin (−20, −20), size (40, 40). | Snow |
| `VehicleSpawner` | Six Spawn Points on a 13 m circle, every 60° from north, each facing the centre. | Vehicle |
| `VehicleWorld`, `MatchDriver`, `MatchScope` | Network drivers and the scene scope. | Vehicle, Match, Bootstrap |
| `CameraRig` | The only camera; `_arenaRoot` = `Arena`. | CameraRig, Hud (scene `Camera`) |
| `Hud` | `Hud.prefab` with its EventSystem. | Hud |

## Layout

- Perimeter: fences on all four sides; a building stands where the fence has a gap, front face on the wall line. Vehicles bounce off the invisible wall boxes, never off the building meshes.
- Centre: cauldron (1.5 m Obstacle), Snow-Free Area to 5 m, then a ring of twelve Hay Bales at 7.5 m in four arcs on the diagonals. The north, south, east and west lanes into the cauldron stay open.
- Mid field: Wood Piles (6, 15) and (−15.5, 0), Carts (15.5, 0) and (−14.5, 15), Well (−6, −15.5), Chicken Coop in the north-east corner (16, 16.3), a Hay Bale cluster in the south-east corner. Every obstacle stays at least 5 m from a Spawn Point; the open fields between them are where Snow is collected.

## Obstacle prefabs (`Obstacles/`)

`HayBale` (circle r 0.6), `WoodPile`, `Cart`, `Well`, `ChickenCoop` (boxes sized to the model): a root with the collider and the nested visual prefab `Art/Environment/<Asset>/V_<Asset>.prefab` as `Model`. They are gameplay prefabs of the map (architecture: a piece of a map belongs to the map); the look changes by editing the `V_` prefab. Colliders are read as world AABBs, so an instance turned 90° stays correct; any other angle widens its box.

## Rules

- Anything Vehicles should hit goes under `Arena` with a `BoxCollider` (box) or `SphereCollider` / `CapsuleCollider` (circle). Anything else goes under `Scenery`.
- Changing the Arena size means changing `SnowConfig` origin and size, the wall boxes, the `Floor` scale, and the Spawn Point circle together; `SnowGridDriver.MaxWords` (1024) caps the grid at 40 × 40 m with 0.5 m Cells.
- Keep at least the Snow-Free radius (`DropOffConfig.SnowFreeRadius`, 5 m) plus a Vehicle length clear around the cauldron.
- The scene stays at `Scenes/Match.unity` while it is the only map; a second map moves each into `Levels/<Map>/` with its scene.
