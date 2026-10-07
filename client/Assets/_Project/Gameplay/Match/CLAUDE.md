# Match

The Match lifecycle inside a Session: Countdown → Playing → Results → next Match, the Match clock, and the placement table (GDD 3.1–3.2). Rules live in `Simulation/` as deterministic C#; the Host steps them once per Fusion tick. Spec: `.scratch/match/spec.md`.

## Entry points

- `MatchRules` — phase machine and clock, built from `MatchSettings`: `NextPhase(phase, phaseElapsed, restartRequested)`, `PhaseRemaining`, `PlayingElapsed`, `PlayingRemaining`, static `IsInputLocked(phase)`, `StartsNextMatch(from, to)`, and `PhaseElapsed(tick, phaseStartTick, deltaTime)`; `WaitsForRestartRequest` when Results has no time limit. Holds no state.
- `MatchPlacementRules.Rank(slots, scores, count, output)` — fills `MatchPlacement`s (Slot, Score, Place) ordered by Score descending, ties by Slot; tied Scores share a place and skip the next (1, 2, 2, 4), like the backend. Allocation-free; returns the rows written.
- `IMatchClock` — read-only clock on every peer: `IsRunning`, `Phase`, `MatchNumber`, `PhaseRemaining`, `PlayingElapsed`, `PlayingRemaining`.
- `IMatchResults` — the recorded placement table (`PlacementCount`, `GetPlacement(index)`), `WaitsForHost` (Results has no time limit), and the Host's `RequestRestart` (`CanRequestRestart` is true only on the Host during Results).
- `MatchSettings` — durations, filled from `MatchConfig`.

## Rules worth knowing

- Countdown 3 s → Playing 180 s → Results 10 s → Countdown of the next Match. `ResultsDuration` 0 means Results waits for the Host's restart. A restart request only counts during Results.
- The networked state is the phase and the tick it started; every peer derives the clock from its own tick, so nothing is written per tick except the input lock. A client's tick is predicted ahead of the Host, so near a transition its clock can read a fraction of a second past the phase end until the new phase arrives; views clamp (Countdown never shows 0).
- Input is locked outside Playing through Vehicle's `IsImmobilised` Modifier, written by the Host every tick for every registered Vehicle. `MatchDriver` carries `[DefaultExecutionOrder(-100)]` so it runs before `VehicleWorldDriver` and the lock applies on the very tick Playing ends. Match is that Modifier's only owner until Freeze exists.
- Placements are recorded once, on entering Results, from DropOff's `IScoreReader`, so Score credited by a Delivery still unloading after the end does not change the table.
- A new Match respawns every Player's Vehicle in its Slot (`VehicleSpawner.RespawnAll`): the fresh Vehicle starts at its Spawn Point with an empty Bucket and zero Score, so Match never resets Bucket or DropOff state itself.

## Network

- `MatchDriver` — scene `NetworkObject` (`Prefabs/MatchDriver.prefab`), implements both interfaces and Snow's `ISnowClock` (`IsPlaying`, `PlayingElapsed`, `MatchRestarted`); registered in `MatchScope` with `RegisterComponentInHierarchy` as `IMatchClock`, `IMatchResults`, `ISnowClock`, and itself. The Host enters Countdown on spawn (Match number 1). `MatchRestarted` is a host-side C# event raised after the respawn of a new Match; Snow resets its grid on it, since the Snow Grid outlives Vehicles.
- Players who join mid-Match spawn in the current phase and are locked or free like everyone else.
- `MatchConfig` — the ScriptableObject behind `MatchSettings`; asset `_Project/Configs/MatchConfig.asset`, registered in `RootLifetimeScope`.

## Depends on

DropOff (`IScoreReader`), Snow (`ISnowClock`, implemented here), Vehicle (`VehicleRegistry`, `VehicleSpawner`, `NetworkVehicle`), Fusion, VContainer. Hud depends on Match. Snow cannot reference Match (Match → DropOff → Snow), which is why the clock seam is Snow's interface. The assembly is in Fusion's `AssembliesToWeave`. Submitting the Match Result and WaitingForPlayers belong to Meta.
