import uuid
from datetime import datetime, timedelta
from typing import Any

from httpx import AsyncClient

from tests.features.matches.fakes import FakeClock

type Player = dict[str, str]

PLAYED_SECONDS = 180
SETTLE_SECONDS = 303
BOT_SCORES = [40, 30, 20]


async def login(client: AsyncClient) -> Player:
    response = await client.post("/accounts/login", json={"device_id": str(uuid.uuid4())})
    return response.json()


def bearer(player: Player) -> dict[str, str]:
    return {"Authorization": f"Bearer {player['token']}"}


async def register(client: AsyncClient, host: Player, *others: Player | None) -> str:
    seats = [host, *others]
    roster = [
        {"slot": slot, "account_id": None if seat is None else seat["account_id"]}
        for slot, seat in enumerate(seats)
    ]
    response = await client.post("/matches", json={"roster": roster}, headers=bearer(host))
    assert response.status_code == 201
    return response.json()["match_id"]


async def vote(
    client: AsyncClient,
    match_id: str,
    player: Player,
    table: list[int],
    interrupted_at: int | None = None,
) -> dict[str, Any]:
    response = await client.post(
        f"/matches/{match_id}/votes",
        json={
            "scores": [{"slot": slot, "score": score} for slot, score in enumerate(table)],
            "interrupted_at_seconds": interrupted_at,
        },
        headers=bearer(player),
    )
    assert response.status_code == 200
    return response.json()


async def play_solo(
    client: AsyncClient,
    clock: FakeClock,
    player: Player,
    score: int,
    interrupted_at: int | None = None,
) -> dict[str, Any]:
    match_id = await register(client, player, None, None, None)
    clock.advance(PLAYED_SECONDS)
    return await vote(client, match_id, player, [score, *BOT_SCORES], interrupted_at)


async def leaderboard(client: AsyncClient, player: Player, **params: Any) -> dict[str, Any]:
    response = await client.get("/tournament/leaderboard", params=params, headers=bearer(player))
    assert response.status_code == 200
    return response.json()


async def around_me(client: AsyncClient, player: Player, **params: Any) -> dict[str, Any]:
    response = await client.get("/tournament/leaderboard/me", params=params, headers=bearer(player))
    assert response.status_code == 200
    return response.json()


async def medals(client: AsyncClient, player: Player, *account_ids: str) -> dict[str, Any]:
    response = await client.get(
        "/tournament/medals", params={"account_id": list(account_ids)}, headers=bearer(player)
    )
    assert response.status_code == 200
    return response.json()


def ranked(board: dict[str, Any]) -> list[tuple[int, str, int]]:
    return [(entry["rank"], entry["account_id"], entry["score"]) for entry in board["entries"]]


def next_midnight(clock: FakeClock) -> datetime:
    return datetime.combine(
        clock.now.date() + timedelta(days=1), datetime.min.time(), clock.now.tzinfo
    )


async def test_leaderboard_ranks_each_players_best_credited_score_of_the_day(
    client: AsyncClient, clock: FakeClock
) -> None:
    alice, bob = await login(client), await login(client)
    await play_solo(client, clock, alice, 100)
    await play_solo(client, clock, bob, 200)
    await play_solo(client, clock, alice, 300)
    await play_solo(client, clock, alice, 150)

    board = await leaderboard(client, alice)

    assert ranked(board) == [(1, alice["account_id"], 300), (2, bob["account_id"], 200)]
    assert [entry["nickname"] for entry in board["entries"]] == [
        alice["nickname"],
        bob["nickname"],
    ]
    assert board["players"] == 2
    assert board["day"] == clock.now.date().isoformat()
    assert board["final"] is False


async def test_leaderboard_ends_at_next_midnight_utc(client: AsyncClient, clock: FakeClock) -> None:
    player = await login(client)

    board = await leaderboard(client, player)

    assert datetime.fromisoformat(board["ends_at"]) == next_midnight(clock)


async def test_leaderboard_never_ranks_bots_or_unconfirmed_seats(
    client: AsyncClient, clock: FakeClock
) -> None:
    host, absent = await login(client), await login(client)
    match_id = await register(client, host, absent, None, None)
    clock.advance(PLAYED_SECONDS)
    await vote(client, match_id, host, [100, 900, 800, 700])

    board = await leaderboard(client, host)

    assert ranked(board) == [(1, host["account_id"], 100)]


