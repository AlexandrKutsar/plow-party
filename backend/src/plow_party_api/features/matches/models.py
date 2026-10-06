import uuid
from datetime import datetime

from sqlalchemy import (
    Boolean,
    DateTime,
    Float,
    ForeignKey,
    Integer,
    SmallInteger,
    Text,
    UniqueConstraint,
    Uuid,
)
from sqlalchemy.dialects.postgresql import JSONB
from sqlalchemy.orm import Mapped, mapped_column, relationship

from plow_party_api.core.models import Base


class Match(Base):
    __tablename__ = "matches"

    id: Mapped[uuid.UUID] = mapped_column(Uuid, primary_key=True, default=uuid.uuid4)
    host_account_id: Mapped[uuid.UUID] = mapped_column(Uuid, ForeignKey("accounts.id"))
    registered_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    status: Mapped[str] = mapped_column(Text)
    rejection_reason: Mapped[str | None] = mapped_column(Text)
    interrupted: Mapped[bool | None] = mapped_column(Boolean)
    played_seconds: Mapped[int | None] = mapped_column(Integer)
    weight: Mapped[float | None] = mapped_column(Float)
    decided_at: Mapped[datetime | None] = mapped_column(DateTime(timezone=True))

    participants: Mapped[list["MatchParticipant"]] = relationship(
        order_by="MatchParticipant.slot", lazy="selectin"
    )
    votes: Mapped[list["MatchVote"]] = relationship(lazy="selectin")


class MatchParticipant(Base):
    __tablename__ = "match_participants"
    __table_args__ = (UniqueConstraint("match_id", "account_id"),)

    match_id: Mapped[uuid.UUID] = mapped_column(
        Uuid, ForeignKey("matches.id", ondelete="CASCADE"), primary_key=True
    )
    slot: Mapped[int] = mapped_column(SmallInteger, primary_key=True)
    account_id: Mapped[uuid.UUID | None] = mapped_column(Uuid, ForeignKey("accounts.id"))
    confirmed: Mapped[bool] = mapped_column(Boolean)
    score: Mapped[int | None] = mapped_column(Integer)
    placement: Mapped[int | None] = mapped_column(SmallInteger)
    credited_score: Mapped[int | None] = mapped_column(Integer)


class MatchVote(Base):
    __tablename__ = "match_votes"

    match_id: Mapped[uuid.UUID] = mapped_column(
        Uuid, ForeignKey("matches.id", ondelete="CASCADE"), primary_key=True
    )
    account_id: Mapped[uuid.UUID] = mapped_column(Uuid, ForeignKey("accounts.id"), primary_key=True)
    submitted_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    scores: Mapped[dict[str, int]] = mapped_column(JSONB)
    interrupted_at_seconds: Mapped[int | None] = mapped_column(SmallInteger)
