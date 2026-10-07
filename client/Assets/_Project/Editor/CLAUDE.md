# Editor

Editor-only tooling for the project: import pipelines, validators, menu commands. One asmdef, `PlowParty.Editor`, compiled only in the Editor and referenced by nothing at runtime. Each tool gets a sub-folder.

## Entry points

- `ArtImport/ArtImportPostprocessor` — enforces the import rules in `Art/CLAUDE.md` for every model and texture under `Art/`. The rules live here as code, so a rebuilt or new asset needs no manual Inspector setup.
- `Build/AndroidBuild` — menu `Plow Party/Build`: `Apply Android Player Settings` writes the Android Player settings below into `ProjectSettings`; `Android Development APK` applies them and builds the enabled scenes of the Build Settings list (Boot, Menu, Match) into `client/Builds/Android/PlowParty.apk` as a development build (the camera preset switcher stays in). `BuildDevelopmentApkFromCommandLine` is the batch-mode entry: it exits 0 on success, 1 otherwise.

## Android settings

Package `com.alexandrkutsar.plowparty`, product "Plow Party", IL2CPP, ARM64 only, landscape only (auto-rotation between both landscape orientations), min API 26 (Android 8.0: Vulkan-capable devices, still nearly every phone in use), target API = highest installed. Plain HTTP is allowed in development builds only (`InsecureHttpOption.DevelopmentOnly`): the local backend has no TLS, and the Editor counts as a development build. Graphics: the `Mobile` quality level (Android's default) uses `Settings/Mobile_RPAsset` with `Mobile_Renderer`: no renderer features (no SSAO), no depth or opaque texture, no soft shadows, render scale 0.8. Snow's `SH_SnowSurface` needs only vertex texture fetch (`#pragma target 3.0`), which every GLES3 and Vulkan device has. Change a setting in `AndroidBuild`, not only in the Player Settings window, or the next build reverts it.

## Rules

- Runtime assemblies never reference this one; anything a build needs belongs in a runtime module.
- A tool that changes assets in bulk is a menu command or an import hook, never code that runs on Editor load.
