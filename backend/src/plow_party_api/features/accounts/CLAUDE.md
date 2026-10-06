# accounts

Guest Accounts (GDD 9.1): login by Device Id, the Account's Nickname, and the Auth Token every other feature authenticates with. Decisions: ADR-0011.

## Entry points

- `POST /accounts/login` — `{device_id}` → `{account_id, nickname, token}`. Creates the Account on first sight of a Device Id (upsert, race-safe) and rotates the Auth Token on every call.
- `GET /accounts/me`, `PATCH /accounts/me` — read the calling Account; change its Nickname.
- `service.CurrentAccountDep` — the only way other features learn who is calling: resolves `Authorization: Bearer` to a `CurrentAccount`, or answers 401.
- `AccountService.existing_ids` — which of a set of Account ids exist; matches uses it to validate a Roster.

## Rules

`rules.py` holds the Nickname rules (NFC, trimmed, 3–16 characters of letters, ASCII digits, space, `_`, `-`, no double space) and the default `Plower0000`-style Nickname. The request schema runs them, so a broken rule is a 422 whose `msg` names it.

## State

Table `accounts`: `device_id` and `token_hash` are unique; only the SHA-256 of the Auth Token is stored. The Device Id and the hash never leave the service.
