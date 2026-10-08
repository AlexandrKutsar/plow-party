# 01 Party: rules and hidden Party Session

Status: ready-for-agent
Blocked by: none

Spec: `.scratch/party/spec.md` (stories 7–24, 65–68; "Party as a hidden Session").

Build `Meta/Party`: pure Party rules (members in join order, capacity 6, Leader, Leader succession, removal, Ready owned by each member, Ready reset for a member who stops the search, can-start rule: every non-Leader member Ready, solo Leader always; mode field Quick Play / Custom Game) and the hidden Party Session in place of Rooms: create with a Party Code, join by code ("Группа не найдена", "Группа заполнена"), networked Party state object, Leader-only removal ("Вас исключили из группы"), leave, and automatic return after "В меню" (Leader or first member back re-creates the same Party Code; members rejoin with ~10 s retries). Remove Room Code / Room flows. No new Menu visuals (ticket 04); keep the existing Menu usable through minimal hooks so the build stays playable. Write the ADR for "Party as a hidden Session" (amend or succeed ADR-0016). Glossary: Party, Party Leader, Party Code, Ready; retire Room Code.
