# The Fusion Session outlives scenes; Players gather in the Menu

Players meet in a Fusion Session while still in the Menu, and the Host moves everyone into the Match with Fusion's networked scene load. The `NetworkRunner` therefore lives in the root scope (`NetworkSession`, `DontDestroyOnLoad`) instead of the Match scene. Each scene scope binds itself to the running Session for its lifetime (`NetworkScopeBinding`): its `IObjectResolver` becomes the one `ResolverNetworkObjectProvider` instantiates through (so ADR-0004 still holds per scene), and its `NetworkRunnerEvents` are added to the runner's callbacks. A scope that binds to an already running Session is replayed `PlayerJoined` for every active Player once Fusion reports the scene load done, so Match code that spawns on join (`VehicleSpawner`) needs no "already connected" branch.

Quick Play first tries a random join filtered by the Matchmaking Pool (build version plus `DevSessionName`, so Editors of different worktrees never meet) and, if nothing is open, hosts a new Session whose search deadline is a creation-time session property; a Room is a Session named after its Room Code, which random Quick Play joiners may also enter. The Host closes and hides the Session when it starts the Match. Leaving shuts the runner down and loads the Menu; a client whose Host disappears is sent back to the Menu by the same root service.

## Considered Options

- Matchmaking inside the Match scene (WaitingForPlayers only): one scene, but no lobby UI, no Room Code entry before the Match, and the Menu could not show who is waiting.
- `AutoHostOrClient` with a null session name: Fusion's own join-or-create, but its session properties are both the filter and the initial properties, and Fusion refuses to add a property after creation, so the search deadline could not be published.
- A runner per scene, restarted on every scene change: simple lifetimes, but every Player would have to reconnect and rejoin the same Session between Menu and Match.

## Consequences

A Session can outlive the scope that started it, so nothing scene-scoped may hold the runner; scopes reach it only through the root `NetworkSession`. The random-join-then-host sequence has a race: two Players who search at the same moment may each host an empty Session. The search timer is shown from a wall-clock deadline, so a device with a skewed clock shows a skewed countdown; only the Host's clock decides when the Match starts. The Editor shortcut `MatchSceneQuickStart` still starts a Session from inside the Match scene when no Session is running.
