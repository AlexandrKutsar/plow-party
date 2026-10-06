import string
import unicodedata

NICKNAME_MIN_LENGTH = 3
NICKNAME_MAX_LENGTH = 16
NICKNAME_SYMBOLS = frozenset(string.digits + " _-")
DEFAULT_NICKNAME_PREFIX = "Plower"
DEFAULT_NICKNAME_NUMBERS = range(10_000)


class NicknameError(ValueError):
    pass


def normalize_nickname(raw: str) -> str:
    nickname = unicodedata.normalize("NFC", raw).strip()
    if len(nickname) < NICKNAME_MIN_LENGTH:
        raise NicknameError(f"Nickname must be at least {NICKNAME_MIN_LENGTH} characters")
    if len(nickname) > NICKNAME_MAX_LENGTH:
        raise NicknameError(f"Nickname must be at most {NICKNAME_MAX_LENGTH} characters")
    if not all(char.isalpha() or char in NICKNAME_SYMBOLS for char in nickname):
        raise NicknameError("Nickname may contain only letters, digits, spaces, '_' and '-'")
    if "  " in nickname:
        raise NicknameError("Nickname must not contain consecutive spaces")
    return nickname


def default_nickname(number: int) -> str:
    if number not in DEFAULT_NICKNAME_NUMBERS:
        raise ValueError(f"Default nickname number must be in 0..{DEFAULT_NICKNAME_NUMBERS[-1]}")
    return f"{DEFAULT_NICKNAME_PREFIX}{number:04d}"
