# Tournament

Status: ready-for-agent

## Problem Statement

The backend reaches a Verdict on every Match and stores a Credited Score per Confirmed Player (`.scratch/matches/spec.md`), but nothing ranks them. GDD 9.2 asks for a daily Tournament: every credited Match counts (Quick Play, Room Codes, Matches against Bots only), attempts are unlimited, each Account keeps its best result of the day, the table resets at 00:00 UTC and shows a top and an "around me" view, Bots never appear, and at reset the top places earn a cosmetic Medal shown on HUD portraits.

## Solution

The Tournament owns no tables. It derives everything on read from the Credited Scores of accepted Matches, which it gets through one method of the matches service. A Match counts for the Tournament Day (UTC date) on which it was registered. Before reading a day, the matches service reaches the Verdict of every overdue open Match registered that day, so no Match waits for someone to open it. The Tournament ranks Daily Bests with pure rules: higher score first, the earlier Match breaking ties, so every Rank is unique. Medals belong to the Medal Day, the latest Tournament Day whose results can no longer change: Rank 1, 2, 3 earn gold, silver, bronze. Nothing is materialized: once a day is final its ranking cannot change, so computing it again always gives the same Medals.

## User Stories

1. As a Player, I want every accepted Match I played to count for the Tournament, whether Quick Play, a Room Code, or a Match against Bots only, so that any Match is worth playing.
2. As a Player, I want only my best Credited Score of the day on the Leaderboard, so that playing more never lowers my standing.
3. As a Player, I want the Leaderboard to start empty at 00:00 UTC, so that every day is a fresh competition.
4. As a Player, I want Bots and unconfirmed seats never ranked, so that I compete only with people.
5. As a Player, I want Interrupted Matches to count with their reduced Credited Score, so that the Weight decided by the Verdict is respected without a second rule.
6. As a Player, I want rejected and still-open Matches ignored, so that only accepted results rank.
7. As a Player, I want the top N of today's Leaderboard with Ranks, Nicknames and Daily Bests, so that I see who leads.
8. As a Player, I want my own Rank with K neighbours above and below, so that I see whom I can overtake.
9. As a Player who has not played today, I want "around me" to answer with no Standing rather than an error, so that the client shows an empty state.
10. As a Player, I want ties broken by who reached the score first, so that Ranks are unique and racing to a score matters.
11. As a Player, I want to read a past day's Leaderboard, so that I see yesterday's final result.
12. As a Player, I want a future day refused with 422, so that client date bugs surface.
13. As a Player, I want to know whether a day's Leaderboard is final and when it ends, so that the client can show a countdown and a "final" badge.
14. As a Player, I want the total number of ranked Players, so that "57th of 230" can be shown.
15. As a Player, I want a Match registered before midnight and decided after it to count for the day it started, so that a late Verdict never moves my result to another day.
16. As a Player whose opponents never voted, I want my Match to reach its Verdict when anyone reads the Leaderboard, so that my score appears without anyone opening the Match.
17. As a Player who placed 1st, 2nd or 3rd on the last final day, I want a gold, silver or bronze Medal, so that I wear a portrait frame.
18. As a Player, I want a Medal never to change once shown, so that it is an award, not a guess.
19. As a Player in a Match, I want the Medals of every Account in the Roster in one call, so that the HUD shows other Players' frames.
20. As a Player, I want no Medal for a Daily Best of 0, so that an idle day cannot crown anyone.
21. As a client developer, I want 401 without a valid Auth Token and 422 for out-of-range parameters, so that every endpoint behaves like the rest of the API.
22. As a backend developer, I want ranking, windows and Medal decisions as pure functions with exhaustive unit tests, so that Tournament rules are safe to change.
23. As a maintainer, I want the seam between matches and tournament recorded in an ADR, so that the choice to derive instead of store is explicit.

## Implementation Decisions

