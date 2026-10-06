## Summary

Guest Accounts for the backend (GDD 9.1): log in by Device Id, get a Nickname and an Auth Token that Matches will use (GDD 9.3). Decisions in ADR-0011; terms Account, Nickname, Device Id, Auth Token added to `GLOSSARY.md`.

```text
POST  /accounts/login  {device_id}  → {account_id, nickname, token}   upsert by Device Id, new token every call
GET   /accounts/me     Bearer       → {account_id, nickname}          401 + WWW-Authenticate otherwise
PATCH /accounts/me     Bearer {nickname} → {account_id, nickname}     422 msg names the broken rule
```

```text
login(device_id)
  token = random 32 bytes
  INSERT account(device_id, "Plower" + 4 digits, sha256(token))
    ON CONFLICT (device_id) UPDATE token_hash    # keeps id and Nickname, old token dies
  return account, token

CurrentAccountDep                                  # for matches, imported from accounts/service.py
  Authorization: Bearer <token> → lookup by sha256 → CurrentAccount | 401
```

```diff
 backend/src/plow_party_api/
 ├── main.py                       # + include accounts router
 └── features/
+    └── accounts/                 # router, schemas, service, rules, repository, models, CLAUDE.md
 backend/migrations/
 ├── env.py                        # + TABLE_MODULES = (accounts_models,)
+└── versions/…_create_accounts.py
 docs/
 ├── api/openapi.json              # regenerated
 ├── backend-architecture.md       # accounts → active; two layer exceptions recorded
+└── adr/0011-guest-accounts-device-id-and-opaque-token.md
```

Nickname rules (`rules.py`, pure): NFC, trimmed, 3–16 chars of letters (Cyrillic included), ASCII digits, space, `_`, `-`; no double space; not unique; no cooldown.

## Evidence

- **Before:** `POST /accounts/login` → 404; no `accounts` table.
  **After:** `uv run pytest` → `43 passed` against Postgres 17 from testcontainers with all migrations applied. ruff, strict pyright, `check_no_comments.py` clean; `openapi.json` up to date.

```text
login new Device Id               → 200, Plower\d{4}, token, no device_id in body
login same Device Id twice / UPPER → same account_id and Nickname
5 concurrent first logins          → one account_id
login again                        → old token 401, new token 200
bad Authorization (none, Basic, "Bearer ", unknown, no scheme) → 401 + WWW-Authenticate: Bearer
PATCH "  Снежок_7 "               → 200 "Снежок_7", visible on GET /me
PATCH "Ice  Queen"                 → 422 "…consecutive spaces"
```

## Merge Danger

**Door:** two-way

New table and new routes only; nothing existing changes behaviour. Rolling back is `alembic downgrade` plus revert. The Device Id and token formats become a contract with the client once it ships (ADR-0011).

**Blast Radius:** additive

No client code yet. The Device Id is the real credential and there is no rate limiting; both are recorded in ADR-0011.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
