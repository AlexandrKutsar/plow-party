# DropOff

Status: ready-for-agent

## Problem Statement

Vehicles fill their Buckets, but nothing turns Load into Score: there is no Drop-Off Zone, no unloading, no Multiplier, and no Score anywhere in the Match. The loop "collect, carry, deliver" (GDD 4.1–4.3) stops halfway. On top of that the Snow Grid covers the whole Arena, so the ground around the zone would be ordinary Snow that Vehicles keep collecting while they stand there to unload.

## Solution

A DropOff feature in Gameplay. One Drop-Off Zone sits in the centre of the Arena as a scene gameplay prefab that nests the cauldron visual. Its rules live in one pure C# type, `DropOffRules`: whether a point is in the zone or in the Snow-Free Area, which Multiplier a Load locks, and one `Tick` that advances a Delivery (start, unload at the configured rate, credit Score, end). A `NetworkDelivery` on the Vehicle prefab holds the networked Score and the current Delivery and runs `Tick` on the Host each Fusion tick. A host-side entry point, `DropOffHost`, interrupts a Delivery when Bucket reports a Spill. Score is read on every peer through `IScoreboard`. Snow gains a generic `ISnowFreeArea` seam that DropOff implements with a circle around the zone, so Snow never references DropOff.

## User Stories

1. As a Player, I want to unload my Bucket by standing in the Drop-Off Zone, so that the Snow I collected becomes Score.
2. As a Player, I want unloading to be gradual (a full Bucket in about 1.5 s), so that the zone is a place where I am exposed for a moment (GDD 4.2).
3. As a Player, I want the Multiplier fixed by my Load at the moment I enter the zone (1–50 ×1, 51–99 ×1.5, full ×2), so that bringing a full Bucket pays off.
4. As a Player, I want the Multiplier to stay the same while my Load drops during unloading, so that I get the rate I earned on entry.
5. As a Player who leaves the zone mid-Delivery, I want the part already unloaded credited at the locked Multiplier, so that nothing I delivered is lost.
6. As a Player Rammed in the zone, I want to Spill as usual and keep the Score for what I had already unloaded, so that the zone is the main point of conflict (GDD 4.2).
7. As a Player still in the zone after a Spill, I want unloading to continue with a Multiplier relocked by my remaining Load, so that I do not have to drive out and back in.
8. As a Player with an empty Bucket, I want entering the zone to do nothing, so that no Delivery or Multiplier appears.
9. As a Player, I want the ground around the zone free of Snow (no initial cover, no Regrowth, no Blizzard refill), so that the zone reads as a clearing and standing there collects nothing by accident.
10. As a Player, I want Snow Piles from a Spill to land in that clearing like anywhere else, so that a Ram in the zone still starts a scramble.
11. As a Player, I want Blizzard Piles to avoid the clearing, so that a Blizzard still pulls me away from the zone (GDD 4.4).
12. As a developer building Match and Hud, I want every Vehicle's Score, locked Multiplier, and "is delivering" readable on every peer through a read-only interface, so that the scoring table and announcements can be built on it.
13. As a game designer, I want the zone radius, the Snow-Free radius, the unload time, and the Multiplier tiers in a config asset, so that I balance without code.
14. As a developer, I want the DropOff rules covered by fast EditMode tests through one entry point.

## Implementation Decisions

