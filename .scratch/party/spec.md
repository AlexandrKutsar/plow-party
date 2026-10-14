# Party, Quick Play search and Custom Game

Status: ready-for-agent

## Problem Statement

Friends who want to play together have only the Room Code, and it does the wrong job: a Room is a Session that random Quick Play Players also fall into, it has no timer, and the Host presses Start while a friend may still be connecting. After every Match the group falls apart, and an automatic "Play again" keeps away-from-phone Players in an endless loop of zero-score Matches. Quick Play itself picks the first open Session it finds, retries blindly and then hosts, so two Players searching together often end up in two empty Sessions. The search screen shows a Player count and a countdown computed from each device's clock, so a skewed clock shows a skewed timer. A Player alone waits about 18 s before Countdown, because Bots trickle in over an 8 s window even when nobody else is coming. There is no way to play only with friends, or alone against chosen Bots. All Vehicles look alike, and the Menu is a flat panel with no picture of the Player's own tractor.

## Solution

A **Party** replaces the Room. Friends gather in a Party in the Menu by its Party Code; the Party Leader starts the search once every other member has marked Ready; the whole Party lands in the same Match; after the Match everyone returns to the Party automatically. A Party can play **Quick Play** (counts for the Tournament, free Slots go to random Players and Bots) or a **Custom Game** (only the Party plus Bots the Leader places, does not count, nothing is sent to the backend).

Quick Play reads the list of open Lobbies of its Matchmaking Pool and joins the best one that fits the whole Party; if none fits, the Party itself becomes a public Lobby, and while it waits it moves into an older Lobby that fits. The search screen shows "Поиск игры…" and a stopwatch from the local device; any member can stop the search.

In the Match, Bots arrive quickly when every expected Player is already there, and the wait cap only matters while someone is still loading. Every Participant has a unique **Participant Color** from an 8-colour palette, shown on the Nickname, the tractor body and the in-Match score row; in a Custom Game each Player picks a free colour and a Critter. The Menu gets two bottom tabs, Игра and Турнир, and a 3D **Podium** showing the tractor and Critter of every Party member.

If the Host leaves mid-Match, the Match ends for everyone as Interrupted and the Party returns to the Menu; there is no host migration.

## User Stories

### Menu and Podium
1. As a Player, I want the Menu to have two bottom tabs, Игра and Турнир, so that I can switch between playing and the Tournament with one tap.
2. As a Player, I want my Nickname and its edit button in the top bar on both tabs, so that I always see who I am.
3. As a Player, I want to see my own tractor with my Critter on a Podium on the Игра tab, so that the Menu feels like my garage.
4. As a Party member, I want every member's tractor and Critter on the Podium side by side, so that I see who is in my Party.
5. As a Player on a phone, I want the Podium to cost little performance, so that the Menu stays smooth.
6. As a Player, I want no explanatory hint text on screens, only titles, buttons and states, so that the UI looks like a game.

### Party
7. As a Player, I want to create a Party and get a 5-character Party Code, so that friends can join me.
8. As a Player, I want to join a Party by entering its code, so that I play with my friends.
9. As a Player, I want to copy the Party Code with one tap, so that I can send it in a messenger.
10. As a Player entering a wrong or unknown code, I want "Группа не найдена", so that I know to check it.
11. As a Player, I want a Party to hold at most 6 Players, so that a Party always fits one Match.
12. As a Player trying to join a full Party, I want "Группа заполнена", so that I know why it failed.
13. As a Party member, I want to see every member's Nickname, Participant Color dot, and Ready state, so that I know who we are waiting for.
14. As a Party Leader, I want a crown next to my name, so that everyone knows who starts the search.
15. As a Party member other than the Leader, I want a Ready toggle that only I can change, so that the Leader knows I am ready.
16. As a Party Leader, I want the search button enabled only when every other member is Ready, so that nobody is pulled into a Match unprepared.
17. As a Party Leader alone in a Party, I want to search at once, so that I am not blocked by a Ready check of nobody.
18. As a Party Leader, I want to remove a member from the Party, so that I can drop someone who went idle.
19. As a removed member, I want to land in the Menu with "Вас исключили из группы", so that I understand what happened.
20. As a Party member, I want a Leave Party button separate from the search controls, so that stopping a search never kicks me out.
21. As a Party, I want leadership to pass to the member who has been in the Party longest when the Leader leaves, so that the Party survives.
22. As a Party member, I want Ready to reset only for a member who stopped the search, so that the rest stay ready and the Leader can restart quickly once that member is Ready again.
23. As a new member, I want to join as Not Ready, so that the Leader cannot start before I confirm.
24. As a Party member, I want the Party to be invisible to Quick Play while we are not searching, so that strangers never wander in.

