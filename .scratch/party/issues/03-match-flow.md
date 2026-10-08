# 03 Match flow: quick Bots, no Play again, Host left, camera

Status: ready-for-agent
Blocked by: none

Spec: `.scratch/party/spec.md` (stories 42–44, 47, 69–73; "Match start and WaitingForPlayers", "Camera").

Gameplay-side only. WaitingForPlayers: when every expected Player is seated the remaining Bots arrive in 0.5–2.5 s (config); the existing window and 15 s cap apply only while an expected Player is missing. Remove the automatic "Play again" and the Results timer; Results stay until "В меню". Host leaves during Playing: clients show "Хост вышел, матч прерван", the existing Interrupted vote runs, then return through Meta's exit path (Party return lands with 01; until then the Menu). Camera: Follow is the only preset in builds, remove the dev preset button. Keep the "exactly one Strong Bot" rule as is (Custom Game exception comes in 06).
