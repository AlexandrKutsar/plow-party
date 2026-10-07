# Hud

The in-Match screen layer: the WaitingForPlayers banner, virtual stick, Countdown and Match timer, Load bar and Multiplier over the local Vehicle, Score popups, Score list, Blizzard announcement, Drop-Off arrow, and the Results screen (GDD 3.4, 4.2, 4.4). Views only read other modules' seams; the formatting and placement rules live in `Simulation/` as pure C#. Spec: `.scratch/hud/spec.md`.

## Entry points

- `Prefabs/Hud.prefab` — one Screen Space Overlay canvas (1920×1080 reference, match height) with an `EventSystem` (Input System UI module), `WorldAnchored` (Load bar, Score popups, Drop-Off arrow, positioned in screen pixels), `SafeArea` (timer, Score list, Blizzard banner, WaitingForPlayers banner, stick), and `Results`. Placed once in the Match scene.
- Views (`View/`, each registered in `MatchScope` with `RegisterComponentInHierarchy`): `MatchTimerView`, `LoadBarView`, `ScorePopupView`, `ScoreListView` (+ `ScoreRowView` rows), `BlizzardAnnouncementView`, `DropOffArrowView`, `ResultsView`, `VirtualStickView` (on `SafeArea`, shows `SafeArea/Stick` only while the Match is running and in Playing), `WaitingForPlayersView` (on `SafeArea/WaitingForPlayers`, shows its `Panel` during WaitingForPlayers: "Match starts soon. Waiting for players N/Max", N counting Players and Bots seated in `ParticipantRoster`, and the wait cap counting down). `SafeAreaFitter` and `LocalVehicle` (finds the Vehicle with input authority, projects a point above it) need no injection.
- Rules (`Simulation/`): `HudText` (every HUD string: seconds rounding, `m:ss`, `×1.5`, ordinals, GO, the waiting, Blizzard, next-Match and Score-popup texts), `BlizzardAnnouncement` (seconds to the next wave inside the warning lead), `ScorePopupBatch` (batches Score growth into popups), `DropOffArrow` (screen-edge position and angle for an off-screen point).
- `HudConfig` — warning lead, GO banner, Load bar height, popup interval/duration/rise, arrow margin; asset `_Project/Configs/HudConfig.asset`, registered in `RootLifetimeScope`.

## Rules worth knowing

- The stick is Input System's `OnScreenStick` on `SafeArea/Stick/Knob`, bound to `<Gamepad>/leftStick`; Vehicle's `Player/Move` action already reads that control, so Players on touch and keyboard share one input source. The stick object is deactivated outside Playing (Countdown, Results, no session yet), which also releases the virtual `<Gamepad>` control.
- uGUI, not UI Toolkit: world-following elements and `OnScreenStick` need it, and Unity 6.0's UI Toolkit has no world-space panels. Text uses the built-in `LegacyRuntime.ttf`; sprites are Unity's built-in UI skin until `UI_` art lands in `Art/UI/`.
- Score popups are client-side deltas of the local Player's Score: DropOff credits Score every tick of a Delivery, so rises are summed and popped at most once per `ScorePopupInterval`; a lower Score (a new Match) or a new Vehicle object resets the baseline.
- The Blizzard schedule is read from `SnowConfig.ToSettings().BlizzardTimes` against `IMatchClock.PlayingElapsed`, so the warning matches Snow as soon as Snow runs on the Match clock.
- `SafeAreaFitter.ClampedSafeArea()` clamps `Screen.safeArea` to the screen (the Device Simulator can report a safe area larger than the Game view); the safe-area root and the Drop-Off Arrow both use it, so the arrow never sits under a notch.
- Score Popups only fire during Playing and the Load Bar hides during Results: a Delivery still unloading after the end keeps crediting Score that the frozen Placements ignore.
- Labels are rewritten only when the shown value changes, so steady frames allocate nothing.
- Names come from `ParticipantRoster.NicknameOf(slot)` (the Player's Nickname or the Bot's pool name, "Player N" when unknown); the local row is marked by the highlight, not by a "You" label. Bots are not marked (GDD 8).
- Strings: all HUD text is English and lives in `HudText`, so a later localisation swaps one file. The GDD's Russian waiting text ("Матч скоро начнётся. Ожидание игроков N/Max") is shown in English for consistency with the rest of the HUD.
- The clock panel and Score list show only in Countdown and Playing; WaitingForPlayers has its own banner.

## Depends on

Match (`IMatchClock`, `IMatchResults`), Participants (`ParticipantRoster`), DropOff (`IScoreReader`, `DropOffZone`), Bucket (`BucketRegistry`, `BucketConfig`), Snow (`SnowConfig`), Vehicle (`VehicleRegistry`, `NetworkVehicle`, `PlaneProjection`), VContainer, uGUI, and the scene `Camera` registered in `MatchScope` (the `CameraRig` prefab's camera, the only one in `Match.unity`). Nothing references Hud except Bootstrap. No `NetworkBehaviour`, so the assembly is not woven.
