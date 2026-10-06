# Match

Status: ready-for-agent

## Problem Statement

A Session drops every Player straight into free driving: there is no start signal, no 180-second limit, no winner, and no way to play again without restarting the Session. The loop "collect, carry, deliver" has nothing to compete for until a Match has a beginning, an end, and a placement table (GDD 3.1–3.2).

## Solution

A Match feature in Gameplay. Its rules live in pure C#: `MatchRules` steps the phase machine Countdown → Playing → Results → Countdown and answers clock questions from the time spent in the current phase; `MatchPlacementRules` turns Scores into a placement table with shared places for ties. A scene `NetworkObject`, `MatchDriver`, holds the networked phase, phase start tick, Match number, and the recorded placements; the Host steps it once per tick, locks Vehicles through Vehicle's existing Immobilised Modifier outside Playing, records placements from DropOff's `IScoreReader` when Playing ends, and starts the next Match in the same Session by respawning every Vehicle. Every peer reads the Match through two interfaces: `IMatchClock` (phase, Match number, remaining and elapsed time) and `IMatchResults` (placements, restart request).

## User Stories

1. As a Player, I want a 3-second Countdown with every Vehicle on its Spawn Point and unable to move, so that the Match starts fairly (GDD 3.2).
2. As a Player, I want the Match to run for 180 seconds of Playing, so that every Match has the same length.
3. As a Player, I want driving to stop when time runs out, so that nobody scores after the end.
4. As a Player, I want a placement table at the end with ties sharing a place (1, 2, 2, 4), so that the result reads like the backend's.
5. As a Player, I want the Score at the moment Playing ends to be the Score that counts, even if a Delivery was still running, so that the table is final.
6. As a Player, I want a new Match to start in the same Session after Results, with Score, Load, and positions reset, so that friends can play again without reconnecting.
7. As a Host, I want to start the next Match early from the Results screen, so that we do not wait.
8. As a Player who joins mid-Match, I want to drive in the current phase, so that joining late still works.
9. As a developer building Snow, Hud, and Meta, I want a read-only Match clock (phase, elapsed Playing time, remaining time) on every peer, so that Regrowth, Blizzards, and the HUD use one clock.
10. As a game designer, I want the Countdown, Playing, and Results durations in a config asset.
11. As a developer, I want the phase machine, clock, and placement covered by EditMode tests.

## Implementation Decisions

- **Module:** `Gameplay/Match`, asmdef `PlowParty.Gameplay.Match`, references DropOff, Vehicle, Fusion, VContainer. Folders `Simulation`, `Network`, `Config`, `Prefabs`, `Tests`. Added to `AssembliesToWeave`. Feature index row `planned` → `active`.
- **Phases:** `MatchPhase` Countdown, Playing, Results; no WaitingForPlayers (Meta/Session owns it). The first Match starts with Countdown when the driver spawns on the Host.
- **Clock from ticks:** the networked state is the phase and the tick it started; phase elapsed = (tick − start tick) × tick delta. Clients compute the same numbers from their own tick, so the clock needs no per-tick writes.
- **Transitions (`MatchRules.NextPhase`):** Countdown → Playing at `CountdownDuration`; Playing → Results at `PlayingDuration`; Results → Countdown at `ResultsDuration` or at once when the Host requested a restart. `ResultsDuration` 0 waits for the Host. A restart request outside Results is ignored.
- **Numbers (MatchConfig):** Countdown 3 s, Playing 180 s, Results 10 s.
- **Input lock:** Vehicle's `IsImmobilised` Modifier, written by the Host every tick as `phase != Playing`. Clients predict with it like any Modifier. Match is that Modifier's owner until Freeze arrives; Freeze will need its own Modifier or a combined owner (Vehicle's one-owner rule).
- **Placement:** `MatchPlacementRules.Rank(slots, scores, count, output)` sorts by Score descending, ties by Slot ascending, and gives competition ranks (1, 2, 2, 4). Allocation-free. Recorded once, on entering Results, into networked arrays (Slot, Score, Place per row), so the table is frozen even if a Delivery keeps unloading.
- **Restart:** entering Countdown from Results increments `MatchNumber`, clears the placements, and calls `VehicleSpawner.RespawnAll`, which despawns and respawns each Player's Vehicle in its Slot. A fresh Vehicle brings a fresh Bucket (Load 0) and Delivery (Score 0) at its Spawn Point, so Match does not reach into Bucket or DropOff. `MatchRestarted` (host-side C# event) fires after the respawn; it is the seam a Snow reset plugs into.
- **Read seams:** `IMatchClock` (`IsRunning`, `Phase`, `MatchNumber`, `PhaseRemaining`, `PlayingElapsed`, `PlayingRemaining`) and `IMatchResults` (`PlacementCount`, `GetPlacement`, `CanRequestRestart`, `RequestRestart`). Both implemented by `MatchDriver`, registered in `MatchScope` with `RegisterComponentInHierarchy`.
- **Prefab:** `Prefabs/MatchDriver.prefab` (`NetworkObject`, `MatchDriver`), placed once in the Match scene at integration.

## Testing Decisions

- Tests build `MatchRules` from small settings and call `MatchPlacementRules` with plain arrays. Assembly `PlowParty.Gameplay.Match.Tests`, names `Method_Condition_ExpectedResult`.
- Cover: every transition at and before its duration; restart request in and outside Results; Results duration 0 waits; input lock per phase; remaining and elapsed Playing time in every phase; elapsed from ticks, including a tick before the phase start; placement order, tie places, tie order by Slot, all-zero, partial count, output shorter than count, empty.
- Not unit-tested: `MatchDriver`, the respawn; checked in Play Mode on the Host.

## Out of Scope

- WaitingForPlayers, bots fill, SubmitToApi, `MatchResult` in Shared, return to menu (Meta).
- Snow reset and the Snow clock wiring (integration; Snow cannot reference Match, see Further Notes).
- Interrupted Match on Host leave.

## Further Notes

- Out-of-module edits: `Vehicle/Network/VehicleSpawner.cs` (`RespawnAll`), Vehicle `CLAUDE.md` (Immobilised owner); `MatchScope`, `RootLifetimeScope` (+ prefab field), Bootstrap asmdef and `CLAUDE.md`; `NetworkProjectConfig.fusion`; `GLOSSARY.md` (Countdown, Playing, Results, Match Number, Placement).
- Snow cannot reference Match (Match → DropOff → Snow would cycle). The Snow clock and Snow reset therefore need a seam Snow declares (for example an elapsed-Playing-time provider and a reset call), implemented by Match or wired in Bootstrap from `IMatchClock.PlayingElapsed` and `MatchDriver.MatchRestarted`.
- `MatchScope` registers `MatchDriver` and the Hud views from the scene; the scene must contain the prefabs or the scope fails to build.
- `IMatchClock` and `IMatchResults` have one implementation each. `docs/architecture.md` asks features to depend on another feature's interface (DropOff's `IScoreReader` set the precedent); `docs/coding-standards.md` allows an interface only with two implementations or a test double. The two rules conflict; this change follows the architecture rule and flags the conflict.
- Review follow-ups applied: the next-Match transition test lives in `MatchRules.StartsNextMatch`; `MatchDriver` runs before `VehicleWorldDriver` (`[DefaultExecutionOrder(-100)]`), so the lock lands on the tick Playing ends; `IMatchResults.WaitsForHost` distinguishes an untimed Results from a timed one at 0.
