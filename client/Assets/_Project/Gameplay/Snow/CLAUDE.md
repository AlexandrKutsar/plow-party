# Snow

The Snow Grid: Snow lying on the Arena, scraped by every Vehicle's Blade, regrowing slowly, re-covered by Blizzard waves, and stacked into Snow Piles by a Spill (GDD 4.1, 4.3, 4.4). Rules live in `Simulation/` as deterministic C#; the Host steps them once per Fusion tick. Decisions: ADR-0011. Spec: `.scratch/snow/spec.md`.

## Entry points

- `SnowGrid` — the single test seam. Built from `SnowSettings`, a `VehicleArena` (its boxes and circles mask Cells), and a seed. `Scrape(blade, room)` returns the Depth steps taken, `Spill(point, steps)` returns the steps that landed, `Tick(elapsedPlayingTime)` applies Regrowth and Blizzards due by then, `GetDepth(x, y)` reads a Cell, `WordCount` / `GetWord` / `SetWord` expose the packed form for sync.
- `SnowBlade` — the oriented strip a Blade scrapes; `SnowBlade.Ahead(position, forward, settings)` places it `BladeForwardOffset` in front of a Vehicle.
- `IBladeRoom` — how many Depth steps a Slot's Vehicle can still take. `UnlimitedBladeRoom` is the default until Bucket implements it; Snow never references Bucket.
- `SnowSettings` — tunable numbers, filled from `SnowConfig`.

## Rules worth knowing

- Cells are `CellSize` squares over the `Origin` + `Size` rectangle (Vector2 x = world x, y = world z, as in Vehicle). Depth is 4 bits: 0 cleared, `FullDepth` (3) fully covered, up to `SnowGrid.MaxDepth` (15) as a Snow Pile. Eight Cells pack into one `int`, row-major from Cell (0, 0).
- A Cell whose centre lies inside an Arena box or circle is masked: Depth 0 forever, never scraped, regrown, blizzarded, or piled on.
- `Scrape` visits Cells under the Blade row by row and stops once `room` steps are taken, so a partial scrape is deterministic. Snow Pile steps are collected like any other.
- `Spill` fills the Cell under the point to `MaxDepth`, then square rings around it, each ring row by row, skipping masked Cells.
- Regrowth: every `RegrowthInterval` seconds each unmasked Cell below `FullDepth` gains one step with chance `RegrowthChance`, decided by a stateless hash of (seed, Cell, Regrowth step). The result depends only on elapsed time, not on how ticks slice it. Time going backwards is ignored.
- Blizzard: a wave starts at each `BlizzardTimes` entry and sweeps the grid in `BlizzardDuration` from one of four sides, picked by hashing (seed, wave). Cells the front passes are raised to `FullDepth`; a Cell cleared behind the front stays cleared until the next wave; Snow Piles are untouched; a wave missed entirely completes in one `Tick`.
- Snow speaks in Depth steps, never in Load; Bucket owns the conversion.

## Network and view

- `SnowGridDriver` — scene `NetworkObject` in `Match.unity`. Mirrors the grid into `[Networked, Capacity(512)] NetworkArray<int>`; Fusion's delta compression sends only changed words. On the Host each tick: scrape under every registered Vehicle's Blade in Slot order (room from `IBladeRoom`), raise `Scraped` (`VehicleScrape`: Vehicle, steps) for non-zero results, `Tick` with time since the driver spawned, copy words. `Spill(point, steps)` is Host-only. Clients only read words through `CopyWordsTo`. The seed is picked on the Host at spawn and networked.
- Temporary clock: elapsed Playing time is counted from the driver's spawn tick until the Match module exists and supplies the real one.
- `SnowSurfaceView` — paints the grid into a runtime RGBA32 texture (one texel per Cell, bilinear) set as `_BaseMap` on the `Model` renderer through a property block, and fits the `Model` quad to the grid rectangle. Repaints only words that changed. Cosmetic pre-clear: Blades of the local Player's Vehicle from the last `PreClearDuration` seconds are scraped on the View's own copy, so the track appears before the Host confirms.
- `SnowConfig` — the ScriptableObject behind `SnowSettings`; asset `_Project/Configs/SnowConfig.asset`, registered in `RootLifetimeScope`.
- Scene object `SnowGrid`: `NetworkObject`, `SnowGridDriver` (arena root = `Arena`), `SnowSurfaceView`, and the nested visual prefab `Art/Environment/Snow/V_SnowSurface.prefab` as `Model` (a quad with `M_SnowSurface`).

## Depends on

Simulation: `UnityEngine` math and Vehicle's `VehicleArena`. Network and View: Fusion, VContainer, Vehicle (`VehicleRegistry`, `NetworkVehicle`, `VehicleArenaReader`). Vehicle never references Snow. The assembly is in Fusion's `AssembliesToWeave`.
