# Bootstrap

Composition root: the only module that references every other module. It owns the VContainer lifetime scopes and nothing else; services and rules belong to their own modules.

## Entry points

- `RootLifetimeScope` — app-wide scope. `RootLifetimeScope.prefab` is assigned as the root in `VContainerSettings.asset` (both in this folder; the settings asset sits in PlayerSettings → Preloaded Assets), so VContainer instantiates it before any scene scope. Registers the config assets from `_Project/Configs/` and Infrastructure services.
- `MatchScope` — the Match scene's scope: network session, runner events, Vehicle registry, input poller, and the scene components Fusion does not instantiate (`VehicleSpawner`, `VehicleWorldDriver`, `SnowGridDriver`, `SnowGridView`); Bucket's `BucketRegistry` (also as Snow's `IScrapeLimit`) and the `BucketHost` entry point; DropOff's `DropOffZone` (scene), `DropOffSnowFreeArea` (as Snow's `ISnowFreeArea`), `DeliveryRegistry` (also as `IScoreReader`), and the `DropOffHost` entry point. Match's `MatchDriver` (scene, also as `IMatchClock` and `IMatchResults`); the scene `Camera` and Hud's views (scene components of `Hud.prefab`), grouped in `RegisterHud`. Every `RegisterComponentInHierarchy` target must exist in the Match scene, or the scope fails to build.
- `MatchSceneQuickStart` — temporary entry point that starts a dev session the moment the Match scene loads, so the scene is playable on its own. Replaced by `Meta/Session` once matchmaking exists.
- `MenuScope` — planned.

## Rules

- A registration lives in the scope whose lifetime matches the object's: app-wide in root, per-visit in the scene scope.
- When a module gains a service the scopes must register, add the asmdef reference here and register it in the matching scope.

## Decisions

- One root plus one scope per scene — `docs/adr/0001-vcontainer-for-dependency-injection.md`.
