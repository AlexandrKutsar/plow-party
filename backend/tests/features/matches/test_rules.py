import uuid
from datetime import UTC, datetime, timedelta

import pytest

from plow_party_api.features.matches.rules import (
    Accepted,
    Rejected,
    RosterError,
    Seat,
    Vote,
    VoteError,
    VoteTimingError,
    check_roster,
    check_vote,
    confirmation_open,
    credited_score,
    decide,
    finalization_due,
    placements,
)

REGISTERED_AT = datetime(2026, 10, 6, 12, 0, tzinfo=UTC)


def at(seconds: float) -> datetime:
    return REGISTERED_AT + timedelta(seconds=seconds)


def vote(scores: dict[int, int] | None = None, interrupted_at: int | None = None) -> Vote:
    return Vote(
        scores={0: 100, 1: 80, 2: 60, 3: 40} if scores is None else scores,
        interrupted_at_seconds=interrupted_at,
    )


def seats(*slots: int) -> list[Seat]:
    return [Seat(slot=slot, account_id=uuid.uuid4()) for slot in slots]


@pytest.mark.parametrize("slots", [(0, 1, 2, 3), (0, 1, 2, 3, 4), (5, 4, 3, 2, 1, 0)])
def test_check_roster_four_to_six_distinct_slots_is_accepted(slots: tuple[int, ...]) -> None:
    roster = seats(*slots)

    assert check_roster(roster) == sorted(roster, key=lambda seat: seat.slot)


@pytest.mark.parametrize("slots", [(), (0, 1, 2)])
def test_check_roster_fewer_than_four_seats_is_rejected(slots: tuple[int, ...]) -> None:
    with pytest.raises(RosterError, match="at least 4"):
        check_roster(seats(*slots))


def test_check_roster_more_than_six_seats_is_rejected() -> None:
    with pytest.raises(RosterError, match="at most 6"):
        check_roster(seats(0, 1, 2, 3, 4, 5, 5))


@pytest.mark.parametrize("slot", [-1, 6])
def test_check_roster_slot_outside_zero_to_five_is_rejected(slot: int) -> None:
    with pytest.raises(RosterError, match="Slot must be within 0-5"):
        check_roster(seats(0, 1, 2, slot))


def test_check_roster_repeated_slot_is_rejected() -> None:
    with pytest.raises(RosterError, match="Slots must be distinct"):
        check_roster(seats(0, 1, 2, 2))


def test_check_roster_same_account_twice_is_rejected() -> None:
    account_id = uuid.uuid4()
    roster = [Seat(slot=0, account_id=account_id), Seat(slot=1, account_id=account_id)]

    with pytest.raises(RosterError, match="Account may hold only one Slot"):
        check_roster([*roster, *seats(2, 3)])


def test_check_roster_bots_may_repeat_the_empty_account() -> None:
    roster = [Seat(slot=slot, account_id=None) for slot in (1, 2, 3)]

    assert check_roster([*seats(0), *roster])[1:] == roster


def test_check_roster_only_bots_is_rejected() -> None:
    with pytest.raises(RosterError, match="at least one Player"):
        check_roster([Seat(slot=slot, account_id=None) for slot in range(4)])


@pytest.mark.parametrize("seconds", [0, 14.9, 15])
def test_confirmation_open_until_fifteen_seconds_after_registration(seconds: float) -> None:
    assert confirmation_open(REGISTERED_AT, at(seconds))


def test_confirmation_closed_after_fifteen_seconds() -> None:
    assert not confirmation_open(REGISTERED_AT, at(15.001))


def test_check_vote_full_match_at_countdown_plus_180_less_tolerance_is_accepted() -> None:
    check_vote({0, 1, 2, 3}, vote(), REGISTERED_AT, at(178))


def test_check_vote_full_match_sooner_than_countdown_plus_180_less_tolerance_is_too_early() -> None:
    with pytest.raises(VoteTimingError, match="too early"):
        check_vote({0, 1, 2, 3}, vote(), REGISTERED_AT, at(177.9))


def test_check_vote_interrupted_match_may_arrive_after_its_own_played_time() -> None:
    check_vote({0, 1, 2, 3}, vote(interrupted_at=60), REGISTERED_AT, at(58))


def test_check_vote_interrupted_match_sooner_than_its_played_time_is_too_early() -> None:
    with pytest.raises(VoteTimingError, match="too early"):
        check_vote({0, 1, 2, 3}, vote(interrupted_at=60), REGISTERED_AT, at(57.9))


def test_check_vote_at_window_close_is_accepted() -> None:
    check_vote({0, 1, 2, 3}, vote(), REGISTERED_AT, at(303))


def test_check_vote_after_window_close_is_refused() -> None:
    with pytest.raises(VoteTimingError, match="window"):
        check_vote({0, 1, 2, 3}, vote(), REGISTERED_AT, at(303.001))


@pytest.mark.parametrize(
    "scores", [{0: 1, 1: 1, 2: 1}, {0: 1, 1: 1, 2: 1, 3: 1, 4: 1}, {0: 1, 1: 1, 2: 1, 5: 1}]
)
def test_check_vote_slots_other_than_roster_are_rejected(scores: dict[int, int]) -> None:
    with pytest.raises(VoteError, match="Slots of the Roster"):
        check_vote({0, 1, 2, 3}, vote(scores), REGISTERED_AT, at(200))


def test_check_vote_negative_score_is_rejected() -> None:
    with pytest.raises(VoteError, match="non-negative"):
        check_vote({0, 1, 2, 3}, vote({0: 1, 1: 1, 2: 1, 3: -1}), REGISTERED_AT, at(200))