async def test_leaderboard_ignores_rejected_matches(client: AsyncClient, clock: FakeClock) -> None:
    cheater = await login(client)
    match = await play_solo(client, clock, cheater, 999_999)
    assert match["status"] == "rejected"

    board = await leaderboard(client, cheater)

    assert board["entries"] == []
    assert board["players"] == 0


async def test_leaderboard_ranks_an_interrupted_match_at_its_credited_score(
    client: AsyncClient, clock: FakeClock
) -> None:
    player = await login(client)
    await play_solo(client, clock, player, 301, interrupted_at=120)

    board = await leaderboard(client, player)

    assert ranked(board) == [(1, player["account_id"], 150)]


async def test_leaderboard_breaks_a_tie_by_who_reached_the_score_first(
    client: AsyncClient, clock: FakeClock
) -> None:
    early, late = await login(client), await login(client)
    await play_solo(client, clock, early, 200)
    await play_solo(client, clock, late, 200)

    board = await leaderboard(client, late)

    assert ranked(board) == [(1, early["account_id"], 200), (2, late["account_id"], 200)]


async def test_leaderboard_limit_takes_the_top_entries(
    client: AsyncClient, clock: FakeClock
) -> None:
    players = [await login(client) for _ in range(3)]
    for score, player in zip([100, 300, 200], players, strict=True):
        await play_solo(client, clock, player, score)

    board = await leaderboard(client, players[0], limit=2)

    assert [entry["score"] for entry in board["entries"]] == [300, 200]
    assert board["players"] == 3


async def test_leaderboard_shows_the_current_nickname(
    client: AsyncClient, clock: FakeClock
) -> None:
    player = await login(client)
    await play_solo(client, clock, player, 100)
    await client.patch("/accounts/me", json={"nickname": "Снежок"}, headers=bearer(player))

    board = await leaderboard(client, player)

    assert board["entries"][0]["nickname"] == "Снежок"


async def test_leaderboard_resets_at_midnight_utc(client: AsyncClient, clock: FakeClock) -> None:
    player = await login(client)
    await play_solo(client, clock, player, 100)
    played_day = clock.now.date().isoformat()
    clock.now = next_midnight(clock)

    today = await leaderboard(client, player)
    yesterday = await leaderboard(client, player, day=played_day)

    assert today["entries"] == []
    assert ranked(yesterday) == [(1, player["account_id"], 100)]


async def test_match_settling_before_midnight_counts_for_that_day(
    client: AsyncClient, clock: FakeClock
) -> None:
    player = await login(client)
    clock.now = next_midnight(clock) - timedelta(seconds=SETTLE_SECONDS + 1)
    registered_day = clock.now.date().isoformat()
    await play_solo(client, clock, player, 100)
    clock.now = next_midnight(clock)

    registered = await leaderboard(client, player, day=registered_day)
    following = await leaderboard(client, player)

    assert ranked(registered) == [(1, player["account_id"], 100)]
    assert following["entries"] == []


async def test_match_settling_at_midnight_counts_for_the_next_day(
    client: AsyncClient, clock: FakeClock
) -> None:
    player = await login(client)
    clock.now = next_midnight(clock) - timedelta(seconds=SETTLE_SECONDS)
    registered_day = clock.now.date().isoformat()
    await play_solo(client, clock, player, 100)
    clock.now = next_midnight(clock)

    registered = await leaderboard(client, player, day=registered_day)
    following = await leaderboard(client, player)

    assert registered["entries"] == []
    assert ranked(following) == [(1, player["account_id"], 100)]


async def test_leaderboard_read_reaches_the_verdict_of_an_overdue_match(
    client: AsyncClient, clock: FakeClock
) -> None:
    host, silent = await login(client), await login(client)
    match_id = await register(client, host, silent, None, None)
    confirmed = await client.post(f"/matches/{match_id}/confirm", headers=bearer(silent))
    assert confirmed.status_code == 204
    clock.advance(PLAYED_SECONDS)
    match = await vote(client, match_id, host, [120, 80, 10, 10])
    assert match["status"] == "open"
    clock.advance(SETTLE_SECONDS)

    board = await leaderboard(client, host)

    assert ranked(board) == [(1, host["account_id"], 120), (2, silent["account_id"], 80)]


async def test_past_day_is_final_from_midnight(client: AsyncClient, clock: FakeClock) -> None:
    player = await login(client)
    played_day = clock.now.date().isoformat()
    clock.now = next_midnight(clock) - timedelta(microseconds=1)

    settling = await leaderboard(client, player)
    clock.now = next_midnight(clock)
    settled = await leaderboard(client, player, day=played_day)

    assert settling["final"] is False
    assert settled["final"] is True


