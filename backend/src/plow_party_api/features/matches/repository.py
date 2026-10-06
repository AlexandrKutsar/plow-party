import uuid

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from plow_party_api.features.matches.models import Match


class MatchRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    def add(self, match: Match) -> None:
        self._session.add(match)

    async def find(self, match_id: uuid.UUID, *, for_update: bool = False) -> Match | None:
        statement = select(Match).where(Match.id == match_id)
        if for_update:
            statement = statement.with_for_update().execution_options(populate_existing=True)
        return await self._session.scalar(statement)
