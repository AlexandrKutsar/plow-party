from datetime import UTC, datetime, timedelta


class FakeClock:
    def __init__(self) -> None:
        self.now = datetime(2026, 10, 6, 12, 0, tzinfo=UTC)

    def __call__(self) -> datetime:
        return self.now

    def advance(self, seconds: float) -> None:
        self.now += timedelta(seconds=seconds)
