# A Party is a hidden Fusion Session hosted by its Leader

Amends ADR-0016: Rooms and Room Codes are replaced by Parties; the Session still outlives scenes.

Friends gather in a Party before they search. A Party is a Fusion Session in the Matchmaking Pool named `<pool>-party-<Party Code>`, hosted by the Party Leader, created hidden (`IsVisible = false`) but open, so Quick Play's random join never sees it while a join by name does. Its state (members in join order, Ready flags, mode) lives on a networked `PartyLink` object the Host spawns: the Host holds the authoritative rules object (`PartyState`), members change state only through RPCs that the Host checks against the caller's `PlayerRef`, so a member changes only their own Ready and only the Leader removes members or changes the mode. The Leader is always the Host, because the Host admits itself first.

There is no host migration. When the Leader leaves, the Session ends; every member computes the next Leader from the last published members (the longest-standing one) and runs the same return loop as after a Match: the Leader re-creates the Party Session under the same Party Code, a member rejoins it with retries for about 10 s and re-creates it itself if nobody did, and a taken code means someone came back first, so join them. The Party Code, role and mode survive scene changes in a root `PartyMemory`, which is how "В меню" after a Match brings every member back without typing the code.

Until list-based Quick Play lands (ticket 02), the Leader's search starts the Match straight from the Party Session; later the Party Session itself turns into a public Lobby or the whole Party moves to another one.

## Considered Options

- Keep the Room as a Session random Quick Play joiners can enter: friends could not play only together, and the Room had no Ready check.
- Party state in Session properties: Fusion cannot add properties after creation and only the Host can set them, so members could not change their own Ready, and every change would go through the Photon master server.
- A Party kept on the backend: the backend would need presence and push, which it does not have; Fusion already connects the members.
- Host migration for the Party Session: Fusion 2 host migration needs extra snapshot plumbing for a state of a few bytes; re-creating the Session under the same name is simpler and doubles as the post-Match return.

## Consequences

Meta/Party is a new module below Meta/Session and Meta/Lobby; `MatchmakingPool` moved to Infrastructure because both Party and Session name Sessions with it. `NetworkSession` gained `SessionStart.IsVisible` and the `Full` outcome. Members who come back while the Host is still on Results find the Party Session closed and keep retrying until it ends or the give-up time runs out. Removal is a Host RPC to the removed member followed by a disconnect after a short grace, so the member sees "Вас исключили из группы" rather than a lost connection.
