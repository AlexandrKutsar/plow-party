# WaitingForPlayers and Bot fill run in the Match, on the Host

Matchmaking stays in Meta: it finds the Players and hands the Match a `MatchmakingResult` (expected Players, Slot count) through `MatchmakingResultStore`, and each Player's Nickname and Account Id as a `ParticipantToken` in the Fusion connection token. Everything after the Match scene loads belongs to Gameplay: a new first phase, WaitingForPlayers, in which the Host seats Players as they connect and adds Bots one by one into the Slots not reserved for expected Players, at random times in the first seconds, so the Arena fills up visibly. Countdown starts the moment every Slot is taken; at the wait cap every free Slot gets a Bot at once. From Countdown on the Slots are fixed, which is the Roster the Host registers (ADR-0013); a Player connecting later is refused. "Play again" keeps the Session's Participants, refills a Slot a Player left with a Bot, and goes straight to Countdown.

Who sits in a Slot is its own networked record, `ParticipantRoster` (Nickname, Critter Species, Bot or Player per Slot) on a scene object, not on the Vehicle, so it survives the respawn at every new Match. Bots drive Vehicles with no input authority by writing the same `VehicleInput` a Player sends (`NetworkVehicle.SetHostInput` → `LastMove`), so nothing downstream can tell them apart.

## Considered Options

- Waiting and Bot fill in Meta/Session before loading the Match (the original architecture note): Meta would have to spawn or describe Bots before the Match scene exists, and Players would wait on a menu instead of watching the Arena fill. The Host only learns who actually arrived after the scene's network objects exist.
- Bots spawned all at once at the cap: simpler, but the first seconds show an empty Arena and the "N/Max" counter jumps.
- Profile on the Vehicle (`NetworkBehaviour` on `Vehicle.prefab`): resets with every respawn, so Nicknames and Critters would have to be rewritten each Match.
- A separate input source object per Bot fed into Fusion's input: Fusion only polls input for connected Players; writing the Host-side fallback `LastMove` is the seam the Vehicle already had.

## Consequences

Match now depends on Infrastructure (`MatchmakingResultStore`, runner events), Shared and Participants; `VehicleSpawner` became a plain spawn-by-Slot service and lost its join handling. A late Player is refused at the connection request, so a client's session start fails with `SessionStartOutcome.Refused` and Meta brings that Player to the Menu with "Матч уже начался". The Match scene started on its own (no Matchmaking Result) expects only the Players already connected and fills six Slots, which keeps the Editor quick start playable with Bots.

## Amendment: one Match per Session, quick Bots (Party work)

"Play again" is gone: a Session plays exactly one Match, and Results stays, with no timer, until each Player presses "В меню" (`.scratch/party/spec.md`, stories 69–73). An automatic restart kept away-from-phone Players looping through zero-Score Matches, and the Party now gathers again in the Menu after every Match. The restart request, the Results timer, the respawn of every Vehicle and the Snow Grid reset on a new Match went with it.

The wait no longer runs the full Bot window when nobody else is coming: the tick every expected Player is seated, the Bots not seated yet are rescheduled to random times 0.5–2.5 s from then (`MatchConfig`), and the 15 s cap only fills the Slots while an expected Player is still missing. A solo Player reaches Countdown in about 2.5 s instead of about 8.

When the Host leaves mid-Match there is still no host migration: clients lose the Session, Tournament votes the Match Interrupted when it was Playing (ADR-0013), and Meta's `SessionExit` brings them back with "Хост вышел, матч прерван"; after Results it brings them back without a notice.
