# Bots and WaitingForPlayers

Status: ready-for-human

## Problem Statement

A Match only has the Players who happen to be connected: a lone Player drives around an empty farm, nobody competes for Snow, and the HUD shows "Player N" for everyone. GDD 3.1–3.3 and 8 want 4–6 Participants per Match, empty seats taken by Bots that are indistinguishable from people, a short wait for the Players matchmaking found, and a mix of one strong and several medium or weak Bots.

## Solution

Three pieces, split by module:

- **Match** gains a first phase, WaitingForPlayers. The Host reads the `MatchLineup` Meta stored (expected Players, Slot count) or falls back to the Players present in six Slots, seats Players as they connect, adds Bots one by one at random times inside an arrival window into the Slots not reserved for expected Players, and starts Countdown when every Slot is taken. At the wait cap every free Slot gets a Bot at once. A Player connecting after that is refused. Seating lives in `MatchSeating`; the rules in pure `WaitingRules`.
- **Participants** (new module) holds the Participant Profile per Slot on a scene `NetworkObject`: Nickname (from the `ParticipantToken` in the Fusion connection token, else "Player N"; Bots from a pool), Critter Species (random, preferring unused), and whether it is a Bot; plus the Account Id host-side for the future Roster. A view seats the Critter model on each Vehicle.
- **Bots** (new module) drives every Bot's Vehicle on the Host with a utility AI: it perceives everything (Snow Grid, Loads, Piles, clock), chooses between collecting, delivering, chasing a Snow Pile, ramming a loaded rival and evading a rammer, navigates a coarse grid with A*, avoids other Vehicles, and gets unstuck. Difficulty profiles make it human-like: reaction delay, decision interval, steering noise, mistakes, aggression, greed, caution.

## User Stories

1. As a Player, I want the Match to wait a few seconds for the friends matchmaking found, so that we play together.
2. As a Player, I want to see "Match starts soon. Waiting for players N/Max" and the time left, with N counting everyone already in, so that I know what is happening.
3. As a Player, I want empty seats to fill with Bots one after another during the first seconds, so that the Arena comes alive instead of popping full.
4. As a Player, I want Countdown to start as soon as every seat is taken, or at the wait cap with Bots in the remaining seats, so that I never wait long.
5. As a Player who arrives after Countdown started, I want to be turned away cleanly, so that the Match I would join is not unfair; Meta will send me back to the Menu.
6. As a Player, I want Bots to have ordinary names and Critters and no marker, so that I cannot tell them from people (GDD 8).
7. As a Player, I want to see my Nickname and every rival's in the Score list and Results, and my Critter on my Vehicle.
8. As a Player, I want Bots to collect Snow, deliver it, top up to a Multiplier tier when it is cheap, chase Blizzard Piles, ram loaded rivals and dodge rams, so that they feel like real opponents.
9. As a Player, I want exactly one strong Bot per Match and the rest medium or weak, so that I sometimes win and sometimes lose (GDD 8).
10. As a Player, I want Bots never to sit stuck on a hay bale or a wall, so that the Match looks alive.
11. As a Player pressing "Play again", I want the same Participants back, a Bot in the seat of anyone who left, and the next Countdown at once.
12. As a Host on a phone, I want Bots to think a few times per second, not every tick, so that my device stays smooth.
13. As a developer building the backend Roster, I want each Player's Account Id on the Host, so that Match registration can send it.
14. As a game designer, I want the wait cap, the Bot arrival window, the fallback Slot count, the Bot name pool, every Bot tunable and the three difficulty profiles in config assets.
15. As a developer, I want waiting rules, naming rules, utility scoring, grid building, A*, steering, stuck detection and difficulty assignment covered by EditMode tests.

## Implementation Decisions

