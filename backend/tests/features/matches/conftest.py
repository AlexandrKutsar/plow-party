from collections.abc import AsyncIterator

import pytest
from httpx import ASGITransport, AsyncClient

from plow_party_api.core.clock import get_clock
from plow_party_api.core.database import Database
from plow_party_api.core.settings import Settings
from plow_party_api.main import create_app
from tests.features.matches.fakes import FakeClock


@pytest.fixture
def clock() -> FakeClock:
    return FakeClock()


@pytest.fixture
async def client(database_url: str, clock: FakeClock) -> AsyncIterator[AsyncClient]:
    app = create_app(Settings(database_url=database_url))
    app.dependency_overrides[get_clock] = lambda: clock
    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as http:
        yield http
    database: Database = app.state.database
    await database.dispose()
