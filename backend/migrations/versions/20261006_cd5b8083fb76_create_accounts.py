from collections.abc import Sequence

import sqlalchemy as sa
from alembic import op

revision: str = "cd5b8083fb76"
down_revision: str | Sequence[str] | None = None
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    op.create_table(
        "accounts",
        sa.Column("id", sa.Uuid(), nullable=False),
        sa.Column("device_id", sa.Uuid(), nullable=False),
        sa.Column("nickname", sa.String(length=16), nullable=False),
        sa.Column("token_hash", sa.String(length=64), nullable=False),
        sa.Column(
            "created_at",
            sa.DateTime(timezone=True),
            server_default=sa.text("now()"),
            nullable=False,
        ),
        sa.PrimaryKeyConstraint("id", name=op.f("pk_accounts")),
        sa.UniqueConstraint("device_id", name=op.f("uq_accounts_device_id")),
        sa.UniqueConstraint("token_hash", name=op.f("uq_accounts_token_hash")),
    )


def downgrade() -> None:
    op.drop_table("accounts")
