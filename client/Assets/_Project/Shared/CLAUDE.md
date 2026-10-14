# Shared

Plain data types that cross the Gameplay/Meta boundary, such as `MatchResult` and `PlayerId`. References nothing, so both sides can use it.

## Rules

- Only immutable data types and value objects; behaviour belongs to the module that owns the rule.
- A type goes here only when both Gameplay and Meta need it; otherwise it stays in its feature.

The asmdef sets `overrideReferences` with no precompiled DLLs, so Fusion types cannot leak in.

## Types

- `MatchmakingResult` — what Meta's matchmaking found before the Match scene loads: `ExpectedHumans` (Players the Host waits for), `MaxSlots`, `Mode` (`PartyMode`) and, for a Custom Game (`new MatchmakingResult(setup)`), the `Setup`. Meta writes it into Infrastructure's `MatchmakingResultStore`; Match reads it on the Host during WaitingForPlayers.
- `PartyMode` — Quick Play or Custom Game: the Party's chosen mode and the mode of the Match it starts.
- `CustomGameSetup` / `CustomGameSlot` — a Custom Game's Slots in order, empty ones dropped: a Player (`PlayerId` = `PlayerRef.RawEncoded`, picked Participant Color and Critter index in `CritterSpecies` order) or a Bot with its `BotDifficulty`; `PlayerCount`, `BotCount`, `SlotOfPlayer`.
- `BotDifficulty` — Weak, Medium, Strong.
- `ParticipantToken` — a Player's Account Id and Nickname, carried as the Fusion connection token (at most `MaxBytes`). Meta encodes it when joining; Gameplay decodes it on the Host to name the Participant and to build the backend Roster.
