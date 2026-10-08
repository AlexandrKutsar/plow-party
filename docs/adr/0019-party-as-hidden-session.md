# A Party is a hidden Fusion Session hosted by its Leader

Amends ADR-0016: Rooms and Room Codes are replaced by Parties; the Session still outlives scenes.

Friends gather in a Party before they search. A Party is a Fusion Session in the Matchmaking Pool named `<pool>-party-<Party Code>`, hosted by the Party Leader, created hidden (`IsVisible = false`) but open, so Quick Play's random join never sees it while a join by name does. Its state (members in join order, Ready flags, mode) lives on a networked `PartyLink` object the Host spawns: the Host holds the authoritative rules object (`PartyState`), members change state only through RPCs that the Host checks against the caller's `PlayerRef`, so a member changes only their own Ready and only the Leader removes members or changes the mode. The Leader is always the Host, because the Host admits itself first.

There is no host migration. When the Leader leaves, the Session ends; every member computes the next Leader from the last published members (the longest-standing one) and runs the same return loop as after a Match: the Leader re-creates the Party Session under the same Party Code, a member rejoins it with retries for about 10 s and re-creates it itself if nobody did, and a taken code means someone came back first, so join them. The Party Code, role and mode survive scene changes in a root `PartyMemory`, which is how "В меню" after a Match brings every member back without typing the code.

Quick Play (ticket 02) searches from the Party: see the amendment below.

## Considered Options

- Keep the Room as a Session random Quick Play joiners can enter: friends could not play only together, and the Room had no Ready check.
- Party state in Session properties: Fusion cannot add properties after creation and only the Host can set them, so members could not change their own Ready, and every change would go through the Photon master server.
- A Party kept on the backend: the backend would need presence and push, which it does not have; Fusion already connects the members.
- Host migration for the Party Session: Fusion 2 host migration needs extra snapshot plumbing for a state of a few bytes; re-creating the Session under the same name is simpler and doubles as the post-Match return.

## Consequences

Meta/Party is a new module below Meta/Session and Meta/Lobby; `MatchmakingPool` moved to Infrastructure because both Party and Session name Sessions with it. `NetworkSession` gained `SessionStart.IsVisible` and the `Full` outcome. Members who come back while the Host is still on Results find the Party Session closed and wait for it to end (amendment below). Removal is a Host RPC to the removed member followed by a disconnect after a short grace, so the member sees "Вас исключили из группы" rather than a lost connection.

## Amendment: list-based Quick Play (ticket 02)

The searcher (a solo Player, or the Party Leader once every member is Ready) reads Photon's session list through a second, lobby-only `NetworkRunner` (`SessionListFeed`), because the main runner is inside the Party Session and a runner in a room receives no lobby updates. It joins the best open Lobby of its pool (room for the whole Party, at least 3 s before its `start`; most Players, then soonest start; the next candidate on a failed join). A Party moves together: the Leader sends the target name through `PartyLink`, waits a short grace for the RPC to leave, and every member leaves the Party Session and joins the target by name. When nothing fits, the Party Session itself opens as the Lobby (`IsVisible = true`, `start`/`opened` updated with `SessionInfo.UpdateCustomProperties`; a solo Player hosts a fresh Session). While its Lobby holds only its own Party, the Host keeps reading the list and moves into an older fitting Lobby; age is the `opened` time with the session name as tie-break, so of two Lobbies exactly one moves.

This corrects the earlier note that Fusion cannot change properties after creation: values can change, but Photon only lists in the lobby the keys a room was created with, so every Lobby-capable Session, the hidden Party Session included, is created with `pool`, `start` and `opened` (`LobbyProperties`).

The search state is a networked `IsSearching` flag on `PartyLink`; any member's stop ends it, makes that member Not Ready and disconnects strangers who had joined the open Party Session (they are guests, never members). A Party sitting in a stranger's Lobby has no `PartyLink` there, so every Lobby Host spawns a `LobbyLink` whose single RPC tells everyone "Party <code> stopped"; that Party's members leave and run the return loop, keeping their Ready (remembered in `PartyMemory`) except the member who stopped.

The gap of ticket 01 (members returning after "В меню" find the Party Session closed while the Host is still on Results, when the Party Session became the Match Session) is closed in the return rules: `Refused` means the Party's Match still runs under that name, so members keep rejoining and restart the wait instead of counting toward the 20 s give-up or re-creating the Party; once the Host leaves, the usual rules apply. Renaming the Match Session was not possible (Fusion cannot rename a running Session), and giving the returning Party a new name would break joins by code.

Considered: a solo Player always creating a Party of one (every Lobby would carry a `PartyLink` and no `LobbyLink` would be needed, but the Menu would show a Party nobody asked for); members polling the list for their Leader instead of an RPC (slower, and the Leader is not visible in the list). Accepted risk: if the target fills between the pick and the joins, the members who got in stay there while the rest of the Party returns to its Party Session; with the room-for-everyone and 3 s rules this needs two searches to race within a second.
