import hashlib
import secrets
import uuid
from collections.abc import Collection
from dataclasses import dataclass
from typing import Annotated, Any

from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from plow_party_api.core.dependencies import SessionDep
from plow_party_api.features.accounts.models import Account
from plow_party_api.features.accounts.repository import AccountRepository
from plow_party_api.features.accounts.rules import DEFAULT_NICKNAME_NUMBERS, default_nickname


@dataclass(frozen=True)
class CurrentAccount:
    id: uuid.UUID
    nickname: str


@dataclass(frozen=True)
class LoginResult:
    account: CurrentAccount
    token: str


def _hash_token(token: str) -> str:
    return hashlib.sha256(token.encode()).hexdigest()


def _current(account: Account) -> CurrentAccount:
    return CurrentAccount(id=account.id, nickname=account.nickname)


class AccountService:
    def __init__(self, session: SessionDep) -> None:
        self._session = session
        self._accounts = AccountRepository(session)

    async def login(self, device_id: uuid.UUID) -> LoginResult:
        token = secrets.token_urlsafe(32)
        nickname = default_nickname(secrets.randbelow(len(DEFAULT_NICKNAME_NUMBERS)))
        account = await self._accounts.upsert_by_device(device_id, nickname, _hash_token(token))
        await self._session.commit()
        return LoginResult(account=_current(account), token=token)

    async def authenticate(self, token: str) -> CurrentAccount | None:
        account = await self._accounts.find_by_token_hash(_hash_token(token))
        return None if account is None else _current(account)

    async def rename(self, account_id: uuid.UUID, nickname: str) -> CurrentAccount:
        account = await self._accounts.get(account_id)
        account.nickname = nickname
        await self._session.commit()
        return _current(account)

    async def existing_ids(self, account_ids: Collection[uuid.UUID]) -> set[uuid.UUID]:
        return await self._accounts.existing_ids(account_ids)

    async def nicknames(self, account_ids: Collection[uuid.UUID]) -> dict[uuid.UUID, str]:
        return await self._accounts.nicknames(account_ids)


AccountServiceDep = Annotated[AccountService, Depends()]

_bearer = HTTPBearer(auto_error=False)

UNAUTHORIZED_RESPONSE: dict[int | str, dict[str, Any]] = {
    401: {"description": "Missing, malformed, or superseded Auth Token"}
}


async def get_current_account(
    service: AccountServiceDep,
    credentials: Annotated[HTTPAuthorizationCredentials | None, Depends(_bearer)],
) -> CurrentAccount:
    account = None if credentials is None else await service.authenticate(credentials.credentials)
    if account is None:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Missing or invalid auth token",
            headers={"WWW-Authenticate": "Bearer"},
        )
    return account


CurrentAccountDep = Annotated[CurrentAccount, Depends(get_current_account)]