- **Phases:** `MatchPhase` WaitingForPlayers (new, first, value 0), Countdown, Playing, Results. `MatchRules.NextPhase` gained `allSlotsFilled`: WaitingForPlayers → Countdown only when every Slot is taken; the cap is enforced by `WaitingRules.BotsToSeat` returning every free Slot once the phase reaches `WaitingDuration`, which fills them the same tick. `PhaseRemaining` in WaitingForPlayers counts the cap down; input stays locked; Playing time is untouched.
- **Seat plan (`WaitingRules.PlanSeats`):** with a lineup, `MaxSlots` and `ExpectedHumans`, clamped to the Spawn Points; without one, the Players present (at least the Host) in `FallbackSlotCount` 6. `ScheduleBotArrivals` draws one time per planned Bot inside `BotArrivalStart`–`BotArrivalEnd` (1–8 s), sorted. `BotsToSeat` seats the Bots that are due, never into a Slot still reserved for an expected Player; an extra Player takes a free Slot and so one Bot fewer arrives.
- **Late Players:** refused at Fusion's connection request once Countdown started or every Slot is taken (`NetworkRunnerEvents.ConnectRequested` is new); a join that slips through is disconnected. Both are logged; `MatchSceneQuickStart` logs a failed start instead of throwing. Meta will route back to the Menu.
- **Play again:** the Session keeps its Participants and their profiles; `RespawnAll` respawns every Vehicle (Players and Bots) in its Slot, then every Slot a Player left gets a Bot, then Countdown. No second wait: the Roster is fixed per Match and a returning Player cannot join mid-Session anyway.
- **Seating (`MatchSeating`):** a host-side VContainer entry point owning Slot assignment; `VehicleSpawner` lost its join handling and became `Spawn(runner, slot, driver)` / `Despawn` / `RespawnAll` keyed by Slot. Players take the first free Slot, Bots a random free one. Arrivals are queued on `PlayerJoined` and seated on the next tick, after the roster has spawned.
- **Participant Profile:** `ParticipantRoster` (scene `NetworkObject`) with `[Networked] NetworkArray<ParticipantSeat>` per Slot (Nickname as `NetworkString<_16>` matching the backend's 16-character limit, Species, IsBot, IsSeated) and the open Slot count. Account Ids stay on the Host. Hud reads `NicknameOf(slot)` (cached string); the local row keeps its highlight and drops "You". Critter: `CritterSeatView` on `Vehicle.prefab` instantiates `V_Critter_<Species>` under the `CritterSeat` anchor.
- **Bot control:** `NetworkVehicle.SetHostInput` writes `LastMove` for a Vehicle with no input authority (`IsDrivenByHost`); `VehicleWorldDriver` already falls back to `LastMove` when no Player input exists, and clients predict Bots from the same field.
- **Bot loop (`BotDriver`, scene `NetworkObject`, Host only, after `MatchDriver` and before `VehicleWorldDriver`):** per tick a `BotWorld` snapshot (Slot, position, velocity, forward, Load per Vehicle; Playing time left); the snow map refreshes every 0.25 s; each `BotBrain` decides every 0.2–0.5 s (profile) and steers every tick. Bots idle outside Playing and reset at every Playing start.
- **Navigation:** `NavGrid` 1 m cells over the Snow Grid rectangle, blocked within `NavClearance` 0.7 m of any Obstacle box or circle; `NavPathfinder` A* (8-connected, no corner cutting, octile heuristic, stamped arrays, array heap, no allocation per search), string-pulled waypoints, direct line when in sight, replans throttled to 0.5 s and only when the goal moved over 1.5 m. Local avoidance pushes away from Vehicles ahead within 2.5 m. Unstuck: no 0.5 m of progress in 1 s while driving → 0.7 s toward open space (away from nearby blocked cells, ±45°) and a replan; still for 3 s is logged.
- **Utility:** five actions scored from a `BotSituation`. Collect grows with free capacity and nearby Snow richness. Deliver grows with Load (floor + share^1.5, a bonus when full and when the zone is near), is discounted by greed when a tier (51 or 100) is within 15 Load and Snow is near, and wins outright when the Match ends sooner than the trip plus 4 s. ChasePile grows with free capacity and Snow scarcity, falls with distance. Ram needs a rival with ≥ 20 Load, in sight, within 10 m, outside the zone, weighted by aggression. Evade needs a rival closing at ≥ 3 m/s inside a 35° cone within 6 m, weighted by caution and own Load. The current action gets a 0.15 commitment bonus; a mistake roll under the profile's chance picks the second best, but never an action scoring zero.
- **Human-likeness:** a changed action applies after the reaction delay; steering is rotated by a per-decision noise; snow targets are jittered by the mistake chance; weak profiles cap throttle. The Collect target is kept until reached (2 m), mostly scraped, or 3 s old, so bots drive lines instead of hopping between cells.
- **Difficulty:** `BotDifficultyRules`: the first Bot seated in a Session is Strong, later ones Weak or Medium by `WeakShare` 0.5. Bots persist across "Play again", so every Match keeps exactly one Strong Bot. Profiles (`BotProfileStrong/Medium/Weak.asset`): reaction 0.15 / 0.35 / 0.6 s, decision 0.2–0.3 / 0.3–0.4 / 0.4–0.5 s, noise 4 / 12 / 18°, mistakes 3 / 12 / 25 %, aggression 1 / 0.6 / 0.3, greed 0.9 / 0.5 / 0.2, caution 1 / 0.6 / 0.25, throttle cap 1 / 0.93 / 0.85.
- **HUD strings:** English, all in `HudText` (GO, the waiting banner, next Match, Blizzard, Score popup); the GDD's Russian waiting text is kept as the meaning, not the language, for consistency.
- **Config:** `MatchConfig` (+ wait cap 15 s, arrival window 1–8 s, fallback Slots 6), `ParticipantsConfig` (24 Bot Nicknames, six Critter prefabs), `BotConfig` (navigation, perception, weights, the three profiles), all in `_Project/Configs/`, registered in `RootLifetimeScope`.
- **Modules:** `Gameplay/Participants` (asmdef refs Vehicle, Fusion, VContainer) and `Gameplay/Bots` (Participants, Match, DropOff, Bucket, Snow, Vehicle, Fusion, VContainer), both woven. Match now references Shared, Infrastructure and Participants.

## Testing Decisions

- EditMode, `Method_Condition_ExpectedResult`, pure Simulation only.
- Match: `WaitingRulesTests` (lineup and fallback plans, clamping, arrival schedule, due Bots, reserved Slots, extra Player, cap fill, full Slots), phase and clock tests for WaitingForPlayers.
- Participants: `ParticipantProfilesTests` (token Nickname trimmed and cut, fallback, unused pool name, numbered duplicate, empty pool, unused species, all species taken).
- Bots: `NavGridTests`, `NavPathfinderTests` (straight leg, wall detour with clear legs, walled-in goal, ring with one gap, start inside clearance), `BotSnowMapTests`, `BotUtilityTests` (full, empty, greedy and modest at a tier, past a tier, Match ending, ram aggressive and peaceful, evade loaded and empty, pile when Snow is scarce, mistake, no false mistake, commitment), `BotSteeringTests`, `BotStuckWatchTests`, `BotDifficultyRulesTests`, `BotBrainTests` (drives into Snow, heads to the zone when full, stands still to unload, reaction delay, unstuck, mistake-prone bot still delivers).
- Hud: new `HudText` strings.
- Not unit-tested: `MatchSeating`, `ParticipantRoster`, `BotDriver`, `CritterSeatView`; checked in Play Mode as Host from `Match.unity`.

## Out of Scope

- Gadgets and Loot for Bots (no such features yet).
- Menu routing of a refused late Player (Meta).
- Sending the Roster with Account Ids to the backend (Meta / Match registration).
- Player-chosen Critters.
- Host migration of Bot brains.

## Further Notes

- Out-of-module edits: Vehicle (`VehicleSpawner` rewritten as a Slot-keyed spawn service, `NetworkVehicle.IsDrivenByHost` / `SetHostInput`, `VehicleRegistry.IsSlotTaken` removed, `Vehicle.prefab` + `CritterSeatView`), Snow (`SnowGridDriver.Width` / `Height` / `GetHostDepth`), Infrastructure (`NetworkRunnerEvents.ConnectRequested`), Hud (names from the roster, `WaitingForPlayersView`, strings into `HudText`, `Hud.prefab`), Bootstrap (`MatchScope`, `RootLifetimeScope` + prefab, asmdef, `MatchSceneQuickStart`), `NetworkProjectConfig.fusion`, `Match.unity` (`ParticipantRoster`, `BotDriver`), `docs/architecture.md`, `GLOSSARY.md`, ADR-0016.
- Contradictions resolved: the architecture note put "waits for players, fills with bots" in Meta/Session; with the user it moved into Match (ADR-0016). GLOSSARY lists "Lineup" under Roster's _Avoid_ while the Meta contract type is `MatchLineup`; left as is and flagged.
- Playtest (Host alone in the Editor, fallback lineup, five Bots, two Matches back to back via the automatic restart): the waiting banner climbed to 6/6 and Countdown started within the 1–8 s arrival window. Match 1: Noodle (Strong) 2284, Snowdrop (Medium) 2252, Cocoa (Weak) 2204, Waffles (Weak) 1993, Pebble (Weak) 1951, the idle Host 927. Match 2 (same Participants, no re-wait): Snowdrop 2182, Waffles 2114, Noodle 2092, Pebble 2047, Cocoa 1862, Host 1390. Every Bot switched to Collect, Deliver and ChasePile many times; the Strong Bot chose Ram 5 / 10 times and Evade twice; Rams dealt 2–10 per Bot (collisions included). One unstuck manoeuvre in 360 Bot-minutes, zero 3-second stuck reports. Screenshots: `waiting.png`, `mid-match.png`, `results.png`, `results-match-2.png`.
- An earlier build left Weak Bots circling a reached Snow target with a full Bucket (mistake rolls re-chose Collect on its commitment bonus every tick and cancelled the pending Deliver); fixed by deciding once per arrival and never "mistaking" into a zero-score action; covered by `Step_FullBucketWhileMistakeProne_StillSwitchesToDeliver` and `Choose_MistakeWithNoRealSecondChoice_StillPicksTheBest`.
- Tuning left open: the Strong Bot does not dominate yet (spread about 15 %); Strong/Weak profiles and the utility weights are the knobs.