@pytest.mark.parametrize("second", [0, 180])
def test_check_vote_interrupted_second_outside_1_to_179_is_rejected(second: int) -> None:
    with pytest.raises(VoteError, match="1-179"):
        check_vote({0, 1, 2, 3}, vote(interrupted_at=second), REGISTERED_AT, at(200))


def test_finalization_due_when_every_confirmed_player_voted() -> None:
    assert finalization_due(confirmed_players=3, votes=3, registered_at=REGISTERED_AT, now=at(200))


def test_finalization_not_due_while_votes_missing_and_window_open() -> None:
    assert not finalization_due(
        confirmed_players=3, votes=2, registered_at=REGISTERED_AT, now=at(303)
    )


@pytest.mark.parametrize("votes", [0, 2])
def test_finalization_due_after_window_closes_whatever_the_votes(votes: int) -> None:
    assert finalization_due(
        confirmed_players=3, votes=votes, registered_at=REGISTERED_AT, now=at(303.001)
    )


HONEST = {0: 300, 1: 250, 2: 200, 3: 150}
FORGED = {0: 9000, 1: 250, 2: 200, 3: 150}
OTHER = {0: 300, 1: 250, 2: 200, 3: 0}


def test_decide_no_votes_is_rejected() -> None:
    assert decide([]) == Rejected("No Match Result was submitted")


def test_decide_single_vote_is_accepted_at_full_weight() -> None:
    assert decide([vote(HONEST)]) == Accepted(
        scores=HONEST, interrupted=False, played_seconds=180, weight=1.0
    )


@pytest.mark.parametrize(
    "tables",
    [
        [HONEST, HONEST],
        [HONEST, HONEST, FORGED],
        [FORGED, HONEST, HONEST],
        [HONEST, HONEST, HONEST, FORGED, OTHER],
        [HONEST, HONEST, HONEST, HONEST, FORGED, FORGED],
    ],
)
def test_decide_version_with_strict_majority_wins(tables: list[dict[int, int]]) -> None:
    verdict = decide([vote(table) for table in tables])

    assert isinstance(verdict, Accepted)
    assert verdict.scores == HONEST


@pytest.mark.parametrize(
    "tables",
    [
        [HONEST, OTHER],
        [HONEST, HONEST, OTHER, OTHER],
        [HONEST, OTHER, FORGED],
        [HONEST, HONEST, OTHER, OTHER, FORGED],
        [HONEST, HONEST, HONEST, OTHER, OTHER, OTHER],
    ],
)
def test_decide_without_strict_majority_is_rejected(tables: list[dict[int, int]]) -> None:
    assert decide([vote(table) for table in tables]) == Rejected("Votes have no majority")


def test_decide_same_scores_but_one_interrupted_do_not_agree() -> None:
    assert decide([vote(HONEST), vote(HONEST, interrupted_at=90)]) == Rejected(
        "Votes have no majority"
    )


def test_decide_interrupted_votes_agree_whatever_the_second() -> None:
    verdict = decide([vote(HONEST, interrupted_at=91), vote(HONEST, interrupted_at=90)])

    assert verdict == Accepted(scores=HONEST, interrupted=True, played_seconds=90, weight=0.5)


def test_decide_interrupted_played_time_is_the_smallest_among_winning_votes() -> None:
    verdict = decide(
        [
            vote(HONEST, interrupted_at=95),
            vote(HONEST, interrupted_at=93),
            vote(OTHER, interrupted_at=40),
        ]
    )

    assert isinstance(verdict, Accepted)
    assert verdict.played_seconds == 93


def test_decide_interrupted_at_thirty_seconds_is_accepted() -> None:
    assert isinstance(decide([vote({0: 0, 1: 0, 2: 0, 3: 0}, interrupted_at=30)]), Accepted)


def test_decide_interrupted_before_thirty_seconds_is_rejected() -> None:
    assert decide([vote({0: 0, 1: 0, 2: 0, 3: 0}, interrupted_at=29)]) == Rejected(
        "Interrupted Match is shorter than 30 s"
    )


def test_decide_score_at_thirty_per_second_is_accepted() -> None:
    assert isinstance(decide([vote({0: 5400, 1: 0, 2: 0, 3: 0})]), Accepted)


def test_decide_score_over_thirty_per_second_rejects_whole_match() -> None:
    assert decide([vote({0: 5401, 1: 0, 2: 0, 3: 0})]) == Rejected(
        "Score in Slot 0 exceeds 30 per second"
    )


def test_decide_score_rate_uses_interrupted_played_time() -> None:
    assert decide([vote({0: 0, 1: 1801, 2: 0, 3: 0}, interrupted_at=60)]) == Rejected(
        "Score in Slot 1 exceeds 30 per second"
    )


def test_placements_order_by_score_descending() -> None:
    assert placements({0: 10, 1: 30, 2: 20, 3: 0}) == {1: 1, 2: 2, 0: 3, 3: 4}


def test_placements_ties_share_the_higher_place_and_skip_the_next() -> None:
    assert placements({0: 50, 1: 40, 2: 40, 3: 10, 4: 40}) == {0: 1, 1: 2, 2: 2, 4: 2, 3: 5}


@pytest.mark.parametrize(
    ("score", "weight", "expected"), [(301, 1.0, 301), (301, 0.5, 150), (0, 0.5, 0)]
)
def test_credited_score_is_score_times_weight_rounded_down(
    score: int, weight: float, expected: int
) -> None:
    assert credited_score(score, weight) == expected
