import uuid
from collections.abc import Collection

from sqlalchemy import select
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.ext.asyncio import AsyncSession

from plow_party_api.features.accounts.models import Account


class AccountRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def upsert_by_device(
        self, device_id: uuid.UUID, new_nickname: str, token_hash: str
    ) -> Account:
        statement = (
            insert(Account)
            .values(device_id=device_id, nickname=new_nickname, token_hash=token_hash)
            .on_conflict_do_update(
                index_elements=[Account.device_id],
                set_={Account.token_hash: token_hash},
            )
            .returning(Account)
            .execution_options(populate_existing=True)
        )
        return (await self._session.scalars(statement)).one()

    async def find_by_token_hash(self, token_hash: str) -> Account | None:
        return await self._session.scalar(select(Account).where(Account.token_hash == token_hash))

    async def get(self, account_id: uuid.UUID) -> Account:
        return await self._session.get_one(Account, account_id)

    async def existing_ids(self, account_ids: Collection[uuid.UUID]) -> set[uuid.UUID]:
        statement = select(Account.id).where(Account.id.in_(account_ids))
        return set(await self._session.scalars(statement))
