# Vehicle

Status: ready-for-agent

## Problem Statement

A Participant needs a Vehicle that drives the way a cartoon snowplow arcade should feel on a phone: responsive to a virtual stick, bouncy against walls and other Vehicles, and readable in a 4–6 Participant free-for-all. Every other gameplay feature — Bucket, Drop-Off Zone, Drifts, Loot, Gadgets, Bots — depends on Vehicles that move, collide, and Ram predictably on the Host and on every client. Today the client has no Gameplay code at all.

## Solution

A Vehicle feature in Gameplay whose rules are a deterministic, engine-free simulation stepped once per Fusion tick. Each tick, the simulation takes every Vehicle's state and its `VehicleInput`, integrates movement, resolves collisions against static map obstacles and against other Vehicles with a springy bounce, and reports Ram events when one Vehicle hits another in the side or rear. A thin Fusion adapter feeds networked state and input into the simulation and writes results back; a view plays the result back on screen. Players and Bots drive through the same `VehicleInput`, so a Vehicle cannot tell who controls it.

## User Stories

1. As a Player, I want my Vehicle to accelerate in the direction I push the left stick, so that driving feels direct on a touch screen.
2. As a Player, I want stick deflection to scale my target speed, so that I can creep precisely around the Drop-Off Zone or drive at full speed across the yard.
3. As a Player, I want my Vehicle to turn toward the stick direction at a limited turn rate, so that it feels like a vehicle and not a cursor.
4. As a Player, I want my Vehicle to coast to a stop when I release the stick, so that stopping feels natural instead of instant.
5. As a Player, I want my Vehicle to bounce noticeably off walls and obstacles, so that collisions feel cartoonish and fun rather than sticky.
6. As a Player, I want my Vehicle never to pass through walls or obstacles, so that the map's layout matters.
7. As a Player, I want two Vehicles that collide to push each other apart with a visible bounce, so that bumping into opponents is part of the play.
8. As a Player, I want ramming an opponent in the side or rear to count as a Ram, so that I can knock Snow out of their Bucket.
9. As a Player, I want a head-on collision not to count as a Ram against me, so that facing an attacker is a valid defence.
10. As a Player who Rams, I want my own Vehicle to bounce back a little, so that ramming has a cost and feels physical.
11. As a Player, I want the same pair of Vehicles to be unable to Ram each other again for about one second, so that two Vehicles stuck together do not pinball endlessly.
12. As a Player, I want my Vehicle to be slower when my Bucket is full, so that carrying a full Load is a real decision (GDD 4.1).
13. As a Player, I want a Turbo pickup or the Turbo Rocket Gadget to boost my speed for a short time, so that those items feel powerful.
14. As a Player, I want a Ram during a Turbo Rocket dash to hit harder, so that the Gadget's promise holds (GDD 7.2).
15. As a Player hit by Freeze, I want my Vehicle to stop and ignore my stick for the effect's duration, so that the effect is clear and fair.
16. As a Player hit by a snowball, I want my Vehicle to be knocked back, so that the hit is visible and felt.
17. As a Player, I want my Vehicle to stay still and ignore input during Countdown, so that nobody gets a head start (GDD 3.2).
18. As a Player on a laggy connection, I want my own Vehicle to respond immediately to my stick, so that the game feels local even though the Host is authoritative.
19. As a Player, I want other Vehicles to move smoothly on my screen, so that I can judge Rams and dodges.
20. As a Player, I want my Vehicle and Critter to face the direction it is moving, so that I can read where everyone is heading.
21. As a Bot, I want to drive with exactly the same `VehicleInput` as a Player, so that I am indistinguishable from a human and obey the same rules.
22. As a Bot, I want to read Vehicle state without touching networking, so that my decision logic stays testable.
23. As the Host, I want the simulation to produce identical results for identical inputs, so that prediction and reconciliation stay consistent with clients.
24. As the Host, I want each Ram reported once with rammer and victim, so that Bucket can apply the Spill exactly once.
25. As a game designer, I want every Vehicle number (max speed, acceleration, turn rate, drag, bounce, Ram angle, Ram cooldown, Turbo strength, knockback) in a config asset, so that I can tune feel without code changes.
26. As a game designer, I want all Vehicles to share identical stats, so that the game has no hero roster (GDD 1).
27. As a developer, I want the Vehicle rules covered by fast EditMode tests, so that tuning and refactors do not silently break driving or ramming.
28. As a developer building Bucket, Gadgets, and Bots, I want Vehicle to expose modifiers and Ram events rather than knowing about my feature, so that dependencies point one way.
29. As a developer, I want Vehicle spawned by Fusion to receive its dependencies through VContainer, so that it follows ADR-0004.

## Implementation Decisions