### Mode switch
25. As a Party Leader, I want a Быстрая игра / Своя игра switch next to the search button, so that I choose how we play.
26. As a Party member, I want to see the chosen mode, so that I know whether the Match counts.
27. As a Player without a Party, I want Quick Play from the Игра tab directly, so that I can play solo in one tap.
28. As a solo Player, I want to start a Custom Game alone, so that I can train against Bots.

### Quick Play search
29. As a Player, I want the search screen to say "Поиск игры…" with a stopwatch from the moment I started, so that I know it is working without a misleading Player count.
30. As a Player, I want the stopwatch to come from my own device's clock since I pressed search, so that a wrong system clock never shows a wrong timer.
31. As any Party member, I want to stop the search, so that I can step away; I become Not Ready.
32. As a Party, I want Quick Play to join the open Lobby with the most Players that still has room for the whole Party and at least 3 s before it starts, so that Players gather instead of scattering.
33. As a Party, I want ties broken by the Lobby that starts soonest, so that we wait less.
34. As a Party, I want a failed join (the Lobby filled meanwhile) to try the next candidate, so that one race does not end the search.
35. As a Party, I want our own Party to become a public Lobby when no open Lobby fits, so that others can join us without anyone reconnecting.
36. As a Party hosting a Lobby with only our own members, I want to move into an older open Lobby that fits us, so that two Parties searching at the same moment end up together.
37. As a Lobby Host, I want the search to end after the configured time (10 s) or when 6 Players are in, so that Matches start promptly.
38. As a Player in a Lobby, I want only the Lobby Host's clock to decide the start, so that devices never disagree.
39. As Players of different builds, we never meet in a Lobby, so that the Match runs on the same code.

### Match start and WaitingForPlayers
40. As Players, we want the Host to load the Match for everyone in the Lobby at once, so that nobody reconnects.
41. As a Party that lands together, we want all of us seated in the same Match, so that we play together.
42. As a solo Player in Quick Play, I want Bots to arrive within 0.5–2.5 s once every expected Player is in, so that I do not wait for nobody.
43. As Players, we want the wait cap (15 s) to apply only while an expected Player is still loading, so that a slow phone gets a fair chance.
44. As a Quick Play Player who loads too late, I want my Slot given to a Bot and myself sent to the Menu with "Матч уже начался", so that the Match is not held hostage.
45. As a Custom Game Party, we want the same wait for loading members, so that a slow phone can still play.
46. As a Custom Game Party, we want a member who did not load in time simply left out, with no Bot in their place, so that the Leader's setup stays as chosen.
47. As Players, we want the waiting label "Матч скоро начнётся. Ожидание игроков N/Max" to count Players and Bots, so that we see the Arena filling up.

### Custom Game
48. As a Party Leader in a Custom Game, I want 6 Slots shown, Party members in theirs and the rest empty, so that I see the setup.
49. As a Party Leader, I want to put a Bot into any empty Slot and choose its Bot Difficulty (слабый, средний, сильный), so that I shape the challenge.
50. As a Party Leader, I want to remove a Bot from a Slot, so that I can change my mind.
51. As a Party Leader, I want empty Slots to stay empty if I leave them, so that we can play a small Match.
52. As a Party member in a Custom Game, I want to pick my Participant Color from the colours nobody else holds, so that every colour is unique.
53. As a Party member, I want taken colours shown as unavailable, so that I do not try to pick them.
54. As a Party member, I want to pick my Critter freely (repeats allowed), so that I play my favourite animal.
55. As a Party Leader, I want a Start button that starts the Custom Game with the current setup once every member is Ready, so that we play immediately without matchmaking.
56. As a Player, I want a Custom Game marked "Не в зачёт турнира", so that I know it does not count.
57. As the backend, I want Custom Games never registered or voted, so that the Tournament only sees Quick Play.
58. As Bots in a Custom Game, we keep the Difficulty the Leader chose, so that "exactly one Strong Bot" does not override the Leader.

### Participant Color
59. As a Participant, I want a Participant Color unique in the Match, so that I can tell tractors apart.
60. As a Player, I want my colour on my Nickname above the tractor, the tractor body, and my row in the in-Match score list, so that I find myself instantly.
61. As a Quick Play Participant, I want the Host to give me a random free colour at seating, so that colours never collide.
62. As a Custom Game Player, I want the Host to keep the colour I picked, so that my choice holds.
63. As a designer, I want 8 palette colours that read well on snow and for colour-blind Players, defined in config, so that 6 Slots always have spare colours.
64. As a Player, I want to see my colour already in WaitingForPlayers, so that I know which tractor is mine before Countdown.

