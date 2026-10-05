# Infrastructure

Engine, platform, and network plumbing used by both Gameplay and Meta: things that talk to Unity APIs, the filesystem, the network, or the backend. Holds no game rules.

## Entry points

- `Scenes/ISceneLoader` — loads a scene by name, awaitable via UniTask. Registered in `RootLifetimeScope`.
- `Network/INetworkSession` — starts the Fusion session for the active scene (`AutoHostOrClient` for now). Creates the `NetworkRunner` with `NetworkSceneManagerDefault` and the resolver provider; shuts the runner down when its scope is disposed.
- `Network/ResolverNetworkObjectProvider` — Fusion's default provider with `InstantiatePrefab` routed through the scope's `IObjectResolver`, so every spawned `NetworkObject` receives `[Inject]` on Host and clients (ADR-0004). Scene `NetworkObject`s are not instantiated by Fusion; register them with `RegisterComponentInHierarchy` instead.
- `Network/NetworkRunnerEvents` — the single `INetworkRunnerCallbacks` for the runner, re-exposed as C# events (`PlayerJoined`, `PlayerLeft`, `InputRequested`). Add an event here when a feature needs another callback.

## Planned

- Backend HTTP client (tournament API), persistence (account, settings).

## Depends on

UniTask, VContainer, `Fusion.Unity`.
