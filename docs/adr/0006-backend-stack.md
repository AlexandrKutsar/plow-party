# Backend stack: FastAPI, async SQLAlchemy, PostgreSQL, uv

The tournament API is FastAPI on Python 3.13 with SQLAlchemy 2.0 async (asyncpg), Alembic, and Pydantic v2, managed by uv and checked by ruff and strict pyright. FastAPI gives typed request handling and a generated OpenAPI contract for free; PostgreSQL handles the leaderboard queries (window functions for "around me") and the transactional result voting. SQLModel was rejected because it merges table models and API schemas, which hides the layer boundary this portfolio is meant to show. Tests run against a real Postgres through testcontainers instead of SQLite, because the leaderboard and locking behaviour differ between the two.

## Consequences

Running the full test suite locally requires Docker. Deployment is containerised (`backend/Dockerfile`) and host-agnostic; the hosting target is open because the existing VPS is unreachable from Russia, where the players are.

The Tournament ranks in Python instead of with window functions; see ADR-0014.