### After the Match and failures
65. As a Player, I want "В меню" on Results to bring me back to my Party automatically, so that we can queue again without re-entering a code.
66. As the returning Leader, I want to re-create the Party under the same Party Code, so that members can find it.
67. As a returning member, I want to rejoin the Party automatically, retrying for about 10 s, so that I do not have to type the code.
68. As a returning member whose Leader quit the game, I want the first member back to re-create the Party and become Leader, so that the Party survives.
69. As a Player, I want no automatic "Play again", so that idle Players do not loop through Matches.
70. As a Player whose Host left mid-Match, I want "Хост вышел, матч прерван" and to return to my Party, so that I can continue.
71. As the backend, I want clients of a Quick Play Match whose Host left during Playing to vote it Interrupted, so that the Match still counts with a lower Weight.

### Camera
72. As a Player, I want the camera to follow my tractor without rotating, as the only camera, so that the view is consistent.
73. As a Player of a build, I want no camera preset button on screen, so that the HUD stays clean.

## Implementation Decisions

### Modules
- **Meta/Party (new):** Party state and rules: membership, Party Leader, Ready, Leader succession, removal, mode (Quick Play or Custom Game), and the Custom Game setup (Slots with Bots and their Difficulty, colour and Critter picks). Holds the Party Code across Matches so members rejoin after "В меню".
- **Meta/Session:** the Matchmaker changes from "random join, retry, host" to "read the Lobby list, pick, join, else open the Party as a Lobby, keep watching and merge". Rooms and Room Codes go away; the Party Code takes their role. Session properties no longer carry a search deadline for the UI; the stopwatch is local.
- **Meta/Lobby:** the Menu views: bottom tabs, top bar, Party panel, mode switch, search screen, Custom Game setup, Tournament tab. The Podium view.
- **Gameplay/Participants:** Participant Color joins the Participant Profile; the Host assigns colours at seating.
- **Gameplay/Match:** WaitingForPlayers gets the quick Bot window and the Custom Game rules.
- **Gameplay/CameraRig:** Follow is the only preset in builds; the preset switcher is removed from builds.
- **Art:** an 8-colour Participant palette, a tractor body material that takes a per-Vehicle colour, and the Podium stage set.

### Party as a hidden Session (ADR to write)
- A Party is a Fusion Session hosted by the Party Leader, named after the Party Code within the Matchmaking Pool, created hidden and closed to Quick Play.
- Party state (members in join order, Leader, Ready flags, mode, Custom Game setup) is networked on a Party object in that Session; only its owner changes a member's Ready, colour and Critter pick, and only the Leader changes mode, Bots and removals.
- When Quick Play finds no fitting Lobby, the Party Session itself turns into a public Lobby: it becomes visible and open within the Matchmaking Pool with the Party's size as its Player count. Nobody reconnects.
- When Quick Play picks another Lobby, the Leader tells members the target, all members leave the Party Session and join the target together, and remember the Party Code and Leader for the return.
- After the Match, the Leader (or the first member back if the Leader quit) re-creates the Party Session with the same Party Code; members rejoin with retries for about 10 s. Leadership passes to the longest-standing member.
- A Party holds at most 6 Players; Party Codes use the existing 5-symbol alphabet without I, L, O, 0, 1.

### Quick Play search
- Inputs: the open Lobbies of the Matchmaking Pool as reported by Fusion's session list (Player count, capacity, start-time property, creation order), the Party size.
- Pick rule: candidates have free Slots ≥ Party size and ≥ 3 s left before their start; order by most Players, then soonest start. A failed join moves to the next candidate.
- No candidate: the Party opens itself as a Lobby and starts its own 10 s search timer (config); start at 6 Players.
- Merge rule: while hosting a Lobby that holds only its own Party, the Host keeps reading the list; when an open Lobby older than its own fits the Party, it moves there. The older one never moves, so two Parties converge.
- The search UI is a local stopwatch since the press; any member can stop the search, which takes the whole Party back to the hidden Party Session and makes that member Not Ready.
- Only the Lobby Host's clock decides the start; the start time is a Session property used for the 3 s rule, not for display.

