# Lobby

The Menu's play panel and Lobby panel: "Быстрая игра", "Создать комнату", Room Code entry, and, once in a Session, the title, search timer or Room instructions, Players found, "Начать" for a Room Host, and "Выйти" (GDD 3.2 Lobby, 3.3). Presentation only; the matchmaking logic is `Meta/Session`. Spec: `.scratch/meta/spec.md`.

## Entry points

- `LobbyPresenter` — MenuScope entry point; wires `PlayMenuView` and `LobbyPanelView` to `Matchmaker`. Labels are rewritten only when the shown number changes.
- `PlayMenuView`, `LobbyPanelView` (`View/`) — uGUI views in `Scenes/Menu.unity`, raising C# events for clicks.
- `LobbyText` (`Simulation/`) — every Russian string and number format of the panel.

## Menu scene

`Scenes/Menu.unity` (build index 1): `Camera`, `EventSystem` (Input System UI module), `MenuScope`, and the `Menu` canvas (Screen Space Overlay, 1920×1080 reference, match height) with Infrastructure's `SafeAreaFitter` on `SafeArea`. `SafeArea/Left` holds Account's `Nickname` and `EditPanel`, this module's `PlayMenu` and `Lobby`; `SafeArea/Tournament` is Tournament's panel. Columns are anchored to halves of the safe area, rows by fixed heights. Text uses the built-in `LegacyRuntime.ttf` and Unity's built-in UI sprites until `UI_` art lands. Every `RegisterComponentInHierarchy` target in `MenuScope` must exist in this scene.

## Depends on

Session (`Matchmaker`, `LobbyStage`, `LobbyMode`), UniTask, VContainer, uGUI.
