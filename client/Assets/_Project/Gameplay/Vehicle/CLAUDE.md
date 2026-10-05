# Vehicle

The snowplow every Participant drives: movement, collisions, and Rams (GDD 3.4, 5). Rules live in `Simulation/` as deterministic C# stepped once per Fusion tick; every Vehicle has identical stats.

## Entry points

- `VehicleWorld` — owns all Vehicles of a Match. `Add` a `VehicleState`, `SetControl` with a `VehicleInput` and `VehicleModifiers` each tick, `Tick(deltaTime)`, read back with `GetVehicle`. The single test seam.
- `VehicleInput` — stick vector (clamped to length 1) and gadget flag; the only control surface, identical for Players and Bots.
- `VehicleModifiers` — how other features steer a Vehicle without Vehicle knowing them: speed multiplier (Bucket Load, Turbo), immobilised (Freeze, Countdown), one-shot impulse (snowball, Turbo Rocket), Ram strength multiplier.
- `VehicleSettings` — tunable numbers, filled from the config asset.

## Rules worth knowing

- Turning is capped by `TurnRateDegrees`; speed follows stick magnitude × `MaxSpeed` × speed multiplier, approached at `Acceleration` while the stick is held and `Deceleration` when released.
- Immobilised zeroes velocity and freezes facing and position.
- An impulse is consumed by the tick that applies it; persistent modifiers stay until the next `SetControl`.

## Depends on

Nothing but `UnityEngine` math. Bucket, Gadgets, and Bots depend on this module, never the reverse.
