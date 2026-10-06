# Camera

Status: ready-for-agent

## Problem Statement

The Match scene has one static perspective camera placed by hand for a 30×30 m Arena. It does not follow the local Player's Vehicle, cannot be tuned without editing the scene, and has no way to react to impacts. The user wants to find the right camera feel by playing, so several framings must be comparable in one Play session, with every number in a config asset.

## Solution

A CameraRig feature in Gameplay. Pure C# in `Simulation/` computes a `CameraPose` for each Camera Preset: `OverviewFraming` fits the whole Arena rectangle into the screen; `CameraFollow` follows a `CameraTarget` (the local Player's Vehicle) with smoothing, look-ahead, Arena edge clamping, optional yaw following, and a snap on teleport; `CameraShake` turns trauma added through `ICameraShake` into a decaying offset and roll. A `CameraDirector` MonoBehaviour on the `CameraRig` prefab finds the local Vehicle in `VehicleRegistry`, steps the active preset in `LateUpdate`, and writes the pose to its `Camera`. A development-only `CameraPresetSwitcher` cycles presets from an on-screen button or a key. `CameraRamShake` shakes the camera when the local Vehicle takes part in a Ram the Host reports.

## User Stories

1. As a Player, I want the camera to follow my Vehicle smoothly, so that I always see where I drive.
2. As a Player, I want to see more of the Arena in the direction I am driving, so that I can react to what is ahead.
3. As a Player near the Arena edge, I want the camera to stop before it shows mostly empty space beyond the wall.
4. As a Player, I want north to stay up by default, so that the Drop-Off Zone and the walls are always in the same place on screen.
5. As a Player, I want an optional mode where the camera turns with my Vehicle, so that the user can compare it with north-up.
6. As a Player, I want an overview that shows the whole Arena whatever its size or my screen's aspect, so that the Arena can grow from 30×30 to 40×40 m without retuning.
7. As a Player whose Vehicle spawns late or is teleported at a Match restart, I want the camera to jump to it at once, so that it does not swoop across the map.
8. As a Player who rams or is rammed, I want the camera to shake, so that the impact is felt.
9. As a developer of Snow or Gadgets, I want one `ICameraShake.Add(strength)` call, so that any effect can shake the local camera without knowing the rig.
10. As the game designer, I want every number (pitch, distance, field of view or orthographic size, smoothing, look-ahead, edge overscan, snap distance, shake) in one config asset that applies live in Play Mode, so that I tune by playing.
11. As the game designer, I want to cycle presets during play in the Editor and development builds, so that I compare them side by side, and I want that control absent from release builds.
12. As a developer, I want the camera math covered by fast EditMode tests.

## Implementation Decisions

