import os
from collections.abc import AsyncIterator, Iterator
from pathlib import Path

import pytest
from alembic import command
from alembic.config import Config
from httpx import ASGITransport, AsyncClient
from testcontainers.community.postgres import PostgresContainer

from plow_party_api.core.database import Database
from plow_party_api.core.settings import Settings
from plow_party_api.main import create_app

BACKEND_ROOT = Path(__file__).resolve().parents[1]


@pytest.fixture(scope="session")
def database_url() -> Iterator[str]:
    with PostgresContainer("postgres:17-alpine", driver="asyncpg") as postgres:
        url = postgres.get_connection_url()
        os.environ["PLOW_DATABASE_URL"] = url
        command.upgrade(Config(str(BACKEND_ROOT / "alembic.ini")), "head")
        yield url


@pytest.fixture
async def client(database_url: str) -> AsyncIterator[AsyncClient]:
    app = create_app(Settings(database_url=database_url))
    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as http:
        yield http
    database: Database = app.state.database
    await database.dispose()
