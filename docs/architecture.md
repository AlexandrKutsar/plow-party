# Architecture — client

Scope: the Unity client in `client/`. The backend gets its own document when it starts.

## Modules and dependency direction

```
                 Bootstrap
                /    |    \
             Meta    |   Gameplay
                \    |    /
             Infrastructure
                     |
                  Shared
```

Arrows point down: a module references only modules below it, never sideways across the Gameplay/Meta line and never up. Asmdef references enforce this at compile time — a missing reference is a compile error, which is the point.

| Area | Folder | Owns | Allowed references |
|---|---|---|---|
| Shared | `_Project/Shared/` | Plain types crossing the Gameplay/Meta line (`MatchResult`, `PlayerId`) | none |
| Infrastructure | `_Project/Infrastructure/` | Engine and network plumbing: scene loading, backend HTTP client, persistence, network object provider | Shared, VContainer, UniTask, Fusion |
| Meta | `_Project/Meta/<Feature>/` | Everything outside a match: account, lobby, session/matchmaking, tournament | Shared, Infrastructure, other Meta features |
| Gameplay | `_Project/Gameplay/<Feature>/` | Everything inside a match, from Countdown to Results | Shared, Infrastructure, other Gameplay features |
| Bootstrap | `_Project/Bootstrap/` | Lifetime scopes and composition; the only module that sees everything | all |

Asmdef naming: `PlowParty.<Area>` or `PlowParty.<Area>.<Feature>`; namespaces match. Feature-to-feature references inside an area are allowed but must stay acyclic; prefer depending on another feature's interface over its concrete types.

## Gameplay/Meta boundary

Gameplay knows nothing about menus, accounts, or the backend. The two sides meet at exactly two points:

1. **Into a match** — `Meta/Session` starts a Fusion `NetworkRunner` session (quick play or room code, waits for players, fills with bots) and loads the Match scene; `MatchScope` takes over from Countdown.
2. **Out of a match** — `Gameplay/Match` publishes a `MatchResult` (in Shared); Meta submits it to the backend and returns to the menu.

## Lifetime scopes (VContainer)

| Scope | Lives in | Lifetime | Registers |
|---|---|---|---|
| `RootLifetimeScope` | prefab referenced by `VContainerSettings` | whole app | configs (ScriptableObjects), Infrastructure services, account |
| `MenuScope` | Menu scene | menu visit | lobby, session, tournament screens |
| `MatchScope` | Match scene | one Fusion session | Gameplay systems, `INetworkObjectProvider`, HUD |

Scene flow: `Boot` → `Menu` ↔ `Match`. Scene scopes are children of the root. Plain C# classes run through VContainer entry points (`IStartable`, `ITickable`, `IAsyncStartable`, `IDisposable`); `MonoBehaviour`s are views only.

## Feature anatomy

```
Gameplay/Bucket/
  CLAUDE.md                      module context
  PlowParty.Gameplay.Bucket.asmdef
  Simulation/                    pure C#: rules, math, state transitions
  Network/                       NetworkBehaviour adapters
  View/                          MonoBehaviour presentation
  Config/                        ScriptableObject config types
  Tests/                         EditMode tests, own asmdef
```

Folders appear only when they have content.

**Simulation** holds the rules as plain deterministic C#: input state in, output state out, no Fusion types, no `MonoBehaviour`, no `Time.*`, no `Random` without an injected seed. It is where EditMode tests point.

**Network** holds `NetworkBehaviour`s. Each one is a thin adapter: in `FixedUpdateNetwork` it reads `[Networked]` state and input, calls Simulation, writes the result back. Visual reaction to state changes uses `ChangeDetector` in `Render`.

**View** reads state and plays it back to the player (animation, VFX, UI). It never writes simulation state.

## Fusion and DI

Fusion, not VContainer, instantiates networked prefabs. `MatchScope` registers a custom `INetworkObjectProvider` that instantiates through the scope's `IObjectResolver`, so every spawned `NetworkObject` (vehicles, loot, snowballs) gets `[Inject]` dependencies on host and clients alike. See ADR-0004.

Players and bots drive a vehicle through the same input-source abstraction; the vehicle cannot tell them apart. Bots run on the host only.

## Configuration

Tunable numbers live in ScriptableObject configs, one per concern (`MatchConfig`, `BucketConfig`, `GadgetConfig`, `BotConfig`, ...), each defined in its feature's `Config/`. The asset instances are registered in `RootLifetimeScope` with `RegisterInstance` and injected like any dependency. Simulation code receives the config values, never looks them up.

## Feature index

Status: `planned` — designed in the GDD, no folder yet; `active` — folder exists.

| Module | Path | Status | Purpose |
|---|---|---|---|
| Bootstrap | `_Project/Bootstrap/` | active | Lifetime scopes, app start |
| Infrastructure | `_Project/Infrastructure/` | active | Scene loading; later backend client, persistence, network object provider |
| Shared | `_Project/Shared/` | active | Cross-boundary types |
| Account | `_Project/Meta/Account/` | planned | Guest login by device id, nickname (GDD 9.1) |
| Lobby | `_Project/Meta/Lobby/` | planned | Main menu: quick play, room code entry |
| Session | `_Project/Meta/Session/` | planned | Fusion session start, matchmaking, room codes, bot fill after timeout (GDD 3.3) |
| Tournament | `_Project/Meta/Tournament/` | planned | Daily tournament leaderboard, result submission (GDD 9.2–9.3) |
| Match | `_Project/Gameplay/Match/` | planned | Match state machine Countdown → Playing → Results, timer, scoring table (GDD 3.2) |
| Vehicle | `_Project/Gameplay/Vehicle/` | planned | Kinematics, collisions, ramming, input source (GDD 5) |
| Snow | `_Project/Gameplay/Snow/` | planned | Snow grid, regrowth, blizzard waves, snow piles (GDD 4.4) |
| Bucket | `_Project/Gameplay/Bucket/` | planned | Load, capacity, speed penalty, spill on hit (GDD 4.1, 4.3) |
| DropOff | `_Project/Gameplay/DropOff/` | planned | Drop-off zone, unloading, multipliers (GDD 4.2) |
| Drifts | `_Project/Gameplay/Drifts/` | planned | Breakable drifts and respawn (GDD 6) |
| Loot | `_Project/Gameplay/Loot/` | planned | Owned loot drops, pickup, expiry (GDD 6) |
| Gadgets | `_Project/Gameplay/Gadgets/` | planned | Gadget slot, targeted/instant/thrown gadgets, immunity (GDD 7) |
| Bots | `_Project/Gameplay/Bots/` | planned | Utility-AI bots on the host (GDD 8) |
| Hud | `_Project/Gameplay/Hud/` | planned | Joysticks, gadget button, portraits, timer, announcements |
