import uuid
from datetime import date, datetime

from pydantic import BaseModel, Field

from plow_party_api.features.tournament.rules import Medal


class StandingResponse(BaseModel):
    rank: int = Field(description="Place on the Leaderboard, unique; 1 is the leader")
    account_id: uuid.UUID = Field(description="Account holding the Standing")
    nickname: str = Field(description="Current Nickname of the Account")
    score: int = Field(description="Daily Best: the Account's best Credited Score of the day")


class LeaderboardResponse(BaseModel):
    day: date = Field(description="Tournament Day (UTC date) the Leaderboard ranks")
    ends_at: datetime = Field(description="Midnight UTC that ends the Tournament Day")
    final: bool = Field(
        description="Whether the day's results can no longer change (from its ending midnight)"
    )
    players: int = Field(description="Number of Accounts ranked that day")
    me: StandingResponse | None = Field(
        description="The calling Account's Standing, or null if it has no Daily Best that day"
    )
    entries: list[StandingResponse] = Field(description="Standings ordered by Rank")


class MedalResponse(BaseModel):
    account_id: uuid.UUID = Field(description="Requested Account")
    medal: Medal | None = Field(
        description="Medal from the Medal Day for Rank 1, 2 or 3, or null for none"
    )


class MedalsResponse(BaseModel):
    day: date = Field(description="Medal Day: the latest Tournament Day whose results are final")
    medals: list[MedalResponse] = Field(
        description="One entry per distinct requested Account, in request order"
    )
