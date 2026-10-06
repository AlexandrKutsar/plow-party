import uuid
from collections.abc import Generator
from contextlib import contextmanager

from fastapi import APIRouter, HTTPException, status

from plow_party_api.features.accounts.service import UNAUTHORIZED_RESPONSE, CurrentAccountDep
from plow_party_api.features.matches.schemas import (
    MatchResponse,
    ParticipantResponse,
    RegisterMatchRequest,
    VoteRequest,
)
from plow_party_api.features.matches.service import (
    MatchConflictError,
    MatchForbiddenError,
    MatchInvalidError,
    MatchNotFoundError,
    MatchServiceDep,
    MatchView,
)

router = APIRouter(prefix="/matches", tags=["matches"])

ERROR_STATUS: dict[type[Exception], int] = {
    MatchNotFoundError: status.HTTP_404_NOT_FOUND,
    MatchForbiddenError: status.HTTP_403_FORBIDDEN,
    MatchConflictError: status.HTTP_409_CONFLICT,
    MatchInvalidError: status.HTTP_422_UNPROCESSABLE_CONTENT,
}

NOT_FOUND_RESPONSE = {404: {"description": "No Match with this id"}}


@contextmanager
def _http_errors() -> Generator[None]:
    try:
        yield
    except tuple(ERROR_STATUS) as error:
        raise HTTPException(status_code=ERROR_STATUS[type(error)], detail=str(error)) from error


def _response(match: MatchView) -> MatchResponse:
    return MatchResponse(
        match_id=match.id,
        status=match.status,
        rejection_reason=match.rejection_reason,
        registered_at=match.registered_at,
        interrupted=match.interrupted,
        played_seconds=match.played_seconds,
        weight=match.weight,
        participants=[
            ParticipantResponse(
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


@router.post(
    "",
    status_code=status.HTTP_201_CREATED,
    summary="Register a Match with its Roster",
    description=(
        "Called by the Host when Countdown starts. The Host becomes a Confirmed Player; every "
        "other Player must confirm within 15 s. Answers 422 if the Roster breaks a rule, the "
        "Host holds no Slot, or an Account does not exist."
    ),
    response_model=MatchResponse,
    responses=UNAUTHORIZED_RESPONSE,
)
async def register_match(
    request: RegisterMatchRequest, account: CurrentAccountDep, service: MatchServiceDep
) -> MatchResponse:
    with _http_errors():
        return _response(await service.register(account.id, request.to_roster()))


@router.post(
    "/{match_id}/confirm",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Confirm the calling Player's seat in a Match",
    description=(
        "Must arrive within 15 s of registration; a seat left unconfirmed counts as a Bot. "
        "Confirming again succeeds."
    ),
    responses={
        **UNAUTHORIZED_RESPONSE,
        **NOT_FOUND_RESPONSE,
        403: {"description": "The Account holds no Slot in this Match"},
        409: {"description": "Confirmation closed when Countdown ended"},
    },
)
async def confirm_match(
    match_id: uuid.UUID, account: CurrentAccountDep, service: MatchServiceDep
) -> None:
    with _http_errors():
        await service.confirm(match_id, account.id)


@router.post(
    "/{match_id}/votes",
    summary="Submit the calling Player's Match Result as a Vote",
    description=(
        "One Vote per Confirmed Player; resubmitting the same table succeeds, a different one "
        "answers 409. The Verdict is reached when every Confirmed Player has voted, or on the "
        "first read after the submission window closes (303 s after registration). "
        "Votes are refused while confirmation is still open."
    ),
    response_model=MatchResponse,
    responses={
        **UNAUTHORIZED_RESPONSE,
        **NOT_FOUND_RESPONSE,
        403: {"description": "The Account is not a Confirmed Player of this Match"},
        409: {
            "description": (
                "Too early for the Match to have been played, window closed, Verdict already "
                "reached, or the Vote differs from the one already submitted"
            )
        },
    },
)
async def submit_vote(
    match_id: uuid.UUID,
    request: VoteRequest,
    account: CurrentAccountDep,
    service: MatchServiceDep,
) -> MatchResponse:
    with _http_errors():
        return _response(await service.vote(match_id, account.id, request.to_vote()))


@router.get(
    "/{match_id}",
    summary="A Match with its Verdict",
    description="Reaches the Verdict first if the submission window has closed.",
    response_model=MatchResponse,
    responses={**UNAUTHORIZED_RESPONSE, **NOT_FOUND_RESPONSE},
)
async def get_match(
    match_id: uuid.UUID, _: CurrentAccountDep, service: MatchServiceDep
) -> MatchResponse:
    with _http_errors():
        return _response(await service.get(match_id))
