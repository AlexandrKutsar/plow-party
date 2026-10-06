import uuid
from typing import Any

import pytest
from httpx import AsyncClient, Response

from tests.features.matches.fakes import FakeClock

type Player = dict[str, str]

HONEST = [300, 250, 200, 150]
OTHER = [300, 250, 200, 0]


async def login(client: AsyncClient) -> Player:
    response = await client.post("/accounts/login", json={"device_id": str(uuid.uuid4())})
    return response.json()


def bearer(player: Player) -> dict[str, str]:
    return {"Authorization": f"Bearer {player['token']}"}


def roster(*players: Player | None) -> dict[str, Any]:
    return {
        "roster": [
            {"slot": slot, "account_id": None if player is None else player["account_id"]}
            for slot, player in enumerate(players)
        ]
    }


async def register(client: AsyncClient, host: Player, *others: Player | None) -> str:
    response = await client.post("/matches", json=roster(host, *others), headers=bearer(host))
    assert response.status_code == 201
    return response.json()["match_id"]


async def confirm(client: AsyncClient, match_id: str, player: Player) -> int:
    response = await client.post(f"/matches/{match_id}/confirm", headers=bearer(player))
    return response.status_code


async def vote(
    client: AsyncClient,
    match_id: str,
    player: Player,
    table: list[int],
    interrupted_at: int | None = None,
) -> Response:
    return await client.post(
        f"/matches/{match_id}/votes",
        json={
            "scores": [{"slot": slot, "score": score} for slot, score in enumerate(table)],
            "interrupted_at_seconds": interrupted_at,
        },
        headers=bearer(player),
    )


async def get_match(client: AsyncClient, match_id: str, player: Player) -> dict[str, Any]:
    response = await client.get(f"/matches/{match_id}", headers=bearer(player))
    assert response.status_code == 200
    return response.json()


async def three_player_match(client: AsyncClient) -> tuple[str, Player, Player, Player]:
    host, second, third = await login(client), await login(client), await login(client)
    match_id = await register(client, host, second, third, None)
    assert await confirm(client, match_id, second) == 204
    assert await confirm(client, match_id, third) == 204
    return match_id, host, second, third


def column(match: dict[str, Any], key: str) -> list[Any]:
    return [participant[key] for participant in match["participants"]]


async def test_register_match_returns_open_match_with_host_confirmed(client: AsyncClient) -> None:
    host, guest = await login(client), await login(client)

    response = await client.post(
        "/matches", json=roster(guest, host, None, None), headers=bearer(host)
    )

    assert response.status_code == 201
    match = response.json()
    uuid.UUID(match["match_id"])
    assert match["status"] == "open"
    assert match["registered_at"] == "2026-10-06T12:00:00Z"
    assert match["rejection_reason"] is None
    assert match["interrupted"] is None
    assert match["played_seconds"] is None
    assert match["weight"] is None
    assert column(match, "slot") == [0, 1, 2, 3]
    assert column(match, "account_id") == [guest["account_id"], host["account_id"], None, None]
    assert column(match, "confirmed") == [False, True, False, False]
    assert column(match, "score") == [None] * 4
    assert column(match, "placement") == [None] * 4
    assert column(match, "credited_score") == [None] * 4


async def test_register_match_without_token_is_unauthorized(client: AsyncClient) -> None:
    response = await client.post("/matches", json=roster(None, None, None, None))

    assert response.status_code == 401


async def test_register_match_broken_roster_rule_is_rejected_naming_it(
    client: AsyncClient,
) -> None:
    host = await login(client)

    response = await client.post("/matches", json=roster(host, None, None), headers=bearer(host))

    assert response.status_code == 422
    assert "at least 4" in response.json()["detail"][0]["msg"]


async def test_register_match_host_outside_roster_is_rejected(client: AsyncClient) -> None:
    host, other = await login(client), await login(client)

    response = await client.post(
        "/matches", json=roster(other, None, None, None), headers=bearer(host)
    )

    assert response.status_code == 422
    assert response.json()["detail"] == "The Host must hold a Slot in the Roster"


async def test_register_match_unknown_account_is_rejected(client: AsyncClient) -> None:
    host = await login(client)
    stranger = {"account_id": str(uuid.uuid4())}

    response = await client.post(
        "/matches", json=roster(host, stranger, None, None), headers=bearer(host)
    )

    assert response.status_code == 422
    assert response.json()["detail"] == "Every Account in the Roster must exist"


