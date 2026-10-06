# Inject Fusion-spawned objects through a custom INetworkObjectProvider

Fusion, not VContainer, instantiates networked prefabs, on the Host and on every client, so those objects would otherwise receive no injection. The session (`NetworkSession`, resolved from `MatchScope`) adds a `ResolverNetworkObjectProvider` to the runner's GameObject and injects it with the scope's `IObjectResolver`; the provider instantiates prefabs through that resolver, so every `NetworkObject` gets its `[Inject]` dependencies the same way on all peers. Scene `NetworkObject`s are attached by Fusion rather than instantiated, so they are injected by registering them in the scope with `RegisterComponentInHierarchy`.

## Considered Options

- A static registry that network objects query in `Spawned()`: rejected, it is a service locator and hides dependencies from the container and from tests.
- Injecting manually after `Runner.Spawn`: rejected, it only runs on the spawning peer, leaving proxies on other clients uninjected.
