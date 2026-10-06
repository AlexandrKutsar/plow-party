# Bucket

Status: ready-for-agent

## Problem Statement

Vehicles scrape Snow off the Snow Grid, but nothing holds it: the Blade clears an unlimited amount, driving with a full load costs nothing, and a Ram has no consequence beyond the Knockback. The scoring loop "collect, carry, deliver" (GDD 4.1–4.3) needs a per-Vehicle container with a capacity, a speed cost for carrying, and a Spill that turns a Ram into a scramble for the lost Snow.

## Solution

A Bucket feature in Gameplay. Its rules live in one pure C# type, `BucketRules`, that works on Load held in whole Depth steps: how many free steps are left, what collecting adds, how many steps a Spill throws out, and the speed multiplier for a given Load. A `NetworkBucket` on the Vehicle prefab holds the networked Load. A host-side entry point, `BucketHost`, fills Buckets from `SnowGridDriver.Scraped`, Spills on `VehicleRegistry.Rammed`, and writes the Vehicle's `SpeedMultiplier`. Snow asks for the free space through `IScrapeLimit`, an interface Snow declares and Bucket implements, so Snow still never references Bucket.

## User Stories

1. As a Player, I want driving over Snow to fill my Bucket, so that I have something to deliver.
2. As a Player, I want my Bucket to stop filling at capacity, so that I have to deliver before collecting more.
3. As a Player with a full Bucket, I want my Blade to leave Snow on the ground, so that nothing is wasted and my track shows I am full.
4. As a Player, I want my local track preview to stop clearing when my Bucket is full, so that the screen never shows Snow vanishing that the Host keeps on the ground.
5. As a Player, I want a heavier Bucket to slow me down smoothly, up to −25% when full, so that carrying a big load is a risk.
6. As a Player who gets Rammed, I want to lose 30% of my Load, so that being hit matters.
7. As a Player who Rams someone, I want their Snow to land in front of my nose, so that a successful Ram pays off if I am quick.
8. As any Player, I want spilled Snow to lie as a Snow Pile anyone can collect, so that a Ram starts a scramble (GDD 4.3).
9. As a Player with an empty Bucket, I want a Ram to cost me nothing, so that no phantom Pile appears.
10. As a developer building Gadgets, I want a host-side "Spill this Vehicle's Bucket at a point" call, so that Snowball reuses the Ram rule.
11. As a developer building DropOff and Hud, I want every Vehicle's Load readable on every peer in GDD units (0–100) and as "full", so that multipliers and the HUD can be built on it.
12. As a game designer, I want capacity, the steps-per-Load rate, the speed penalty, and the Spill share in a config asset, so that I balance without code.
13. As a developer, I want the Bucket rules covered by fast EditMode tests through one entry point.

## Implementation Decisions

- **Module:** `Gameplay/Bucket`, asmdef `PlowParty.Gameplay.Bucket`, references Snow, Vehicle, Infrastructure, Fusion, VContainer. Folders `Simulation`, `Network`, `Config`, `Tests`. Added to `AssembliesToWeave`. Feature index row `planned` → `active`.
- **Load unit:** Load is stored in whole Depth steps (`LoadSteps`); capacity is `Capacity × StepsPerLoad` steps. Load in GDD units is `LoadSteps / StepsPerLoad`, rounded down; "full" means `LoadSteps` equals the capacity in steps. Lossless: a Spill puts back exactly the steps it takes out.
- **Numbers (BucketConfig):** `Capacity` 100, `StepsPerLoad` 6 (fresh Snow ≈ 13 steps/m with the current Blade, ≈ 2.2 Load/m, GDD's "~2 per metre"), `MaxSpeedPenalty` 0.25, `SpillShare` 0.3.
- **Speed penalty:** linear, `1 − MaxSpeedPenalty × LoadSteps / CapacitySteps`. Closes GDD 11's "stepped or linear" for MVP; revisit at playtest.
- **Spill amount:** `ceil(LoadSteps × SpillShare)`, a product within float error of a whole number counting as that number, so a non-empty Bucket always loses something; steps that find no Cell (`SnowGrid.Spill` returns fewer) are lost, never returned to the Bucket.
- **Snow Pile placement on a Ram:** the midpoint of the Rammer's and the Victim's positions after the tick, right in front of the Rammer's nose.
- **Single test seam — `BucketRules`:** pure C#, built from `BucketSettings`; `CapacitySteps`, `FreeStepsFor(load)`, `Collect(load, steps)`, `SpillSteps(load)`, `SpeedMultiplier(load)`, `LoadUnits(load)`, `IsFull(load)`. Load passes in and out as `int` steps; the networked value is the only state.
- **Scrape-limit seam:** Snow declares `IScrapeLimit.LimitFor(NetworkVehicle)` in `Snow/Network`; `SnowGridDriver` passes it to `Scrape`, `SnowGridView` shares it across all pre-clear samples of a frame. `BucketRegistry` (Vehicle → `NetworkBucket` map) implements it; "Room" is avoided because the glossary reserves it.
- **Network:** `NetworkBucket` on `Vehicle.prefab` with `[Networked] int LoadSteps`; `Collect(steps)` and `TakeSpill()` that act only with state authority; each Load change writes the Vehicle's `SpeedMultiplier` (Bucket is that Modifier's only owner). `BucketHost` (VContainer entry point in `MatchScope`) subscribes to `Scraped` and `Rammed`, both raised only on the Host, and exposes `Spill(vehicle, point)` for Gadgets.
- **Config:** `BucketConfig` ScriptableObject → `BucketSettings`; asset `_Project/Configs/BucketConfig.asset`, registered in `RootLifetimeScope`.

## Testing Decisions

- Tests build `BucketRules` from small settings and assert only on return values. Assembly `PlowParty.Gameplay.Bucket.Tests`, names `Method_Condition_ExpectedResult`.
- Cover: capacity in steps; room shrinks with Load and never goes negative; collect clamps at capacity; spill rounds up, is zero for an empty Bucket, never exceeds Load; speed multiplier 1 empty, `1 − penalty` full, linear between; Load units round down; full only at capacity.
- Not unit-tested: `NetworkBucket`, `BucketHost`, Snow's use of `IScrapeLimit`; checked in Play Mode by driving into a full Bucket and Ramming via `eval`.

## Out of Scope

- Unloading and multipliers — DropOff.
- Snowball and Immunity — Gadgets (they call `BucketHost.Spill`).
- HUD Load gauge and Bucket visuals.
- Client prediction of Load.

## Further Notes

- Out-of-module edits: `Snow/Network/IScrapeLimit.cs` (new), `SnowGridDriver` and `SnowGridView` use it, Snow `CLAUDE.md`; `Vehicle.prefab` gets `NetworkBucket`, Vehicle `CLAUDE.md`; `MatchScope`, `RootLifetimeScope` (+ prefab field), `NetworkProjectConfig.fusion`; Bootstrap `CLAUDE.md`; two worktree gotchas in `client/CLAUDE.md`; `GLOSSARY.md` unchanged (Bucket, Load, Spill exist); GDD 11 row on the speed penalty marked decided.
- ADRs touched: ADR-0005 (Simulation free of Fusion), ADR-0012 ("Bucket owns the conversion").
