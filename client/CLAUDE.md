# client — Unity project

Unity 6000.0, URP, Android target. Photon Fusion 2 (host mode), VContainer for DI, UniTask for async. Architecture: `docs/architecture.md`. Rules: `docs/coding-standards.md`.

## Where code goes

All project code lives under `Assets/_Project/`. Each module is a folder with its own asmdef and `CLAUDE.md`; the feature index in `docs/architecture.md` lists them. Third-party content (Photon, plugins) stays outside `_Project/`.

When adding a script, place it in the module that owns the concept (check the feature index and `GLOSSARY.md`). When the owner is a new module, create the folder, asmdef, and `CLAUDE.md` together and add the index row.

## Self-documenting code

Code carries zero comments: no `//`, no `/* */`, no XML-doc `///`, no `#region`, no commented-out code, no TODO markers. Intent lives in names, small methods, and tests; context that names cannot hold goes into the module's `CLAUDE.md` or an ADR.

## Verify loop

The Unity Editor must be open on `client/` (the `com.unity.pipeline` package serves the CLI). After each logical change:

1. `unity recompile --project-path <absolute path to client>` — must report zero errors. Fix and repeat. Pass the absolute path: with several Editors open, a relative path can reach the wrong one.
2. `unity command --project-path <absolute path to client> --timeout 300 --result-only run_tests --mode editor --filter PlowParty.Gameplay.<Feature> --filter_type assembly` — run the feature's suite while iterating, every suite before committing. `unity test` only works in batch mode and refuses while the Editor is open.

Leave Play Mode (`editor_stop`) before running tests or recompiling; a forgotten Play Mode session makes later commands hang.

If the Editor is closed, `unity status` shows nothing connected: open it with `unity open client` (the project root is `client/`, never the repo root), or ask the user. Avoid batch mode while the user may have the Editor open.

`eval` / `eval_file` run a method body: no `using` directives, so write fully qualified type names.

`unity command` arguments are flags: `unity command --project-path client eval_file --file <path>`. Package changes in `manifest.json` reach an unfocused Editor only after `unity command --project-path client package_resolve`. The `unity-cli` skill covers the remaining commands (scenes, prefabs, play mode, console logs).

## Android build

Requires the Unity 6000.0.80f1 Android Build Support module (with Android SDK & NDK Tools and OpenJDK). The Editor on the project must be closed for a batch build:

```
unity run <absolute path to client> --log-file <log path> -- -buildTarget Android -executeMethod PlowParty.Editor.Build.AndroidBuild.BuildDevelopmentApkFromCommandLine
```

Or menu **Plow Party → Build → Android Development APK** in an open Editor. Output: `client/Builds/Android/PlowParty.apk` (gitignored), a development build that starts in Boot, logs in, and opens the Menu. Install with `adb install -r client/Builds/Android/PlowParty.apk`. Player settings and their rationale: `Assets/_Project/Editor/CLAUDE.md`.

## Backend

The Menu, the Tournament panel and Match reporting talk to the tournament API (`backend/`). Start it with `docker compose up --build` in `backend/` (`backend/CLAUDE.md`); it listens on port 8000. The Editor uses `BackendConfig`'s Editor URL (`http://localhost:8000`). A phone cannot reach `localhost`: set `_Project/Configs/BackendConfig.asset` → Device Base Url to `http://<PC LAN IP>:8000` (`ipconfig` on the PC, same Wi-Fi, port 8000 allowed through the PC firewall) before building; it is committed as the development PC's address, so another PC or network needs it changed (Inspector, or the `_deviceBaseUrl` line of the asset) and the APK rebuilt. Plain HTTP works only in development builds. Without a backend the game is fully playable: the Nickname falls back to the cached one or "Гость", the Tournament shows "Нет связи", and Matches are not reported. Each Editor (and each Multiplayer Play Mode virtual player) keeps its own Account in `client/Library/PlowParty/account.json`; delete it to start as a new Account.

## Multiplayer check

