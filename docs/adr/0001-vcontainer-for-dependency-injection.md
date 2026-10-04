# VContainer for dependency injection

Every service and system gets its dependencies from VContainer; singletons, static service locators, and `Find*` lookups are excluded. VContainer was chosen over Zenject/Extenject for its speed, zero-allocation resolution, active maintenance, and first-class entry points (`IStartable`, `ITickable`), which let gameplay logic live in plain C# classes outside `MonoBehaviour`. The container hierarchy is one root scope plus one scope per scene (see `docs/architecture.md`).
