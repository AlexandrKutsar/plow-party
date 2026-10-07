# Infrastructure

Engine, platform, and network plumbing used by both Gameplay and Meta: things that talk to Unity APIs, the filesystem, the network, or the backend. Holds no game rules.

## Entry points

- `Scenes/SceneLoader` — loads a scene by name, awaitable via UniTask. Registered in `RootLifetimeScope`.
- `Network/NetworkSession` — starts the Fusion session for the active scene (`AutoHostOrClient` for now); cancelling the token cancels the start itself. Creates the `NetworkRunner` with `NetworkSceneManagerDefault` and the resolver provider; shuts the runner down when its scope is disposed.
- `Network/ResolverNetworkObjectProvider` — Fusion's default provider with `InstantiatePrefab` routed through the scope's `IObjectResolver`, so every spawned `NetworkObject` receives `[Inject]` on Host and clients (ADR-0004). Scene `NetworkObject`s are not instantiated by Fusion; register them with `RegisterComponentInHierarchy` instead.
- `Network/NetworkRunnerEvents` — the single `INetworkRunnerCallbacks` for the runner, re-exposed as C# events (`PlayerJoined`, `PlayerLeft`, `InputRequested`). Add an event here when a feature needs another callback.

- `Session/MatchLineupStore` — app-lifetime holder of the `MatchLineup` that Meta sets before loading the Match scene and Match reads; registered in `RootLifetimeScope`. Empty when the Match scene is started directly in the Editor.

## Planned

- Backend HTTP client (tournament API), persistence (account, settings).

## Depends on

Shared, UniTask, VContainer, `Fusion.Unity`.
