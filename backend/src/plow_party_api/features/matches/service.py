import uuid
from collections.abc import Sequence
from dataclasses import dataclass
from datetime import datetime
from typing import Annotated

from fastapi import Depends

from plow_party_api.core.clock import ClockDep
from plow_party_api.core.dependencies import SessionDep
from plow_party_api.features.accounts.service import AccountService
from plow_party_api.features.matches.models import Match, MatchParticipant, MatchVote
from plow_party_api.features.matches.repository import MatchRepository
from plow_party_api.features.matches.rules import (
    Accepted,
    MatchStatus,
    Seat,
    Vote,
    VoteError,
    VoteTimingError,
    check_vote,
    confirmation_open,
    credited_score,
    decide,
    finalization_due,
    placements,
)


class MatchNotFoundError(Exception):
    pass


class MatchForbiddenError(Exception):
    pass


class MatchConflictError(Exception):
    pass


class MatchInvalidError(Exception):
    pass


@dataclass(frozen=True)
class ParticipantView:
    slot: int
    account_id: uuid.UUID | None
    confirmed: bool
    score: int | None
    placement: int | None
    credited_score: int | None


@dataclass(frozen=True)
class MatchView:
    id: uuid.UUID
    status: MatchStatus
    rejection_reason: str | None
    registered_at: datetime
    interrupted: bool | None
    played_seconds: int | None
    weight: float | None
    participants: list[ParticipantView]


def _view(match: Match) -> MatchView:
    return MatchView(
        id=match.id,
        status=MatchStatus(match.status),
        rejection_reason=match.rejection_reason,
        registered_at=match.registered_at,
        interrupted=match.interrupted,
        played_seconds=match.played_seconds,
        weight=match.weight,
        participants=[
            ParticipantView(
                slot=participant.slot,
                account_id=participant.account_id,
                confirmed=participant.confirmed,
                score=participant.score,
                placement=participant.placement,
                credited_score=participant.credited_score,
            )
            for participant in match.participants
        ],
    )


def _as_vote(stored: MatchVote) -> Vote:
    return Vote(
        scores={int(slot): score for slot, score in stored.scores.items()},
        interrupted_at_seconds=stored.interrupted_at_seconds,
    )


def _is_confirmed_player(participant: MatchParticipant) -> bool:
    return participant.account_id is not None and participant.confirmed


class MatchService:
    def __init__(self, session: SessionDep, clock: ClockDep) -> None:
        self._session = session
        self._clock = clock
        self._matches = MatchRepository(session)
        self._accounts = AccountService(session)

    async def register(self, host_account_id: uuid.UUID, roster: Sequence[Seat]) -> MatchView:
        account_ids = {seat.account_id for seat in roster if seat.account_id is not None}
        if host_account_id not in account_ids:
            raise MatchInvalidError("The Host must hold a Slot in the Roster")
        if await self._accounts.existing_ids(account_ids) != account_ids:
            raise MatchInvalidError("Every Account in the Roster must exist")
        match = Match(
            host_account_id=host_account_id,
            registered_at=self._clock(),
            status=MatchStatus.OPEN,
            participants=[
                MatchParticipant(
                    slot=seat.slot,
                    account_id=seat.account_id,
                    confirmed=seat.account_id == host_account_id,
                )
                for seat in roster
            ],
            votes=[],
        )
        self._matches.add(match)
        await self._session.commit()
        return _view(match)

    async def confirm(self, match_id: uuid.UUID, account_id: uuid.UUID) -> None:
        match = await self._locked(match_id)
        participant = next((p for p in match.participants if p.account_id == account_id), None)
        if participant is None:
            raise MatchForbiddenError("The Account holds no Slot in this Match")
        if participant.confirmed:
            return
        if not confirmation_open(match.registered_at, self._clock()):
            raise MatchConflictError("Confirmation closed when Countdown ended")
        participant.confirmed = True
        await self._session.commit()

    async def vote(self, match_id: uuid.UUID, account_id: uuid.UUID, vote: Vote) -> MatchView:
        match = await self._locked(match_id)
        if not any(
            _is_confirmed_player(p) and p.account_id == account_id for p in match.participants
        ):
            raise MatchForbiddenError("Only a Confirmed Player of this Match may vote")
        previous = next((v for v in match.votes if v.account_id == account_id), None)
        if previous is not None:
            if _as_vote(previous) != vote:
                raise MatchConflictError("A Vote cannot be changed")
            return _view(match)
        if match.status != MatchStatus.OPEN:
            raise MatchConflictError("The Match already has a Verdict")
        now = self._clock()
        try:
            check_vote([p.slot for p in match.participants], vote, match.registered_at, now)
        except VoteError as error:
            raise MatchInvalidError(str(error)) from error
        except VoteTimingError as error:
            raise MatchConflictError(str(error)) from error
        match.votes.append(
            MatchVote(
                account_id=account_id,
                submitted_at=now,
                scores={str(slot): score for slot, score in vote.scores.items()},
                interrupted_at_seconds=vote.interrupted_at_seconds,
            )
        )
        self._finalize_if_due(match, now)
        await self._session.commit()
        return _view(match)

    async def get(self, match_id: uuid.UUID) -> MatchView:
        match = await self._matches.find(match_id)
        if match is None:
            raise MatchNotFoundError("Match not found")
        if self._due(match, self._clock()):
            match = await self._locked(match_id)
            self._finalize_if_due(match, self._clock())
            await self._session.commit()
        return _view(match)

    async def _locked(self, match_id: uuid.UUID) -> Match:
        match = await self._matches.find(match_id, for_update=True)
        if match is None:
            raise MatchNotFoundError("Match not found")
        return match

    def _due(self, match: Match, now: datetime) -> bool:
        return match.status == MatchStatus.OPEN and finalization_due(
            confirmed_players=sum(_is_confirmed_player(p) for p in match.participants),
            votes=len(match.votes),
            registered_at=match.registered_at,
            now=now,
        )

    def _finalize_if_due(self, match: Match, now: datetime) -> None:
        if not self._due(match, now):
            return
        verdict = decide([_as_vote(stored) for stored in match.votes])
        match.decided_at = now
        if not isinstance(verdict, Accepted):
            match.status = MatchStatus.REJECTED
            match.rejection_reason = verdict.reason
            return
        match.status = MatchStatus.ACCEPTED
        match.interrupted = verdict.interrupted
        match.played_seconds = verdict.played_seconds
        match.weight = verdict.weight
        places = placements(verdict.scores)
        for participant in match.participants:
            score = verdict.scores[participant.slot]
            participant.score = score
            participant.placement = places[participant.slot]
            if _is_confirmed_player(participant):
                participant.credited_score = credited_score(score, verdict.weight)


MatchServiceDep = Annotated[MatchService, Depends()]
