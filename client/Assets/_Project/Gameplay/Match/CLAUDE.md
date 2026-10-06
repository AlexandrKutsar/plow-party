# Match

The Match lifecycle inside a Session: Countdown → Playing → Results → next Match, the Match clock, and the placement table (GDD 3.1–3.2). Rules live in `Simulation/` as deterministic C#; the Host steps them once per Fusion tick. Spec: `.scratch/match/spec.md`.

## Entry points

- `MatchRules` — phase machine and clock, built from `MatchSettings`: `NextPhase(phase, phaseElapsed, restartRequested)`, `PhaseRemaining`, `PlayingElapsed`, `PlayingRemaining`, static `IsInputLocked(phase)` and `PhaseElapsed(tick, phaseStartTick, deltaTime)`. Holds no state.
- `MatchPlacementRules.Rank(slots, scores, count, output)` — fills `MatchPlacement`s (Slot, Score, Place) ordered by Score descending, ties by Slot; tied Scores share a place and skip the next (1, 2, 2, 4), like the backend. Allocation-free; returns the rows written.
- `IMatchClock` — read-only clock on every peer: `IsRunning`, `Phase`, `MatchNumber`, `PhaseRemaining`, `PlayingElapsed`, `PlayingRemaining`. Snow's Regrowth and Blizzards should run on `PlayingElapsed`.
- `IMatchResults` — the recorded placement table (`PlacementCount`, `GetPlacement(index)`) and the Host's `RequestRestart` (`CanRequestRestart` is true only on the Host during Results).
- `MatchSettings` — durations, filled from `MatchConfig`.

## Rules worth knowing

- Countdown 3 s → Playing 180 s → Results 10 s → Countdown of the next Match. `ResultsDuration` 0 means Results waits for the Host's restart. A restart request only counts during Results.
- The networked state is the phase and the tick it started; every peer derives the clock from its own tick, so nothing is written per tick except the input lock.
- Input is locked outside Playing through Vehicle's `IsImmobilised` Modifier, written by the Host every tick for every registered Vehicle. Match is that Modifier's only owner until Freeze exists.
- Placements are recorded once, on entering Results, from DropOff's `IScoreReader`, so Score credited by a Delivery still unloading after the end does not change the table.
- A new Match respawns every Player's Vehicle in its Slot (`VehicleSpawner.RespawnAll`): the fresh Vehicle starts at its Spawn Point with an empty Bucket and zero Score, so Match never resets Bucket or DropOff state itself.

## Network

- `MatchDriver` — scene `NetworkObject` (`Prefabs/MatchDriver.prefab`), implements both interfaces; registered in `MatchScope` with `RegisterComponentInHierarchy` as `IMatchClock`, `IMatchResults`, and itself. The Host enters Countdown on spawn (Match number 1). `MatchRestarted` is a host-side C# event raised after the respawn of a new Match; it is the seam for resetting state that outlives Vehicles (the Snow Grid).
- Players who join mid-Match spawn in the current phase and are locked or free like everyone else.
- `MatchConfig` — the ScriptableObject behind `MatchSettings`; asset `_Project/Configs/MatchConfig.asset`, registered in `RootLifetimeScope`.

## Depends on

DropOff (`IScoreReader`), Vehicle (`VehicleRegistry`, `VehicleSpawner`, `NetworkVehicle`), Fusion, VContainer. Hud depends on Match. Snow cannot reference Match (Match → DropOff → Snow), so wiring the Snow clock and a Snow reset needs a seam declared by Snow and implemented here or in Bootstrap. The assembly is in Fusion's `AssembliesToWeave`. Submitting the Match Result and WaitingForPlayers belong to Meta.
