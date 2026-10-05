# Architecture — backend

Scope: the tournament API in `backend/`. Product rules live in GDD 9; this file covers how the code is shaped.

## Request flow

```
router  →  service  →  repository  →  PostgreSQL
  │           │
schemas     rules (pure functions)
```

| Layer | File | Owns | Knows about |
|---|---|---|---|
| Router | `router.py` | HTTP: path, status codes, auth dependency, OpenAPI text | service, schemas |
| Schemas | `schemas.py` | Pydantic request/response models — the public contract | nothing |
| Service | `service.py` | Use cases: orchestrates repository calls and rules in one transaction | repository, rules, models |
| Rules | `rules.py` | Pure domain decisions (majority vote, plausibility checks, best-of-day) | plain data only |
| Repository | `repository.py` | SQLAlchemy queries for the feature's tables | models, `AsyncSession` |
| Models | `models.py` | SQLAlchemy tables, subclasses of `core.models.Base` | nothing |

Files appear only when they have content: `health` has just a router and schemas.

**Rules** are the heart of the anti-cheat logic (GDD 9.3) and take dataclasses or Pydantic models in, decisions out, with no I/O. They get exhaustive unit tests that run without a database. Everything that touches the database is tested through the HTTP API against a real Postgres.

## Dependencies

FastAPI `Depends` is the DI container. `core/dependencies.py` exposes `SessionDep`; each feature adds its own `Annotated[..., Depends(...)]` aliases for its service. A feature imports another feature only through its `service.py`, never its repository or models.

## Application assembly

`main.create_app(settings)` is a factory: it builds the `Database`, attaches it to `app.state`, and includes every feature router. Tests call it with a test `Settings`; uvicorn runs it with `--factory`. There is no module-level `app`.

## Configuration

`core/settings.py` reads `PLOW_*` environment variables (and `.env` locally). Secrets never get defaults beyond the local development database.

## Contract

FastAPI generates the OpenAPI schema; `scripts/export_openapi.py` writes it to `docs/api/openapi.json`, which is committed. CI fails when it is stale. Operation ids are the route function names, so the client's C# DTOs and calls map one-to-one. See ADR-0007.

## Feature index

| Feature | Path | Status | Purpose |
|---|---|---|---|
| health | `features/health/` | active | Liveness of API and database |
| accounts | `features/accounts/` | planned | Guest login by device id, nickname, auth token (GDD 9.1) |
| matches | `features/matches/` | planned | Match registration, participant confirmation, result submission and verification (GDD 9.3) |
| tournament | `features/tournament/` | planned | Daily best score, leaderboard top and around-me, reset and Medals (GDD 9.2) |
