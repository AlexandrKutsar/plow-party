# Bootstrap

Composition root: the only module that references every other module. It owns the VContainer lifetime scopes and the adapters that let Meta read Gameplay without either side referencing the other; services and rules belong to their own modules.

## Entry points

- `RootLifetimeScope` — app-wide scope. `RootLifetimeScope.prefab` is assigned as the root in `VContainerSettings.asset` (both in this folder; the settings asset sits in PlayerSettings → Preloaded Assets), so VContainer instantiates it before any scene scope. Registers the config assets from `_Project/Configs/` (Gameplay configs, including `ParticipantsConfig` and `BotConfig`, which references the three Bot profile assets, plus `BackendConfig`, `MatchmakingConfig`, `TournamentConfig`), Infrastructure services (`SceneLoader`, `MatchmakingResultStore`, `NetworkSession`, `BackendClient`, a `LocalFileStore` on `StorageFolder.For(...)`), and Meta's app-lifetime services in `RegisterMeta`: `AccountService`, the `MatchmakingPool` (`<DevSessionName>-<Application.version>`), the `SessionExit` entry point, `MatchReportApi`. On build it sets `Application.targetFrameRate` from its `_targetFrameRate` field (60; Android otherwise caps at 30).
- `BootScope` — the Boot scene's scope (build index 0, the scene a player build starts in): its only entry point, `BootSceneFlow`, logs in (`AccountService.EnsureSignedInAsync`, offline falls through) and loads `Menu`. The load destroys the Boot scope that owns the token, so the load's cancellation is suppressed.
- `MenuScope` — the Menu scene's scope (build index 1): `NetworkRunnerEvents` and `NetworkScopeBinding` (bound in a build callback), Account's `NicknameView` + `NicknamePresenter`, Session's `Matchmaker`, Lobby's `PlayMenuView`, `PartyPanelView`, `SearchView`, `PodiumView`, `MemberLooks`, `LobbyPresenter`, `MenuTabsView`, `MenuTabsPresenter`, Tournament's `TournamentService`, `TournamentView`, `TournamentPresenter`.
- `MatchScope` — the Match scene's scope: runner events and `NetworkScopeBinding` (the root `NetworkSession` is bound to this scope from a build callback, ADR-0016), Vehicle registry, input poller, and the scene components Fusion does not instantiate (`VehicleSpawner`, `VehicleWorldDriver`, `SnowGridDriver`, `SnowGridView`); Bucket's `BucketRegistry` (also as Snow's `IScrapeLimit`) and the `BucketHost` entry point; DropOff's `DropOffZone` (scene), `DropOffSnowFreeArea` (as Snow's `ISnowFreeArea`), `DeliveryRegistry` (also as `IScoreReader`), and the `DropOffHost` entry point. Match's `MatchDriver` (scene, also as `IMatchClock`, `IMatchResults` and Snow's `ISnowClock`) and the `MatchSeating` entry point (also as itself); Participants' scene `ParticipantRoster`; Bots' scene `BotDriver`; the scene `Camera`, Hud's views (scene components of `Hud.prefab`, `WaitingForPlayersView` included), `MatchExitAdapter` as Hud's `IMatchExit` and the `MatchStandingFeed` entry point, grouped in `RegisterHud`. CameraRig's `CameraShake` (also as `ICameraShake`), the `CameraRamShake` and `CameraPileShake` entry points, and the scene `CameraDirector`, grouped in `RegisterCamera`. Tournament's reporting in `RegisterMatchReport`: `MatchProgressAdapter` as `IMatchProgress`, `ParticipantRosterAdapter` as `IMatchRoster`, `MatchReportLinks`, the `MatchReporter` entry point. Every `RegisterComponentInHierarchy` target must exist in the Match scene, or the scope fails to build.
- `MatchSceneQuickStart` — Editor shortcut for playing `Match.unity` on its own: when no Session is running (the Match was not reached through the Menu) it logs in and starts `AutoHostOrClient` with the active scene as the network scene and the Player's connection token. A start that fails goes back to the Menu through `SessionExit.LeaveToMenu(notice)`: "Матч уже начался" when the Host refused the join (`SessionStartOutcome.Refused`), "Не удалось подключиться" otherwise. `DevSessionName` names that Session: a player build and the main checkout's Editor use the fixed `plow-party-dev`, so the Editor and a phone meet in Quick Play and Rooms; an Editor on an agent worktree (`/.claude/worktrees/` in its path) uses `plow-party-dev-<hash of the project folder>`, so parallel agents never meet, while Multiplayer Play Mode virtual players (data path `<project>/Library/VP/<player>/Assets`) hash the same folder and join; the environment variable `PLOW_PARTY_DEV_SESSION` overrides it (to join two checkouts on purpose). The same name prefixes the Matchmaking Pool.

## Adapters

`Adapters/` translates between Gameplay seams and Meta ports, so neither side references the other (ADR-0017):

- `MatchProgressAdapter` — Match's `IMatchClock`/`IMatchResults`, DropOff's `IScoreReader` and Vehicle's `VehicleRegistry` as Tournament's `IMatchProgress`; any `MatchPhase` Meta does not know maps to `Waiting`.
- `ParticipantRosterAdapter` — Tournament's `IMatchRoster` over Participants' `ParticipantRoster`: one seat per open Slot; a seated Player's Account Id comes from the Host-side `TryGetAccountId` (the Host's own Account when its token carried none), a Bot or an empty Slot is a Bot seat.
- `MatchExitAdapter` — Hud's `IMatchExit` → Session's `SessionExit.LeaveToMenu()`.
- `MatchStandingFeed` — `ITickable` in `MatchScope`: while Match's `IMatchClock` runs, writes Session's `MatchStanding` into `SessionExit.ObserveMatch` every frame (Results → `Finished`, any earlier phase → `Underway`), so a lost Session picks its notice from the last phase seen; the clock is gone by the time the Session ends.

## Rules

- A registration lives in the scope whose lifetime matches the object's: app-wide in root, per-visit in the scene scope.
- When a module gains a service the scopes must register, add the asmdef reference here and register it in the matching scope.
- Adapters hold no rules: a mapping that needs a decision belongs to the module that owns the port.

## Decisions

- One root plus one scope per scene — `docs/adr/0001-vcontainer-for-dependency-injection.md`.
- The Session outlives scenes; scopes bind to it — `docs/adr/0016-session-outlives-scenes-lobby-in-menu.md`.
- Meta reads Gameplay through adapters here — `docs/adr/0017-match-report-from-meta-through-read-seams.md`.