- **Module:** `Gameplay/DropOff`, asmdef `PlowParty.Gameplay.DropOff`, references Bucket, Snow, Vehicle, Fusion, VContainer. Folders `Simulation`, `Network`, `Config`, `Prefabs`, `Tests`. Added to `AssembliesToWeave`. Feature index row `planned` → `active`.
- **Zone shape:** a circle of `ZoneRadius` around the `DropOffZone` transform; a Vehicle is in the zone when its centre is inside. The cauldron itself is a circle Obstacle (a `SphereCollider` on the gameplay prefab, which sits under the scene's `Arena` so `VehicleArenaReader` picks it up), so Vehicles stand on a ring around it.
- **Numbers (DropOffConfig):** `ZoneRadius` 3.5 m (a 1.5 m ring between the cauldron Obstacle and the edge), `SnowFreeRadius` 5 m (zone plus the Blade's reach, ≈ 0.85 m offset + half its depth and width, so a Vehicle standing anywhere in the zone scrapes no fresh Snow and cannot start a stray ×1 Delivery; found in Play Mode with a 4.5 m radius), `FullUnloadDuration` 1.5 s, `BaseMultiplier` 1, tiers `51 → ×1.5`, `100 → ×2`. Cauldron obstacle radius 1.5 m (bonfire 3.4 m wide).
- **Unload granularity:** in Depth steps, Bucket's storage unit, so nothing is rounded twice. Rate = Bucket capacity in steps / `FullUnloadDuration` (600 / 1.5 = 400 steps/s). Steps due are computed from the Delivery's elapsed time (`floor(elapsed × rate)` minus already delivered), so the amount does not depend on the tick rate.
- **Score crediting:** per tick, as the difference `floor(delivered × Multiplier / StepsPerLoad)` before and after; Score therefore rises smoothly, an interruption needs no separate "credit" step, and a Delivery rounds down only once in total (51 Load at ×1.5 → 76).
- **Delivery lifecycle:** starts on the first tick a Vehicle is in the zone with Load > 0 and no active Delivery; the Multiplier is locked from the Load at that tick (tiers by Load units, any Load under one unit counts as the base tier; Load 0 starts nothing). Ends when the Bucket is empty, the Vehicle leaves the zone, or a Spill interrupts it. A Vehicle still in the zone after an interruption starts a new Delivery on the next tick with the Multiplier of its remaining Load. Snow collected in the zone (a Pile) joins the running Delivery at its locked Multiplier.
- **Single test seam — `DropOffRules`:** pure C#, built from `DropOffSettings` and Bucket's `BucketSettings`; `IsInZone`, `IsSnowFree`, `MultiplierFor(loadSteps)`, `Tick(delivery, inZone, loadSteps, deltaTime)` returning `DeliveryTick` (next `Delivery`, unloaded steps, Score gained). `Delivery` is a value: locked Multiplier, delivered steps, elapsed time; `Multiplier 0` means none.
- **Network:** `NetworkDelivery` on `Vehicle.prefab` beside `NetworkBucket`: `[Networked] Score`, `Multiplier`, delivered steps, elapsed. `FixedUpdateNetwork` acts only with state authority: reads the Delivery, calls `Tick`, unloads through the new `NetworkBucket.Unload(steps)`, adds Score, writes back. `Interrupt()` clears the Delivery. `DeliveryRegistry` maps Vehicle → `NetworkDelivery` and implements `IScoreboard.ScoreOf(vehicle)`. `DropOffHost` (entry point in `MatchScope`) subscribes to the new `BucketHost.Spilled` event.
- **Events for Hud/announcements:** none for now. Host-side C# events would not reach clients; Hud reads the networked `Score`, `Multiplier`, and `IsDelivering` on every peer (via `ChangeDetector` when it needs edges).
- **Snow-free seam:** Snow declares `ISnowFreeArea.Contains(point)` in `Snow/Simulation`; `SnowGrid` takes it at construction and marks those Cells snow-free: Depth 0 at start, skipped by Regrowth and Blizzard fronts, skipped when placing Blizzard Piles; Scrape and Spill treat them like any Cell, so Piles land and are collected there. `SnowGridDriver` injects it. DropOff's `DropOffSnowFreeArea` implements it with the `SnowFreeRadius` circle around `DropOffZone`. The View needs nothing: it copies the Host's words, and pre-clear only lowers Depth.
- **Bucket additions:** `BucketRules.UnloadSteps(load, steps)`, `NetworkBucket.Unload(steps)` (state authority only, updates the speed Modifier), `BucketHost.Spilled` (raised after a non-empty Spill, Host only).
- **Config:** `DropOffConfig` ScriptableObject → `DropOffSettings`; asset `_Project/Configs/DropOffConfig.asset`, registered in `RootLifetimeScope`.
- **Prefab:** `Gameplay/DropOff/Prefabs/DropOffZone.prefab`: root with `DropOffZone` and the cauldron `SphereCollider`, nested `Art/Environment/DropOffZone/V_DropOffZone.prefab` as `Model`. Not a `NetworkObject`: it holds no state.

## Testing Decisions

- Tests build `DropOffRules` from small settings and assert only on return values. Assembly `PlowParty.Gameplay.DropOff.Tests`, names `Method_Condition_ExpectedResult`.
- Cover: zone and Snow-Free circles (inside, edge, outside); Multiplier tiers at 0, below one unit, 1, 50, 51, 99, one step short of full, full, tiers given out of order; Tick outside the zone, entering empty, locking at entry, keeping the lock as Load falls, unload rate, a full Bucket over the full duration (all steps, ×2 Score, done within the configured duration), Load running out, leaving midway, rounding once per Delivery, zero duration, relock after interruption.
- Snow: snow-free Cells start cleared, never regrow, stay cleared after a Blizzard, take a Spill, give up a Pile to a Scrape; Blizzard Piles avoid them.
- Bucket: `UnloadSteps` within Load, beyond Load, negative.
- Not unit-tested: `NetworkDelivery`, `DropOffHost`, `DeliveryRegistry`, `DropOffSnowFreeArea`; checked in Play Mode.

## Out of Scope

- HUD score display, Multiplier popup, announcements — Hud.
- Match timer and Match Result table — Match (consumes `IScoreboard`).
- Several zones, moving zones (GDD 4.2, post-MVP).
- Zone ring visual on the ground; the clearing marks it for now.
- Client prediction of unloading.

## Further Notes

- Out-of-module edits: Snow (`ISnowFreeArea`, `SnowGrid`, `SnowGridDriver`, tests, `CLAUDE.md`); Bucket (`UnloadSteps`, `Unload`, `Spilled`, tests, `CLAUDE.md`); Vehicle (`Vehicle.prefab` gets `NetworkDelivery`, `CLAUDE.md`); Bootstrap (`MatchScope`, `RootLifetimeScope` + prefab field, asmdef, `CLAUDE.md`); `Match.unity` (zone instance under `Arena`); `NetworkProjectConfig.fusion`; `GLOSSARY.md` (Delivery, Snow-Free Area); GDD 4.2 one line on the clearing.
- ADRs touched: ADR-0005 (Simulation free of Fusion), ADR-0008 (visual vs gameplay prefab), ADR-0012 (Host-only Snow simulation; the snow-free mask lives only in the Host's grid like the Arena mask).
