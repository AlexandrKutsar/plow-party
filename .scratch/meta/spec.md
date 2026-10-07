# Meta: Account, Lobby, Session, Tournament

Status: ready-for-agent

## Problem Statement

The client boots straight into a dev Match: there is no Account, no way to find other Players, no Room Code for friends, and nothing reaches the backend that already registers Matches, takes Votes and ranks the daily Tournament (GDD 3.2–3.3, 9). A judged portfolio build needs the whole loop: start the app, see your Nickname, find a Match, play it, have the result counted, and see it in the Tournament, while staying fully playable without a backend.

## Solution

Four Meta features and an Infrastructure backend client. `Meta/Account` logs in as a guest by Device Id and owns the Auth Token. `Meta/Session` gathers Players in a Fusion Session while still in the Menu (Quick Play with a search timer, or a Room Code), then the Host loads the Match scene for everyone through Fusion. `Meta/Lobby` draws the Menu's play and Lobby panels. `Meta/Tournament` shows "Турнир дня" and reports every Match (register, confirm, Vote) from a Meta-side reporter that reads Gameplay only through Bootstrap adapters. The Fusion runner moves to the root scope so a Session spans Menu and Match (ADR-0016); reporting is recorded in ADR-0017.

## User Stories

1. As a Player, I want to start the app and land in the Menu already logged in, so that I never see a login form.
2. As a new Player, I want a Nickname assigned on first launch, so that I can play right away.
3. As a Player, I want to change my Nickname with the backend's rules explained in Russian, so that I know why a name is refused.
4. As a Player without a connection, I want to play anyway, so that a dead backend never blocks the game.
5. As a Player, I want "Быстрая игра" to put me with other searching Players or start a Session for me, with a visible timer and the number of Players found, so that I know the Match is coming.
6. As a Player, I want the Match to start when the timer ends, with Bots in the empty Slots, so that I never wait long.
7. As a Host of a Room, I want a 5-symbol code without ambiguous symbols and a "Начать" button, so that I can gather friends and start when ready.
8. As a friend, I want to type the code and land in that Room, and see "Комната не найдена" if I mistype it.
9. As a Player at Results, I want "В меню" to leave the Session, and "Ещё раз" (Host) to keep playing in it.
10. As a Player whose Host left, I want to be returned to the Menu instead of a frozen Match.
11. As a Player, I want every Match I finish to count for the Tournament without doing anything, so that my best Score of the day shows up.
12. As a Player, I want "Турнир дня" to show the top, my neighbours, my place, and my Medal, or "Нет связи" offline.
13. As a developer, I want the Match scene to stay playable on its own in the Editor, so that gameplay work does not need the Menu.
14. As a developer, I want the backend address in a config asset, `localhost` in the Editor and the PC's LAN address on a phone.

## Implementation Decisions

