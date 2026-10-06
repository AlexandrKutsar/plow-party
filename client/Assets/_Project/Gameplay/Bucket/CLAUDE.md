# Bucket

The Vehicle's snow container: Load filled by the Blade, capped at capacity, slowing the Vehicle as it fills, and Spilled as a Snow Pile when Rammed (GDD 4.1, 4.3). Rules live in `Simulation/` as deterministic C#; the Host applies them as Snow and Vehicle report events. Spec: `.scratch/bucket/spec.md`.

## Entry points

- `BucketRules` — the single test seam. Built from `BucketSettings`; Load passes in and out as whole Depth steps: `CapacitySteps`, `RoomFor(load)`, `Collect(load, steps)`, `SpillSteps(load)`, `SpeedMultiplier(load)`, `LoadUnits(load)`, `IsFull(load)`. Holds no state.
- `BucketSettings` — tunable numbers, filled from `BucketConfig`.

## Rules worth knowing

- Load is stored in Depth steps, so Snow and Bucket exchange exact amounts. Capacity is `Capacity × StepsPerLoad` steps. Load in GDD units (0–100) is steps / `StepsPerLoad`, rounded down; full means the stored steps reach capacity.
- `StepsPerLoad` 6 is tuned to the current Blade (1.08 m wide over 0.5 m Cells of Depth 3 ≈ 13 steps per metre of fresh Snow ≈ 2.2 Load per metre, GDD's "~2"). Changing the Blade, Cell size, or `FullDepth` changes the fill rate.
- Speed penalty is linear: `1 − MaxSpeedPenalty × Load / capacity`. GDD 11 left linear vs stepped to playtest; MVP is linear.
- A Spill takes `SpillShare` of the Load rounded up (a float product within rounding error of a whole number counts as that number), so a non-empty Bucket always loses at least one step. Steps the grid cannot place are lost.

## Network

- `NetworkBucket` — on `Vehicle.prefab` beside `NetworkVehicle`. `[Networked] LoadSteps` is the only state; `Load`, `IsFull`, `Room` read it on every peer. Host-only `Collect` and `TakeSpill` change it and write the Vehicle's `SpeedMultiplier`; Bucket is that Modifier's only owner.
- `BucketRegistry` — maps each `NetworkVehicle` to its `NetworkBucket`; implements Snow's `IBladeRoom`, so the Host's scrape and the local View's pre-clear both stop at the Bucket's room. A Vehicle without a Bucket has no room.
- `BucketHost` — VContainer entry point in `MatchScope`. Subscribes to `SnowGridDriver.Scraped` (fills the Bucket) and `VehicleRegistry.Rammed` (Spills the Victim's Bucket at the midpoint of Rammer and Victim, in front of the Rammer's nose); both events fire only on the Host. `Spill(vehicle, point)` is the host-side call Gadgets (Snowball) use.
- `BucketConfig` — the ScriptableObject behind `BucketSettings`; asset `_Project/Configs/BucketConfig.asset`, registered in `RootLifetimeScope`.

## Depends on

Snow (`SnowGridDriver`, `IBladeRoom`, `VehicleScrape`) and Vehicle (`NetworkVehicle`, `VehicleRegistry`, `VehicleRam`), Fusion, VContainer. Snow and Vehicle never reference Bucket. The assembly is in Fusion's `AssembliesToWeave`. Unloading and multipliers belong to DropOff; the Load gauge to Hud.
