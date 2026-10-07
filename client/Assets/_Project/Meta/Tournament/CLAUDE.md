# Tournament

The daily Tournament screen and the client side of Match reporting: register the Match with its Roster, confirm each Player's seat, and submit every Confirmed Player's Vote (GDD 9.2–9.3, ADR-0013, ADR-0014, ADR-0017). Spec: `.scratch/meta/spec.md`.

## Entry points

- `TournamentService` — `LoadAsync` reads `/tournament/leaderboard` (top `TopCount`), `/tournament/leaderboard/me` (`AroundMeRadius`) and `/tournament/medals` for the local Account and maps them with `TournamentMapping`; `null` means offline.
- `TournamentPresenter` + `TournamentView` / `StandingRowView` (`View/`) — the Menu's "Турнир дня" panel: summary ("Ваше место: N из M", or "Нет связи" offline), own Medal, rows with the local Account highlighted, "Обновить".
- `TournamentMapping`, `TournamentText`, `Standing`, `TournamentBoard`, `Medal` (`Simulation/`) — top rows, then a gap row and the "around me" rows not already in the top.
- `MatchReporter` (`Network/`, MatchScope entry point) — per Match Number: the Host registers at Countdown start (`POST /matches`), publishes the `match_id` and Roster Slots on `MatchReportLink`; a client confirms (`POST /matches/{id}/confirm`) when the link shows the current Match; at Results each confirmed peer (the Host is confirmed by registering) votes Scores per Slot (`POST /matches/{id}/votes`). If the Session is lost during Playing, a confirmed client votes an Interrupted Match at the last sampled Playing second with the last sampled live Scores.
- `MatchReportRules` (`Simulation/`) — `BuildRoster` (Slots 0–5, malformed or repeated Account Ids become Bots), `CanRegister` (4–6 Slots, the Host seated), `SlotMask`/`SlotsOf`, `BuildVote` (every Roster Slot once, missing Score 0), `TryGetInterruptedSecond` (1–179).
- `IMatchProgress`, `IMatchRoster`, `MatchProgressPhase` (`Network/`) — the ports the reporter reads Gameplay through; Bootstrap implements them (`MatchProgressAdapter`, `ParticipantRosterAdapter`).
- `MatchReportLink` (`Network/`, `Prefabs/MatchReportLink.prefab`) — runtime-spawned by the Host through the scope's provider; `[Networked]` `MatchId`, `MatchNumber`, `RosterSlotMask`. `MatchReportLinks` (MatchScope) holds the spawned one. The assembly is in `AssembliesToWeave`.
- `MatchReportApi` — root-lifetime HTTP calls for registration, confirmation and Votes, cancelled only when the app quits; logs every outcome.
- `TournamentConfig` — `TopCount` (5), `AroundMeRadius` (2), the link prefab; asset `_Project/Configs/TournamentConfig.asset`.

## Rules worth knowing

- A Match is reported only when the Host is online and the Roster has 4–6 Slots; offline Players are seated as Bots; nothing is retried or queued.
- The backend refuses a Vote earlier than registration + Countdown + played seconds (5 s tolerance), so Matches shortened in `MatchConfig` for testing are rejected as "too early".
- The Roster comes from Participants' `ParticipantRoster` through `ParticipantRosterAdapter`: every open Slot, with the Account Id of a seated Player and a Bot otherwise. It is read at Countdown, after WaitingForPlayers has seated every Slot (`MatchProgressAdapter` maps WaitingForPlayers to `Waiting`).

## Depends on

Account (`AccountService`), Infrastructure (`BackendRequest`, `NetworkSession`), Fusion, UniTask, VContainer, uGUI, Newtonsoft JSON (DTOs in `Api/`).