Multiplayer Play Mode (`com.unity.multiplayer.playmode`) runs extra virtual players beside the main Editor; Fusion supports it. Window → Multiplayer → Multiplayer Play Mode, tick Player 2 (up to Player 4). For the full flow open `Assets/_Project/Scenes/Boot.unity`, press Play, and press "Быстрая игра" in each instance within the search time (or press "Создать группу" in one and type its Party Code in the others). For gameplay only, open `Assets/_Project/Scenes/Match.unity` and press Play in the main Editor. `MatchSceneQuickStart` joins every instance to the same `AutoHostOrClient` session: the first becomes Host, the rest Clients. The session name is derived from the project folder (virtual players share it), so Editors of different worktrees never join each other; set `PLOW_PARTY_DEV_SESSION` to the same value in two Editors to join them on purpose. Keyboard input goes only to the focused Game view. `PhotonAppSettings` pins `FixedRegion` to `eu`: without a fixed region every instance picks its own best region and they never meet, and region pinging from Russia can time out (`PhotonCloudTimeout`). Virtual players are a user-side check: the CLI drives only the main Editor.

## Unity gotchas

- Every asset and folder under `Assets/` has a `.meta` file holding its GUID. Move and rename with `git mv` on both the file and its `.meta`, or through the Editor; never delete a `.meta` for an asset that stays.
- Scenes, prefabs, and ScriptableObject assets are YAML with cross-file GUID references. Create and edit them through the Editor or `unity command`, not by hand.
- Fusion 2.1.3 lives in `Assets/Photon/` (imported `.unitypackage`, upgrade by re-importing). The App ID is in `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`; network settings in `NetworkProjectConfig.fusion` next to it. Fusion's Weaver rewrites `NetworkBehaviour` IL after compilation, so `[Networked]` properties must be auto-properties `{ get; set; }`.
- Fusion's Weaver only processes assemblies listed in `AssembliesToWeave` in `NetworkProjectConfig.fusion`. Every asmdef that declares a `NetworkBehaviour` or `INetworkInput` is added there, or the session fails to start with "has not been weaved".
- In Play Mode the Editor ignores keyboard input unless the Game view has focus. An Editor started with `unity open` comes to the foreground, so typing meant for another window (WASD in ordinary words) reaches the Game view and drives the local Vehicle, and a shortcut such as Ctrl+P stops Play Mode; this is the "Host's idle Vehicle drives by itself" effect, not game code. `capture_game_view --save_path` is project-relative and lands under `Assets/`: move the file out and delete the folder with its `.meta` afterwards. Gamepads are not gated by focus: an `eval_file` that adds a virtual `Gamepad` and queues `GamepadState.leftStick` from an `EditorApplication.update` callback drives the local Vehicle through the real `Player/Move` path. A human using the same Game view competes with it.
- An asmdef that declares an `[Rpc]` sets `"allowUnsafeCode": true`. The weaved RPC calls Fusion's internal `NetworkBehaviourUtils.CheckInvokeRpc`, and Mono allows that only from an assembly compiled as unsafe; without it the first call throws `MethodAccessException: ... CheckInvokeRpc ... is inaccessible`, although the assembly is woven. `Assembly-CSharp` gets the flag from Player Settings. `Infrastructure/Tests/NetworkWeavingTests` fails when either requirement is missed.
- Precompiled DLLs (Fusion runtime) are visible to every asmdef by default. An asmdef that must stay Fusion-free sets `"overrideReferences": true` and lists only the DLLs it may use; source asmdefs (`Fusion.Unity`, `VContainer`, `UniTask`) are referenced by name.
- A new git worktree may check out LFS files (the Fusion DLLs in `Assets/Photon/Fusion/Assemblies/`) as 130-byte pointers; Photon then fails to compile and the Editor stops at "Enter Safe Mode?". Run `git lfs pull` in the worktree before opening it.
- After adding an assembly to `AssembliesToWeave`, the Weaver may run before the edited config is reimported: request a clean script compilation (`CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache)` via `eval`) before entering Play Mode.