async def test_confirm_seat_marks_player_confirmed_and_repeats_safely(
    client: AsyncClient,
) -> None:
    host, guest = await login(client), await login(client)
    match_id = await register(client, host, guest, None, None)

    assert await confirm(client, match_id, guest) == 204
    assert await confirm(client, match_id, guest) == 204

    match = await get_match(client, match_id, guest)
    assert column(match, "confirmed") == [True, True, False, False]


async def test_confirm_at_fifteen_seconds_succeeds(client: AsyncClient, clock: FakeClock) -> None:
    host, guest = await login(client), await login(client)
    match_id = await register(client, host, guest, None, None)
    clock.advance(15)

    assert await confirm(client, match_id, guest) == 204


async def test_confirm_after_countdown_is_conflict(client: AsyncClient, clock: FakeClock) -> None:
    host, guest = await login(client), await login(client)
    match_id = await register(client, host, guest, None, None)
    clock.advance(16)

    assert await confirm(client, match_id, guest) == 409


async def test_confirm_outside_roster_is_forbidden(client: AsyncClient) -> None:
    host, stranger = await login(client), await login(client)
    match_id = await register(client, host, None, None, None)

    assert await confirm(client, match_id, stranger) == 403


async def test_confirm_unknown_match_is_not_found(client: AsyncClient) -> None:
    player = await login(client)

    assert await confirm(client, str(uuid.uuid4()), player) == 404


async def test_get_unknown_match_is_not_found(client: AsyncClient) -> None:
    player = await login(client)

    response = await client.get(f"/matches/{uuid.uuid4()}", headers=bearer(player))

    assert response.status_code == 404


async def test_every_confirmed_player_voting_alike_accepts_the_match(
    client: AsyncClient, clock: FakeClock
) -> None:
    match_id, host, second, third = await three_player_match(client)
    clock.advance(185)

    assert (await vote(client, match_id, host, HONEST)).json()["status"] == "open"
    assert (await vote(client, match_id, second, HONEST)).json()["status"] == "open"
    response = await vote(client, match_id, third, HONEST)

    assert response.status_code == 200
    match = response.json()
    assert match["status"] == "accepted"
    assert match["rejection_reason"] is None
    assert match["interrupted"] is False
    assert match["played_seconds"] == 180
    assert match["weight"] == 1.0
    assert column(match, "score") == HONEST
    assert column(match, "placement") == [1, 2, 3, 4]
    assert column(match, "credited_score") == [300, 250, 200, None]


async def test_majority_outvotes_a_forged_table(client: AsyncClient, clock: FakeClock) -> None:
    match_id, host, second, third = await three_player_match(client)
    clock.advance(185)

    await vote(client, match_id, host, [5000, 0, 0, 0])
    await vote(client, match_id, second, HONEST)
    match = (await vote(client, match_id, third, HONEST)).json()

    assert match["status"] == "accepted"
    assert column(match, "score") == HONEST


async def test_split_votes_reject_the_match(client: AsyncClient, clock: FakeClock) -> None:
    host, guest = await login(client), await login(client)
    match_id = await register(client, host, guest, None, None)
    await confirm(client, match_id, guest)
    clock.advance(185)

    await vote(client, match_id, host, HONEST)
    match = (await vote(client, match_id, guest, OTHER)).json()

    assert match["status"] == "rejected"
    assert match["rejection_reason"] == "Votes have no majority"
    assert column(match, "credited_score") == [None] * 4


async def test_solo_match_with_bots_is_accepted_on_its_single_vote(
    client: AsyncClient, clock: FakeClock
) -> None:
    host = await login(client)
    match_id = await register(client, host, None, None, None, None, None)
    clock.advance(183)

    match = (await vote(client, match_id, host, [*HONEST, 100, 90])).json()

    assert match["status"] == "accepted"
    assert column(match, "credited_score") == [300, None, None, None, None, None]


async def test_unconfirmed_seat_counts_as_bot(client: AsyncClient, clock: FakeClock) -> None:
    host, absent = await login(client), await login(client)
    match_id = await register(client, host, absent, None, None)
    clock.advance(185)

    absent_vote = await vote(client, match_id, absent, HONEST)
    match = (await vote(client, match_id, host, HONEST)).json()

    assert absent_vote.status_code == 403
    assert match["status"] == "accepted"
    assert column(match, "credited_score") == [300, None, None, None]


