# Snow polish

Status: ready-for-agent

## Problem Statement

The Snow Grid plays, but for a vertical slice it reads poorly. Regrowth is a seeded coin flip per Cell, so a cleared track speckles back in random dots instead of fading as a track. The floor is a flat quad with one texel per Cell: from the gameplay camera the Cell grid is visible, tracks have hard pixel edges, and a Snow Pile is only a paler patch with no volume. A Snow Pile is also weightless to drive through: a Blade swallows 150 steps in a tick or two and the Vehicle does not slow. Finally the networked word array holds 512 words, while the Arena is about to grow to 40 × 40 m (800 words).

## Solution

Regrowth becomes age-based: the Host remembers when each Cell was last lowered, waits `RegrowthDelay`, then adds one Depth step every `RegrowthStep` until `FullDepth`, so the oldest part of a track refills first and a track fades as a tail. The floor becomes a displaced mesh: the View writes a height texture (one texel per Cell, height eased toward the target Depth so nothing pops), and a URP vertex shader displaces a mesh of several vertices per Cell by a smoothed (B-spline) sample of it, deriving normals from the same field. Snow Piles get real volume and tracks soft banks. Snow Piles become heavy: a Blade over Pile Cells slows its Vehicle through a speed factor that composes with Bucket's, takes at most a capped number of Pile steps per tick, and raises a networked per-Slot "plowing a Pile" flag that drives a snow burst at the Blade and that Camera can later read for shake. The word array capacity grows to 1024.

## User Stories

1. As a Player, I want my track to fade from its oldest end, so that I can read where I drove and in what order.
2. As a Player, I want regrowth to be predictable, so that I can plan to come back to an old track.
3. As a Player, I want the snow to look like a soft surface, not a grid of squares, at the gameplay camera distance and from a top view.
4. As a Player, I want a Snow Pile to look like a mound with real height, so that I spot it across the Arena.
5. As a Player, I want my track to have soft banks, so that it reads as plowed snow.
6. As a Player, I want snow to grow back and be scraped without sudden pops, so that the surface feels continuous.
7. As a Player, I want my track to still appear instantly under my Blade, so that clearing feels local.
8. As a Player, I want plowing a Snow Pile to slow me down, so that a Pile feels heavy.
9. As a Player, I want a Pile to take a noticeable moment to plow through, so that grabbing one is a commitment.
10. As a Player, I want a burst of snow at my Blade while I plow a Pile, so that the effort is visible.
11. As a developer building Camera, I want a per-Vehicle "plowing a Pile" state readable on every peer, so that I can add camera shake without touching Snow.
12. As a developer, I want Bucket's Load penalty and Snow's Pile penalty to multiply rather than overwrite each other, so that two features can slow a Vehicle.
13. As a level designer, I want a 40 × 40 m Arena at 0.5 m Cells to fit the networked grid, and a clear error when a config does not fit.
14. As a game designer, I want regrowth delay and step, Pile slowdown, Pile pickup cap, snow and Pile heights, and easing times in config.

## Implementation Decisions

