# Infrastructure

Engine, platform, and network plumbing used by both Gameplay and Meta: things that talk to Unity APIs, the filesystem, the network, or the backend. Holds no game rules.

## Entry points

- `Scenes/SceneLoader` — loads a scene by name, awaitable via UniTask; `ActiveSceneName`. `Scenes/SceneNames` holds `Boot`, `Menu`, `Match`. Registered in `RootLifetimeScope`.
- `Network/NetworkSession` — root-lifetime owner of the one `NetworkRunner` (ADR-0016). `StartAsync(SessionStart)` creates the runner (`DontDestroyOnLoad`, `NetworkSceneManagerDefault`, resolver provider) and starts it with the given `GameMode`, session name, properties, connection token and player count, optionally with the active scene as the network scene (the Editor's direct Match start); it answers `SessionStartOutcome` (`Started`, `NotFound`, `NameTaken`, `Failed`). The caller's token cancels only the start: Fusion keeps its start token for the runner's whole life, so the session links it to a private source for the duration of the start. Also `IsRunning`, `IsHost`, `Runner`, `PlayerCount`, `TryGetProperty`, `Close` (Host: closed and hidden), `LoadScene` (Host: Fusion scene load for every peer), `LeaveAsync`, and `Ended` (`Left` when we asked, `Lost` otherwise).
- `Network/NetworkScopeBinding` — a scene scope registers it and calls `Bind()` from a build callback: the scope's `IObjectResolver` becomes the provider's resolver and its `NetworkRunnerEvents` join the runner's callbacks until the scope is disposed. Binding to a running Session replays `PlayerJoined` for every active Player on Fusion's next `OnSceneLoadDone`.
- `Network/ResolverNetworkObjectProvider` — Fusion's default provider with `InstantiatePrefab` routed through the bound scope's `IObjectResolver` (`Use`), so every spawned `NetworkObject` receives `[Inject]` on Host and clients (ADR-0004). Scene `NetworkObject`s are not instantiated by Fusion; register them with `RegisterComponentInHierarchy` instead.
- `Network/NetworkRunnerEvents` — one `INetworkRunnerCallbacks` per scope, re-exposed as C# events (`PlayerJoined`, `PlayerLeft`, `InputRequested`, `SceneLoadDone`, `ShutDown`). `NetworkSession` keeps a private instance for its own bookkeeping. Add an event here when a feature needs another callback.
- `Session/MatchLineupStore` — app-lifetime holder of the `MatchLineup` that Meta sets before loading the Match scene and Match reads; registered in `RootLifetimeScope`. Empty when the Match scene is started directly in the Editor; cleared when the Player leaves to the Menu.
- `Backend/BackendClient` — `SendAsync(BackendRequest)` over `UnityWebRequest` against `BackendConfig.BaseUrlFor(Application.isEditor)` with a short timeout; answers a `BackendResponse` (`Outcome`: `Ok`, `Offline`, `Unauthorized`, `Rejected`; `Read<T>()`; `ValidationMessage()` for FastAPI `detail`). Never throws for HTTP failures; cancellation still throws. `BackendJson` is the Newtonsoft setup (nulls written, unknown members ignored). Authentication lives in `Meta/Account`.
- `Backend/BackendConfig` — Editor base URL (`http://localhost:8000`, the backend's Docker port), device base URL (the PC's LAN address, edit before building for a phone), timeout; asset `_Project/Configs/BackendConfig.asset`.
- `Storage/LocalFileStore` + `StorageFolder` — JSON files in a per-install folder: `<project>/Library/PlowParty` in the Editor (so each worktree and each Multiplayer Play Mode virtual player has its own), `persistentDataPath/PlowParty` in builds.
- `UI/SafeAreaFitter` — fits a `RectTransform` to `Screen.safeArea` clamped to the screen; used by the Menu. Hud still has its own copy (follow-up: switch Hud to this one).

## Depends on

Shared, UniTask, VContainer, `Fusion.Unity`, Newtonsoft JSON (`com.unity.nuget.newtonsoft-json`).
