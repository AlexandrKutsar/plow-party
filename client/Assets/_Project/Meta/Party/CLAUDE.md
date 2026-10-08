# Party

Friends gathered in the Menu by a Party Code: members in join order, the Party Leader, each member's Ready, the mode (Quick Play or Custom Game), and the automatic return to the Party after a Match (ADR-0019). No UI; `Meta/Lobby` draws it. Spec: `.scratch/party/spec.md`, ticket `.scratch/party/issues/01-party-session.md`.

## Entry points

- `PartyService` — MenuScope entry point. `CreateAsync` hosts a hidden Session named `PartyCode.SessionName(pool, code)` with a fresh Party Code (retrying `CodeAttempts` times on a name clash) and spawns the `PartyLink`; `JoinAsync(code)` joins one ("Код — 5 символов", "Группа не найдена", "Группа заполнена", "Матч уже начался" for a Party already in a Match, "Не удалось подключиться"); `LeaveAsync` forgets the Party; `SetReady`, `StopSearch`, `Remove(memberId)` (Leader), `SetMode` (Leader); `CloseForMatch` (Host: remembers the Party and despawns the link before the Match scene loads). State for the UI: `Stage` (`None`, `Connecting`, `InParty`), `Code`, `Party` (the last `PartyState` read from the link), `LocalId`, `IsLeader`, `IsReady`, `CanStart`, `Failure`, `Changed`. A removed member lands in the Menu with "Вас исключили из группы".
- `PartyMemory` — root singleton: the Party Code, this device's role (Leader or member) and the mode, kept across scenes. `PartyService.Start` returns to the remembered Party, so "В меню" after a Match brings everyone back without typing the code.
- `Network/PartyLink` — the networked Party object in the Party Session, spawned by the Host (`PartyConfig.PartyLinkPrefab`, `Prefabs/PartyLink.prefab`). The Host keeps the authoritative `PartyState`, admits joiners (`IPlayerJoined`, Nickname from the `ParticipantToken`), drops leavers, and publishes members (`PartySeat`: Player, Nickname, Ready), `Mode` and a `Revision` counter. Members change state only through RPCs to the Host, which apply the rules with the caller's `PlayerRef` (`RpcHostMode.SourceIsHostPlayer`), so only a member changes their own Ready and only the Leader removes members or changes the mode. A removal sends `RPC_Removed` to that member and disconnects them after `RemovalGraceSeconds` if they are still there.
- `Network/PartyLinks` — MenuScope registry of the current `PartyLink` (`Attach`/`Detach` from `Spawned`/`Despawned`) and the `Removed` event.
- `PartyState` (`Simulation/`) — pure rules: join order, capacity, Leader = longest-standing member, `Remove` (Leader only, never themself), `SetReady` (members other than the Leader), `StopSearch` (that member Not Ready), `CanStart` (every non-Leader member Ready; a solo Leader always), `SetMode` (Leader only), `Restore` from published members.
- `PartyReturnRules` (`Simulation/`) — the return loop: a Leader re-creates the Party Session first, a member rejoins; a taken code means someone came back first, so join them; a member who finds nothing for `RejoinSeconds` (10 s) re-creates the Party and leads it; `Full` or `GiveUpSeconds` stops with "Группа не найдена" / "Группа заполнена". `RoleAfterLeaderLeft` picks the longest-standing remaining member as the next Leader.
- `PartyCode` (`Simulation/`) — 5 symbols from `ABCDEFGHJKMNPQRSTUVWXYZ23456789` (no I, L, O, 0, 1); `Generate(Random)`, `TryParse`, `SessionName(pool, code)` = `<pool>-party-<code>`.
- `PartyConfig` — capacity (6), code attempts, rejoin and give-up seconds, retry interval, removal grace, `PartyLink` prefab; asset `_Project/Configs/PartyConfig.asset`.

## Rules worth knowing

- The Party Leader is always the Host of the Party Session: the Host admits itself first. There is no host migration, so a Leader who leaves ends the Session; every member then runs the return loop, the longest-standing one as Leader.
- A Party Session is hidden (`IsVisible = false`) but open: random Quick Play joins never see it, a join by name does. Fusion's `MaxPlayers` = capacity answers a seventh joiner `Full`.
- Until ticket 02 the Leader's "Искать матч" starts the Match straight from the Party Session (`Matchmaker.StartPartyMatch`), so the Party plays with Bots in the free Slots. Members returning while the Host is still on Results meet a closed Session and keep retrying until the Host leaves or the give-up time runs out.
- New members join Not Ready; after a Match everyone rejoins Not Ready.

## Depends on

Infrastructure (`NetworkSession`, `MatchmakingPool`), Account (`AccountService` for the connection token), Shared (`ParticipantToken`), Fusion, UniTask, VContainer. `Meta/Session` and `Meta/Lobby` depend on this module, not the other way round. Listed in `AssembliesToWeave`.
