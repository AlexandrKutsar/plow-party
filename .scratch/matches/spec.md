# Matches

Status: ready-for-agent

## Problem Statement

The daily Tournament (GDD 9.2) ranks Match Scores, but the Host is an ordinary phone: whoever runs it could report any Score for anyone. The backend has guest Accounts and nothing else. It cannot tell a real Match from a made-up one, which Accounts actually played, or whether a reported table is believable. GDD 9.3 asks for registration before the start, confirmation by every Participant, majority voting on the Match Result, plausibility checks, and lower weight for Interrupted Matches.

## Solution

A Match is registered by its Host at the start of Countdown with its full Roster. Every other Player confirms their seat with their own Auth Token while Countdown lasts. After the Match each Confirmed Player submits the Match Result as a Vote. The backend reaches a Verdict once everyone has voted, or on the first read after the submission window closes: the version a strict majority of Votes agree on is accepted if it passes the plausibility checks, otherwise the whole Match is rejected with a reason. Accepted Matches carry a Weight (1 for a full Match, 0.5 for an Interrupted Match) and a Credited Score for each Confirmed Player. Bots and unconfirmed seats never earn Credited Score. All timing comes from the server clock.

## User Stories

1. As a Host, I want to register the Match with its Roster when Countdown starts, so that the backend knows the Match exists before anyone has a Score.
2. As a Host, I want to get a Match id back, so that I can hand it to every client through the Session.
3. As a Host, I want to be a Confirmed Player as soon as I register, so that I do not need a second call.
4. As a Host, I want a Roster of 4–6 Participants with distinct Slots 0–5 accepted, so that every legal Match can be registered.
5. As a Host, I want a malformed Roster (too few or too many seats, a Slot outside 0–5, a repeated Slot, the same Account twice) rejected with a 422 naming the rule, so that client bugs surface at once.
6. As a Host, I want registration refused if I am not in the Roster, so that nobody registers Matches for other people.
7. As a Host, I want registration refused if the Roster names an Account that does not exist, so that typos and forged ids surface.
8. As a Player, I want to confirm my seat with my own Auth Token, so that only I can make my Account take part.
9. As a Player, I want confirming twice to just succeed, so that a retry after a network error is safe.
10. As a Player, I want confirmation refused after Countdown is over, so that a Host cannot keep seats open to fill later.
11. As a Player, I want confirmation refused if I am not in the Roster, so that I cannot attach myself to strangers' Matches.
12. As a Player who did not confirm, I want my seat treated as a Bot, so that a Host cannot credit or vote for my Account without me.
13. As a Confirmed Player, I want to submit the final table (Score per Slot, and whether the Match was interrupted and at which second), so that my view of the Match counts.
14. As a Confirmed Player, I want a Vote whose Slots differ from the Roster rejected with a 422, so that tables from a different Match cannot be mixed in.
15. As a Confirmed Player, I want resubmitting the same table to succeed and a different table to be refused, so that retries are safe but nobody can change their Vote.
16. As a backend, I want a Vote refused when it arrives sooner than Countdown plus the claimed play time after registration, so that a Host cannot fake a Match in seconds.
17. As a backend, I want Votes refused after the submission window closes (2 minutes after a full Match would end), so that a Match does not stay open forever.
18. As a backend, I want Votes refused once the Match has a Verdict, so that a Verdict never changes.
19. As a Player in a Match with several live Players, I want the version that a strict majority of Votes agree on to win, so that one cheating Host cannot outvote the rest.
20. As a Player, I want the Match rejected when Votes split without a strict majority, so that a 1-vs-1 or 2-vs-2 dispute never credits a forged table.
21. As a Player in "me + bots", I want my single Vote accepted after plausibility checks, so that solo Matches count in the Tournament (GDD 9.2).
22. As a backend, I want a Match rejected if any Score exceeds the maximum Score per second over the played time, so that impossible Scores never reach the Tournament.
23. As a backend, I want the whole Match rejected rather than single rows, so that a forged table cannot partially succeed.
24. As a Player whose Host left, I want the Interrupted Match accepted with Weight 0.5, so that my effort still counts, but less than a full Match.
25. As a backend, I want Interrupted Matches shorter than 30 seconds rejected, so that quitting at once cannot farm entries.
26. As a backend, I want the played time of an Interrupted Match to be the smallest second among the winning Votes, so that the plausibility check uses the conservative value.
27. As a Player, I want the Verdict reached the moment the last Confirmed Player votes, so that I see the outcome on the Results screen.
28. As a Player whose opponents never voted, I want the Verdict reached on the first read after the window closes, so that my Match still counts.
29. As a backend, I want a Match with no Votes at all rejected once its window closes, so that it never counts.
30. As any authenticated Account, I want to read a Match: its status, Verdict reason, Weight, played time, and each seat's Account, confirmation, Score, placement and Credited Score, so that the client can show the outcome.
31. As a Player, I want placements computed by the backend from Scores, with ties sharing a place, so that placements cannot be forged separately from Scores.
32. As a backend, I want two last Votes arriving together to reach exactly one Verdict, so that concurrency never double-credits.
33. As a client developer, I want 404 for an unknown Match, 403 when I am not allowed to act on it, and 409 when the Match is in the wrong phase, so that I can react to each case.
34. As a backend developer, I want every decision as a pure function with exhaustive unit tests, so that the anti-cheat logic is safe to change.
35. As a backend developer building the Tournament, I want accepted Matches to carry Credited Score per Account, so that the Tournament only sums and ranks.
36. As a maintainer, I want the weak points (solo Matches, collusion) written down, so that the README describes the protection honestly.

