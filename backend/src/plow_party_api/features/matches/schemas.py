import uuid
from datetime import datetime
from typing import Annotated

from pydantic import AfterValidator, BaseModel, Field

from plow_party_api.features.matches.rules import (
    INTERRUPTED_SECONDS,
    SLOTS,
    MatchStatus,
    RosterSlot,
    Vote,
    check_roster,
)


class RosterSlotRequest(BaseModel):
    slot: int = Field(description="Slot number, 0-5")
    account_id: uuid.UUID | None = Field(
        description="Account of the Player holding the Slot, or null for a Bot"
    )


def _roster(entries: list[RosterSlotRequest]) -> list[RosterSlot]:
    return [RosterSlot(slot=entry.slot, account_id=entry.account_id) for entry in entries]


def _checked_roster(entries: list[RosterSlotRequest]) -> list[RosterSlotRequest]:
    check_roster(_roster(entries))
    return entries


class RegisterMatchRequest(BaseModel):
    roster: Annotated[list[RosterSlotRequest], AfterValidator(_checked_roster)] = Field(
        description=(
            "Every Slot of the Match: 4-6 distinct Slots 0-5, each Account at most once, "
            "and the calling Host among them"
        )
    )

    def to_roster(self) -> list[RosterSlot]:
        return _roster(self.roster)


class SlotScore(BaseModel):
    slot: int = Field(ge=SLOTS[0], le=SLOTS[-1], description="Slot number, 0-5")
    score: int = Field(ge=0, description="Score of the Participant in that Slot")


def _each_slot_once(scores: list[SlotScore]) -> list[SlotScore]:
    if len({entry.slot for entry in scores}) != len(scores):
        raise ValueError("Vote must score each Slot once")
    return scores


class VoteRequest(BaseModel):
    scores: Annotated[list[SlotScore], AfterValidator(_each_slot_once)] = Field(
        description="Score of every Slot in the Roster, exactly once each"
    )
    interrupted_at_seconds: int | None = Field(
        default=None,
        ge=INTERRUPTED_SECONDS[0],
        le=INTERRUPTED_SECONDS[-1],
        description="Second of play at which the Host left, or null for a full 180-second Match",
    )

    def to_vote(self) -> Vote:
        return Vote(
            scores={entry.slot: entry.score for entry in self.scores},
            interrupted_at_seconds=self.interrupted_at_seconds,
        )


class ParticipantResponse(BaseModel):
    slot: int = Field(description="Slot number, 0-5")
    account_id: uuid.UUID | None = Field(description="Account holding the Slot; null for a Bot")
    confirmed: bool = Field(description="Whether the Player confirmed in time; false for a Bot")
    score: int | None = Field(description="Accepted Score; null until the Match is accepted")
    placement: int | None = Field(
        description="Place by Score, ties share the higher place; null until accepted"
    )
    credited_score: int | None = Field(
        description=(
            "Score times Weight, rounded down, credited to the Tournament; "
            "null for Bots, unconfirmed seats, and Matches not accepted"
        )
    )


class MatchResponse(BaseModel):
    match_id: uuid.UUID = Field(description="Id the Host hands to every client")
    status: MatchStatus = Field(description="open until the Verdict, then accepted or rejected")
    rejection_reason: str | None = Field(description="Why the Match was rejected")
    registered_at: datetime = Field(description="Server time of registration")
    interrupted: bool | None = Field(description="Whether the accepted Match was interrupted")
    played_seconds: int | None = Field(description="Seconds of play the Verdict counted")
    weight: float | None = Field(description="1 for a full Match, 0.5 for an Interrupted Match")
    participants: list[ParticipantResponse] = Field(description="Every Slot, ordered by Slot")
