import uuid
from typing import Annotated

from pydantic import AfterValidator, BaseModel, Field

from plow_party_api.features.accounts.rules import normalize_nickname


class LoginRequest(BaseModel):
    device_id: uuid.UUID = Field(
        description="UUID the client generated once and keeps in local storage"
    )


class AccountResponse(BaseModel):
    account_id: uuid.UUID = Field(description="Public id of the Account")
    nickname: str = Field(description="Display name shown in the HUD and the Tournament")


class LoginResponse(AccountResponse):
    token: str = Field(
        description="Auth Token for the Authorization: Bearer header; replaces any earlier one"
    )


class RenameRequest(BaseModel):
    nickname: Annotated[str, AfterValidator(normalize_nickname)] = Field(
        description=(
            "New Nickname: trimmed, 3-16 letters, digits, spaces, '_' or '-', no consecutive spaces"
        )
    )
