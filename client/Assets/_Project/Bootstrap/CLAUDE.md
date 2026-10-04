# Bootstrap

Composition root: the only module that references every other module. It owns the VContainer lifetime scopes and nothing else; services and rules belong to their own modules.

## Entry points

- `RootLifetimeScope` — app-wide scope. Its prefab is assigned as the root in the `VContainerSettings` asset, so VContainer creates it before any scene scope. Registers configs and Infrastructure services.
- `MenuScope`, `MatchScope` — planned, one per scene, children of the root.

## Rules

- A registration lives in the scope whose lifetime matches the object's: app-wide in root, per-visit in the scene scope.
- When a module gains a service the scopes must register, add the asmdef reference here and register it in the matching scope.

## Decisions

- One root plus one scope per scene — `docs/adr/0001-vcontainer-for-dependency-injection.md`.
