import math
import uuid
from collections import Counter
from collections.abc import Collection, Mapping, Sequence
from dataclasses import dataclass
from datetime import datetime, timedelta
from enum import StrEnum
from typing import NamedTuple

MIN_PARTICIPANTS = 4
MAX_PARTICIPANTS = 6
SLOTS = range(MAX_PARTICIPANTS)
MATCH_SECONDS = 180
INTERRUPTED_SECONDS = range(1, MATCH_SECONDS)
COUNTDOWN = timedelta(seconds=3)
CONFIRMATION_WINDOW = timedelta(seconds=15)
TIMING_TOLERANCE = timedelta(seconds=5)
SUBMISSION_GRACE = timedelta(seconds=120)
MIN_INTERRUPTED_SECONDS = 30
FULL_WEIGHT = 1.0
INTERRUPTED_WEIGHT = 0.5
MAX_SCORE_PER_SECOND = 30
RESULTS_SETTLE = COUNTDOWN + timedelta(seconds=MATCH_SECONDS) + SUBMISSION_GRACE


class MatchStatus(StrEnum):
    OPEN = "open"
    ACCEPTED = "accepted"
    REJECTED = "rejected"


class RosterError(ValueError):
    pass


class VoteError(ValueError):
    pass


class VoteTimingError(ValueError):
    pass


@dataclass(frozen=True)
class RosterSlot:
    slot: int
    account_id: uuid.UUID | None


def check_roster(roster: Sequence[RosterSlot]) -> list[RosterSlot]:
    if len(roster) < MIN_PARTICIPANTS:
        raise RosterError(f"Roster must have at least {MIN_PARTICIPANTS} Slots")
    if len(roster) > MAX_PARTICIPANTS:
        raise RosterError(f"Roster must have at most {MAX_PARTICIPANTS} Slots")
    if any(entry.slot not in SLOTS for entry in roster):
        raise RosterError(f"Slot must be within {SLOTS[0]}-{SLOTS[-1]}")
    if len({entry.slot for entry in roster}) != len(roster):
        raise RosterError("Slots must be distinct")
    accounts = [entry.account_id for entry in roster if entry.account_id is not None]
    if not accounts:
        raise RosterError("Roster must have at least one Player")
    if len(set(accounts)) != len(accounts):
        raise RosterError("An Account may hold only one Slot")
    return sorted(roster, key=lambda entry: entry.slot)


class ResultVersion(NamedTuple):
    scores: tuple[tuple[int, int], ...]
    interrupted: bool


@dataclass(frozen=True)
class Vote:
    scores: Mapping[int, int]
    interrupted_at_seconds: int | None

    @property
    def played_seconds(self) -> int:
        return self.interrupted_at_seconds or MATCH_SECONDS

    @property
    def version(self) -> ResultVersion:
        return ResultVersion(
            scores=tuple(sorted(self.scores.items())),
            interrupted=self.interrupted_at_seconds is not None,
        )


@dataclass(frozen=True)
class Accepted:
    scores: Mapping[int, int]
    interrupted: bool
    played_seconds: int
    weight: float


@dataclass(frozen=True)
class Rejected:
    reason: str


type Verdict = Accepted | Rejected


def confirmation_open(registered_at: datetime, now: datetime) -> bool:
    return now <= registered_at + CONFIRMATION_WINDOW


def submission_deadline(registered_at: datetime) -> datetime:
    return registered_at + RESULTS_SETTLE


def check_vote(
    roster_slots: Collection[int], vote: Vote, registered_at: datetime, now: datetime
) -> None:
    if set(vote.scores) != set(roster_slots):
        raise VoteError("Vote must score exactly the Slots of the Roster")
    if any(score < 0 for score in vote.scores.values()):
        raise VoteError("Scores must be non-negative")
    if (
        vote.interrupted_at_seconds is not None
        and vote.interrupted_at_seconds not in INTERRUPTED_SECONDS
    ):
        raise VoteError(
            f"Interrupted second must be within {INTERRUPTED_SECONDS[0]}-{INTERRUPTED_SECONDS[-1]}"
        )
    played_by = registered_at + COUNTDOWN + timedelta(seconds=vote.played_seconds)
    if confirmation_open(registered_at, now) or now < played_by - TIMING_TOLERANCE:
        raise VoteTimingError("Vote arrived too early for the Match to have been played")
    if now > submission_deadline(registered_at):
        raise VoteTimingError("Submission window is closed")


def verdict_due(confirmed_players: int, votes: int, registered_at: datetime, now: datetime) -> bool:
    return votes >= confirmed_players or now > submission_deadline(registered_at)


def decide(votes: Sequence[Vote]) -> Verdict:
    if not votes:
        return Rejected("No Match Result was submitted")
    version, count = Counter(vote.version for vote in votes).most_common(1)[0]
    if count * 2 <= len(votes):
        return Rejected("Votes have no majority")
    winners = [vote for vote in votes if vote.version == version]
    interrupted = version.interrupted
    played_seconds = min(vote.played_seconds for vote in winners)
    if interrupted and played_seconds < MIN_INTERRUPTED_SECONDS:
        return Rejected(f"Interrupted Match is shorter than {MIN_INTERRUPTED_SECONDS} s")
    scores = winners[0].scores
    for slot, score in sorted(scores.items()):
        if score > MAX_SCORE_PER_SECOND * played_seconds:
            return Rejected(f"Score in Slot {slot} exceeds {MAX_SCORE_PER_SECOND} per second")
    return Accepted(
        scores=scores,
        interrupted=interrupted,
        played_seconds=played_seconds,
        weight=INTERRUPTED_WEIGHT if interrupted else FULL_WEIGHT,
    )


def placements(scores: Mapping[int, int]) -> dict[int, int]:
    return {
        slot: 1 + sum(other > score for other in scores.values()) for slot, score in scores.items()
    }


def credited_score(score: int, weight: float) -> int:
    return math.floor(score * weight)
