# Lobby

The Menu's screens: two bottom tabs (Игра, Турнир), the Игра tab's solo panel, Party panel and search screen, and the 3D Podium with the tractor and Critter of every Party member (GDD 3.2 Lobby, 3.3). Presentation only; the logic is `Meta/Session` and `Meta/Party`, the Турнир tab's content is `Meta/Tournament`, the top bar's Nickname is `Meta/Account`. Spec: `.scratch/party/spec.md` ("Menu and Podium"), tickets `.scratch/party/issues/04-menu-tabs-podium.md` and `06-custom-game.md`. Screens carry titles, buttons and states only, no hint texts.

## Entry points

- `MenuTabsPresenter` + `MenuTabsView` (`View/`) — MenuScope entry point; the tab bar switches page roots on and off: the Игра page is the `Play` UI and the `Podium` stage, the Турнир page is Tournament's panel. Opens on Игра. The chosen tab's button is non-interactable, which is its selected look.
- `LobbyPresenter` — MenuScope entry point; picks one screen with `LobbyScreens.For(PartyStage, LobbyStage, mode)`: the search while `Matchmaker.Stage` is not `Idle`, else the Party while `PartyService.Stage` is not `None` or the solo Player chose Своя игра, else solo. Notices (`LobbyText.Notice(PartyNotice, SessionNotice)`, Party first) show on the solo panel and on the Party panel, so a member back from a Match whose Host left sees "Хост вышел, матч прерван" too. Rebuilds the Podium lineup on every change; rewrites the stopwatch only when the whole second changes.
  - Solo (`PlayMenuView`): "Быстрая игра", "Своя игра" (switches the solo mode, which opens the Party panel titled "Своя игра" without code, rows or Leave), "Создать группу", Party Code field, "Войти по коду".
  - Party (`PartyPanelView`, rows `PartyMemberRowView`): "Группа <code>" with "Копировать" (system clipboard), up to 6 rows in join order (colour dot, crown for the Leader, "Готов" / "Не готов" for others, "Исключить" for the Leader), the mode switch "Быстрая игра" / "Своя игра" (Leader only), "Я готов" / "Не готов" for members, "Искать матч" ("Начать" in a Custom Game) for the Leader (enabled when `Matchmaker.CanSearch`), the notice line (`Actions/Failure`), "Выйти из группы".
  - Custom Game setup (`CustomGameSetupView`, rows `SetupSlotRowView`), shown in place of the member rows while the mode is Custom Game: "Не в зачёт турнира", 6 Slots (member with colour dot and Ready mark, "Бот · слабый/средний/сильный", or "Пусто"), the Leader's "+ Бот" (Medium) on an empty Slot, the difficulty button (cycles слабый → средний → сильный) and "×" on a Bot; the 8 palette swatches (another member's colour faded and disabled, the own one enlarged) and the Critter switch "‹ Лиса ›".
  - Search (`SearchView`): "Поиск игры…", a local stopwatch `m:ss` since the press ("Матч начинается…" while `Starting`), "Остановить поиск" for anyone who `CanStopSearch`.
- `PodiumView` + `PodiumSpot` (`View/`) — the Podium: at `Awake` instantiates `capacity` (6) copies of the Vehicle visual prefab `V_Vehicle` under `Row`, each turned by `facing` toward the camera; `Show(looks)` places the first N in a row centred on the stage (`PodiumLayout.Offset`), shrinks the row when it is wider than `stageWidth` (`PodiumLayout.Scale`), and seats the look's Critter visual prefab on the `CritterSeat` anchor. `_critterModels` lists `V_Critter_*` in `CritterSpecies` order (Fox, Bear, Rabbit, Raccoon, Penguin, Beaver).
- `MemberLooks` / `MemberLook` — **the single place a member's look comes from**: `LookOf(memberId)` returns the Participant Color (row dot, later the tractor body) and the Critter index. It reads the member's colour and Critter from `PartyService.Party` (the solo state without a Party) and the colour from Party's `IParticipantPalette`. `PodiumSpot.Show` tints the `Body` renderer of its `V_Vehicle` copy through a `MaterialPropertyBlock` `_BaseColor`, like Participants' `VehicleBodyColorView`.
- `Simulation/` — pure presentation rules, EditMode-tested: `LobbyScreens`, `PartyPanelState` (title, code, rows, mode switch, which button shows and whether it is enabled, the Custom Game setup: `Slots` as `SetupSlotRow`, `Colors` as `ColorChoice`, Critter name, `NextDifficulty`), `PartyRow`, `PodiumLayout`, `LobbyText` (every Russian string, the Party and Session notices included, and the stopwatch format), `MenuTab`, `LobbyScreen`.

## Menu scene

`Scenes/Menu.unity` (build index 1): `Camera` (also films the Podium), `EventSystem` (Input System UI module), `MenuScope`, `Podium` (ground plane, `Sun` directional light, `Row`), and the `Menu` canvas (Screen Space Overlay over the Podium, 1920×1080 reference, match height) with Infrastructure's `SafeAreaFitter` on `SafeArea`:

- `TopBar` — Account's `Nickname` and `EditPanel`, on both tabs.
- `Play` — `PlayMenu` (right column), `Party` (`Members` left column, `Setup` left column in its place for a Custom Game, `Actions` right column), `Search` (right column); the centre is left free for the Podium.
- `Tournament` — Tournament's panel, centred.
- `TabBar` — `PlayTab`, `TournamentTab`.

Text uses the built-in `LegacyRuntime.ttf`, buttons Unity's built-in UI sprites, the crown and the colour dot the built-in knob sprite, the ground the pipeline's default material, until `UI_` art and a Podium stage set land (`.scratch/level/art-requests.md`). Every `RegisterComponentInHierarchy` target in `MenuScope` must exist in this scene.

## Depends on

Session (`Matchmaker`, `LobbyStage`), Party (`PartyService`, `PartyStage`, `PartyState`, `IParticipantPalette`), Shared (`PartyMode`, `BotDifficulty`), UniTask, VContainer, uGUI. Art's `V_Vehicle` and `V_Critter_*` prefabs are referenced from the scene only; no Gameplay assembly.
