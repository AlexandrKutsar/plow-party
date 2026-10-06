import uuid
from datetime import date
from typing import Annotated

from fastapi import APIRouter, HTTPException, Query, status

from plow_party_api.features.accounts.service import UNAUTHORIZED_RESPONSE, CurrentAccountDep
from plow_party_api.features.tournament.rules import (
    DEFAULT_RADIUS,
    DEFAULT_TOP,
    MAX_MEDAL_ACCOUNTS,
    MAX_RADIUS,
    MAX_TOP,
)
from plow_party_api.features.tournament.schemas import (
    LeaderboardResponse,
    MedalResponse,
    MedalsResponse,
    StandingResponse,
)
from plow_party_api.features.tournament.service import (
    LeaderboardView,
    StandingView,
    TournamentDayError,
    TournamentServiceDep,
)

router = APIRouter(prefix="/tournament", tags=["tournament"])

DayQuery = Annotated[
    date | None,
    Query(description="Tournament Day (UTC date) to read; today when omitted, never in the future"),
]

DAY_RESPONSES = {
    **UNAUTHORIZED_RESPONSE,
    422: {"description": "Future day or parameter out of range"},
}


def _standing(standing: StandingView) -> StandingResponse:
    return StandingResponse(
        rank=standing.rank,
        account_id=standing.account_id,
        nickname=standing.nickname,
        score=standing.score,
    )


def _leaderboard(board: LeaderboardView) -> LeaderboardResponse:
    return LeaderboardResponse(
        day=board.day,
        ends_at=board.ends_at,
        final=board.final,
        players=board.players,
        me=_standing(board.me) if board.me else None,
        entries=[_standing(entry) for entry in board.entries],
    )


def _day_error(error: TournamentDayError) -> HTTPException:
    return HTTPException(status_code=status.HTTP_422_UNPROCESSABLE_CONTENT, detail=str(error))


@router.get(
    "/leaderboard",
    summary="Top of a day's Leaderboard",
    description=(
        "Ranks every Account's best Credited Score of the Tournament Day; ties go to whoever "
        "reached the score first. A Match counts for the UTC day it was registered on. "
        "Reaches the Verdict of overdue Matches first."
    ),
    response_model=LeaderboardResponse,
    responses=DAY_RESPONSES,
)
async def get_leaderboard(
    account: CurrentAccountDep,
    service: TournamentServiceDep,
    day: DayQuery = None,
    limit: Annotated[
        int, Query(ge=1, le=MAX_TOP, description="How many top Standings to return")
    ] = DEFAULT_TOP,
) -> LeaderboardResponse:
    try:
        return _leaderboard(await service.leaderboard(account.id, day, limit))
    except TournamentDayError as error:
        raise _day_error(error) from error


@router.get(
    "/leaderboard/me",
    summary="The calling Account's Standing with its neighbours",
    description=(
        "Standings within `radius` Ranks above and below the caller; empty with `me` null "
        "when the caller has no Daily Best that day."
    ),
    response_model=LeaderboardResponse,
    responses=DAY_RESPONSES,
)
async def get_leaderboard_around_me(
    account: CurrentAccountDep,
    service: TournamentServiceDep,
    day: DayQuery = None,
    radius: Annotated[
        int, Query(ge=0, le=MAX_RADIUS, description="Ranks to show above and below the caller")
    ] = DEFAULT_RADIUS,
) -> LeaderboardResponse:
    try:
        return _leaderboard(await service.around_me(account.id, day, radius))
    except TournamentDayError as error:
        raise _day_error(error) from error


@router.get(
    "/medals",
    summary="Current Medals of a set of Accounts",
    description=(
        "Medals come from the Medal Day, the latest Tournament Day whose results are final: "
        "gold, silver and bronze for Rank 1-3 with a Daily Best above 0. Pass the Roster's "
        "Accounts to frame HUD portraits; unknown Accounts get no Medal."
    ),
    response_model=MedalsResponse,
    responses={
        **UNAUTHORIZED_RESPONSE,
        422: {"description": f"No Account or more than {MAX_MEDAL_ACCOUNTS}"},
    },
)
async def get_medals(
    _: CurrentAccountDep,
    service: TournamentServiceDep,
    account_id: Annotated[
        list[uuid.UUID],
        Query(min_length=1, max_length=MAX_MEDAL_ACCOUNTS, description="Accounts, repeated"),
    ],
) -> MedalsResponse:
    result = await service.medals(account_id)
    return MedalsResponse(
        day=result.day,
        medals=[
            MedalResponse(account_id=entry.account_id, medal=entry.medal) for entry in result.medals
        ],
    )
