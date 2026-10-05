# Vehicle

The snowplow every Participant drives: movement, collisions, and Rams (GDD 3.4, 5). Rules live in `Simulation/` as deterministic C# stepped once per Fusion tick; every Vehicle has identical stats.

## Entry points

- `VehicleWorld` — owns all Vehicles of a Match. `Add` a `VehicleState`, `SetControl` with a `VehicleInput` and `VehicleModifiers` each tick, `Tick(deltaTime)`, read back with `GetVehicle` and `Rams`. The single test seam.
- `VehicleInput` — stick vector (clamped to length 1) and gadget flag; the only control surface, identical for Players and Bots.
- `VehicleModifiers` — how other features steer a Vehicle without Vehicle knowing them: speed multiplier (Bucket Load, Turbo), immobilised (Freeze, Countdown), one-shot impulse (snowball, Turbo Rocket), Ram strength multiplier.
- `RamEvent` — rammer index, victim index, strength; `VehicleWorld.Rams` holds this tick's events and is cleared on the next `Tick`. Bucket turns them into a Spill.
- `VehicleArena` — static obstacles of the map: axis-aligned boxes and circles, passed to `VehicleWorld` at construction.
- `VehicleSettings` — tunable numbers, filled from the config asset.

## Rules worth knowing

- Turning is capped by `TurnRateDegrees`; speed follows stick magnitude × `MaxSpeed` × speed multiplier, approached at `Acceleration` while the stick is held and `Deceleration` when released.
- Immobilised zeroes velocity and freezes facing and position.
- Vehicles are circles of `Radius` in the XZ plane (Vector2 x = world x, y = world z). Tick order: integrate every Vehicle → resolve Vehicle pairs → resolve obstacles last, so a Vehicle never ends a tick inside a wall even when pushed there.
- Contacts reflect only the approaching velocity component, scaled by `Restitution`; tangential speed is kept, so Vehicles slide along walls.
- A Ram needs: the faster of the two approaching at `RamMinSpeed` or more, the hit landing on the victim's side or rear (angle between victim's forward and the direction to the rammer ≥ `RamMinAngleDegrees`), and the pair's cooldown at zero. A Ram adds `RamKnockback × RamStrengthMultiplier` to the victim and `RamRecoil` back to the rammer, then sets the pair cooldown to `RamCooldown`.
- Pair cooldowns are state the network adapter must sync: `GetRamCooldown` / `SetRamCooldown`.
- An impulse is consumed by the tick that applies it; persistent modifiers stay until the next `SetControl`.

## Network and view

- `NetworkVehicle` — one per Participant, spawned by `VehicleSpawner` on the Host with the Player as input authority. Holds `[Networked]` position, velocity, forward, and `LastMove`; `Render` interpolates the transform. It does no simulation itself.
- `VehicleWorldDriver` — scene `NetworkObject` that steps the whole `VehicleWorld` once per tick: reads every registered `NetworkVehicle`, takes each Player's input via `TryGetInputForPlayer`, ticks, writes back, and reports Rams through `VehicleRegistry.Rammed` on forward ticks only. Both it and every `NetworkVehicle` call `SetIsSimulated`, so clients predict all Vehicles and the Host corrects them.
- Remote Players' input is unknown on a client, so prediction uses their last confirmed `LastMove`. Bots will write `LastMove` on the Host.
- `VehicleRegistry` — Vehicles of the Match ordered by `NetworkId`, which fixes their index in `VehicleWorld`; `Rammed` is how Bucket will learn about Rams.
- `VehicleArenaReader` — builds the `VehicleArena` from the arena's `BoxCollider`s (boxes) and `CapsuleCollider` / `SphereCollider`s (circles); colliders are assumed axis-aligned.
- `VehicleInputPoller` — fills `VehicleNetworkInput` from the Input System action `Player/Move`.
- `VehicleConfig` — the ScriptableObject behind `VehicleSettings`, registered in `RootLifetimeScope`.

## Depends on

Simulation: nothing but `UnityEngine` math. Network: Fusion, VContainer, Input System, Infrastructure. Bucket, Gadgets, and Bots depend on this module, never the reverse. The assembly is in Fusion's `AssembliesToWeave`.
