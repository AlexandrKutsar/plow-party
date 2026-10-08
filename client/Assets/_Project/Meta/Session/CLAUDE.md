# Session

Matchmaking in the Menu: Quick Play, Room Codes, the Lobby's search timer, starting the Match for everyone, and leaving back to the Menu (GDD 3.2–3.3, ADR-0016). No UI; `Meta/Lobby` draws it. Spec: `.scratch/meta/spec.md`.

## Entry points

- `Matchmaker` — MenuScope entry point (`ITickable`). `QuickPlayAsync` random-joins an open Session of this Matchmaking Pool; when none is found (or the join is refused) it waits a random jitter (`HostJitterMinSeconds`–`HostJitterMaxSeconds`) and tries the join once more, and only then hosts a new one with a search deadline, so two Players pressing Quick Play together rarely end up hosting two empty Sessions; `CreateRoomAsync` hosts a Session named after a fresh Room Code (retrying on a name clash); `JoinRoomAsync(code)` joins one; `LeaveAsync`; `RequestStart` (Room Host). Every join presents `AccountService.ToParticipantToken()` as the connection token. `Tick` on the Host starts the Match when `LobbyRules` says so: writes `MatchmakingResult(players, MaxSlots)` to `MatchmakingResultStore`, closes and hides the Session, and loads the Match scene through Fusion for every peer. State for the UI: `Stage` (`Idle`, `Connecting`, `Gathering`, `Starting`), `Mode`, `RoomCode`, `PlayerCount`, `SecondsLeft`, `Failure` (Russian text: "Матч уже начался" for a refused join, "Комната не найдена", "Не удалось подключиться", "Связь с хостом потеряна"; a new `Matchmaker` starts with the notice `SessionExit` kept), `Changed`.
- `SessionExit` — root entry point. `LeaveToMenu()` (or `LeaveToMenu(notice)`, which the Menu then shows through `TakeNotice()`) shuts the Session down and loads the Menu (Hud's "В меню" reaches it through Bootstrap's `MatchExitAdapter`); when a Session is lost while the Match scene is active (the Host left, or the Host disconnected a late Player), it loads the Menu too with "Связь с хостом потеряна". Clears the `MatchmakingResultStore`.
- `RoomCode` (`Simulation/`) — 5 symbols from `ABCDEFGHJKMNPQRSTUVWXYZ23456789` (no I, L, O, 0, 1); `Generate(Random)`, `TryParse` (trims, upper-cases, rejects anything outside the alphabet).
- `LobbyRules` (`Simulation/`) — `ShouldStart` (Quick Play: timer over or Session full; Room: Host pressed Start), `MatchmakingResultFor` (clamps to 1..MaxSlots), `SecondsLeft` from a Unix-millisecond deadline, `FoundNothingToJoin` (`NotFound` or `Refused`), `HostJitterSeconds(roll, min, max)`.
- `MatchmakingConfig` — search seconds (10), max Slots (6), Room Code attempts, Quick Play host jitter (0.3–1.5 s); asset `_Project/Configs/MatchmakingConfig.asset`.
- `MatchmakingPool` — the pool name every Session carries as its `pool` property: `<DevSessionName>-<Application.version>`, built by `RootLifetimeScope`, so Editors on agent worktrees never match each other while the main checkout's Editor matches phones and builds of different versions never mix.

## Rules worth knowing

- Session properties: `pool` (filter for Quick Play), `deadline` (Quick Play Host's search end, Unix ms as a string; Fusion cannot add a property after creation, so it is set when hosting), `code` (Room Sessions; a Quick Play joiner who lands in a Room switches to Room mode and waits for that Host).
- Room Session names are `<pool>-room-<code>`; joining one is `GameMode.Client` with that name, and `GameNotFound` becomes "Комната не найдена".
- Only the Host decides the start; clients show the deadline from the property. The Menu scope being destroyed by the networked scene load must not leave the Session, so `Matchmaker.Dispose` only unsubscribes.

## Depends on

Infrastructure (`NetworkSession`, `MatchmakingResultStore`, `SceneLoader`, `SceneNames`), Account (`AccountService`), Shared (`MatchmakingResult`), Fusion, UniTask, VContainer.