- **Modules:** `Meta/Account`, `Meta/Session`, `Meta/Lobby`, `Meta/Tournament`, each with asmdef, `CLAUDE.md`, `Simulation/` rules and `Tests/`. `Meta.Tournament` declares `MatchReportLink` and is added to `AssembliesToWeave`.
- **Backend client (Infrastructure/Backend):** `BackendClient.SendAsync(BackendRequest)` over `UnityWebRequest` with `BackendConfig.TimeoutSeconds` (4 s); outcomes `Ok`, `Offline` (no response), `Unauthorized` (401), `Rejected` (other non-2xx). JSON is Newtonsoft (`com.unity.nuget.newtonsoft-json` 3.2.2, already a transitive dependency, now explicit): `JsonUtility` cannot write `null` for a Bot's `account_id` or the full-Match `interrupted_at_seconds`, nor read `me: null`. `BackendConfig`: Editor `http://localhost:8000` (backend `compose.yaml`), device `http://<PC LAN IP>:8000`; Player Settings allow plain HTTP in development builds only (`InsecureHttpOption.DevelopmentOnly`, also applied by `AndroidBuild`).
- **Persistence (Infrastructure/Storage):** `LocalFileStore` writes JSON files to `StorageFolder.For(...)`: `<project>/Library/PlowParty` in the Editor (per worktree and per Multiplayer Play Mode virtual player), `persistentDataPath/PlowParty` in builds. PlayerPrefs rejected: virtual players share them, and two processes on one Account rotate each other's Auth Token.
- **Account:** login on every app start (`EnsureSignedInAsync`), re-login on 401 (ADR-0011); the default Nickname comes from the backend; `NicknameRules` mirrors `normalize_nickname`; the backend's 422 message is shown when it still refuses.
- **Session lifetime (ADR-0016):** `NetworkSession` is root-scoped; `MenuScope` and `MatchScope` bind their resolver and `NetworkRunnerEvents` with `NetworkScopeBinding` at build; a scope bound to a running Session gets `PlayerJoined` replayed after Fusion's scene load. The start's cancellation token is linked only for the start itself, so destroying the Menu never cancels the running Session.
- **Matchmaking:** Quick Play = random `GameMode.Client` join filtered by `pool`, else `GameMode.Host` with `pool` and `deadline`; Room = `GameMode.Host` named `<pool>-room-<code>` with `code`; join = `GameMode.Client` by name. Max players 6. Host starts by writing `MatchmakingResult(players, 6)`, closing and hiding the Session, and `runner.LoadScene("Match")`. Connection token = `ParticipantToken(accountId, nickname)`.
- **Leaving:** Hud's Results gets "В меню", raising Hud's own `IMatchExit` seam; Bootstrap's `MatchExitAdapter` calls `Meta/Session`'s `SessionExit`, which shuts the runner down and loads the Menu. A Session lost in the Match scene also returns to the Menu.
- **Reporting (ADR-0017):** `MatchReporter` in `MatchScope`; ports `IMatchProgress` and `IMatchRoster` implemented in Bootstrap over `IMatchClock`, `IMatchResults`, `IScoreReader`, `VehicleRegistry` and Fusion connection tokens. `match_id`, Match Number and Roster Slot mask travel on the runtime-spawned `MatchReportLink`. The temporary Roster seats Bots in every free Slot up to the Matchmaking Result's `MaxSlots` (6 by default), matching what the Bots feature fills.
- **Menu scene:** `Scenes/Menu.unity` at build index 1; Boot logs in, then loads it. Two columns anchored to halves of the safe area; all strings Russian; built-in font.
- **Configs:** `BackendConfig`, `MatchmakingConfig`, `TournamentConfig` in `_Project/Configs/`, registered in `RootLifetimeScope`.

## Testing Decisions

- EditMode, `Method_Condition_ExpectedResult`, return values only: `NicknameRules`, `RoomCode`, `LobbyRules`, `LobbyText`, `MatchReportRules` (Roster, Vote, interruption second, slot mask), `TournamentMapping`, `TournamentText`, API JSON round-trips against real backend payloads, `BackendResponse.OutcomeFor` and validation message, `StorageFolder`.
- Not unit-tested (Play Mode against the Docker backend): `NetworkSession`, `Matchmaker`, presenters, `MatchReporter`.

## Out of Scope

- Deployment of the backend, HTTPS, rate limiting.
- Medal frames on HUD portraits (Hud has no portraits yet).
- A Cyrillic art font (on the art request list).
- Host migration; retrying or queueing reports made offline.

## Further Notes

- Out-of-module edits: Infrastructure (`NetworkSession` rewritten for root lifetime, `NetworkScopeBinding`, events `SceneLoadDone`/`ShutDown`, `ResolverNetworkObjectProvider.Use`, `SceneLoader.ActiveSceneName`, `SceneNames`, Backend, Storage, `UI/SafeAreaFitter`), Bootstrap (scopes, adapters, `MenuScope`, `BootSceneFlow` → Menu, `MatchSceneQuickStart` skips when a Session runs), Hud (`IMatchExit`, `ResultsView._menuButton`, "Menu" button in `Hud.prefab`), Editor (`AndroidBuild` HTTP option), `NetworkProjectConfig.fusion`, `manifest.json`, `ProjectSettings` (HTTP option), Build Settings.
- Follow-up with the Bots branch: switch `ConnectionTokenRoster` to Match's networked Participant profile; Countdown must begin after WaitingForPlayers (the adapter already maps unknown phases to Waiting); keep one `MatchScope` registration list when both branches touch it.
- Follow-up: Hud's own `SafeAreaFitter` duplicates Infrastructure's `UI/SafeAreaFitter`; move Hud to it.
