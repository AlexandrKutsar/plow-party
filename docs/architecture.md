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

Visual content lives outside these areas in `_Project/Art/`, grouped by kind of content, one folder per asset, and holds no code. Each asset has a script-free visual prefab `V_<Asset>`; a feature's gameplay prefab nests it as `Model`. Editor-only tooling lives in `_Project/Editor/` (asmdef `PlowParty.Editor`, referenced by no runtime assembly). Rules: `_Project/Art/CLAUDE.md`, ADR-0008, ADR-0009.

Asmdef naming: `PlowParty.<Area>` or `PlowParty.<Area>.<Feature>`; namespaces match. Feature-to-feature references inside an area are allowed but must stay acyclic; prefer depending on another feature's interface over its concrete types.

## Gameplay/Meta boundary

Gameplay knows nothing about menus, accounts, or the backend. The two sides meet at exactly two points:

1. **Into a match** — `Meta/Session` gathers Players in a Fusion Session while in the Menu (Quick Play, or a Party's hidden Session from `Meta/Party`), each presenting its `ParticipantToken` as connection token, writes the `MatchmakingResult` (Shared: expected Players, Slot count) into Infrastructure's `MatchmakingResultStore`, and loads the Match scene for every peer through Fusion; `MatchScope` binds to the running Session and takes over from WaitingForPlayers, where the Host seats the arriving Players and fills the remaining Slots with Bots (ADR-0016, ADR-0018).
2. **Out of a match** — `Meta/Tournament`'s reporter reads the Match through ports Bootstrap adapts from Gameplay's read seams (`IMatchClock`, `IMatchResults`, `IScoreReader`, Participants' `ParticipantRoster`) and votes the Match Result to the backend; Hud's "В меню" raises Hud's `IMatchExit`, which Bootstrap routes to `Meta/Session` (ADR-0017).

## Lifetime scopes (VContainer)

| Scope | Lives in | Lifetime | Registers |
|---|---|---|---|
| `RootLifetimeScope` | prefab referenced by `VContainerSettings` | whole app | configs (ScriptableObjects), Infrastructure services (including the Fusion `NetworkSession`), account, session exit, match report API |
| `MenuScope` | Menu scene | menu visit | nickname panel, matchmaker, lobby and tournament screens |
| `MatchScope` | Match scene | one visit of the Match scene | Gameplay systems, HUD, match reporter and its Bootstrap adapters |

Scene flow: `Boot` → `Menu` ↔ `Match`. Scene scopes are children of the root. Plain C# classes run through VContainer entry points (`IStartable`, `ITickable`, `IAsyncStartable`, `IDisposable`); `MonoBehaviour`s are views only.

## Feature anatomy

```
Gameplay/Bucket/
  CLAUDE.md                      module context
  PlowParty.Gameplay.Bucket.asmdef
  Simulation/                    pure C#: rules, math, state transitions
  Network/                       NetworkBehaviour adapters
  View/                          MonoBehaviour presentation scripts (animator, VFX, feedback)
  Config/                        ScriptableObject config types (assets live in _Project/Configs/)
  Prefabs/                       gameplay prefabs; each nests its Art visual prefab as Model
  Tests/                         EditMode tests, own asmdef
```

Folders appear only when they have content.

**Simulation** holds the rules as plain deterministic C#: input state in, output state out, no Fusion types, no `MonoBehaviour`, no `Time.*`, no `Random` without an injected seed. It is where EditMode tests point.

**Network** holds `NetworkBehaviour`s. Each one is a thin adapter: in `FixedUpdateNetwork` it reads `[Networked]` state and input, calls Simulation, writes the result back. Visual reaction to state changes uses `ChangeDetector` in `Render`.

**View** reads state and plays it back to the player (animation, VFX, UI). It never writes simulation state. View scripts live in the feature; the meshes, materials, and effects they drive live in `Art/`.

## Fusion and DI

Fusion, not VContainer, instantiates networked prefabs. The session adds a `ResolverNetworkObjectProvider` that instantiates through the `IObjectResolver` of the scene scope currently bound to the Session (`NetworkScopeBinding`; `MatchScope` during a Match), so every spawned `NetworkObject` (vehicles, loot, snowballs) gets `[Inject]` dependencies on host and clients alike; scene `NetworkObject`s are injected through `RegisterComponentInHierarchy`. The runner itself lives in the root scope because one Session spans the Menu and the Match. See ADR-0004 and ADR-0016.

Every assembly that declares a `NetworkBehaviour` or `INetworkInput` must be listed in `AssembliesToWeave` in `NetworkProjectConfig.fusion`.

Players and bots drive a vehicle through the same input-source abstraction; the vehicle cannot tell them apart. Bots run on the host only.

## Configuration

Tunable numbers live in ScriptableObject configs, one per concern (`MatchConfig`, `BucketConfig`, `GadgetConfig`, `BotConfig`, ...). The config type is code and lives in its feature's `Config/`; the asset instance is data and lives in `_Project/Configs/`, one `<Feature>Config.asset` each, so the whole game is balanced from one folder and balance changes show up as their own diffs. The assets are registered in `RootLifetimeScope` with `RegisterInstance` and injected like any dependency. Simulation code receives the config values, never looks them up.

Prefabs follow the same split by role: a gameplay prefab (scripts, networking) lives in its feature's `Prefabs/`; its look is a script-free visual prefab in `Art/`. A prefab assembled from several features (a piece of a map) belongs to the map, under `_Project/Levels/<Map>/` (Farm's obstacles: `Levels/Farm/Obstacles/`); its `CLAUDE.md` documents how the Match scene is composed.

## Feature index

Status: `planned` — designed in the GDD, no folder yet; `active` — folder exists.

| Module | Path | Status | Purpose |
|---|---|---|---|
| Bootstrap | `_Project/Bootstrap/` | active | Lifetime scopes, app start |
| Infrastructure | `_Project/Infrastructure/` | active | Scene loading, root-lifetime Fusion Session with per-scope binding, DI-aware network object provider, backend HTTP client, local file storage, safe-area fitter |
| Shared | `_Project/Shared/` | active | Cross-boundary types |
| Art | `_Project/Art/` | active | Visual content only: models, palette, materials, visual prefabs; sources in `art/` |
| Editor | `_Project/Editor/` | active | Editor-only tooling: art import rules, Android Player settings and development APK build |
| Account | `_Project/Meta/Account/` | active | Guest login by Device Id, Auth Token and 401 re-login, Nickname and its rename panel (GDD 9.1) |
| Lobby | `_Project/Meta/Lobby/` | active | Menu screens: bottom tabs Игра / Турнир, solo panel (Quick Play, Party create, Party Code entry), Party panel (code and copy, member rows with crown, Ready and removal, mode switch, Ready, Leader's search, Leave), "Поиск игры…" with a local stopwatch and Stop, and the 3D Podium of the Party's tractors and Critters |
| Session | `_Project/Meta/Session/` | active | Quick Play search over the Lobby list (pick, Party as Lobby, merge into an older Lobby, stop by any member), Matchmaking Result, networked load of the Match, leaving to the Menu (GDD 3.3, ADR-0016, ADR-0019) |
| Party | `_Project/Meta/Party/` | active | Party in the Menu: hidden Party Session by Party Code, members in join order, Party Leader and succession, Ready, removal, mode, the Party's search flag and moves into another Lobby, return to the Party after a Match (ADR-0019) |
| Tournament | `_Project/Meta/Tournament/` | active | "Турнир дня" panel (top, around me, Medal) and Match reporting: register, confirm, Vote (GDD 9.2–9.3, ADR-0017) |
| Match | `_Project/Gameplay/Match/` | active | Match state machine WaitingForPlayers → Countdown → Playing → Results (one Match per Session), seating Players and Bots into Slots, Match clock, input lock, placement table (GDD 3.2–3.3) |
| Participants | `_Project/Gameplay/Participants/` | active | Networked Participant profile per Slot (Nickname, Critter Species, Participant Color, Bot or Player), Critter on the Vehicle seat, Vehicle body tint, colour palette and assignment rules (GDD 3.1, 8) |
| Vehicle | `_Project/Gameplay/Vehicle/` | active | Kinematics, collisions, ramming, input source (GDD 5) |
| Snow | `_Project/Gameplay/Snow/` | active | Snow Grid, Blade scraping, Regrowth, Blizzard waves, Snow Piles and their weight, displaced snow surface (GDD 4.1, 4.3, 4.4) |
| Bucket | `_Project/Gameplay/Bucket/` | active | Load, capacity, speed penalty, spill on hit (GDD 4.1, 4.3) |
| DropOff | `_Project/Gameplay/DropOff/` | active | Drop-Off Zone, Delivery, Multipliers, per-Vehicle Score, Snow-Free Area (GDD 4.2) |
| Drifts | `_Project/Gameplay/Drifts/` | planned | Breakable drifts and respawn (GDD 6) |
| Loot | `_Project/Gameplay/Loot/` | planned | Owned loot drops, pickup, expiry (GDD 6) |
| Gadgets | `_Project/Gameplay/Gadgets/` | planned | Gadget slot, targeted/instant/thrown gadgets, immunity (GDD 7) |
| Bots | `_Project/Gameplay/Bots/` | active | Utility-AI Bots on the Host: nav grid and A*, collect/deliver/Pile/Ram/evade scoring, difficulty profiles (GDD 8) |
| Hud | `_Project/Gameplay/Hud/` | active | Virtual stick, timer, Load bar, Nickname labels over Vehicles, Score list and popups, Blizzard announcement, Drop-Off arrow, Results; later gadget button and portraits |
| CameraRig | `_Project/Gameplay/CameraRig/` | active | Local camera: Follow preset in builds (overview and follow rotating kept for the Editor, key C), Camera Shake |
| Levels | `_Project/Levels/<Map>/` | active | Map composition, no code: obstacle prefabs and the scene layout note; `Farm` (40 × 40 m) is the only map, laid out in `Scenes/Match.unity` |
