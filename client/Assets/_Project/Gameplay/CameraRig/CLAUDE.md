# CameraRig

The local camera of a Match: three switchable Camera Presets (Overview, Follow, FollowRotating) and Camera Shake. Purely local presentation; nothing is networked, and host and clients run the same code against their own Vehicle. The folder is not called `Camera` because that namespace segment would shadow `UnityEngine.Camera` inside the module. Spec: `.scratch/camera/spec.md`.

## Entry points

- `CameraFollow` — the follow test seam. `Step(target, settings, arena, aspect, deltaTime)` returns a `CameraPose`; `Focus` and `Yaw` expose the state; `Release()` makes the next `Step` snap.
- `OverviewFraming.Fit(arena, settings, aspect)` — the pose that fits the whole `ArenaVolume` (ground rectangle plus ground and top heights) on screen.
- `CameraView` — pitch, yaw, distance, lens: `PoseAt(focus)` and `GroundFootprint(aspect)` (the ground rectangle the view covers, relative to its focus).
- `ICameraShake` — the API other modules call (an interface so Gadgets depend on CameraRig's contract, not the rig). `Add(strength)` is a one-off kick: trauma 0–1, summed and clamped to 1 (0.25 light bump, 0.5 Ram victim, 1 maximum). `Sustain(strength)` holds trauma at least at that level while it is called every frame (a continuous rumble). `CameraShake` implements it and is a `MatchScope` singleton (as itself and as `ICameraShake`).
- `CameraDirector` — MonoBehaviour on `Prefabs/CameraRig.prefab`; injected with `CameraConfig`, `VehicleRegistry`, `CameraShake`. Each `LateUpdate` it picks the local Vehicle (`VehicleRegistry.TryGetLocal`), steps the active preset, adds the shake sample, and writes position, rotation, and lens to its `Camera`. `ActivePreset` / `CyclePreset()`.
- `CameraPresetSwitcher` — development tool on the same prefab: an IMGUI button (top right) and a key (`C`) that call `CyclePreset()`. Destroys itself in `Awake` unless `Debug.isDebugBuild`, so release builds never show it.
- `CameraRamShake` — `ITickable` entry point in `MatchScope`; each frame feeds the local Vehicle's networked Ram counts (`TimesRammed`, `RamsDealt`) to Vehicle's `RamWatch` and shakes once per new Ram: `RamVictimShake` (0.5) as Victim, `RamRammerShake` (0.25) as Rammer. Runs the same on Host and clients.
- `CameraPileShake` — `ITickable` entry point in `MatchScope`; while Snow's `SnowGridDriver.IsPlowingPile(local Vehicle)` is true it calls `Sustain(PilePlowShake)` (0.4: trauma² 0.16, a light rumble). Local Vehicle only, so every peer shakes for its own plowing.
- `CameraConfig` — asset `_Project/Configs/CameraConfig.asset`, registered in `RootLifetimeScope`: default preset, one block per preset, shake, Ram strengths, pile-plowing shake.

## Rules worth knowing

- Arena bounds are the union of `Renderer` bounds under the `Arena` root assigned on the scene instance (`_arenaRoot`), read once in `Start` into an `ArenaVolume` (ground height = minimum Y, top height = maximum Y). A bigger Arena needs no config change. `_arenaRoot` must be wired on the scene instance; the prefab leaves it empty and `Start` throws without it.
- Overview: perspective distance is the smallest that keeps all eight padded corners (ground and wall tops) inside the frustum (per-corner closed form, centred on the Arena); orthographic size is the smallest that contains every corner at `OrthographicDistance`.
- Follow focus = Vehicle position + `velocity × LookAheadTime` capped at `MaxLookAhead`, clamped so the view's ground footprint goes at most `EdgeOverscan` metres past the Arena (negative overscan keeps it inside); on an axis where the Arena is narrower than the footprint, the footprint (not the focus) is centred on the Arena, so a tilted view shows equal margins at both edges. Focus and yaw follow with `SmoothDamp` / `SmoothDampAngle`.
- Snap (no smoothing): first frame with a target, after `Release` (preset change, local Vehicle lost), and whenever the target moved more than `SnapDistance` in one frame — a Match restart teleport or respawn never swoops.
- Follow presets without a local Vehicle (before spawn, spectating) show the Overview.
- Shake: trauma decays linearly at `DecayPerSecond`; offset (camera right/up) and roll scale with trauma², driven by Perlin noise at `Frequency`.
- Settings are structs rebuilt from `CameraConfig` every frame, so Inspector edits in Play Mode apply live without allocations (and are saved into the asset, as with any asset).

## Decisions

- No Cinemachine: not installed; three presets and a shake fit in a few small, unit-tested types tuned from one config asset.
- Defaults are tuned for landscape (two-stick layout); a preset switch snaps instantly.
- Ram shake reads the networked Ram counts, not `VehicleRegistry.Rammed`: that event is raised on the Host only, so clients would never shake, and listening to both would shake the Host twice. A client shakes when the Host's state arrives (about one round trip after the hit), never on a mispredicted Ram.
- `CameraRig.prefab` is placed in `Match.unity` with `_arenaRoot` = `Arena`; `CameraDirector` is registered with `RegisterComponentInHierarchy`. Its `Camera` is the only one in the scene, so Hud's `RegisterComponentInHierarchy<Camera>` resolves it.
- Pile shake lives here, not in Snow or Bootstrap: CameraRig → Snow keeps references acyclic (Snow never needs the camera), and Bootstrap holds no logic. The cost is that Snow can never call `ICameraShake`; Snow exposes state (`IsPlowingPile`) and the camera reacts.

## Depends on

Simulation: `UnityEngine` math only. View and Network: Vehicle (`VehicleRegistry`, `NetworkVehicle`, `PlaneProjection`), Snow (`SnowGridDriver.IsPlowingPile`), Fusion (`HasInputAuthority`), VContainer, Input System. No `NetworkBehaviour`, so the assembly is not in `AssembliesToWeave`. Nothing references this module except Bootstrap; Gadgets will depend on `ICameraShake` only.
