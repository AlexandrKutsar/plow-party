# Snow

Status: ready-for-agent

## Problem Statement

The core loop of Plow Party is "collect Snow, deliver it" (GDD 2, 4.1), but the Match has no Snow yet: Vehicles drive over a bare floor. Bucket, Drop-Off Zone, Bots, and the HUD all need a map-wide Snow Grid that Vehicles clear with their Blade, that slowly regrows, that a Blizzard re-covers on schedule, and onto which a Spill drops a Snow Pile anyone can collect. It must stay consistent between the Host and every client at phone-friendly bandwidth (GDD 11, "Snow Grid and its sync").

## Solution

A Snow feature in Gameplay whose rules live in one pure C# type, `SnowGrid`: a 0.5 m grid of Cells over the Arena, each holding a Depth of 0–15 (0 cleared, 3 full, 4–15 Snow Pile), packed eight Cells per `int`. The Host steps it once per Fusion tick: every Vehicle's Blade scrapes the Cells in front of it, Regrowth raises cleared Cells one step at a time by seeded chance, and a Blizzard front sweeps the Arena at 45, 90, and 135 s of Playing. A scene `NetworkBehaviour` mirrors the packed words into a `[Networked] NetworkArray<int>`; Fusion's per-word delta compression sends only what changed. Clients do not predict the grid; their View paints the grid onto the floor through a runtime texture and clears Cells under the local Blade cosmetically until the Host confirms. Snow speaks only in Depth steps; Bucket will convert them to Load and limit scraping through the `room` it supplies. Decisions: ADR-0012.

## User Stories

1. As a Player, I want the whole yard covered in Snow when the Match starts, so that there is something to collect everywhere.
2. As a Player, I want the strip in front of my Vehicle to scrape Snow off the ground as I drive, so that driving fills my Bucket.
3. As a Player, I want a clean track behind my Vehicle, so that I can see where I and others have already been.
4. As a Player, I want my track to appear immediately on my screen, so that clearing feels local even though the Host is authoritative.
5. As a Player, I want turning on the spot not to clear the Snow under my rear, so that only the Blade collects.
6. As a Player, I want cleared ground to slowly grow Snow back, so that old areas become worth revisiting.
7. As a Player, I want partially regrown Snow to be worth less than fresh Snow, so that choosing where to drive matters.
8. As a Player, I want a Blizzard to sweep across the yard at 45, 90, and 135 s, so that the map refills in visible waves.
9. As a Player, I want the Blizzard schedule to be predictable, so that the HUD can announce it 5 s ahead (GDD 4.4).
10. As a Player, I want Snow I Spill after a Ram to land as a Snow Pile around my Vehicle, so that the loss is visible and recoverable.
11. As a Player, I want anyone, including me, to collect a Snow Pile, so that Spills create a scramble (GDD 4.3).
12. As a Player, I want a Snow Pile to be worth more than ordinary Snow on the same Cells, so that it is worth fighting over.
13. As a Player, I want a Snow Pile to survive Regrowth and Blizzards, so that it lasts until someone takes it.
14. As a Player, I want no Snow inside walls and pillars, so that the map's layout is honest.
15. As a Player with a full Bucket, I want my Blade to leave Snow on the ground, so that I do not waste it.
16. As a Player joining late or recovering from packet loss, I want to see the same grid as everyone else, so that the map is never out of sync.
17. As the Host, I want the grid's network cost to be a few hundred words with only changed words sent, so that it fits phone bandwidth.
18. As the Host, I want identical inputs to produce identical grids, so that the rules are testable and could later be predicted.
19. As a developer building Bucket, I want Snow to report "Vehicle N scraped K steps" each tick and to take a per-Vehicle limit on scraping, so that Bucket owns Load and capacity without Snow depending on it.
20. As a developer building Bucket, I want a host-side way to drop K steps as a Snow Pile at a point, so that a Spill needs no knowledge of the grid's layout.
21. As a developer building Match and Hud, I want the Blizzard schedule driven by elapsed Playing time and readable from config, so that Match can replace the temporary clock and Hud can count down.
22. As a game designer, I want Cell size, Arena rectangle, Blade size, Regrowth interval and chance, and Blizzard times, duration, and Piles per wave and their size in a config asset, so that I can tune without code.
23. As a developer, I want the Snow rules covered by fast EditMode tests through one entry point, so that refactors do not silently change the game.
24. As a Player, I want each Blizzard to leave a few big Snow Piles behind its front, so that the announcement becomes a choice between staying near the Drop-Off Zone and racing for a Pile (GDD 4.4).

