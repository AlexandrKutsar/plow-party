# Inject Fusion-spawned objects through a custom INetworkObjectProvider

Fusion, not VContainer, instantiates networked prefabs, on the Host and on every client, so those objects would otherwise receive no injection. `MatchScope` registers a custom `INetworkObjectProvider` that instantiates prefabs through the scope's `IObjectResolver`, so every `NetworkObject` gets its `[Inject]` dependencies the same way on all peers.

## Considered Options

- A static registry that network objects query in `Spawned()`: rejected, it is a service locator and hides dependencies from the container and from tests.
- Injecting manually after `Runner.Spawn`: rejected, it only runs on the spawning peer, leaving proxies on other clients uninjected.