async def test_leaderboard_of_a_future_day_is_rejected(
    client: AsyncClient, clock: FakeClock
) -> None:
    player = await login(client)
    tomorrow = (clock.now.date() + timedelta(days=1)).isoformat()

    response = await client.get(
        "/tournament/leaderboard", params={"day": tomorrow}, headers=bearer(player)
    )

    assert response.status_code == 422
    assert response.json()["detail"] == "Tournament Day must not be in the future"


async def test_leaderboard_limit_out_of_range_is_rejected(client: AsyncClient) -> None:
    player = await login(client)

    for limit in (0, 101):
        response = await client.get(
            "/tournament/leaderboard", params={"limit": limit}, headers=bearer(player)
        )
        assert response.status_code == 422


async def test_around_me_shows_my_standing_with_neighbours(
    client: AsyncClient, clock: FakeClock
) -> None:
    players = [await login(client) for _ in range(5)]
    for score, player in zip([500, 400, 300, 200, 100], players, strict=True):
        await play_solo(client, clock, player, score)

    board = await around_me(client, players[2], radius=1)

    assert board["me"] == {
        "rank": 3,
        "account_id": players[2]["account_id"],
        "nickname": players[2]["nickname"],
        "score": 300,
    }
    assert [entry["rank"] for entry in board["entries"]] == [2, 3, 4]
    assert board["players"] == 5


async def test_around_me_without_a_daily_best_is_empty(
    client: AsyncClient, clock: FakeClock
) -> None:
    ranked_player, newcomer = await login(client), await login(client)
    await play_solo(client, clock, ranked_player, 100)

    board = await around_me(client, newcomer)

    assert board["me"] is None
    assert board["entries"] == []
    assert board["players"] == 1


async def test_around_me_radius_out_of_range_is_rejected(client: AsyncClient) -> None:
    player = await login(client)

    response = await client.get(
        "/tournament/leaderboard/me", params={"radius": 26}, headers=bearer(player)
    )

    assert response.status_code == 422


async def test_medals_go_to_yesterdays_top_three_from_midnight(
    client: AsyncClient, clock: FakeClock
) -> None:
    players = [await login(client) for _ in range(4)]
    for score, player in zip([100, 400, 300, 200], players, strict=True):
        await play_solo(client, clock, player, score)
    played_day = clock.now.date()
    ids = [player["account_id"] for player in players]
    clock.now = next_midnight(clock)

    result = await medals(client, players[0], *ids)

    assert result["day"] == played_day.isoformat()
    assert result["medals"] == [
        {"account_id": ids[0], "medal": None},
        {"account_id": ids[1], "medal": "gold"},
        {"account_id": ids[2], "medal": "silver"},
        {"account_id": ids[3], "medal": "bronze"},
    ]


async def test_medals_wait_until_midnight(client: AsyncClient, clock: FakeClock) -> None:
    player = await login(client)
    await play_solo(client, clock, player, 400)
    played_day = clock.now.date()
    clock.now = next_midnight(clock) - timedelta(microseconds=1)

    result = await medals(client, player, player["account_id"])

    assert result["day"] == (played_day - timedelta(days=1)).isoformat()
    assert result["medals"] == [{"account_id": player["account_id"], "medal": None}]


async def test_medals_list_each_requested_account_once_in_request_order_without_medal_for_unknown(
    client: AsyncClient,
) -> None:
    player = await login(client)
    stranger = str(uuid.uuid4())

    result = await medals(client, player, stranger, player["account_id"], stranger)

    assert result["medals"] == [
        {"account_id": stranger, "medal": None},
        {"account_id": player["account_id"], "medal": None},
    ]


async def test_medals_for_too_many_accounts_are_rejected(client: AsyncClient) -> None:
    player = await login(client)
    ids = [str(uuid.uuid4()) for _ in range(21)]

    response = await client.get(
        "/tournament/medals", params={"account_id": ids}, headers=bearer(player)
    )

    assert response.status_code == 422


async def test_tournament_without_token_is_unauthorized(client: AsyncClient) -> None:
    for path in ("/tournament/leaderboard", "/tournament/leaderboard/me", "/tournament/medals"):
        response = await client.get(path, params={"account_id": str(uuid.uuid4())})
        assert response.status_code == 401
