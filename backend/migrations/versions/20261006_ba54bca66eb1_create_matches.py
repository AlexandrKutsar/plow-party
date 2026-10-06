from collections.abc import Sequence

import sqlalchemy as sa
from alembic import op
from sqlalchemy.dialects import postgresql

revision: str = "ba54bca66eb1"
down_revision: str | Sequence[str] | None = "cd5b8083fb76"
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    op.create_table(
        "matches",
        sa.Column("id", sa.Uuid(), nullable=False),
        sa.Column("host_account_id", sa.Uuid(), nullable=False),
        sa.Column("registered_at", sa.DateTime(timezone=True), nullable=False),
        sa.Column("status", sa.Text(), nullable=False),
        sa.Column("rejection_reason", sa.Text(), nullable=True),
        sa.Column("interrupted", sa.Boolean(), nullable=True),
        sa.Column("played_seconds", sa.Integer(), nullable=True),
        sa.Column("weight", sa.Float(), nullable=True),
        sa.Column("decided_at", sa.DateTime(timezone=True), nullable=True),
        sa.ForeignKeyConstraint(
            ["host_account_id"], ["accounts.id"], name=op.f("fk_matches_host_account_id_accounts")
        ),
        sa.PrimaryKeyConstraint("id", name=op.f("pk_matches")),
    )
    op.create_table(
        "match_participants",
        sa.Column("match_id", sa.Uuid(), nullable=False),
        sa.Column("slot", sa.SmallInteger(), nullable=False),
        sa.Column("account_id", sa.Uuid(), nullable=True),
        sa.Column("confirmed", sa.Boolean(), nullable=False),
        sa.Column("score", sa.Integer(), nullable=True),
        sa.Column("placement", sa.SmallInteger(), nullable=True),
        sa.Column("credited_score", sa.Integer(), nullable=True),
        sa.ForeignKeyConstraint(
            ["account_id"], ["accounts.id"], name=op.f("fk_match_participants_account_id_accounts")
        ),
        sa.ForeignKeyConstraint(
            ["match_id"],
            ["matches.id"],
            name=op.f("fk_match_participants_match_id_matches"),
            ondelete="CASCADE",
        ),
        sa.PrimaryKeyConstraint("match_id", "slot", name=op.f("pk_match_participants")),
        sa.UniqueConstraint("match_id", "account_id", name=op.f("uq_match_participants_match_id")),
    )
    op.create_table(
        "match_votes",
        sa.Column("match_id", sa.Uuid(), nullable=False),
        sa.Column("account_id", sa.Uuid(), nullable=False),
        sa.Column("submitted_at", sa.DateTime(timezone=True), nullable=False),
        sa.Column("scores", postgresql.JSONB(astext_type=sa.Text()), nullable=False),
        sa.Column("interrupted_at_seconds", sa.SmallInteger(), nullable=True),
        sa.ForeignKeyConstraint(
            ["account_id"], ["accounts.id"], name=op.f("fk_match_votes_account_id_accounts")
        ),
        sa.ForeignKeyConstraint(
            ["match_id"],
            ["matches.id"],
            name=op.f("fk_match_votes_match_id_matches"),
            ondelete="CASCADE",
        ),
        sa.PrimaryKeyConstraint("match_id", "account_id", name=op.f("pk_match_votes")),
    )


def downgrade() -> None:
    op.drop_table("match_votes")
    op.drop_table("match_participants")
    op.drop_table("matches")
