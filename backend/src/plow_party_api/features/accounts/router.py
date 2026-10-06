from fastapi import APIRouter

from plow_party_api.features.accounts.schemas import (
    AccountResponse,
    LoginRequest,
    LoginResponse,
    RenameRequest,
)
from plow_party_api.features.accounts.service import (
    UNAUTHORIZED_RESPONSE,
    AccountServiceDep,
    CurrentAccountDep,
)

router = APIRouter(prefix="/accounts", tags=["accounts"])


@router.post(
    "/login",
    summary="Log in as a guest by Device Id",
    description=(
        "Creates the Account on the first login from a Device Id and returns it with a new "
        "Auth Token. Every call issues a new token and invalidates the previous one."
    ),
    response_model=LoginResponse,
)
async def login(request: LoginRequest, service: AccountServiceDep) -> LoginResponse:
    result = await service.login(request.device_id)
    return LoginResponse(
        account_id=result.account.id, nickname=result.account.nickname, token=result.token
    )


@router.get(
    "/me",
    summary="The calling Account",
    response_model=AccountResponse,
    responses=UNAUTHORIZED_RESPONSE,
)
async def get_me(account: CurrentAccountDep) -> AccountResponse:
    return AccountResponse(account_id=account.id, nickname=account.nickname)


@router.patch(
    "/me",
    summary="Change the calling Account's Nickname",
    description="Nicknames are not unique; a rejected Nickname answers 422 naming the broken rule.",
    response_model=AccountResponse,
    responses=UNAUTHORIZED_RESPONSE,
)
async def rename_me(
    request: RenameRequest, account: CurrentAccountDep, service: AccountServiceDep
) -> AccountResponse:
    renamed = await service.rename(account.id, request.nickname)
    return AccountResponse(account_id=renamed.id, nickname=renamed.nickname)
