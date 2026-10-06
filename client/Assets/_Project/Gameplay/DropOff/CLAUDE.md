# DropOff

The Drop-Off Zone in the Arena centre: a Vehicle standing in it unloads its Bucket in a Delivery, earns Score at the Multiplier locked when the Delivery started, and keeps the Score for whatever was unloaded when the Delivery is cut short (GDD 4.2). Also owns the Snow-Free Area around the zone. Rules live in `Simulation/` as deterministic C#; the Host steps them once per Fusion tick. Spec: `.scratch/dropoff/spec.md`.

## Entry points

- `DropOffRules` — the single test seam. Built from `DropOffSettings` and Bucket's `BucketSettings`; Load passes in as Depth steps. `IsInZone(centre, point)`, `IsSnowFree(centre, point)`, `MultiplierFor(loadSteps)`, and `Tick(delivery, inZone, loadSteps, deltaTime)` returning a `DeliveryTick` (next `Delivery`, unloaded steps, Score gained). Holds no state.
- `Delivery` — value of a running Delivery: locked Multiplier, delivered steps, elapsed seconds. `Delivery.None` (Multiplier 0) means none.
- `DropOffSettings` / `MultiplierTier` — tunable numbers, filled from `DropOffConfig`.

## Rules worth knowing

- A Delivery starts on the first tick a Vehicle's centre is within `ZoneRadius` with Load > 0 and none running. The Multiplier comes from the Load units at that tick: the tier with the highest `MinLoad` reached, else `BaseMultiplier` (1–50 ×1, 51–99 ×1.5, 100 ×2). Load 0 starts nothing.
- Unloading runs in Depth steps at Bucket capacity / `FullUnloadDuration` per second (600 steps in 1.5 s). Steps due follow from the Delivery's elapsed time, so the result does not depend on the tick rate; a `FullUnloadDuration` of 0 empties the Bucket in one tick.
- Score is credited every tick as the growth of `floor(delivered × Multiplier / StepsPerLoad)`, so it rises during the Delivery and a Delivery rounds down once overall. Leaving the zone or a Spill simply ends the Delivery; what was unloaded is already credited.
- A Vehicle still in the zone after a Spill starts a new Delivery next tick with the Multiplier of its remaining Load. Snow picked up in the zone joins the running Delivery at its locked Multiplier.
- The Snow-Free Area is a circle of `SnowFreeRadius` (5 m) around the zone (3.5 m). The margin is the Blade's reach in front of a Vehicle: with less, a Vehicle at the zone edge facing out scrapes fresh Snow, which immediately starts a new ×1 Delivery. Changing the zone radius or the Blade means re-checking it.

## Network

- `DropOffZone` — `MonoBehaviour` on the zone's gameplay prefab; only exposes `Centre` (its transform on the XZ plane). Registered in `MatchScope` with `RegisterComponentInHierarchy`.
- `NetworkDelivery` — on `Vehicle.prefab` beside `NetworkBucket`. `[Networked] Score`, `Multiplier` (locked, 0 when idle), delivered steps, elapsed; `IsDelivering`. `FixedUpdateNetwork` acts only with state authority: `Tick`, `NetworkBucket.Unload`, add Score, write back. `Interrupt()` clears the Delivery.
- `DeliveryRegistry` — maps each `NetworkVehicle` to its `NetworkDelivery` on every peer; implements `IScoreboard`.
- `IScoreboard` — `ScoreOf(vehicle)`, the read-only Score view Match and Hud consume. Hud reads `Multiplier` / `IsDelivering` from `NetworkDelivery` through `DeliveryRegistry.TryGet`; there are no C# events, since Host-side events would not reach clients.
- `DropOffHost` — VContainer entry point in `MatchScope`; interrupts a Vehicle's Delivery on `BucketHost.Spilled` (Host only).
- `DropOffSnowFreeArea` — implements Snow's `ISnowFreeArea` with the Snow-Free circle around `DropOffZone`; registered in `MatchScope` as `ISnowFreeArea`. Snow queries it once, when `SnowGridDriver` builds the grid on spawn.
- `DropOffConfig` — the ScriptableObject behind `DropOffSettings`; asset `_Project/Configs/DropOffConfig.asset`, registered in `RootLifetimeScope`.
- `Prefabs/DropOffZone.prefab` — root with `DropOffZone` and a `SphereCollider` (radius 1.5 m, the cauldron Obstacle), nesting `Art/Environment/DropOffZone/V_DropOffZone.prefab` as `Model`. No `NetworkObject`: it has no state. In `Match.unity` the instance sits under `Arena` at the origin, so `VehicleArenaReader` reads the collider as a circle Obstacle.

## Depends on

Bucket (`BucketSettings`, `NetworkBucket`, `BucketHost`), Snow (`ISnowFreeArea`), Vehicle (`NetworkVehicle`, `PlaneProjection`), Fusion, VContainer. None of them references DropOff. The assembly is in Fusion's `AssembliesToWeave`. Score display and the Match Result belong to Hud and Match.
