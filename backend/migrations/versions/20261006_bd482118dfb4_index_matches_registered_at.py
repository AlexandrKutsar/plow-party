from collections.abc import Sequence

from alembic import op

revision: str = "bd482118dfb4"
down_revision: str | Sequence[str] | None = "ba54bca66eb1"
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    op.create_index(op.f("ix_matches_registered_at"), "matches", ["registered_at"], unique=False)


def downgrade() -> None:
    op.drop_index(op.f("ix_matches_registered_at"), table_name="matches")
