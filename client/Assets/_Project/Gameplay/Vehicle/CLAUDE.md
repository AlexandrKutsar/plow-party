# Vehicle

The snowplow every Participant drives: movement, collisions, and Rams (GDD 3.4, 5). Rules live in `Simulation/` as deterministic C# stepped once per Fusion tick; every Vehicle has identical stats.

## Entry points

- `VehicleWorld` — owns all Vehicles of a Match. `Add` a `VehicleState`, `SetControl` with a `VehicleInput` and `VehicleModifiers` each tick, `Tick(deltaTime)`, read back with `GetVehicle` and `Rams`. The single test seam.
- `VehicleInput` — stick vector (clamped to length 1) and gadget flag; the only control surface, identical for Players and Bots.
- `VehicleModifiers` — how other features steer a Vehicle without Vehicle knowing them: speed multiplier (product of Bucket Load, Snow Pile, later Turbo), immobilised (Freeze, Countdown), one-shot impulse (snowball, Turbo Rocket), Ram strength multiplier.
- `RamEvent` — rammer index, victim index, strength; `VehicleWorld.Rams` holds this tick's events and is cleared on the next `Tick`. Bucket turns them into a Spill.
- `RamWatch` — turns a Vehicle's networked Ram counts into a `RamRole` (None, Rammer, Victim) once per new Ram: the first `Observe` takes a baseline, a rise in `TimesRammed` reports Victim (it wins when both rise), a rise in `RamsDealt` reports Rammer; it remembers the highest counts seen, so a value that drops and returns never fires twice. `Reset` when the watched Vehicle object changes.
- `VehicleArena` — static obstacles of the map: axis-aligned boxes and circles, passed to `VehicleWorld` at construction. `Contains(point)` tells whether a point lies inside any obstacle; Snow masks Cells with it.
- `VehicleSettings` — tunable numbers, filled from the config asset.
- `SegmentMath` — allocation-free closest points for point–segment, segment–segment, and segment–box; the geometry behind capsule contacts.

## Rules worth knowing

- Turning is capped by `TurnRateDegrees`; speed follows stick magnitude × `MaxSpeed` × speed multiplier, approached at `Acceleration` while the stick is held and `Deceleration` when released.
- Immobilised zeroes velocity and freezes facing and position.
- Vehicles are capsules in the XZ plane (Vector2 x = world x, y = world z): a segment of length `2 × HalfLength` along `Forward`, swept by `Radius`. `HalfLength = 0` is a circle. Contacts and Ram normals come from the closest points of the capsule axes, so a hit on the nose is head-on and a hit on the flank is a Ram. The capsule matches the model footprint (1.08 × 1.62 m for the current Vehicle: `Radius` 0.54, `HalfLength` 0.27); when the model changes, re-measure and update the config. Tick order: decay Ram cooldowns → integrate every Vehicle → one Vehicle-pair pass that may produce Rams → alternate obstacle and pair passes until nothing overlaps (capped at `MaxContactIterations`) → a final obstacle pass, so a Vehicle never ends a tick inside a wall or another Vehicle.
- Contacts reflect only the approaching velocity component, scaled by `Restitution`; tangential speed is kept, so Vehicles slide along walls.
- A Ram needs: the faster of the two approaching at `RamMinSpeed` or more, the hit landing on the victim's side or rear (angle between victim's forward and the direction to the rammer ≥ `RamMinAngleDegrees`), and the pair's cooldown at zero. A Ram adds `RamKnockback × RamStrengthMultiplier` to the victim and `RamRecoil` back to the rammer, then sets the pair cooldown to `RamCooldown`.
- Pair cooldowns are state the network adapter must sync: `GetRamCooldown` / `SetRamCooldown`, indexed through `VehiclePairs`.
- An impulse is consumed by the tick that applies it; persistent modifiers stay until the next `SetControl`.

## Network and view

