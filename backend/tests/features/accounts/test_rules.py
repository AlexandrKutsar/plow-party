import pytest

from plow_party_api.features.accounts.rules import (
    NicknameError,
    default_nickname,
    normalize_nickname,
)


def test_normalize_nickname_surrounding_spaces_are_trimmed() -> None:
    assert normalize_nickname("  Snowy  ") == "Snowy"


@pytest.mark.parametrize("raw", ["ab", "   ab   ", ""])
def test_normalize_nickname_shorter_than_three_is_rejected(raw: str) -> None:
    with pytest.raises(NicknameError, match="at least 3"):
        normalize_nickname(raw)


def test_normalize_nickname_sixteen_characters_is_accepted() -> None:
    assert normalize_nickname("a" * 16) == "a" * 16


def test_normalize_nickname_longer_than_sixteen_is_rejected() -> None:
    with pytest.raises(NicknameError, match="at most 16"):
        normalize_nickname("a" * 17)


@pytest.mark.parametrize("raw", ["Снежок", "Plow_Party-7", "Ice Queen", "Plower0042", "ÉlanSnø"])
def test_normalize_nickname_letters_digits_space_underscore_hyphen_are_accepted(raw: str) -> None:
    assert normalize_nickname(raw) == raw


@pytest.mark.parametrize("raw", ["Snow!", "a.b.c", "Plow🚜", "Tab\tName", "Ice²Queen", "Снег٣"])
def test_normalize_nickname_other_characters_are_rejected(raw: str) -> None:
    with pytest.raises(NicknameError, match="letters, digits, spaces"):
        normalize_nickname(raw)


def test_normalize_nickname_double_space_is_rejected() -> None:
    with pytest.raises(NicknameError, match="consecutive spaces"):
        normalize_nickname("Ice  Queen")


def test_normalize_nickname_decomposed_letters_are_composed_to_nfc() -> None:
    assert normalize_nickname("\u0411\u043e\u0438\u0306") == "\u0411\u043e\u0439"


@pytest.mark.parametrize(
    ("number", "expected"), [(0, "Plower0000"), (42, "Plower0042"), (9999, "Plower9999")]
)
def test_default_nickname_is_plower_with_four_digits(number: int, expected: str) -> None:
    assert default_nickname(number) == expected


@pytest.mark.parametrize("number", [-1, 10000])
def test_default_nickname_number_outside_four_digits_is_rejected(number: int) -> None:
    with pytest.raises(ValueError, match=r"0..9999"):
        default_nickname(number)


def test_default_nickname_passes_nickname_rules() -> None:
    assert normalize_nickname(default_nickname(7)) == "Plower0007"
