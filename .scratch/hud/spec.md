# Hud

Status: ready-for-agent

## Problem Statement

A phone Player has no way to drive (the only input is a keyboard or gamepad) and nothing on screen tells them the time left, how full the Bucket is, what Multiplier a Delivery locked, how the others are doing, when the next Blizzard comes, where the Drop-Off Zone is, or who won (GDD 3.4, 4.2, 4.4).

## Solution

A Hud feature in Gameplay: one uGUI canvas prefab with a safe-area root and small view scripts, each reading one read-only seam: `IMatchClock` and `IMatchResults` (Match), `IScoreReader` and `DropOffZone` (DropOff), `BucketRegistry` (Bucket), `VehicleRegistry` (Vehicle), and `SnowConfig` (Blizzard schedule). The pure presentation rules (time formatting, Blizzard warning window, Score rise batching, off-screen arrow placement) live in `Simulation/` and are covered by EditMode tests. The left virtual stick is Input System's `OnScreenStick` bound to `<Gamepad>/leftStick`, which Vehicle's existing `Player/Move` action already reads, so Vehicle's input source is unchanged.

## User Stories

1. As a phone Player, I want a virtual stick on the left, so that I can drive (GDD 3.4).
2. As a Player, I want a big 3-2-1 and "GO!" during Countdown, so that I know when to start.
3. As a Player, I want the time left at the top, so that I can plan the last delivery.
4. As a Player, I want a Load bar over my own Vehicle, highlighted when full, so that I know when to head for the zone.
5. As a Player delivering, I want the locked Multiplier over my Vehicle and floating "+N" as my Score rises, so that a full Bucket feels rewarding.
6. As a Player, I want every Participant's Score listed, so that I know where I stand.
7. As a Player, I want "Blizzard in N!" 5 seconds before a wave, so that I can decide between the zone and a Blizzard Pile (GDD 4.4).
8. As a Player, I want an arrow at the screen edge pointing at the Drop-Off Zone when it is off screen, so that I can find it with a follow camera.
9. As a Player, I want a Results screen with places, names, and Scores, and a countdown to the next Match; as the Host, a "Play again" button.
10. As a Player on a phone with a notch, I want the HUD inside the safe area.
11. As a game designer, I want the warning lead, popup timing, bar height, and arrow margin in a config asset.

## Implementation Decisions

- **Module:** `Gameplay/Hud`, asmdef `PlowParty.Gameplay.Hud`, references Match, DropOff, Bucket, Snow, Vehicle, VContainer, `UnityEngine.UI`. No `NetworkBehaviour`, so not woven. Folders `Simulation`, `View`, `Config`, `Prefabs`, `Tests`. Feature index row `planned` → `active`.
- **uGUI over UI Toolkit:** the Load bar and popups follow a world position every frame, `OnScreenStick` is a uGUI component, and Unity 6.0's UI Toolkit has no world-space panels; uGUI covers all of it with one canvas. Text uses the built-in `LegacyRuntime.ttf` (TextMesh Pro essentials are not imported); sprites are Unity's built-in UI skin (`UISprite`, `Knob`, `Background`). Custom `UI_` art goes to `Art/UI/` when it exists.
- **Canvas:** Screen Space Overlay, `CanvasScaler` scale with screen size, 1920×1080, match height. Children: `EventSystem` (Input System UI module), `WorldAnchored` (Load bar, popups, arrow; full screen, positioned in screen pixels), `SafeArea` (`SafeAreaFitter` clamps `Screen.safeArea` to the screen; timer, Score list, Blizzard banner, stick), `Results` (full-screen dim panel that blocks the stick).
- **Local Vehicle:** the registered `NetworkVehicle` with input authority. Names are placeholders: "You" and "Player N" (Slot + 1) until Nicknames reach the Match.
- **Load bar:** screen point of the Vehicle position plus `LoadBarHeight` metres up; fill = Load / Bucket capacity; full colour when `IsFull`. Multiplier badge (`×1`, `×1.5`, `×2`) while `IsDelivering`.
- **Score popups:** client-side deltas of the local Score (`ScoreRise`): the first observed Score and any drop (a new Match) reset the baseline; rises are batched so at most one popup per `ScorePopupInterval` (0.3 s), because DropOff credits Score every tick during a Delivery. Pool of six labels rising `ScorePopupRise` units over `ScorePopupDuration`. Only the local Player's Score pops.
- **Timer:** remaining Playing time rounded up (`m:ss`); Countdown shows the ceiling of the phase remaining (3, 2, 1), then "GO!" for `GoBannerDuration` of Playing. Labels rewrite only when the shown second changes.
- **Blizzard warning:** `BlizzardWarning` over `SnowConfig.ToSettings().BlizzardTimes` (already public, no Snow change) against `IMatchClock.PlayingElapsed`; shown while 0 < wave − elapsed ≤ `BlizzardWarningLead` (5 s).
- **Drop-Off arrow:** `EdgeArrow.TryPlace` hides the arrow when the zone centre projects inside the screen minus a margin; otherwise pins it to the margin rectangle along the ray from the screen centre (flipped when the point is behind the camera) and rotates the pointer.
- **Results:** rows from `IMatchResults` (place ordinals, local row highlighted), "Next match in N" or "Waiting for the host" when Results has no time limit, "Play again" button visible only when `CanRequestRestart` (Host in Results).
- **DI:** the views are scene components registered in `MatchScope` with `RegisterComponentInHierarchy`, plus the scene `Camera` (needed for world-to-screen). `HudConfig` asset in `_Project/Configs/`, registered in `RootLifetimeScope`.

## Testing Decisions

- EditMode tests on `HudText`, `BlizzardWarning`, `ScoreRise`, `EdgeArrow`. Assembly `PlowParty.Gameplay.Hud.Tests`.
- Views checked in Play Mode on the Host; screenshots `.scratch/hud/playing.png`, `.scratch/hud/results.png`.

## Out of Scope

- Portraits, gadget button, right stick (Gadgets).
- Nicknames, Medals.
- Animated transitions, sound, localisation.

## Further Notes

- Out-of-module edits: `MatchScope` (Hud registrations, `Camera`), `RootLifetimeScope` (+ prefab field), Bootstrap asmdef.
- The Camera module (parallel work) may also register the scene `Camera`; at integration keep one registration.
