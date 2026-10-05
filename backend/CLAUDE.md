# backend — tournament API

FastAPI + PostgreSQL service for guest accounts, match registration, result submission, and the daily Tournament (GDD 9). Python 3.13, uv, SQLAlchemy 2.0 async + asyncpg, Alembic, Pydantic v2. Architecture: `docs/backend-architecture.md`. Contract with the client: `docs/api/openapi.json`.

Run every command from `backend/`.

## Where code goes

`src/plow_party_api/features/<feature>/` holds one vertical slice: `router.py`, `service.py`, `repository.py`, `models.py`, `schemas.py`, and a `CLAUDE.md`. `core/` holds settings, database, and shared dependencies. Tests mirror the source tree under `tests/features/<feature>/`. A new feature starts by writing its `CLAUDE.md`, then registers its router in `main.py` and its models import in `migrations/env.py`.

## Self-documenting code

Code carries zero comments and zero docstrings. OpenAPI text goes into `summary=` and `description=` on the route decorator and `Field(description=...)` on schemas. Tool directives (`# noqa`, `# pyright:`) are the only allowed `#` lines, and each needs a reason that a name cannot express. `scripts/check_no_comments.py` enforces this in CI.

## Verify loop

A PostToolUse hook runs `ruff check --fix` and `ruff format` on every `.py` you edit. After each logical change:

1. `uv run ruff check .` and `uv run pyright` — zero findings (pyright is strict).
2. `uv run python scripts/check_no_comments.py` — no output.
3. `uv run pytest` — needs Docker running; tests start a throwaway Postgres via testcontainers and apply all migrations. Pure-logic tests need no fixtures and run without Docker: `uv run pytest tests/features/<feature>/test_<rules>.py`.
4. When routes or schemas changed: `uv run python scripts/export_openapi.py` and commit `docs/api/openapi.json` with the change.

CI (`.github/workflows/backend.yml`) runs the same steps plus `docker build`.

## Migrations

`uv run alembic revision --autogenerate -m "<what changed>"`, review the generated file, then `uv run alembic upgrade head` against the local database. Never edit a migration that is already on `main`; add a new one.

## Local run

`docker compose up --build` starts Postgres and the API on `http://localhost:8000` (`/docs` for Swagger). For a reload loop, `docker compose up db` and `uv run uvicorn --factory plow_party_api.main:create_app --reload`, with `.env` copied from `.env.example`.