- `NetworkVehicle` — one per Participant, spawned by `VehicleSpawner` on the Host into the first free Slot (which picks its Spawn Point), with the Player as input authority. Holds `[Networked]` state, last input, and Modifiers; `Render` interpolates the transform. It does no simulation itself.
- Modifiers are how other features drive a Vehicle on the Host: call `SetSpeedFactor(source, factor)`, set `IsImmobilised`, `RamStrengthMultiplier`, or call `AddImpulse`. The pending Impulse is networked and cleared by the tick that applies it, so it applies once even across resimulation. Each Modifier has one owner; if two features need the same one, split it rather than letting them overwrite each other. Owners: Match owns `IsImmobilised` (locked outside Playing; Freeze will have to share it through a split).
- Speed is split that way: `[Networked] NetworkArray<float>` holds one factor per `VehicleSpeedSource` (`Load` owned by Bucket, `SnowPile` owned by Snow), all 1 at spawn, and the read-only `SpeedMultiplier` passed to `VehicleModifiers` is their product. A new source (Turbo) adds an enum value at the end, moves `SpeedSourceCount` to it (the networked capacity), and writes only its own factor. The product lives in `NetworkVehicle` rather than Simulation because the factors are networked state read straight into `VehicleModifiers`; Simulation sees one multiplier as before.
- `VehicleWorldDriver` — scene `NetworkObject` that steps the whole `VehicleWorld` once per tick: reads every registered `NetworkVehicle`, takes each Player's input via `TryGetInputForPlayer`, ticks, writes back, and reports Rams through `VehicleRegistry.Rammed` on the Host only, so each Ram is reported once. Ram cooldowns are stored per Slot pair, so a Vehicle joining or leaving never shifts another pair's cooldown; a new Vehicle starts with its Slot's cooldowns cleared.
- Decision: clients predict every Vehicle, not only their own (the driver and every `NetworkVehicle` call `SetIsSimulated`). Collisions need all Vehicles in one step; predicting only the local one would let it drive through opponents until the Host corrects it. Remote Players' input is unknown on a client, so their prediction uses the last confirmed `LastMove`. Bots will write `LastMove` on the Host.
- Spawn and despawn are not rolled back: a resimulated tick runs with the current set of Vehicles.
- `VehicleSpawner.RespawnAll(runner)` — Host only: despawns every Player's Vehicle and spawns a fresh one in the same Slot at its Spawn Point. Match calls it to start the next Match, so per-Vehicle state on other features (Load, Score) resets with the object.
- Ram counts: `[Networked] TimesRammed` and `RamsDealt` on `NetworkVehicle`, incremented by `VehicleWorldDriver` on the Host for each Ram (`CountRamAsVictim` / `CountRamAsRammer`). They are the networked Ram signal: every peer, clients included, sees a Ram as a rise in these counts once the Host's state arrives, and reads them with `RamWatch`. Clients never write them, so a mispredicted Ram never reaches them; a fresh Vehicle (respawn) starts at zero.
- `VehicleRegistry` — Vehicles of the Match ordered by Slot, which fixes their index in `VehicleWorld`; `Rammed` (a `VehicleRam`, Host only) is how Bucket learns about Rams; `TryGetLocal` returns this peer's Vehicle (input authority), used by CameraRig.
- `VehicleArenaReader` — builds the `VehicleArena` from the arena's `BoxCollider`s (boxes) and `CapsuleCollider` / `SphereCollider`s (circles); colliders are assumed axis-aligned.
- `VehicleInputPoller` — fills `VehicleNetworkInput` from the Input System actions `Player/Move` (stick) and `Player/Attack` (gadget). Hud's on-screen stick drives a virtual `<Gamepad>/leftStick`, which `Player/Move` already binds, so touch needs no code here.
- `Prefabs/Vehicle.prefab` — the networked root (`NetworkObject`, `NetworkVehicle`) plus a `Model` child: the nested visual prefab `Art/Vehicles/Vehicle/V_Vehicle.prefab` with separate wheels, `Bucket`, and a `CritterSeat` anchor for the Critter. The root also carries Bucket's `NetworkBucket` and DropOff's `NetworkDelivery`; Bucket owns the `Load` speed factor, Snow the `SnowPile` one. The model is purely visual; collision uses `Radius`, not the mesh.
- `VehicleConfig` — the ScriptableObject type behind `VehicleSettings`; its asset is `_Project/Configs/VehicleConfig.asset`, registered in `RootLifetimeScope`.

## Depends on

Simulation: nothing but `UnityEngine` math. Network: Fusion, VContainer, Input System, Infrastructure. Bucket, Gadgets, and Bots depend on this module, never the reverse. The assembly is in Fusion's `AssembliesToWeave`.
