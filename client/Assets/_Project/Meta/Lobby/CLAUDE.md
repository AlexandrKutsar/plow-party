# Lobby

The Menu's play panel and Lobby panel: "Быстрая игра", "Создать группу", Party Code entry, and then either the search (title "Поиск игры…", a local stopwatch `m:ss` since the press, "Матч начинается…" on the Lobby Host once the Match loads, and "Остановить" for any searcher or Party member) or the Party (title "Группа <code>", one line of members with the Leader and Ready marks, Players, the action button: "Искать матч" for the Leader, enabled when `Matchmaker.CanSearch`, "Я готов" / "Не готов" for other members) and "Выйти", hidden while searching (GDD 3.2 Lobby, 3.3). Presentation only; the logic is `Meta/Session` and `Meta/Party`. These are the minimal Party hooks of ticket 01; the tabbed Menu, Party rows with removal and the mode switch come with ticket 04. Spec: `.scratch/party/spec.md`.

## Entry points

- `LobbyPresenter` — MenuScope entry point; wires `PlayMenuView` and `LobbyPanelView` to `Matchmaker` and `PartyService`; the search view wins while `Matchmaker.Stage` is not `Idle`, then the Party view while `PartyService.Stage` is not `None`; failures come from the Party first. The stopwatch label is rewritten only when the whole second changes.
- `PlayMenuView`, `LobbyPanelView` (`View/`) — uGUI views in `Scenes/Menu.unity`, raising C# events for clicks.
- `LobbyText` (`Simulation/`) — every Russian string and number format of the panel.

## Menu scene

`Scenes/Menu.unity` (build index 1): `Camera`, `EventSystem` (Input System UI module), `MenuScope`, and the `Menu` canvas (Screen Space Overlay, 1920×1080 reference, match height) with Infrastructure's `SafeAreaFitter` on `SafeArea`. `SafeArea/Left` holds Account's `Nickname` and `EditPanel`, this module's `PlayMenu` and `Lobby`; `SafeArea/Tournament` is Tournament's panel. Columns are anchored to halves of the safe area, rows by fixed heights. Text uses the built-in `LegacyRuntime.ttf` and Unity's built-in UI sprites until `UI_` art lands. Every `RegisterComponentInHierarchy` target in `MenuScope` must exist in this scene.

## Depends on

Session (`Matchmaker`, `LobbyStage`), Party (`PartyService`, `PartyStage`, `PartyState`), UniTask, VContainer, uGUI.
