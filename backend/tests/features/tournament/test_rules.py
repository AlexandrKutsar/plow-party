import uuid
from datetime import UTC, date, datetime, timedelta, timezone

import pytest

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

SETTLE = timedelta(seconds=303)
DAY = date(2026, 10, 6)
NOON = datetime(2026, 10, 6, 12, 0, tzinfo=UTC)


def at(seconds: float) -> datetime:
    return NOON + timedelta(seconds=seconds)


def ids(count: int) -> list[uuid.UUID]:
    return sorted(uuid.uuid4() for _ in range(count))


def standings(count: int) -> list[Standing]:
    return [
        Standing(rank=place, account_id=account_id, score=1000 - place)
        for place, account_id in enumerate(ids(count), start=1)
    ]


def test_day_of_is_the_utc_date_of_the_moment() -> None:
    moscow = timezone(timedelta(hours=3))

    assert day_of(datetime(2026, 10, 7, 2, 30, tzinfo=moscow)) == DAY
    assert day_of(datetime(2026, 10, 6, 23, 59, 59, tzinfo=UTC)) == DAY
    assert day_of(datetime(2026, 10, 7, 0, 0, tzinfo=UTC)) == date(2026, 10, 7)


def test_day_bounds_span_midnight_to_midnight_utc() -> None:
    assert day_start(DAY) == datetime(2026, 10, 6, 0, 0, tzinfo=UTC)
    assert day_end(DAY) == datetime(2026, 10, 7, 0, 0, tzinfo=UTC)


def test_is_final_once_the_submission_window_after_midnight_has_passed() -> None:
    settled = day_end(DAY) + SETTLE

    assert not is_final(DAY, settled - timedelta(microseconds=1), SETTLE)
    assert is_final(DAY, settled, SETTLE)


def test_check_day_accepts_today_and_past_days() -> None:
    check_day(DAY, NOON)
    check_day(date(2020, 1, 1), NOON)


def test_check_day_rejects_a_future_day() -> None:
    with pytest.raises(DayError, match="future"):
        check_day(date(2026, 10, 7), NOON)


def test_rank_orders_by_score_descending() -> None:
    low, high, middle = ids(3)

    ranked = rank(
        [
            DailyBest(account_id=low, score=10, achieved_at=at(0)),
            DailyBest(account_id=high, score=300, achieved_at=at(0)),
            DailyBest(account_id=middle, score=150, achieved_at=at(0)),
        ]
    )

    assert ranked == [
        Standing(rank=1, account_id=high, score=300),
        Standing(rank=2, account_id=middle, score=150),
        Standing(rank=3, account_id=low, score=10),
    ]


def test_rank_breaks_a_score_tie_by_who_reached_it_first() -> None:
    first, second = ids(2)

    ranked = rank(
        [
            DailyBest(account_id=first, score=200, achieved_at=at(600)),
            DailyBest(account_id=second, score=200, achieved_at=at(60)),
        ]
    )

    assert [standing.account_id for standing in ranked] == [second, first]
    assert [standing.rank for standing in ranked] == [1, 2]


def test_rank_breaks_a_full_tie_by_account_id() -> None:
    smaller, larger = ids(2)

    ranked = rank(
        [
            DailyBest(account_id=larger, score=200, achieved_at=at(0)),
            DailyBest(account_id=smaller, score=200, achieved_at=at(0)),
        ]
    )

    assert [standing.account_id for standing in ranked] == [smaller, larger]


def test_rank_of_nobody_is_empty() -> None:
    assert rank([]) == []


def test_top_takes_the_first_standings() -> None:
    table = standings(5)

    assert top(table, 3) == table[:3]
    assert top(table, 10) == table


def test_standing_of_finds_the_accounts_standing() -> None:
    table = standings(3)

    assert standing_of(table, table[1].account_id) == table[1]
    assert standing_of(table, uuid.uuid4()) is None


def test_around_returns_my_standing_and_neighbours_within_radius() -> None:
    table = standings(10)

    me, window = around(table, table[4].account_id, 2)

    assert me == table[4]
    assert [standing.rank for standing in window] == [3, 4, 5, 6, 7]


def test_around_is_clipped_at_both_ends_of_the_table() -> None:
    table = standings(4)

    _, at_top = around(table, table[0].account_id, 2)
    _, at_bottom = around(table, table[3].account_id, 2)

    assert [standing.rank for standing in at_top] == [1, 2, 3]
    assert [standing.rank for standing in at_bottom] == [2, 3, 4]


def test_around_with_zero_radius_is_just_me() -> None:
    table = standings(3)

    me, window = around(table, table[1].account_id, 0)

    assert window == [me]


def test_around_without_my_standing_is_empty() -> None:
    me, window = around(standings(3), uuid.uuid4(), 3)

    assert me is None
    assert window == []


def test_medal_day_is_yesterday_once_yesterday_is_final() -> None:
    settled = day_end(DAY) + SETTLE

    assert medal_day(settled, SETTLE) == DAY
    assert medal_day(settled + timedelta(hours=23), SETTLE) == DAY


def test_medal_day_stays_on_the_day_before_until_yesterday_is_final() -> None:
    just_before = day_end(DAY) + SETTLE - timedelta(microseconds=1)

    assert medal_day(just_before, SETTLE) == DAY - timedelta(days=1)


@pytest.mark.parametrize(
    ("place", "expected"),
    [(1, Medal.GOLD), (2, Medal.SILVER), (3, Medal.BRONZE), (4, None), (50, None)],
)
def test_medal_goes_to_the_top_three_ranks(place: int, expected: Medal | None) -> None:
    assert medal(Standing(rank=place, account_id=uuid.uuid4(), score=100)) == expected


def test_medal_needs_a_daily_best_above_zero() -> None:
    assert medal(Standing(rank=1, account_id=uuid.uuid4(), score=0)) is None