### Match start and WaitingForPlayers
- The Matchmaking Result gains the mode (Quick Play or Custom Game) and, for a Custom Game, the Slot setup (which Slots are Bots and their Difficulty, each Player's colour and Critter pick). The Host writes it before the networked scene load, as today.
- Bot arrival: when every expected Player is seated, the remaining Bots arrive at random times in 0.5–2.5 s (config); while an expected Player is missing, the existing window and the 15 s cap apply.
- Quick Play at the cap: free Slots get Bots, as today. Custom Game at the cap: Countdown starts with whoever is seated plus the Leader's Bots; a missing Player's Slot stays empty.
- Custom Game Bots keep the Difficulty the Leader chose; the "exactly one Strong Bot" rule applies only to Quick Play.
- The automatic "Play again" and its Results timer go away; Results end only through "В меню". The Results screen stays until each Player leaves.

### Participant Color
- Palette of 8 colours in config (order matters for "nearest free"), checked for contrast on snow.
- The Host assigns colours at seating: Quick Play gives each new Participant a random free colour; Custom Game uses each Player's pick and gives Bots free colours. Colours are unique within a Match.
- Shown on the Nickname label above the Vehicle, the Vehicle body (per-Vehicle material property, no material instances per frame), and the Participant's row in the in-Match score list.
- Future "Garage" preferred colour: keep the first-seated Player's colour, give a later conflicting Player the nearest free palette colour. Not built now; the assignment rule is written so a preference can be passed in later.

### Backend and Tournament
- Custom Games are never registered, confirmed or voted.
- Quick Play reporting is unchanged (ADR-0013); a Party of 6 in Quick Play counts.
- Host leaving during Playing: clients show "Хост вышел, матч прерван", vote Interrupted (existing), and return to the Party.

### Menu and Podium
- Two bottom tabs (Игра, Турнир); top bar with Nickname and edit, visible on both.
- The Podium is a 3D stage in the Menu scene (no per-slot render textures): up to 6 tractor-plus-Critter models in a row lit by one camera, the UI overlaid. Solo shows the Player's own tractor. Models come from the existing Vehicle and Critter visual prefabs, tinted with the Participant Color.
- All player-facing strings are Russian; screens carry titles, buttons and states only, no explanatory hints.

### Camera
- Follow (fixed orientation, follows the local Vehicle) is the only preset used in builds; the dev preset button is removed. The other presets may stay in code for the Editor.

## Testing Decisions

- Good tests drive a rule through its public entry point with plain inputs and assert the outcome a Player would notice (who leads, who may start, which Lobby is chosen, which colour a Participant gets, when Countdown starts). No assertions on private state, Fusion objects, or UI layout.
- One pure rules seam per concern, each an EditMode test target:
  - **Party rules:** join order, capacity 6, Leader succession, removal, Ready reset on stop, can-start (all non-Leader members Ready; solo Leader always), mode change, Custom Game setup edits (Bot add/remove with Difficulty, colour uniqueness, Critter repeats).
  - **Lobby pick and merge rules:** candidate filtering (room for the Party, ≥ 3 s left), ordering (most Players, soonest start), next candidate on failure, merge only into an older fitting Lobby and only while hosting just the own Party.
  - **Seat Plan / WaitingForPlayers rules (extend the existing ones):** quick Bot window when all expected Players are seated, cap behaviour per mode, Custom Game Slots with Leader Bots and no replacement Bots, Strong-Bot rule only in Quick Play.
  - **Participant Color assignment:** random free colour in Quick Play, picks honoured in Custom Game, Bots get free colours, uniqueness, the future preferred-colour hook (nearest free).
- Prior art: `WaitingRules` and Match rules tests, `LobbyRules` and `RoomCode` tests in Meta/Session, Participants tests, Bot profile assignment tests.
- Play Mode verification: one short pass per flow (solo Quick Play, Party of two via Multiplayer Play Mode if available, Custom Game alone with Bots), grep logs for the outcome lines, a few screenshots. No balance runs.

## Out of Scope

- Host migration (GDD MVP+ 5).
- A Garage tab and preferred colour or Critter outside Custom Game.
- Invites through platform friends lists or deep links; only the Party Code.
- Voice or text chat in the Party.
- Leader handing leadership over by hand.
- Final menu art, icons and a Cyrillic font (on the art request list).
- Reconnecting to a running Match after leaving or losing the connection.

## Further Notes

- Delivery in two PRs to keep reviews small: (1) Party, Quick Play search, Menu tabs and Podium, WaitingForPlayers timing, no "Play again", Host-left handling, camera; (2) Custom Game and Participant Color (palette, tinting, picks, assignment).
- Glossary: add Party (группа), Party Leader, Party Code (replacing Room Code), Ready, Custom Game (своя игра), Participant Color, Podium; retire Room Code and the Room sense of Lobby. Roster and Matchmaking Result keep "party" under Avoid in their own sense; the new Party entry must state it is the Menu group, not the registered Roster.
- ADR-0016 (Session outlives scenes, Lobby in the Menu) gets an amendment or a successor ADR for the Party as a hidden Session and the list-based search.
- PR #18 (main Editor shares the dev pool with phones) should be merged before this work starts.
