import uuid
from collections.abc import Sequence
from datetime import datetime

from sqlalchemy import select
from sqlalchemy.dialects.postgresql import distinct_on
from sqlalchemy.ext.asyncio import AsyncSession

from plow_party_api.features.matches.models import Match, MatchParticipant
from plow_party_api.features.matches.rules import MatchStatus


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

    async def lock_open_registered_between(
        self, registered_from: datetime, registered_before: datetime
    ) -> Sequence[Match]:
        statement = (
            select(Match)
            .where(
                Match.status == MatchStatus.OPEN,
                Match.registered_at >= registered_from,
                Match.registered_at < registered_before,
            )
            .order_by(Match.id)
            .with_for_update()
            .execution_options(populate_existing=True)
        )
        return (await self._session.scalars(statement)).all()

    async def best_credited_scores(
        self, registered_from: datetime, registered_before: datetime
    ) -> list[tuple[uuid.UUID, int, datetime]]:
        statement = (
            select(
                MatchParticipant.account_id, MatchParticipant.credited_score, Match.registered_at
            )
            .join(Match, Match.id == MatchParticipant.match_id)
            .where(
                Match.registered_at >= registered_from,
                Match.registered_at < registered_before,
                Match.status == MatchStatus.ACCEPTED,
                MatchParticipant.credited_score.is_not(None),
            )
            .ext(distinct_on(MatchParticipant.account_id))
            .order_by(
                MatchParticipant.account_id,
                MatchParticipant.credited_score.desc(),
                Match.registered_at,
            )
        )
        return [
            (account_id, score, registered_at)
            for account_id, score, registered_at in await self._session.execute(statement)
            if account_id is not None and score is not None
        ]
