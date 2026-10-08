# Lobby

The Menu's play panel and Lobby panel: "Быстрая игра", "Создать группу", Party Code entry, and, once in a Session, either the Quick Play search (title, search timer, Players found) or the Party (title "Группа <code>", one line of members with the Leader and Ready marks, Players, the action button: "Искать матч" for the Leader, enabled when the Party `CanStart`, "Я готов" / "Не готов" for other members) and "Выйти" (GDD 3.2 Lobby, 3.3). Presentation only; the logic is `Meta/Session` and `Meta/Party`. These are the minimal Party hooks of ticket 01; the tabbed Menu, Party rows with removal and the mode switch come with ticket 04. Spec: `.scratch/party/spec.md`.

## Entry points

- `LobbyPresenter` — MenuScope entry point; wires `PlayMenuView` and `LobbyPanelView` to `Matchmaker` and `PartyService`; the Party view wins while `PartyService.Stage` is not `None`, failures come from the Party first. Labels are rewritten only when the shown number changes.
- `PlayMenuView`, `LobbyPanelView` (`View/`) — uGUI views in `Scenes/Menu.unity`, raising C# events for clicks.
- `LobbyText` (`Simulation/`) — every Russian string and number format of the panel.

## Menu scene

`Scenes/Menu.unity` (build index 1): `Camera`, `EventSystem` (Input System UI module), `MenuScope`, and the `Menu` canvas (Screen Space Overlay, 1920×1080 reference, match height) with Infrastructure's `SafeAreaFitter` on `SafeArea`. `SafeArea/Left` holds Account's `Nickname` and `EditPanel`, this module's `PlayMenu` and `Lobby`; `SafeArea/Tournament` is Tournament's panel. Columns are anchored to halves of the safe area, rows by fixed heights. Text uses the built-in `LegacyRuntime.ttf` and Unity's built-in UI sprites until `UI_` art lands. Every `RegisterComponentInHierarchy` target in `MenuScope` must exist in this scene.

## Depends on

Session (`Matchmaker`, `LobbyStage`), Party (`PartyService`, `PartyStage`, `PartyState`), UniTask, VContainer, uGUI.
