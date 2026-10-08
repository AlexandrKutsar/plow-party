# 02 Quick Play: Lobby list pick, Party as Lobby, merge

Status: ready-for-agent
Blocked by: 01

Spec: `.scratch/party/spec.md` (stories 29–39; "Quick Play search").

Rewrite the Matchmaker: read the open Lobbies of the Matchmaking Pool from Fusion's session list, pick by the pure pick rule (room for the whole Party, ≥ 3 s before start; most Players, then soonest start; next candidate on failed join); when none fits, the Party Session itself opens as a public Lobby with its own search timer (10 s, start at 6); while hosting only its own Party, merge into an older fitting Lobby. Whole Party moves together into a picked Lobby. Search state for the UI: local stopwatch since the press; any member can stop (back to the hidden Party Session, that member Not Ready). Remove the deadline-for-display property and the jitter retry. Matchmaking Result gains the mode field (Quick Play now; Custom Game setup comes in 06).
