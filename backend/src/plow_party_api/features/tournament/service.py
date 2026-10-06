import uuid
from collections.abc import Mapping, Sequence
from dataclasses import dataclass
from datetime import date, datetime
from typing import Annotated

from fastapi import Depends

from plow_party_api.core.clock import ClockDep
from plow_party_api.core.dependencies import SessionDep
from plow_party_api.features.accounts.service import AccountService
from plow_party_api.features.matches.service import RESULTS_SETTLE, MatchService
from plow_party_api.features.tournament.rules import (
    DailyBest,
    DayError,
    Medal,
    Standing,
    around,
    check_day,
    day_end,
    day_of,
    day_start,
    is_final,
    medal,
    medal_day,
    rank,
    standing_of,
    top,
)


class TournamentDayError(Exception):
    pass


@dataclass(frozen=True)
class StandingView:
    rank: int
    account_id: uuid.UUID
    nickname: str
    score: int


@dataclass(frozen=True)
class LeaderboardView:
    day: date
    ends_at: datetime
    final: bool
    players: int
    me: StandingView | None
    entries: list[StandingView]


@dataclass(frozen=True)
class MedalView:
    account_id: uuid.UUID
    medal: Medal | None


@dataclass(frozen=True)
class MedalsView:
    day: date
    medals: list[MedalView]


def _view(standing: Standing, nicknames: Mapping[uuid.UUID, str]) -> StandingView:
    return StandingView(
        rank=standing.rank,
        account_id=standing.account_id,
        nickname=nicknames[standing.account_id],
        score=standing.score,
    )


def _checked_day(day: date | None, now: datetime) -> date:
    if day is None:
        return day_of(now)
    try:
        check_day(day, now)
    except DayError as error:
        raise TournamentDayError(str(error)) from error
    return day


class TournamentService:
    def __init__(self, session: SessionDep, clock: ClockDep) -> None:
        self._clock = clock
        self._matches = MatchService(session, clock)
        self._accounts = AccountService(session)

    async def leaderboard(
        self, account_id: uuid.UUID, day: date | None, limit: int
    ) -> LeaderboardView:
        now = self._clock()
        day = _checked_day(day, now)
        standings = await self._standings(day)
        me = standing_of(standings, account_id)
        return await self._board(day, now, standings, me, top(standings, limit))

    async def around_me(
        self, account_id: uuid.UUID, day: date | None, radius: int
    ) -> LeaderboardView:
        now = self._clock()
        day = _checked_day(day, now)
        standings = await self._standings(day)
        me, window = around(standings, account_id, radius)
        return await self._board(day, now, standings, me, window)

    async def medals(self, account_ids: Sequence[uuid.UUID]) -> MedalsView:
        day = medal_day(self._clock(), RESULTS_SETTLE)
        standings = await self._standings(day)
        medals: list[MedalView] = []
        for account_id in dict.fromkeys(account_ids):
            standing = standing_of(standings, account_id)
            medals.append(MedalView(account_id, medal(standing) if standing else None))
        return MedalsView(day=day, medals=medals)

    async def _standings(self, day: date) -> list[Standing]:
        bests = await self._matches.best_credited_scores(day_start(day), day_end(day))
        return rank(
            [
                DailyBest(
                    account_id=best.account_id, score=best.score, achieved_at=best.achieved_at
                )
                for best in bests
            ]
        )

    async def _board(
        self,
        day: date,
        now: datetime,
        standings: Sequence[Standing],
        me: Standing | None,
        entries: Sequence[Standing],
    ) -> LeaderboardView:
        shown = [*entries, *([me] if me else [])]
        nicknames = await self._accounts.nicknames({standing.account_id for standing in shown})
        return LeaderboardView(
            day=day,
            ends_at=day_end(day),
            final=is_final(day, now, RESULTS_SETTLE),
            players=len(standings),
            me=_view(me, nicknames) if me else None,
            entries=[_view(standing, nicknames) for standing in entries],
        )


TournamentServiceDep = Annotated[TournamentService, Depends()]
