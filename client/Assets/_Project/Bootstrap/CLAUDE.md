# Bootstrap

Composition root: the only module that references every other module. It owns the VContainer lifetime scopes and nothing else; services and rules belong to their own modules.

## Entry points

- `RootLifetimeScope` — app-wide scope. `RootLifetimeScope.prefab` is assigned as the root in `VContainerSettings.asset` (both in this folder; the settings asset sits in PlayerSettings → Preloaded Assets), so VContainer instantiates it before any scene scope. Registers the config assets from `_Project/Configs/` (including `ParticipantsConfig` and `BotConfig`, which references the three Bot profile assets) and Infrastructure services, and on build sets `Application.targetFrameRate` from its `_targetFrameRate` field (60; Android otherwise caps at 30).
- `BootScope` — the Boot scene's scope (build index 0, the scene a player build starts in): its only entry point, `BootSceneFlow`, loads `Match` through `SceneLoader`. It will load `Menu` once that scene exists.
- `MatchScope` — the Match scene's scope: network session, runner events, Vehicle registry, input poller, and the scene components Fusion does not instantiate (`VehicleSpawner`, `VehicleWorldDriver`, `SnowGridDriver`, `SnowGridView`); Bucket's `BucketRegistry` (also as Snow's `IScrapeLimit`) and the `BucketHost` entry point; DropOff's `DropOffZone` (scene), `DropOffSnowFreeArea` (as Snow's `ISnowFreeArea`), `DeliveryRegistry` (also as `IScoreReader`), and the `DropOffHost` entry point. Match's `MatchDriver` (scene, also as `IMatchClock` and `IMatchResults`) and the `MatchSeating` entry point (also as itself); Participants' scene `ParticipantRoster`; Bots' scene `BotDriver`; the scene `Camera` and Hud's views (scene components of `Hud.prefab`), grouped in `RegisterHud`. Match's `MatchDriver` is also Snow's `ISnowClock`. Hud's `VirtualStickView` and `WaitingForPlayersView` are in `RegisterHud` too. CameraRig's `CameraShake` (also as `ICameraShake`), the `CameraRamShake` and `CameraPileShake` entry points, and the scene `CameraDirector`, grouped in `RegisterCamera`. Every `RegisterComponentInHierarchy` target must exist in the Match scene, or the scope fails to build.
- `MatchSceneQuickStart` — temporary entry point that starts a dev session the moment the Match scene loads, so the scene is playable on its own; with no `MatchLineup` stored, Match falls back to the Players present plus Bots in six Slots. A failed start (a refused late join) is logged as a warning. Replaced by `Meta/Session` once matchmaking exists. `DevSessionName` names the Session: in the Editor `plow-party-dev-<hash of the project folder>`, so Editors of different worktrees never meet, while Multiplayer Play Mode virtual players (data path `<project>/Library/VP/<player>/Assets`) hash the same folder and join; the environment variable `PLOW_PARTY_DEV_SESSION` overrides it (to join two checkouts on purpose); a player build uses the fixed `plow-party-dev`.
- `MenuScope` — planned.

## Rules

- A registration lives in the scope whose lifetime matches the object's: app-wide in root, per-visit in the scene scope.
- When a module gains a service the scopes must register, add the asmdef reference here and register it in the matching scope.

## Decisions

- One root plus one scope per scene — `docs/adr/0001-vcontainer-for-dependency-injection.md`.