## Implementation Decisions

- **Module:** new feature module `Gameplay/Snow`, asmdef `PlowParty.Gameplay.Snow`, namespace `PlowParty.Gameplay.Snow`, module `CLAUDE.md`; the feature index row moves from `planned` to `active`. Sub-folders `Simulation`, `Network`, `View`, `Config`, `Tests`. References: Vehicle (read-only: `VehicleRegistry`, `NetworkVehicle` state, `VehicleArena`), Infrastructure, Fusion, VContainer. Vehicle never references Snow.
- **Single test seam — `SnowGrid`:** pure C# in Simulation, constructed from `SnowSettings`, the obstacle set of a `VehicleArena`, and a seed. Public surface:
  - `Scrape(blade, room)` → steps removed: a Blade is an oriented rectangle (centre, forward, width, depth); every unmasked Cell whose centre lies inside it loses Depth until `room` steps are taken. Cells are visited in a fixed order (row-major), so a partial scrape is deterministic.
  - `Spill(point, steps)` → steps placed: the Cell under `point` first, then square rings around it, each ring row by row; each unmasked Cell is raised to Depth 15; steps that find no Cell within the grid are dropped and the return value says how many landed.
  - `Tick(elapsedPlayingTime)`: applies every Regrowth step and Blizzard progress due up to that time; time going backwards is ignored.
  - `GetDepth(x, y)`, `Width`, `Height` for the View and tests.
  - `WordCount`, `GetWord(i)`, `SetWord(i, value)`: the packed form the network adapter mirrors.
