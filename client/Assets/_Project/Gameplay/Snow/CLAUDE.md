# Snow

The Snow Grid: Snow lying on the Arena, scraped by every Vehicle's Blade, regrowing slowly, re-covered by Blizzard waves, and stacked into Snow Piles by a Spill (GDD 4.1, 4.3, 4.4). Rules live in `Simulation/` as deterministic C#; the Host steps them once per Fusion tick. Decisions: ADR-0012. Spec: `.scratch/snow/spec.md`.

## Entry points

- `SnowGrid` — the single test seam. Built from `SnowSettings`, a `VehicleArena` (Cells inside `VehicleArena.Contains` are masked), an optional `ISnowFreeArea`, and a seed. `Scrape(blade, room)` returns the Depth steps taken, at most `room`; `Spill(point, steps)` returns the steps that landed; `Tick(elapsedPlayingTime)` applies Regrowth and Blizzards due by then; `GetDepth(x, y)` or `GetDepth(cellIndex)` (row-major) reads a Cell; `WordCount` / `GetWord` / `SetWord` expose the packed form for sync.
- `SnowBlade` — the oriented strip a Blade scrapes; `SnowBlade.Ahead(position, forward, settings)` places it `BladeForwardOffset` in front of a Vehicle.
- `IScrapeLimit` — Snow's capacity seam: `LimitFor(vehicle)` is how many steps that Vehicle's Blade may still take. Bucket implements it, so Snow never references Bucket. The driver passes it to `Scrape` as `room`; the View shares one frame's limit across all pre-clear samples, so a full Bucket shows no pre-cleared track.
- `ISnowFreeArea` — Snow's Snow-Free Area seam: `Contains(point)`. DropOff implements it with a circle around the Drop-Off Zone, so Snow never references DropOff. Queried once per Cell centre when the grid is built.
- `SnowSettings` — tunable numbers, filled from `SnowConfig`.

## Rules worth knowing

- Cells are `CellSize` squares over the `Origin` + `Size` rectangle (Vector2 x = world x, y = world z, as in Vehicle). Depth is 4 bits: 0 cleared, `FullDepth` (3) fully covered, up to `SnowGrid.MaxDepth` (15) as a Snow Pile. Eight Cells pack into one `int`, row-major from Cell (0, 0).
- A Cell whose centre lies inside an Arena box or circle is masked: Depth 0 forever, never scraped, regrown, blizzarded, or piled on.
- A Cell whose centre lies in the `ISnowFreeArea` is snow-free: Depth 0 at start, skipped by Regrowth, Blizzard fronts, and Blizzard Pile placement, but scraped and Spilled on like any other Cell, so Snow Piles land and are collected there. An Obstacle mask wins over snow-free.
- `Scrape` visits Cells under the Blade row by row and stops once `room` steps are taken, so a partial scrape is deterministic. Snow Pile steps are collected like any other.
- `Spill` fills the Cell under the point to `MaxDepth`, then square rings around it, each ring row by row, skipping masked Cells.
- Regrowth (off when `RegrowthInterval` is 0): every `RegrowthInterval` seconds each unmasked Cell below `FullDepth` gains one step with chance `RegrowthChance`, decided by a stateless hash of (seed, Cell, Regrowth step). The result depends only on elapsed time, not on how ticks slice it. Time going backwards is ignored.
- Blizzard: a wave starts at each `BlizzardTimes` entry and sweeps the grid in `BlizzardDuration` (0 fills at once) from one of four `BlizzardSide`s, picked by hashing (seed, wave). Cells the front passes are raised to `FullDepth`; a Cell cleared behind the front stays cleared until the next wave; Snow Piles are untouched; a wave missed entirely completes in one `Tick`. Each wave also leaves `BlizzardPilesPerWave` Snow Piles of `BlizzardPileSteps` each, dropped when the front reaches their Cell; the Cells come from hashing (seed, wave, pile), moved to the next Cell that is neither masked nor snow-free (GDD 4.4), which keeps them out of the Snow-Free Area around the Drop-Off Zone.
- Snow speaks in Depth steps, never in Load; Bucket owns the conversion.

## Network and view

- `SnowGridDriver` — scene `NetworkObject` in `Match.unity`. Mirrors the grid into `[Networked, Capacity(512)] NetworkArray<int>`; Fusion's delta compression sends only changed words. On the Host each tick: scrape under every registered Vehicle's Blade in Slot order, `Tick` with time since the driver spawned, copy words, then raise `Scraped` (`VehicleScrape`: Vehicle, steps) for each non-zero result, so subscribers see the finished tick. `Spill(point, steps)` is Host-only and ignored before spawn. Clients only read words through `CopyWordsTo`.
- Seed: a random number the Host picks at spawn, not networked and not configured; it only varies Regrowth and Blizzard sides between Matches. Clients never simulate, so they do not need it; client prediction would network it.
- Regrowth and Blizzard progress live only in the Host's `SnowGrid`, outside `[Networked]` state. Fine without host migration (ADR-0012).
- Temporary clock: elapsed Playing time is counted from the driver's spawn tick until the Match module exists and supplies the real one.
- `SnowGridView` — paints the grid into a runtime RGBA32 texture (one texel per Cell, bilinear) set as `_BaseMap` on the `Model` renderer through a property block, and fits the `Model` quad to the grid rectangle. Repaints only words that changed. Cosmetic pre-clear: the local Player's Blade is sampled at 60 Hz and every sample from the last `PreClearDuration` seconds is scraped on the View's own copy, so the track appears before the Host confirms. Layout comes from `SnowConfig`; the driver only supplies words. The View needs no `ISnowFreeArea`: it overwrites its copy with the Host's words every frame and pre-clear only lowers Depth.
- `SnowConfig` — the ScriptableObject behind `SnowSettings`; asset `_Project/Configs/SnowConfig.asset`, registered in `RootLifetimeScope`.
- Scene object `SnowGrid`: `NetworkObject`, `SnowGridDriver` (arena root = `Arena`), `SnowGridView`, and the nested visual prefab `Art/Environment/SnowGrid/V_SnowGrid.prefab` as `Model` (a quad with `M_SnowGrid`).

## Depends on

Simulation: `UnityEngine` math and Vehicle's `VehicleArena`. Network and View: Fusion, VContainer, Vehicle (`VehicleRegistry`, `NetworkVehicle`, `VehicleArenaReader`), and an `IScrapeLimit` and `ISnowFreeArea` registered in `MatchScope` (both from other modules). Vehicle never references Snow. The assembly is in Fusion's `AssembliesToWeave`.