- **Module:** new feature slice `tournament` with router, schemas, service, rules and a module `CLAUDE.md`; no models or repository, because it owns no tables. Its row in the backend feature index moves to `active`. Router included by the app factory.
- **Glossary:** Tournament Day, Daily Best, Leaderboard, Rank, Standing, Medal Day (added to `GLOSSARY.md`). Decision record: ADR-0014.
- **Seam (pull):** tournament imports only `MatchService`, `AccountService` and `RESULTS_SETTLE` from the services. `MatchService.daily_bests(registered_from, registered_to)` first reaches the Verdict of every overdue open Match registered in that range (rows locked `FOR UPDATE`, same code path as `GET /matches/{id}`), then returns one `DailyBest(account_id, score, achieved_at)` per Account: its highest Credited Score and the registration time of the earliest Match reaching it. Matches knows nothing about the Tournament.
- **Day:** a Match belongs to the UTC date of its `registered_at`. Today is the UTC date of the server clock. A day is final once `day end + RESULTS_SETTLE` (303 s, the matches submission window) has passed; after that no Vote and no Verdict can change it.
- **Ranking (pure):** sort by score descending, then `achieved_at` ascending, then Account id; Ranks 1..n, unique.
- **Windows (pure):** top N = first N Standings, N in 1–100, default 10. Around me = Standings with Rank within my Rank ± K, K in 0–25, default 3; empty with no Standing if I have no Daily Best that day.
- **Medals (pure):** Medal Day = `(now − RESULTS_SETTLE).date() − 1 day`. Rank 1 gold, 2 silver, 3 bronze, only when the Daily Best is above 0. Computed on read, never stored.
- **Nicknames:** the accounts service gains `nicknames(account_ids) → dict`; the Leaderboard always shows the current Nickname.
- **Index:** a migration adds an index on `matches.registered_at`, which every day query filters by.
- **API contract** (all bearer, 401 from accounts):
  - `GET /tournament/leaderboard?day=&limit=` → 200 Leaderboard; `day` defaults to today.
  - `GET /tournament/leaderboard/me?day=&radius=` → 200 Around Me.
  - `GET /tournament/medals?account_id=…` (1–20 ids, repeated) → 200 Medals for the Medal Day; unknown Accounts get no Medal.
  - Leaderboard: `{day, ends_at, final, players, entries: [Standing]}`. Around Me: Leaderboard fields plus `me: Standing | null`. Standing: `{rank, account_id, nickname, score}`. Medals: `{day, medals: [{account_id, medal: gold | silver | bronze | null}]}` in request order without duplicates.
  - Errors: 422 for a future `day` or out-of-range `limit`, `radius`, `account_id` count.

## Testing Decisions

- Seams: the pure rules (no fixtures, no database) and the HTTP API on a real Postgres via testcontainers with the controllable clock. Nothing reaches into tables.
- Rules tests: day bounds and finality at the exact settle edge, ranking with ties on score and on time, top and around-me windows at both ends of the table, Medal Day before and after the settle edge, Medal tiers and the zero-score rule.
- HTTP tests: best of several Matches; Bots and unconfirmed seats absent; rejected Match ignored; Interrupted Match ranked at its Credited Score; reset at midnight; Match registered before midnight counting for its day; overdue Match decided by a Leaderboard read; around me with and without a Standing; past day; future day 422; Medals before and after the settle edge; 401.
- HTTP tests share one database across tests, so they create Matches on their own far-apart days (the clock is per-test) to stay independent.
- Prior art: `tests/features/matches/` (clock fixture, flow helpers).

## Out of Scope

- Weekly League, Medal history or collection, any reward beyond the current Medal.
- Caching or materialized rankings (ADR-0014 names the point at which to add them).
- Any client code.

## Further Notes

ADR-0006 mentions Postgres window functions for "around me". Ranking here happens in Python over the day's Daily Bests, because the ranking rule belongs to the Tournament while the data belongs to matches; ADR-0014 records the trade-off and the scale at which to move ranking into SQL.