## Implementation Decisions

- **Module:** new feature slice `matches` with router, schemas, service, rules, repository, models and a module `CLAUDE.md`; its row in the backend feature index moves to `active`. Router included by the app factory, models imported by the migration environment.
- **Glossary:** Roster, Confirmed Player, Vote, Verdict, Interrupted Match, Weight, Credited Score (added to `GLOSSARY.md`). Decision record: ADR-0013.
- **Clock:** a small core dependency returning the current UTC time; the matches service reads time only through it, and tests override it.
- **Constants (rules):** Countdown 3 s, Match 180 s, confirmation window 15 s after registration, timing tolerance 5 s, submission window closes 120 s after a full Match would end (303 s after registration), minimum Interrupted Match 30 s, Interrupted Match Weight 0.5, maximum Score per second 30 (a full Bucket is worth 200 and takes well over 6 s to fill and unload).
- **Roster rules (pure):** 4–6 seats, Slots distinct and within 0–5, each Account at most once, at least one Account. Run in the request schema, so a broken rule is a 422 naming it. Host-in-Roster and Accounts-exist are checked by the service and answer 422.
- **Vote rules (pure):** Vote Slots equal the Roster Slots; Scores are non-negative integers; interrupted second, if present, is within 1–179; the Vote's played time is 180 or the interrupted second; it is refused (409) earlier than registration + Countdown + played time − tolerance, or after the window closes.
- **Verdict rules (pure):** Votes agree when their Scores per Slot and their interrupted flag are equal. A version wins with strictly more than half of the Votes; otherwise rejected "no majority". No Votes: rejected. Played time is 180, or the smallest interrupted second among the winning Votes; under 30 s is rejected. Any Score over 30 × played time rejects the whole Match. Accepted: Weight 1 or 0.5, placement by Score with ties sharing the higher place (1, 2, 2, 4), Credited Score = floor(Score × Weight) for Confirmed Players only.
- **Finalization:** when the number of Votes equals the number of Confirmed Players, or on read after the window closes. The Match row is locked (`SELECT … FOR UPDATE`) in every write path and in finalization.
- **API contract:**
  - `POST /matches` — bearer, `{roster: [{slot, account_id | null}]}` → 201 Match.
  - `POST /matches/{match_id}/confirm` — bearer → 204.
  - `POST /matches/{match_id}/votes` — bearer, `{scores: [{slot, score}], interrupted_at_seconds: int | null}` → 200 Match.
  - `GET /matches/{match_id}` — bearer → 200 Match.
  - Match: `{match_id, status: open | accepted | rejected, rejection_reason, registered_at, interrupted, played_seconds, weight, participants: [{slot, account_id, confirmed, score, placement, credited_score}]}`; Verdict fields are null while open.
  - Errors: 401 from accounts; 404 unknown Match; 403 caller not in the Roster (confirm) or not a Confirmed Player (vote); 409 wrong phase (confirmation closed, too early, window closed, Verdict reached, Vote changed); 422 malformed body or Roster/Vote rule.
- **Schema:** `matches` (id, host account, registered_at, status, rejection_reason, interrupted, played_seconds, weight, decided_at); `match_participants` (match, slot, account nullable, confirmed, score, placement, credited_score; unique account per match); `match_votes` (match, account, submitted_at, scores as JSONB, interrupted_at_seconds; one per Account per Match). Account columns reference `accounts.id` by table name. One migration.
- **Cross-feature:** the accounts service gains a method that reports which of a set of Account ids exist; matches uses only `CurrentAccountDep` and that method.

## Testing Decisions

- Seams: the pure rules (no fixtures, no database) and the HTTP API on a real Postgres via testcontainers. Nothing reaches into tables.
- Rules tests cover every Roster rule, Vote timing edges (exactly at the bounds), majority with 1–6 Votes including ties, plausibility bounds, the Interrupted Match minimum and Weight, placements with ties, Credited Score rounding, and when finalization is due.
- HTTP tests drive full flows with a controllable clock: register → confirm → votes → Verdict; solo Match; split Votes; unconfirmed seat; Interrupted Match; lazy finalization after the window; each error status.
- Prior art: accounts' `test_rules.py` and `test_accounts_api.py`; the `client` fixture, extended for matches with a clock override.

## Out of Scope

- Tournament aggregation, daily best, leaderboard, Medals.
- Host migration and reconnection.
- Limiting how many open Matches an Account may have; rate limiting.
- Any client code.

## Further Notes

The protection is partial by design (GDD 9.3): a solo Match is guarded only by plausibility, and colluding Players can outvote an honest one. ADR-0013 records this.
