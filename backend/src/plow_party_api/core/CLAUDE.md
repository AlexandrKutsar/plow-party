# core

Cross-feature plumbing; holds no domain rules.

## Entry points

- `Settings` — `PLOW_*` environment variables; `database_url` is the only one so far.
- `Database` — owns the async engine and session factory; one per app, stored on `app.state.database` by `create_app`.
- `SessionDep` — `AsyncSession` per request, closed after the response. Services commit; repositories never do.
- `Base` — declarative base with a constraint naming convention, so Alembic autogenerate produces stable names.
- `ClockDep` — the current UTC time as a callable; features read time only through it so tests can override `get_clock`.
