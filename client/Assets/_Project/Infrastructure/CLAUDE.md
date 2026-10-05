# Infrastructure

Engine, platform, and network plumbing used by both Gameplay and Meta: things that talk to Unity APIs, the filesystem, the network, or the backend. Holds no game rules.

## Entry points

- `Scenes/ISceneLoader` — loads a scene by name, awaitable via UniTask. Registered in `RootLifetimeScope`.

## Planned

- Backend HTTP client (tournament API), persistence (account, settings), Fusion `INetworkObjectProvider` that instantiates through VContainer (`docs/adr/0004-di-for-fusion-spawned-objects.md`).

## Depends on

UniTask. Fusion runtime DLLs are auto-referenced; add `Fusion.Unity` and `VContainer` to the asmdef when the network object provider lands.
