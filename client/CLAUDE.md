# client — Unity project

Unity 6000.0, URP, Android target. Photon Fusion 2 (host mode), VContainer for DI, UniTask for async. Architecture: `docs/architecture.md`. Rules: `docs/coding-standards.md`.

## Where code goes

All project code lives under `Assets/_Project/`. Each module is a folder with its own asmdef and `CLAUDE.md`; the feature index in `docs/architecture.md` lists them. Third-party content (Photon, plugins) stays outside `_Project/`.

When adding a script, place it in the module that owns the concept (check the feature index and `GLOSSARY.md`). When the owner is a new module, create the folder, asmdef, and `CLAUDE.md` together and add the index row.

## Self-documenting code

Code carries zero comments: no `//`, no `/* */`, no XML-doc `///`, no `#region`, no commented-out code, no TODO markers. Intent lives in names, small methods, and tests; context that names cannot hold goes into the module's `CLAUDE.md` or an ADR.

## Verify loop

The Unity Editor must be open on `client/` (the `com.unity.pipeline` package serves the CLI). After each logical change:

1. `unity recompile --project-path client` — must report zero errors. Fix and repeat.
2. `unity test client --mode EditMode --output client/Logs/test-results.xml` — add `--filter <TestClass>` while iterating, run the full suite before committing.

If the Editor is closed, `unity status` shows nothing connected: open it with `unity open client` (the project root is `client/`, never the repo root), or ask the user. Avoid batch mode while the user may have the Editor open.

`unity command` arguments are flags: `unity command --project-path client eval_file --file <path>`. Package changes in `manifest.json` reach an unfocused Editor only after `unity command --project-path client package_resolve`. The `unity-cli` skill covers the remaining commands (scenes, prefabs, play mode, console logs).

## Unity gotchas

- Every asset and folder under `Assets/` has a `.meta` file holding its GUID. Move and rename with `git mv` on both the file and its `.meta`, or through the Editor; never delete a `.meta` for an asset that stays.
- Scenes, prefabs, and ScriptableObject assets are YAML with cross-file GUID references. Create and edit them through the Editor or `unity command`, not by hand.
- Fusion 2.1.3 lives in `Assets/Photon/` (imported `.unitypackage`, upgrade by re-importing). The App ID is in `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`; network settings in `NetworkProjectConfig.fusion` next to it. Fusion's Weaver rewrites `NetworkBehaviour` IL after compilation, so `[Networked]` properties must be auto-properties `{ get; set; }`.
- Precompiled DLLs (Fusion runtime) are visible to every asmdef by default. An asmdef that must stay Fusion-free sets `"overrideReferences": true` and lists only the DLLs it may use; source asmdefs (`Fusion.Unity`, `VContainer`, `UniTask`) are referenced by name.
