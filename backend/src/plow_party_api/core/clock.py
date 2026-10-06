from collections.abc import Callable
from datetime import UTC, datetime
from typing import Annotated

from fastapi import Depends

type Clock = Callable[[], datetime]


def utc_now() -> datetime:
    return datetime.now(UTC)


def get_clock() -> Clock:
    return utc_now


ClockDep = Annotated[Clock, Depends(get_clock)]