- **Module:** `Gameplay/CameraRig`, asmdef `PlowParty.Gameplay.CameraRig`; `Camera` is avoided as a namespace segment because it would shadow `UnityEngine.Camera` inside the module. References Vehicle, VContainer, Fusion (`HasInputAuthority`), Input System (switcher key). No `NetworkBehaviour`, so it stays out of `AssembliesToWeave`. Feature index row added as `active`.
- **No Cinemachine:** the package is not installed. Three presets, one target, and a trauma shake fit in a few small types that are fully unit-testable; Cinemachine would add a dependency, a second camera object model, and tuning spread over its components instead of one config asset.
- **Presets (`CameraPreset`):** `Overview`, `Follow` (default), `FollowRotating`. Each has its own lens: perspective with a vertical field of view, or orthographic with a size (Overview computes its size from the fit).
- **Overview fit:** the camera looks at the Arena centre from the configured pitch and yaw; for perspective the distance is the smallest one that keeps every padded Arena corner inside the frustum (closed form per corner); for orthographic the size is the smallest that contains every corner, at a fixed distance. Arena bounds come from the union of the `Renderer` bounds under the scene's `Arena` root, read once at start, so a larger Arena needs no config change.
- **Follow:** focus = Vehicle position + look-ahead (`velocity × LookAheadTime`, capped at `MaxLookAhead`), clamped so the ground footprint of the view (the four frustum-corner rays hit on the ground plane) never extends more than `EdgeOverscan` metres past the Arena; if the Arena is narrower than the footprint on an axis, the focus centres on the Arena on that axis. The focus approaches the clamped target with `SmoothDamp` (`FollowSmoothTime`). Yaw is the configured `Yaw` (0 = north up) for `Follow`, and the Vehicle heading smoothed with `SmoothDampAngle` (`YawSmoothTime`) for `FollowRotating`.
- **Snap:** the first frame with a target, a preset change, and any frame where the target moved more than `SnapDistance` since the previous frame (Match restart teleport, respawn) set the focus and yaw at once and zero their velocities. Without a local Vehicle (spectator, before spawn) the follow presets show the Overview.
- **Shake (trauma model):** `ICameraShake.Add(strength)` adds trauma clamped to 0–1; trauma decays linearly at `DecayPerSecond`; offset is `MaxOffset × trauma²` along camera right/up and roll is `MaxRoll × trauma²`, both from Perlin noise at `Frequency`. `CameraShake` is a `MatchScope` singleton registered as itself and as `ICameraShake`; Snow and others depend only on the interface.
- **Ram shake:** `CameraRamShake` (entry point) subscribes to `VehicleRegistry.Rammed` and adds `RamVictimShake` or `RamRammerShake` when the local Vehicle is the Victim or Rammer. `Rammed` is raised on the Host only, so today only the Host's Player feels it; clients need a networked Ram signal (open question).
- **Live tuning:** `CameraDirector` rebuilds the settings structs from `CameraConfig` every `LateUpdate`; they are structs so this allocates nothing. Inspector edits to the asset in Play Mode apply immediately (and persist, as with any asset).
- **Dev switcher:** `CameraPresetSwitcher` on the prefab draws a small IMGUI button and listens for a key (default `C`); it destroys itself in `Awake` when `Debug.isDebugBuild` is false, so release builds never show it.
- **Purely local:** nothing is networked; host and clients run the same code against their own local Vehicle (`HasInputAuthority`).
- **Config:** `CameraConfig` ScriptableObject → `OverviewSettings`, `FollowSettings` (one per follow preset), `ShakeSettings`, ram strengths; asset `_Project/Configs/CameraConfig.asset`, registered in `RootLifetimeScope`.
- **DI:** `MatchScope` registers `CameraShake` (self + `ICameraShake`) and the `CameraRamShake` entry point. `RegisterComponentInHierarchy<CameraDirector>()` is added in the integration change that places the prefab in `Match.unity`, because VContainer throws while building the scope when the component is missing from the scene.

## Testing Decisions

- EditMode tests in `PlowParty.Gameplay.CameraRig.Tests`, asserting only on returned poses, rectangles, and samples.
- Cover: overview keeps every Arena corner on screen and touches the frustum on the limiting side, grows with Arena size, orthographic size fits wide and tall screens; view pose places the camera at the configured distance and pitch; ground footprint is centred sideways and longer ahead than behind for a tilted camera; follow snaps on first target and on teleport, smooths small moves, looks ahead along velocity up to the cap, clamps at the Arena edge with overscan, centres on a narrow axis, keeps yaw fixed or follows heading; shake clamps trauma, decays to zero, gives no offset without trauma.
- Not unit-tested: `CameraDirector`, `CameraPresetSwitcher`, `CameraRamShake`, Arena bounds reading; checked in Play Mode with screenshots of each preset.

## Out of Scope

- Placing the rig in `Match.unity` and removing the old camera (integration, done after parallel work lands).
- Shake calls from Snow Pile plowing and Gadgets (their modules call `ICameraShake`).
- Client-side Ram detection.
- Camera collision with tall props, zoom by speed, split screen.

## Further Notes

- Out-of-module edits: `MatchScope`, `RootLifetimeScope` (+ prefab field), Bootstrap `CLAUDE.md`, `GLOSSARY.md` (Camera Preset, Camera Shake), feature index row in `docs/architecture.md`.
- ADRs touched: ADR-0005 (Simulation free of Fusion and `Time`), ADR-0001 (registrations only in scopes).
