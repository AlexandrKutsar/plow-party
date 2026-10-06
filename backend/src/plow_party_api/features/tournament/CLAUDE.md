# tournament

The daily Tournament (GDD 9.2): each Account's Daily Best, the Leaderboard top and "around me", and Medals for the top three of the last final day. Decisions: ADR-0014. Spec: `.scratch/tournament/spec.md`.

## Entry points

- `GET /tournament/leaderboard?day=&limit=` — top `limit` (1–100, default 10) Standings of a Tournament Day, plus the caller's own Standing as `me`. `day` defaults to today (UTC); a future day is 422.
- `GET /tournament/leaderboard/me?day=&radius=` — the caller's Standing and the Standings within `radius` (0–25, default 3) Ranks of it; `me` null and no entries when the caller has no Daily Best that day.
- `GET /tournament/medals?account_id=…` — Medal of each of 1–20 Accounts on the Medal Day, in request order, duplicates dropped. The client passes the Roster to frame HUD portraits.
- All bearer; 401 from accounts.

## Rules

`rules.py` holds every decision as a pure function: a Tournament Day is a UTC date, a Match counts for the day of its `registered_at`, a day is final `RESULTS_SETTLE` (303 s) after it ends; ranking by score descending, then earlier Match, then Account id, so Ranks are unique; top and around-me windows; Medal Day = the latest final day; gold, silver, bronze for Rank 1–3 with a Daily Best above 0. The service only orchestrates them.

## State

None. The feature owns no tables: every read asks `MatchService.best_credited_scores` for the day's best Credited Score per Account, which first reaches the Verdict of overdue open Matches registered that day, then ranks in Python. Nicknames come from `AccountService.nicknames`, so the Leaderboard always shows current ones. Medals are recomputed on read; a final day cannot change, so they are stable without being stored. ADR-0014 names when to materialize instead.
