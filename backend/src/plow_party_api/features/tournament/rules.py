import uuid
from collections.abc import Sequence
from dataclasses import dataclass
from datetime import UTC, date, datetime, time, timedelta
from enum import StrEnum

DEFAULT_TOP = 10
MAX_TOP = 100
DEFAULT_RADIUS = 3
MAX_RADIUS = 25
MAX_MEDAL_ACCOUNTS = 20
ONE_DAY = timedelta(days=1)


class Medal(StrEnum):
    GOLD = "gold"
    SILVER = "silver"
    BRONZE = "bronze"


MEDALS_BY_RANK = {1: Medal.GOLD, 2: Medal.SILVER, 3: Medal.BRONZE}


class DayError(ValueError):
    pass


@dataclass(frozen=True)
class DailyBest:
    account_id: uuid.UUID
    score: int
    achieved_at: datetime


@dataclass(frozen=True)
class Standing:
    rank: int
    account_id: uuid.UUID
    score: int


def day_of(moment: datetime) -> date:
    return moment.astimezone(UTC).date()


def day_start(day: date) -> datetime:
    return datetime.combine(day, time.min, tzinfo=UTC)


def day_end(day: date) -> datetime:
    return day_start(day) + ONE_DAY


def is_final(day: date, now: datetime, settle: timedelta) -> bool:
    return now >= day_end(day) + settle


def check_day(day: date, now: datetime) -> None:
    if day > day_of(now):
        raise DayError("Tournament Day must not be in the future")


def rank(bests: Sequence[DailyBest]) -> list[Standing]:
    ordered = sorted(bests, key=lambda best: (-best.score, best.achieved_at, best.account_id))
    return [
        Standing(rank=place, account_id=best.account_id, score=best.score)
        for place, best in enumerate(ordered, start=1)
    ]


def top(standings: Sequence[Standing], limit: int) -> list[Standing]:
    return list(standings[:limit])


def standing_of(standings: Sequence[Standing], account_id: uuid.UUID) -> Standing | None:
    return next((standing for standing in standings if standing.account_id == account_id), None)


def around(
    standings: Sequence[Standing], account_id: uuid.UUID, radius: int
) -> tuple[Standing | None, list[Standing]]:
    me = standing_of(standings, account_id)
    if me is None:
        return None, []
    return me, [standing for standing in standings if abs(standing.rank - me.rank) <= radius]


def medal_day(now: datetime, settle: timedelta) -> date:
    return day_of(now - settle) - ONE_DAY


def medal(standing: Standing) -> Medal | None:
    return MEDALS_BY_RANK.get(standing.rank) if standing.score > 0 else None