- **Regrowth (replaces chance-based):** `SnowGrid` keeps a host-only `float[]` with each Cell's next Regrowth time. Lowering a Cell's Depth by `Scrape` sets it to `now + RegrowthDelay`, where `now` is the latest time `Tick` reached. `Tick(t)` raises every snowfall Cell below `FullDepth` one step per `RegrowthStep` while its next time is ≤ t, advancing the next time by `RegrowthStep`. The schedule depends only on elapsed time, so tick slicing does not change the result; time going backwards is ignored. `RegrowthStep` 0 disables Regrowth. Cells at or above `FullDepth` (including Snow Piles) are untouched. `RegrowthChance` and `SnowHash.Chance` are removed.
- **Cells that never regrow:** masked Cells and Snow-Free Area Cells (they start at 0 and are not snowfall Cells). Any snowfall Cell starts full and has no schedule until first lowered. A Blizzard only raises Cells, so it never starts a schedule; a Cell it fills keeps a stale time that only matters once it is lowered again, which resets it. A Spill on a partially regrown Cell keeps that Cell's schedule.
- **Pile pickup cap:** `SnowSettings.PileStepsPerScrape` caps how many steps one `Scrape` call takes from above `FullDepth` across all Cells under the Blade; the Host scrapes once per tick, so it is a per-tick cap. Steps under a Pile cannot be reached until its Pile steps are gone. The View's pre-clear scrapes each 60 Hz sample with the same cap, which roughly matches the Host at a 60 Hz tick.
- **Pile slowdown:** `SnowGrid.SpeedMultiplierUnder(blade)` returns `1 − PileSpeedPenalty` when any Cell under the Blade is deeper than `FullDepth`, else 1. Read before scraping, so the tick that finishes a Pile still slows.
- **Speed factors compose (cross-module, Vehicle and Bucket):** `NetworkVehicle.SpeedMultiplier` becomes a read-only product of `[Networked] NetworkArray<float>` factors indexed by `VehicleSpeedSource` (`Load`, `SnowPile`), each set with `SetSpeedFactor(source, value)` by its single owner. Bucket writes `Load`, Snow writes `SnowPile`. `VehicleModifiers` and `VehicleWorld` are unchanged.
- **Plowing signal:** `SnowGridDriver` holds `[Networked, Capacity(MaxVehicles)] NetworkArray<NetworkBool>` indexed by Slot; on the Host a Slot is plowing a Pile in a tick when its Blade was over a Pile Cell and scraped at least one step. `IsPlowingPile(NetworkVehicle)` reads it on every peer. Camera reads it for shake later; Snow implements no shake.
- **Capacity:** `SnowGridDriver.MaxWords` = 1024 (80 × 80 Cells need 800). Spawning a grid that needs more throws with the grid size and both word counts. Config origin and size are not changed here.
- **Rendering (new ADR-0015):** `SnowGridView` builds at runtime a grid mesh of `VerticesPerCell` vertices per Cell edge over the grid rectangle and an R8 height texture (one texel per Cell). Each Cell's shown height eases toward the height of its Depth (`LowerTime` fast, `RaiseTime` slower, exponential), only for Cells whose word changed or that are still moving; the texture uploads only on frames where a texel changed. Depth → metres is piecewise linear: `SnowHeight` at `FullDepth`, `SnowHeight + PileHeight` at `MaxDepth`. The shader `SH_SnowSurface` (URP, HLSL) samples the texture in the vertex stage with a 4-tap cubic B-spline, displaces Y, derives normals by central differences, and colours by height (ground → snow → pile tint) with main-light shadows received. Cosmetic pre-clear unchanged.
- **Art:** `Art/Environment/SnowGrid/SH_SnowSurface.shader`, `M_SnowSurface.mat` replacing `M_SnowGrid.mat`; `V_SnowGrid.prefab` keeps a MeshFilter and MeshRenderer (mesh replaced at runtime) with `M_SnowSurface`. `Art/VFX/SnowBurst/FX_SnowBurst.prefab`: mesh particles of `SM_SteamPuff` with `M_Palette`, no transparency. New naming prefix `SH_` for hand-written shaders.
- **Burst view:** `SnowPileBurstView` (Snow/View) instantiates one `FX_SnowBurst` per Slot under itself and emits at each Vehicle's Blade while `IsPlowingPile` is set. Added to the `SnowGrid` scene object at integration.
- **Config:** `SnowConfig` gains `RegrowthDelay` (6 s), `RegrowthStep` (3 s), `PileSpeedPenalty` (0.4), `PileStepsPerScrape` (2), `VerticesPerCell` (2), `SnowHeight` (0.12 m), `PileHeight` (0.9 m), `LowerTime` (0.05 s), `RaiseTime` (0.35 s); loses `RegrowthInterval` and `RegrowthChance`.

## Testing Decisions

- **Module under test:** Snow Simulation via `SnowGrid` in EditMode, as before; the behaviours: no Regrowth before the delay, one step per `RegrowthStep` after it, stops at `FullDepth`, older Cells refill first (tail), re-scraping restarts the delay, slicing independence, time backwards, zero step disables, Piles and masked and Snow-Free Cells untouched; Pile cap per scrape, steps under a Pile are unreachable until its Pile steps are gone, plain Cells are not capped; speed multiplier under Blade over Pile, plain snow, cleared.
- **Not unit-tested:** `NetworkVehicle` factor product, driver, View, shader, burst; checked in Play Mode with a throwaway scene.

## Out of Scope

- Camera shake (Camera module reads `IsPlowingPile`).
- Wiring Snow's clock to the Match clock.
- Changing the Arena size or `SnowConfig` origin and size.
- Scene wiring in `Match.unity` (integration adds `SnowPileBurstView` and its prefab).

## Further Notes

- Cross-module edits: Vehicle (`NetworkVehicle`, `VehicleSpeedSource`, CLAUDE.md), Bucket (`NetworkBucket`, CLAUDE.md), Art (`CLAUDE.md` naming row and assets rows).
- ADRs: ADR-0012 updated (Regrowth and render statements); ADR-0015 records the displaced-mesh render.