- **Module:** new feature module `Gameplay/Vehicle` with its own asmdef `PlowParty.Gameplay.Vehicle` and a module `CLAUDE.md`; its row in the feature index in `docs/architecture.md` moves from `planned` to `active`. Sub-folders `Simulation`, `Network`, `View`, `Config`, `Tests` per the feature anatomy.
- **Single test seam — `VehicleWorld.Tick`:** a pure C# step function in Simulation. Inputs: all Vehicle states, one `VehicleInput` per Vehicle, the static obstacle set, tick delta time, and the Vehicle config values. Outputs: the new Vehicle states and the list of Ram events produced this tick. No Fusion types, no `MonoBehaviour`, no `Time` or `Random`; identical inputs produce identical outputs.
- **Tick order:** integrate each Vehicle (steer, accelerate, drag, apply modifiers) → resolve Vehicle–obstacle contacts → resolve Vehicle–Vehicle contacts and detect Rams → advance cooldown and effect timers.
- **Geometry:** top-down 2D in the XZ plane. A Vehicle is a circle with position, velocity, and facing. Static obstacles are axis-aligned boxes and circles supplied by the map as data. Collision response reflects velocity along the contact normal with a configurable restitution, giving the springy bounce; penetration is always resolved so Vehicles never overlap obstacles or each other at the end of a tick.
- **Fallback:** if custom kinematics proves unworkable, Fusion Physics is the fallback named in GDD 5; switching would be recorded in an ADR.
- **`VehicleInput`:** a small value type with a 2D move vector (stick, magnitude 0–1) and a gadget-pressed flag. It is the only control surface of a Vehicle. Player input is collected by the Fusion input callback; Bot input is written on the Host. The gadget flag is carried for the Gadgets feature and ignored by Vehicle rules.
- **Ram rule:** a Vehicle–Vehicle contact counts as a Ram when the impact direction hits the victim's side or rear — the angle between the victim's facing and the direction from victim to rammer exceeds a configured threshold — and the rammer's speed along the contact normal exceeds a configured minimum. One Ram event per contact: rammer, victim, impact strength. A per-pair cooldown (default ~1 s) blocks further Rams between the same two Vehicles; the rammer also bounces back.
- **Modifiers, not dependencies:** Vehicle exposes per-Vehicle modifiers set by other features each tick: a speed multiplier (Bucket Load penalty, Turbo), an immobilised flag (Freeze, Countdown), a one-shot impulse (snowball knockback, Turbo Rocket dash), and a Ram-strength multiplier (Turbo Rocket). Vehicle never references Bucket, Gadgets, or Loot.
- **Ram consumers:** Ram events are published for other Gameplay features; Bucket turns them into a Spill. Vehicle does not change Load.
- **Network adapter:** a `NetworkBehaviour` per Vehicle holds the `[Networked]` state (position, velocity, facing, timers, modifiers, per-pair cooldowns) and in `FixedUpdateNetwork` reads input, builds the simulation input for the tick, calls `VehicleWorld.Tick` through a scene-level runner that owns the collection of active Vehicles, and writes results back. Clients predict their own Vehicle; the Host is authoritative. Visual interpolation happens in `Render`.
- **DI:** the Vehicle prefab is spawned by Fusion through the `MatchScope` network object provider (ADR-0004); the Vehicle config is registered as an instance and injected.
- **Config:** a `VehicleConfig` ScriptableObject holds every tunable number; one asset shared by all Vehicles.
- **Glossary:** Vehicle, Ram, Participant, Host, Critter are used as defined in `GLOSSARY.md`; any new term introduced during implementation (e.g. obstacle naming) is added there first.

## Testing Decisions

- **Good tests** drive `VehicleWorld.Tick` with constructed Vehicle states, inputs, and obstacles and assert only on the returned states and Ram events — positions, velocities, facing, event lists. They never inspect private fields or intermediate steps, so the integration and collision internals can change freely.
- **Module under test:** Vehicle Simulation only, via EditMode tests in the feature's own test asmdef referencing `PlowParty.Gameplay.Vehicle` and NUnit. Run with `unity test client --mode EditMode`.
- **Behaviours to cover:** acceleration toward stick and cap at max speed; stick magnitude scales speed; limited turn rate; coasting to rest without input; no movement while immobilised; speed multiplier; impulse; bounce off a box and a circle obstacle with no penetration; two Vehicles separate after contact; side and rear hits produce a Ram, head-on does not; slow touches below minimum speed do not; per-pair cooldown blocks a repeat Ram and expires after its duration; rammer bounces back; determinism — the same inputs over many ticks give bit-identical results.
- **Not unit-tested:** the `NetworkBehaviour` adapter and the view; verified manually in Play Mode, with a PlayMode test added later if the adapter grows logic.
- **Prior art:** none yet — this is the first EditMode test suite in the client and sets the pattern (test asmdef per feature, `Method_Condition_ExpectedResult` naming from `docs/coding-standards.md`).

## Out of Scope

- Bucket Load, Spill, and Snow Piles — Vehicle only emits Ram events and accepts a speed multiplier.
- Gadget behaviour, targeting UI, and Immunity — Vehicle only provides the modifiers they drive.
- Bot decision-making — Bots only need to produce `VehicleInput`.
- The virtual joystick UI and HUD (Hud feature); a keyboard/gamepad stand-in for the editor is allowed for manual testing.
- Snow Grid clearing under the Vehicle.
- Match state machine; Countdown only sets the immobilised modifier.
- Map content beyond a test arena with a few obstacles.
- Vehicle art, Critter cosmetics, and VFX beyond facing and basic bounce feedback.

## Further Notes

- GDD sources: 3.4 (controls), 5 (physics and Ram), 7.2 (Turbo Rocket, Freeze, snowball effects), 8 (shared input source for Bots).
- Open playtest questions stay in config, not code: linear vs stepped Load speed penalty (GDD 11) is decided by Bucket and arrives here only as a multiplier.
- ADRs touched: ADR-0004 (DI for Fusion-spawned objects), ADR-0005 (vertical feature modules; Simulation free of Fusion types).