async def test_interrupted_match_is_accepted_at_half_weight(
    client: AsyncClient, clock: FakeClock
) -> None:
    match_id, host, second, third = await three_player_match(client)
    clock.advance(70)
    table = [301, 250, 200, 150]

    await vote(client, match_id, host, table, interrupted_at=65)
    await vote(client, match_id, second, table, interrupted_at=64)
    match = (await vote(client, match_id, third, table, interrupted_at=66)).json()

    assert match["status"] == "accepted"
    assert match["interrupted"] is True
    assert match["played_seconds"] == 64
    assert match["weight"] == 0.5
    assert column(match, "credited_score") == [150, 125, 100, None]


async def test_implausible_score_rejects_the_match(client: AsyncClient, clock: FakeClock) -> None:
    host = await login(client)
    match_id = await register(client, host, None, None, None)
    clock.advance(185)

    match = (await vote(client, match_id, host, [5401, 0, 0, 0])).json()

    assert match["status"] == "rejected"
    assert match["rejection_reason"] == "Score in Slot 0 exceeds 30 per second"


async def test_vote_too_early_is_conflict(client: AsyncClient, clock: FakeClock) -> None:
    host = await login(client)
    match_id = await register(client, host, None, None, None)
    clock.advance(177)

    response = await vote(client, match_id, host, HONEST)

    assert response.status_code == 409
    assert "too early" in response.json()["detail"]


async def test_vote_after_window_is_conflict(client: AsyncClient, clock: FakeClock) -> None:
    host, guest = await login(client), await login(client)
    match_id = await register(client, host, guest, None, None)
    await confirm(client, match_id, guest)
    clock.advance(304)

    response = await vote(client, match_id, host, HONEST)

    assert response.status_code == 409


async def test_same_vote_again_succeeds_and_a_different_one_is_conflict(
    client: AsyncClient, clock: FakeClock
) -> None:
    host, guest = await login(client), await login(client)
    match_id = await register(client, host, guest, None, None)
    await confirm(client, match_id, guest)
    clock.advance(185)
    await vote(client, match_id, host, HONEST)

    same = await vote(client, match_id, host, HONEST)
    different = await vote(client, match_id, host, OTHER)

    assert same.status_code == 200
    assert different.status_code == 409


async def test_retrying_the_deciding_vote_after_the_verdict_succeeds(
    client: AsyncClient, clock: FakeClock
) -> None:
    host = await login(client)
    match_id = await register(client, host, None, None, None)
    clock.advance(185)
    await vote(client, match_id, host, HONEST)

    retry = await vote(client, match_id, host, HONEST)

    assert retry.status_code == 200
    assert retry.json()["status"] == "accepted"


@pytest.mark.parametrize(
    "scores",
    [
        [{"slot": slot, "score": 1} for slot in (0, 1, 2)],
        [{"slot": slot, "score": 1} for slot in (0, 1, 2, 3, 4)],
        [{"slot": slot, "score": 1} for slot in (0, 1, 2, 2)],
        [{"slot": slot, "score": -1} for slot in (0, 1, 2, 3)],
    ],
)
async def test_vote_not_matching_the_roster_is_rejected(
    client: AsyncClient, clock: FakeClock, scores: list[dict[str, int]]
) -> None:
    host = await login(client)
    match_id = await register(client, host, None, None, None)
    clock.advance(185)

    response = await client.post(
        f"/matches/{match_id}/votes", json={"scores": scores}, headers=bearer(host)
    )

    assert response.status_code == 422


async def test_missing_votes_are_decided_on_read_after_window(
    client: AsyncClient, clock: FakeClock
) -> None:
    match_id, host, second, _ = await three_player_match(client)
    clock.advance(185)
    await vote(client, match_id, host, HONEST)
    await vote(client, match_id, second, HONEST)
    assert (await get_match(client, match_id, host))["status"] == "open"
    clock.advance(119)

    match = await get_match(client, match_id, host)

    assert match["status"] == "accepted"
    assert column(match, "credited_score") == [300, 250, 200, None]


async def test_match_without_votes_is_rejected_on_read_after_window(
    client: AsyncClient, clock: FakeClock
) -> None:
    host = await login(client)
    match_id = await register(client, host, None, None, None)
    clock.advance(304)

    match = await get_match(client, match_id, host)

    assert match["status"] == "rejected"
    assert match["rejection_reason"] == "No Match Result was submitted"
