# Level (vertical slice integration)

Status: done (PR `feature/level`)

## Problem Statement

Snow polish (#12), Match + Hud (#11), and CameraRig (#10) were built in parallel against a 30 × 30 m test box with a static camera. None of them was wired into `Match.unity`: Snow still ran on a temporary clock and never reset, the camera rig and Hud were not placed, and the arena was grey cubes. The vertical slice needs one playable Match loop on a real map.

## Solution

Merge the three branches with real merge commits, wire their open seams, and build the Farm map: a 40 × 40 m snowed-in farmyard from existing art, readable from the follow camera and the overview.

## User Stories

1. As a Player, I want the Blizzard to arrive exactly when the HUD counts it down, so that the warning is trustworthy.
2. As a Player, I want every Match to start on fresh, full Snow with no leftover Piles, so that Matches are equal.
3. As a Player, I want Snow to stop regrowing and Blizzards to wait while the Match is in Countdown or Results.
4. As a Player plowing a heavy Pile, I want the camera to rumble, so that the slowdown feels physical.
5. As a Player, I want a farmyard with fences and buildings as walls, cover around the cauldron, and open fields of Snow, so that the map reads at a glance.
6. As up to six Players, I want Spawn Points spread evenly and away from obstacles.
7. As a developer with several worktrees open, I want my Editor's dev session not to join another worktree's Editor.

## Implementation Decisions

- **Merges:** `feature/snow-polish`, then `feature/match-hud`, then `feature/camera`, each a `--no-ff` merge. Conflicts kept both sides (GLOSSARY, Vehicle and Bootstrap `CLAUDE.md`, `MatchScope`, `RootLifetimeScope` + prefab, Bootstrap asmdef, feature index). No code assigned `NetworkVehicle.SpeedMultiplier`; ADR numbers stay unique (0015 is Snow's).
- **Snow clock seam:** Snow declares `ISnowClock` (`IsPlaying`, `PlayingElapsed`, `event MatchRestarted`) in `Snow/Network`; `MatchDriver` implements it (Match → Snow is allowed; Snow → Match would cycle through DropOff). `SnowGridDriver` ticks the grid only while `IsPlaying`, with `PlayingElapsed`.
- **Snow reset:** `SnowGrid.Reset(seed)` refills every snowfall Cell to `FullDepth`, clears Piles, Regrowth schedules, Blizzard progress, and the clock; the constructor uses it too. The Host calls it on `MatchRestarted` with a fresh seed and republishes the words.
- **Pile shake:** `CameraPileShake` (CameraRig, `ITickable`) calls `ICameraShake.Sustain(PilePlowShake = 0.4)` while `SnowGridDriver.IsPlowingPile(local Vehicle)`. CameraRig → Snow keeps the graph acyclic; Bootstrap stays logic-free. `Sustain` holds trauma at a floor instead of adding per frame, so the rumble does not depend on frame rate. `VehicleRegistry.TryGetLocal` is shared with `CameraDirector`.
- **Standards:** `docs/coding-standards.md` allows a single-implementation interface when it is a cross-module seam.
- **Dev session:** `DevSessionName.For(dataPath, isEditor, override)` → `plow-party-dev-<FNV-1a of the project folder>` in the Editor; MPPM virtual players (`<project>/Library/VP/<id>/Assets`) resolve to the same folder; `PLOW_PARTY_DEV_SESSION` overrides; builds keep `plow-party-dev`.
- **Scene:** old camera deleted; `CameraRig.prefab` (`_arenaRoot` = `Arena`), `MatchDriver.prefab`, `Hud.prefab` placed; `SnowGridView` Pile Burst = `FX_SnowBurst`. `MatchScope` registers `CameraDirector`, `CameraPileShake`, and `MatchDriver` as `ISnowClock`.
- **Map:** `SnowConfig` origin (−20, −20), size (40, 40) (800 words of 1024). Walls are collider-only boxes outside ±20 m with `V_Fence` rows on the line and five buildings in fence gaps; twelve Hay Bales ring the cauldron at 7.5 m on the diagonals; Wood Piles, Carts, a Well, a Chicken Coop, and a corner Hay Bale cluster in the mid field; Spawn Points on a 13 m circle. Obstacle prefabs in `Levels/Farm/Obstacles/` (collider + nested `V_` model). Decor (`Scenery`) sits outside `Arena` so it neither blocks Vehicles nor widens the camera bounds. Layout note: `Levels/Farm/CLAUDE.md`.

## Testing Decisions

- `SnowGridResetTests`: refills after Scrape, removes Piles, keeps masked and snow-free Cells empty, restarts the clock at 0 (Regrowth after a reset uses new-Match time), re-arms a Blizzard that already passed.
- `CameraShakeTests`: `Sustain` raises to the level, keeps higher trauma, holds against decay.
- Play Mode (Host, `Match.unity`), driven by a virtual Gamepad and `eval_file` hooks: Countdown → Playing → Results → "Play again" → Countdown of Match 2; Snow before/after restart; Blizzard announcement at 43 s ("Blizzard in 2!"); Delivery ×2 with "+N"; Pile plowing (speed 0.50, trauma 0.39, burst); each camera preset. Screenshots in this folder.

## Out of Scope

- Bots, Gadgets, Drifts, Loot (none placed on the map).
- Networked Ram signal for client Ram shake.
- New art; requests are in `art-requests.md`.

## Further Notes

- Out-of-module edits: Vehicle (`VehicleRegistry.TryGetLocal`), Match (implements Snow's seam, asmdef → Snow), CameraRig (asmdef → Snow), Bootstrap, `client/CLAUDE.md`, `docs/coding-standards.md`, `docs/architecture.md` (Levels row), `.scratch/match/spec.md`.
- ADRs touched: ADR-0005 (seams keep features acyclic), ADR-0008 (gameplay obstacle prefabs nest `V_` models), ADR-0012 (Snow state host-only; reset is host-only too).
