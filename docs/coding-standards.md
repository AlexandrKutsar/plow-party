# Coding standards — C#

Binding for everything under `client/Assets/_Project/`. `/code-review` checks diffs against this file. Formatting details live in `client/.editorconfig`; this file covers what a formatter cannot.

## Self-documenting code

Zero comments: no `//`, `/* */`, `///` XML-doc, `#region`, commented-out code, or TODO markers. Intent lives in names, small methods, and tests. Context a name cannot hold goes to the module's `CLAUDE.md` or an ADR.

## Naming

- Names come from `GLOSSARY.md`. A concept missing there is added to the glossary first.
- `PascalCase` types, methods, properties, events, constants; `camelCase` locals and parameters; `_camelCase` private fields; `IPascalCase` interfaces.
- Async methods end with `Async` and return `UniTask` or `UniTask<T>`.
- Test names: `Method_Condition_ExpectedResult`.

## Classes

- `sealed` by default; open a class for inheritance only with a concrete subclass in hand.
- One public type per file, file named after the type.
- Fields `private` or `private readonly`. Inspector fields are `[SerializeField] private`.
- Prefer composition and small interfaces; an interface exists when there are two implementations or a test double needs it.

## Dependencies

- Dependencies arrive through the constructor (plain C#) or an `[Inject]` method (`MonoBehaviour`, `NetworkBehaviour`). Every object gets what it needs from VContainer.
- Static mutable state, singletons, `FindObjectOfType`/`FindAnyObjectByType`, `GameObject.Find`, and `Resources.Load` for services are off-limits: they hide dependencies from the container.
- Registrations happen only in a `LifetimeScope.Configure` in Bootstrap or the owning scene scope.
- A module references another module only through its asmdef and only in the directions allowed by `docs/architecture.md`.

## Simulation code

- Deterministic: no `UnityEngine.Time`, no `UnityEngine.Random`, no wall clock. Delta time, tick, and random seed come in as parameters.
- No Fusion types, no `MonoBehaviour`, no scene access.
- Every rule in Simulation has EditMode tests.

## Network code

- `NetworkBehaviour`s stay thin: read `[Networked]` state and input, call Simulation, write back.
- State that must survive rollback is `[Networked]`; nothing gameplay-relevant lives in plain fields.
- Host-only logic (bots, loot rolls, scoring) checks `HasStateAuthority` / `Runner.IsServer` at the entry point.

## Async

- UniTask everywhere; no `async void`, no coroutines for logic.
- Long-running async work takes a `CancellationToken`, tied to the owning scope's or object's lifetime.

## Unity specifics

- Allocation-free hot paths: no LINQ, closures, or boxing inside `FixedUpdateNetwork`, `Update`, or per-tick Simulation calls.
- Cache component references in `Awake` or inject them; no `GetComponent` per frame.
- Tunable numbers live in ScriptableObject configs, never as literals in code.
