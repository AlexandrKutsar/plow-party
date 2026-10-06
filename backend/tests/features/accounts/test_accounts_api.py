import asyncio
import re
import uuid

from httpx import AsyncClient


async def test_login_new_device_creates_account_with_default_nickname(client: AsyncClient) -> None:
    response = await client.post("/accounts/login", json={"device_id": str(uuid.uuid4())})

    assert response.status_code == 200
    body = response.json()
    assert set(body) == {"account_id", "nickname", "token"}
    uuid.UUID(body["account_id"])
    assert re.fullmatch(r"Plower\d{4}", body["nickname"])
    assert len(body["token"]) >= 32


async def test_login_known_device_returns_same_account(client: AsyncClient) -> None:
    device_id = str(uuid.uuid4())

    first = (await client.post("/accounts/login", json={"device_id": device_id})).json()
    second = (await client.post("/accounts/login", json={"device_id": device_id})).json()

    assert second["account_id"] == first["account_id"]
    assert second["nickname"] == first["nickname"]


async def test_login_device_id_in_upper_case_reaches_same_account(client: AsyncClient) -> None:
    device_id = str(uuid.uuid4())

    lower = (await client.post("/accounts/login", json={"device_id": device_id})).json()
    upper = (await client.post("/accounts/login", json={"device_id": device_id.upper()})).json()

    assert upper["account_id"] == lower["account_id"]


async def test_login_malformed_device_id_is_rejected(client: AsyncClient) -> None:
    response = await client.post("/accounts/login", json={"device_id": "not-a-uuid"})

    assert response.status_code == 422


async def login(client: AsyncClient, device_id: str | None = None) -> dict[str, str]:
    response = await client.post(
        "/accounts/login", json={"device_id": device_id or str(uuid.uuid4())}
    )
    return response.json()


def bearer(token: str) -> dict[str, str]:
    return {"Authorization": f"Bearer {token}"}


async def test_get_me_with_token_returns_account(client: AsyncClient) -> None:
    account = await login(client)

    response = await client.get("/accounts/me", headers=bearer(account["token"]))

    assert response.status_code == 200
    assert response.json() == {"account_id": account["account_id"], "nickname": account["nickname"]}


async def test_get_me_without_token_is_unauthorized(client: AsyncClient) -> None:
    response = await client.get("/accounts/me")

    assert response.status_code == 401
    assert response.headers["WWW-Authenticate"] == "Bearer"


async def test_get_me_with_unknown_token_is_unauthorized(client: AsyncClient) -> None:
    response = await client.get("/accounts/me", headers=bearer("not-a-real-token"))

    assert response.status_code == 401


async def test_login_again_invalidates_previous_token(client: AsyncClient) -> None:
    device_id = str(uuid.uuid4())
    old = await login(client, device_id)
    new = await login(client, device_id)

    old_response = await client.get("/accounts/me", headers=bearer(old["token"]))
    new_response = await client.get("/accounts/me", headers=bearer(new["token"]))

    assert new["token"] != old["token"]
    assert old_response.status_code == 401
    assert new_response.status_code == 200


async def test_rename_me_valid_nickname_is_trimmed_and_visible_on_me(client: AsyncClient) -> None:
    account = await login(client)

    response = await client.patch(
        "/accounts/me", json={"nickname": "  Снежок_7 "}, headers=bearer(account["token"])
    )
    me = await client.get("/accounts/me", headers=bearer(account["token"]))

    assert response.status_code == 200
    assert response.json() == {"account_id": account["account_id"], "nickname": "Снежок_7"}
    assert me.json()["nickname"] == "Снежок_7"


async def test_rename_me_same_nickname_succeeds(client: AsyncClient) -> None:
    account = await login(client)

    response = await client.patch(
        "/accounts/me", json={"nickname": account["nickname"]}, headers=bearer(account["token"])
    )

    assert response.status_code == 200


async def test_rename_me_nickname_taken_by_another_account_succeeds(client: AsyncClient) -> None:
    first = await login(client)
    second = await login(client)
    await client.patch("/accounts/me", json={"nickname": "Twin"}, headers=bearer(first["token"]))

    response = await client.patch(
        "/accounts/me", json={"nickname": "Twin"}, headers=bearer(second["token"])
    )

    assert response.status_code == 200


async def test_rename_me_broken_rule_is_rejected_with_reason(client: AsyncClient) -> None:
    account = await login(client)

    response = await client.patch(
        "/accounts/me", json={"nickname": "Ice  Queen"}, headers=bearer(account["token"])
    )

    assert response.status_code == 422
    assert "consecutive spaces" in response.json()["detail"][0]["msg"]


async def test_rename_me_without_token_is_unauthorized(client: AsyncClient) -> None:
    response = await client.patch("/accounts/me", json={"nickname": "Snowy"})

    assert response.status_code == 401


async def test_login_concurrent_first_logins_share_one_account(client: AsyncClient) -> None:
    device_id = str(uuid.uuid4())

    accounts = await asyncio.gather(*(login(client, device_id) for _ in range(5)))

    assert len({account["account_id"] for account in accounts}) == 1
