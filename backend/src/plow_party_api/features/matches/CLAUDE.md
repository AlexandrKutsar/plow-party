# matches

Match registration, Player confirmation, Votes on the Match Result, and the Verdict that decides what reaches the Tournament (GDD 9.3). Decisions: ADR-0013. Spec: `.scratch/matches/spec.md`.

## Entry points

- `POST /matches` — the Host registers the Roster at the start of Countdown (201). The Host is a Confirmed Player at once; 422 if a Roster rule breaks, the Host holds no Slot, or an Account does not exist.
- `POST /matches/{id}/confirm` — a Player in the Roster confirms within 15 s of registration (204, repeatable). An unconfirmed seat counts as a Bot: no Vote, no Credited Score.
- `POST /matches/{id}/votes` — a Confirmed Player submits Score per Slot and, for an Interrupted Match, the second the Host left. Same Vote again succeeds; a different one is 409. The last Confirmed Player's Vote reaches the Verdict.
- `GET /matches/{id}` — the Match and its Verdict; reaches the Verdict first if the submission window (303 s after registration) has closed.
- Errors: 404 unknown Match, 403 not in the Roster / not a Confirmed Player, 409 wrong phase, 422 malformed input.

## Rules

`rules.py` holds every decision as a pure function, with the limits as constants: Roster shape (4–6 seats, Slots 0–5, one Slot per Account, at least one Player), confirmation window, Vote shape and timing against the server clock, strict-majority Verdict, plausibility (30 Score per second, Interrupted Match at least 30 s), Weight (1 or 0.5), placements with shared ties, Credited Score rounding. The service only orchestrates them.

## State

- `matches` — status `open` → `accepted` | `rejected`, `rejection_reason`, and the accepted `interrupted`, `played_seconds`, `weight`.
- `match_participants` — one row per Slot: Account (null for a Bot), `confirmed`, and after acceptance `score`, `placement`, `credited_score` (Confirmed Players only).
- `match_votes` — one per Confirmed Player: Scores as JSONB keyed by Slot, interrupted second, server `submitted_at`.

Every write locks the `matches` row (`SELECT … FOR UPDATE`), so simultaneous last Votes reach one Verdict. Time comes only from `core.clock`; tests replace it.

## For the Tournament

Accepted Matches carry `credited_score` per Account on `match_participants`. The Tournament reads them through a method this service will expose, and must reach the Verdict of overdue open Matches first, as `GET` does.
