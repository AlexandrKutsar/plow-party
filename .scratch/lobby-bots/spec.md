# Lobby + Bots integration

Status: ready-for-human

## Problem Statement

Meta (#15: Account, Lobby in the Menu, Session, Tournament, Match reporting) and Bots (#16: WaitingForPlayers, Participant profiles, utility-AI Bots) were built in parallel on the same contract commit. Each left a seam for the other: Meta reported a temporary Roster from connection tokens and registered on a phase it did not know; Bots logged a refused late Player instead of sending them anywhere; both wrote an ADR-0016; the hand-over record was called `MatchLineup` although GLOSSARY avoids "Lineup".

## Solution

One branch, `feature/lobby-bots`, that merges `feature/bots` then `feature/meta` with merge commits and wires them:

1. **ADRs:** Meta keeps 0016 (Session outlives scenes) and 0017 (reporting through read seams); the Bots ADR becomes 0018 (WaitingForPlayers and Bot fill in the Match).
2. **Matchmaking Result:** `MatchLineup` → `MatchmakingResult`, `MatchLineupStore` → `MatchmakingResultStore`, `LobbyRules.MatchmakingResultFor`; GLOSSARY gains "Matchmaking Result".
3. **Roster:** `Bootstrap/Adapters/ParticipantRosterAdapter` implements Tournament's `IMatchRoster` over Participants' `ParticipantRoster` (one seat per open Slot; a seated Player's Host-side Account Id, else a Bot). `ConnectionTokenRoster` is gone.
4. **Phase mapping:** `MatchProgressAdapter` maps WaitingForPlayers to `Waiting`, so the Host registers at Countdown, after every Slot is seated.
5. **Late Player:** `SessionStartOutcome.Refused` (Fusion `ConnectionRefused`, `GameClosed`). Room Code join → "Матч уже начался"; Quick Play treats a refusal like "nothing to join"; `MatchSceneQuickStart` leaves to the Menu with the notice through `SessionExit.LeaveToMenu(notice)`; a Session lost in the Match (Host left, late Player disconnected) shows "Связь с хостом потеряна" in the Menu (`SessionExit.TakeNotice` read by the new `Matchmaker`).
6. **Matchmaking Result in the Match:** the Host's `MatchSeating` reads the store Meta writes; verified in Play Mode ("Waiting for 1 Player(s) in 6 Slots (matchmaking result); 5 Bot(s) planned").
7. **Russian strings:** `HudText` (СТАРТ!, waiting banner, next Match, Blizzard, ordinals "1-е", "Ожидание хоста"), `Hud.prefab` static labels (Итоги, Играть снова, СДАЧА, placeholders), fallback Nickname "Игрок N", the development camera button.
8. **Bot difficulty:** every curve constant of `BotUtility` and `BotBrain` moved into `BotConfig` ("Utility curves", "Brain"); a per-profile `DeliverEagerness` (the Deliver score floor) lets Weak Bots deliver small Loads at a low Multiplier while the Strong Bot fills its Bucket. Weak also got lower throttle, more noise, more mistakes, slower reactions.
9. **Quick Play race:** before hosting, wait a random jitter (`MatchmakingConfig` 0.3–1.5 s) and retry the random join once (`LobbyRules.FoundNothingToJoin`, `HostJitterSeconds`, tested).
10. **SafeAreaFitter:** Hud uses `Infrastructure/UI/SafeAreaFitter`; its copy is deleted.
11. **Host's idle Vehicle:** not code. A probe on `Player/Move` showed `/Keyboard/w|a|d` events: the Editor opened by `unity open` takes OS focus, so typing meant for other windows reaches the Game view. Bots write only Vehicles with no input authority (`SetHostInput` guard).

## Testing Decisions

- EditMode: `LobbyRulesTests` (join outcome, jitter window), `HudTextTests` and `ParticipantProfilesTests` (Russian strings), renamed `WaitingRulesTests`; every suite green.
- Play Mode with the backend in Docker: Boot → Menu → rename → Quick Play (jitter + retry visible in the log) → Match "N/6" → Bots fill → Countdown → registration → Playing → Results → Vote accepted → `GET /matches/{id}` accepted with Slot 0 = the Account → leaderboard Rank 1.
- Bot spread measured over three Matches with the Host idle.

## Out of Scope

- `BotSteering` and `BotSnowMap` still hold their own small constants.
- Telling a disconnected late Player "Матч уже начался" instead of "Связь с хостом потеряна" (the Host's disconnect carries no reason).
