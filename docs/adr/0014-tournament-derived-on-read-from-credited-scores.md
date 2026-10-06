# Tournament derived on read from Credited Scores

The Tournament owns no tables. Every Leaderboard and Medal read asks the matches service for the best Credited Score per Account among Matches registered on one UTC day; the matches service first reaches the Verdict of that day's overdue open Matches (ADR-0013), then answers with one aggregate query. The Tournament ranks the result in Python with pure rules. A Match belongs to the UTC date of its server-side registration, not of its Verdict, so a late Verdict never moves a result to another day. A day is final 303 s after it ends, when the last Match registered that day can no longer take a Vote; Medals belong to the latest final day and are recomputed on each read, which is stable because a final day cannot change.

## Considered Options

- Matches pushes each accepted Credited Score into a tournament table: cheap indexed reads, but matches would depend on the Tournament while the Tournament still has to trigger overdue Verdicts, making a dependency cycle; it also duplicates data that can drift from the Verdict.
- Ranking with Postgres window functions, as ADR-0006 anticipated: faster for large days, but the query would live in the matches repository, which would then own the Tournament's ranking rule. The ranking stays a pure, unit-tested rule instead.
- Medals materialized on first read after reset: a table and an idempotent write for a value that is already deterministic.
- Counting a Match on the day of its Verdict: lazy Verdicts are reached at read time, so the day would depend on when someone looked.

## Consequences

Matches knows nothing about the Tournament; the dependency runs one way. Every read sorts the whole day in memory, and a Leaderboard read may write Verdicts. That is fine for the portfolio's scale; once a day holds tens of thousands of Accounts, add a `daily_bests` table written in the Verdict transaction through a callback the matches service accepts, and move ranking into SQL. Medals flip at 00:05:03 UTC, not 00:00, and show the day before yesterday until then. This partly supersedes ADR-0006's note that "around me" uses window functions.