- **Grid:** Cell size 0.5 m over the config rectangle (default origin (-15, -15), size 30 × 30 m → 60 × 60 Cells, 450 words). Vector2 x = world x, y = world z, as in Vehicle.
- **Depth:** 4 bits per Cell, 0–15. `FullDepth` = 3; 4–15 is a Snow Pile. Every unmasked Cell starts at `FullDepth`.
- **Mask:** a Cell whose centre lies inside an Arena box or circle is masked: Depth 0 forever, never scraped, regrown, blizzarded, or piled on. Built once at construction.
- **Regrowth:** every `RegrowthInterval` seconds of Playing time, each unmasked Cell below `FullDepth` gains one step with probability `RegrowthChance`, decided by a stateless hash of (seed, Cell index, Regrowth step number) so the outcome depends only on the step, not on how ticks are sliced. Cells at `FullDepth` or above are untouched.
- **Blizzard:** waves start at `BlizzardTimes` (default 45, 90, 135 s) and last `BlizzardDuration` (default 2.5 s). Each wave's direction is one of four (from west, east, south, north) picked by hashing (seed, wave index). The front moves linearly across the grid; every unmasked Cell the front has passed that is below `FullDepth` is raised to `FullDepth`. A wave whose end time is already past completes in one `Tick`. Each wave leaves `BlizzardPilesPerWave` Snow Piles of `BlizzardPileSteps` (defaults 3 × 150) on Cells picked by hashing (seed, wave, pile), each dropped by `Spill` when the front reaches its Cell; a masked pick moves to the next open Cell.
- **Capacity seam:** `Scrape` takes `room` as a parameter; the driver and the View pass unlimited until Bucket introduces its own capacity interface and supplies it. Snow never references Bucket.
- **Seed:** a random number the Host picks at spawn; not configured and not networked, since clients never simulate.
- **Scrape event:** each Host tick the driver publishes the steps each Slot scraped (only non-zero entries), the way Vehicle publishes `Rammed`; Bucket subscribes later.
- **Blade placement:** centred `BladeForwardOffset` ahead of the Vehicle position along its forward, `BladeWidth` × `BladeDepth` (defaults 1.08 × 0.5 m, offset so it sits in front of the capsule's nose). Values in `SnowConfig`; Snow never reads Vehicle's config.
- **Network adapter — `SnowGridDriver`:** scene `NetworkObject` in `Match.unity`. Holds `[Networked, Capacity(512)] NetworkArray<int>` (450 needed by the default Arena, the rest is headroom checked at spawn) and the networked start tick. On the Host in `FixedUpdateNetwork`: for each registered Vehicle in Slot order, build its Blade and `Scrape` with the capacity seam's room; `Tick` with elapsed time since the start tick (temporary clock until Match exists); copy every word into the array; then raise the scrape event. A host-only `Spill(point, steps)` forwards to `SnowGrid`. Clients only read the array. The assembly is added to `AssembliesToWeave`.
- **View — `SnowGridView`:** owns a 60 × 60 RGBA32 texture (bilinear) on the snow floor's material; repaints Cells whose networked word changed, colouring by Depth between ground, snow, and pile colours. Cosmetic pre-clear: Cells under the local Player's Blade are painted as cleared at once and kept so for a short timeout. The View never writes simulation state.
- **Art:** `Art/Environment/SnowGrid/M_SnowGrid.mat` (URP Simple Lit, base map assigned at runtime) and `V_SnowGrid.prefab`; a Shader Graph with height comes later. Row added to the Art assets table; reason for a new material recorded in ADR-0012.
- **Config:** `SnowConfig` ScriptableObject → `SnowSettings`; asset `_Project/Configs/SnowConfig.asset`, registered in `RootLifetimeScope`.
- **Glossary:** Snow Grid, Cell, Depth, Blade, Snow Pile, Regrowth, Blizzard as defined in `GLOSSARY.md`.

## Testing Decisions

- **Good tests** construct a `SnowGrid` with small settings and an obstacle set, call `Scrape`, `Spill`, `Tick`, and assert only on returned values, `GetDepth`, and packed words. They never reach into private fields.
- **Module under test:** Snow Simulation via EditMode tests in `PlowParty.Gameplay.Snow.Tests`, run with `run_tests --filter PlowParty.Gameplay.Snow --filter_type assembly`. Naming `Method_Condition_ExpectedResult`.
- **Behaviours to cover:** initial Depth full and masked Cells zero; Blade clears only Cells whose centres lie in the oriented rectangle, at any facing; scrape returns removed steps including Pile steps; `room` limits removal deterministically; masked Cells are never scraped; Spill fills centre-out up to 15, skips masked Cells, reports what landed; Regrowth raises only cleared or partial Cells, never above full; Regrowth result is independent of tick slicing; Blizzard leaves Cells ahead of the front alone, fills passed Cells to full, leaves Piles untouched, completes a missed wave in one tick; wave direction depends on seed; each wave leaves all its Pile steps, none before the front reaches them, on open Cells only, placed by seed; pack/unpack round-trip through words reproduces the grid; same seed and inputs give identical words.
- **Not unit-tested:** `SnowGridDriver` and `SnowGridView`; checked in Play Mode and Multiplayer Play Mode (track appears on Host and client, Blizzard sweeps, Spill from `eval` leaves a Pile).
- **Prior art:** `Gameplay/Vehicle/Tests` (`VehicleTestSettings` helper pattern).

## Out of Scope

- Bucket Load, capacity, the steps-to-Load rate, and calling `Spill` on a Ram — Bucket.
- Match state and the real Playing clock — Match replaces the temporary clock.
- HUD Blizzard announcement — Hud reads the schedule.
- Client prediction of the grid (ADR-0012 keeps the door open).
- Shader Graph height or displacement, snow particles, Blizzard VFX.
- Bots' use of the grid.

## Further Notes

- GDD sources: 4.1 (collection, cleared path), 4.3 (Spill and Snow Pile), 4.4 (Regrowth and Blizzard), 11 (grid sync question, answered by ADR-0012).
- Out-of-module edits approved by the user: `NetworkProjectConfig.fusion` (`AssembliesToWeave`), `_Project/Configs/SnowConfig.asset`, one registration in `RootLifetimeScope`, two in `MatchScope`, scene objects in `Match.unity`, `Art/Environment/SnowGrid/` with its row in `Art/CLAUDE.md`.
- ADRs touched: ADR-0004 (DI for scene NetworkObjects), ADR-0005 (Simulation free of Fusion types), ADR-0010 (Vehicle geometry the Blade follows), ADR-0012 (this design).
