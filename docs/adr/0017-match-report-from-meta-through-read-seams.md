# Match reporting lives in Meta and reads Gameplay through adapters

The client side of ADR-0013 (register, confirm, vote) is `Meta/Tournament`'s `MatchReporter`, registered in `MatchScope`. Meta may not reference Gameplay, so the reporter declares two narrow ports, `IMatchProgress` (phase, Match Number, Playing time, final and live Scores per Slot) and `IMatchRoster` (Slot to Account Id, empty for a Bot), and Bootstrap implements them as adapters over Match's `IMatchClock` and `IMatchResults`, DropOff's `IScoreReader`, and Vehicle's `VehicleRegistry`. The `match_id` reaches clients through `MatchReportLink`, a tiny `NetworkObject` the Host spawns at runtime from a prefab referenced by `TournamentConfig`, so the Match scene needs no new scene object. HTTP calls go through the root-scoped `MatchReportApi`, which is cancelled only when the app quits, so a Vote sent at Results survives the player leaving to the Menu.

## Considered Options

- Reporting inside `Gameplay/Match`: direct access to the phase and Placements, but Gameplay would know the backend, against the Gameplay/Meta boundary in `docs/architecture.md`.
- A scene `NetworkObject` for the `match_id` in `Match.unity`: no runtime spawn, but it ties the Meta feature into the Match scene file that Gameplay owns.
- Sending the Roster to clients in an RPC: no new object, but a late scope would miss it; a networked property is simply read when needed.

## Consequences

Phase values Meta does not know (such as a WaitingForPlayers phase) map to `MatchProgressPhase.Waiting` in the adapter, so the reporter registers only at Countdown. The first `IMatchRoster` adapter reads Account Ids from Fusion connection tokens and fills the remaining Slots up to the Matchmaking Result's `MaxSlots` with Bots; once Match owns a networked Participant profile, the adapter switches to it without touching Meta. Live Scores are sampled every frame during Playing so that a client whose Host disappears can still vote an Interrupted Match; clients sample at slightly different moments, so such Votes may disagree and the Match be rejected for lack of a majority.
