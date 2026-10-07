# Account

The local Player's Account: guest login by Device Id, the Auth Token, the Nickname, and its rename screen (GDD 9.1, ADR-0011). Every other Meta feature talks to the backend through this module so the Auth Token and the 401 re-login live in one place. Spec: `.scratch/meta/spec.md`.

## Entry points

- `AccountService` — root-lifetime. `EnsureSignedInAsync` logs in once per app run (Boot calls it before loading the Menu; the Menu and `MatchSceneQuickStart` call it again, which is free); `SignInAsync` always logs in and rotates the Auth Token. `SendAsync(BackendRequest)` attaches the Auth Token, signs in first when offline, and on a 401 logs in again and retries once. `RenameAsync` checks `NicknameRules` before asking the backend and shows the backend's 422 message when the server still refuses. `ToParticipantToken()` is what a Player presents as the Fusion connection token. `Changed` fires on login, rename, and connection loss.
- `NicknameRules.Check` (`Simulation/`) — mirror of the backend's `normalize_nickname`: NFC, trimmed, 3–16 characters, letters, ASCII digits, space, `_`, `-`, no double spaces; error texts are Russian.
- `NicknameView` (`View/`) + `NicknamePresenter` (MenuScope entry point) — the Menu's Nickname panel: name, connection state, "Сменить ник" editor with inline errors.

## Rules worth knowing

- The Device Id is a client-generated UUID, stored with the Account Id, Nickname and Auth Token as `account.json` through Infrastructure's `LocalFileStore`. In the Editor the folder is `<project>/Library/PlowParty/`, so each Multiplayer Play Mode virtual player (its own `Library/VP/<player>/`) and each worktree gets its own Account; a build uses `Application.persistentDataPath/PlowParty/`. PlayerPrefs were rejected because virtual players share them.
- Offline is a normal state: a failed login leaves `IsOnline` false, the cached Nickname (or "Гость") is shown, and the game stays playable; nothing is queued for later.
- Login rotates the Auth Token, so two processes sharing one `account.json` log each other out; that is why storage is per virtual player.

## Depends on

Infrastructure (`BackendClient`, `BackendRequest`, `LocalFileStore`), Shared (`ParticipantToken`), UniTask, VContainer, uGUI. Registered in `RootLifetimeScope` (service) and `MenuScope` (view, presenter).
