import itertools
from collections.abc import AsyncIterator
from datetime import UTC, datetime, timedelta

import pytest
from httpx import ASGITransport, AsyncClient

from plow_party_api.core.clock import get_clock
from plow_party_api.core.database import Database
from plow_party_api.core.settings import Settings
from plow_party_api.main import create_app
from tests.features.matches.fakes import FakeClock

FIRST_TEST_DAY = datetime(2031, 1, 1, 10, 0, tzinfo=UTC)
DAYS_BETWEEN_TESTS = timedelta(days=10)
_test_numbers = itertools.count()


@pytest.fixture
def clock() -> FakeClock:
    fake = FakeClock()
    fake.now = FIRST_TEST_DAY + DAYS_BETWEEN_TESTS * next(_test_numbers)
    return fake


@pytest.fixture
async def client(database_url: str, clock: FakeClock) -> AsyncIterator[AsyncClient]:
    app = create_app(Settings(database_url=database_url))
    app.dependency_overrides[get_clock] = lambda: clock
    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as http:
        yield http
    database: Database = app.state.database
    await database.dispose()
