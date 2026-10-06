# Accounts

Status: ready-for-agent

## Problem Statement

A person who opens Plow Party should be able to play and appear in the daily Tournament without filling in any forms (GDD 9.1). The backend has no notion of who is calling it: Match registration and Match Result submission (GDD 9.3) need every Participant who is a Player to prove which Account they act for, and the Tournament needs a Nickname to show next to each score. Today the backend only has a health probe.

## Solution

A guest Account created on the first login by Device Id. The client generates a Device Id once, logs in on every app start, and gets back its Account id, its Nickname, and a fresh Auth Token. Every other call carries the Auth Token as a bearer header. A new Account gets a generated Nickname immediately; the Player can change it at any time, within simple rules that allow Cyrillic. Other backend features learn who is calling through one dependency exposed by the accounts service.

## User Stories

1. As a Player opening the game for the first time, I want to be logged in without any screen, so that I can start a Match right away.
2. As a Player, I want my Account to survive app restarts, so that my Tournament results and Nickname stay mine.
3. As a Player opening the game for the first time, I want a Nickname assigned automatically, so that I appear in the HUD and the Tournament even if I never pick one.
4. As a Player, I want to change my Nickname whenever I like, so that my friends recognise me.
5. As a Russian-speaking Player, I want to use Cyrillic letters in my Nickname, so that I can write my name naturally.
6. As a Player, I want a clear reason when my Nickname is rejected (too short, too long, forbidden character, double space), so that I can fix it.
7. As a Player, I want leading and trailing spaces trimmed from my Nickname, so that an accidental space does not make it invalid or invisible.
8. As a Player, I want to keep a Nickname someone else already uses, so that I am never blocked by a name collision.
9. As a Player, I want my new Nickname shown everywhere at once, including past Tournament entries, so that I have one identity.
10. As a Player, I want saving the same Nickname again to just succeed, so that the client does not need to special-case it.
11. As a Player, I want to read my own Account (id and Nickname), so that the client can show it after a restart.
12. As a client developer, I want login to be the same call for new and existing Accounts, so that the client has one start-up path.
13. As a client developer, I want a 401 whenever the Auth Token is missing, malformed, or stale, so that the client knows to log in again.
14. As a client developer, I want the Device Id to be a UUID the client generates, so that every virtual Player in Multiplayer Play Mode gets its own Account.
15. As a client developer, I want a malformed Device Id rejected with 422, so that client bugs surface immediately.
16. As a client developer, I want Device Ids that differ only in letter case to reach the same Account, so that formatting quirks never fork an Account.
17. As a client developer, I want the contract in `docs/api/openapi.json` with operation ids equal to the route names, so that I can write C# DTOs one-to-one (ADR-0007).
18. As a Player, I want my Device Id never echoed back by the API, so that it does not leak through logs or responses.
19. As a Player, I want a new login to invalidate my previous Auth Token, so that a leaked token stops working.
20. As a Player whose client fires two first logins at once, I want both to land on one Account, so that a race never creates duplicates.
21. As a backend developer building matches, I want a current-Account dependency from the accounts service, so that I authenticate Participants without reading account tables (ADR-0005 boundary).
22. As a backend developer, I want only a hash of each Auth Token stored, so that a database dump does not hand out working tokens.
23. As a backend developer, I want the Nickname rules as pure functions with exhaustive tests, so that changing them is safe and fast to verify.
24. As a maintainer, I want the security limits of guest accounts written down honestly, so that nobody mistakes the Auth Token for strong authentication (ADR-0011).

## Implementation Decisions

- **Module:** new feature slice `features/accounts` with router, schemas, service, rules, repository, models, and a module `CLAUDE.md`; its row in the backend feature index moves from `planned` to `active`. The router is included by `create_app`; the models are imported by the migration environment.
- **Glossary:** Account, Nickname, Device Id, Auth Token, as defined in `GLOSSARY.md`. Player stays the in-Match concept; the backend talks about Accounts.
- **Device Id:** any syntactically valid UUID, stored in canonical form in a unique column. Never returned.
- **Auth Token:** opaque, generated from a cryptographic random source (32 bytes, URL-safe). The table stores its SHA-256 hex digest in a unique column; lookup is by digest. One active token per Account: each login overwrites the digest. No expiry. Sent as `Authorization: Bearer <token>`; missing, malformed, or unknown tokens give 401 with `WWW-Authenticate: Bearer`. See ADR-0011.
- **Login:** upsert by Device Id with `ON CONFLICT`, so concurrent first logins resolve to one Account; a new Account gets a generated Nickname; the token digest is replaced on every login. Always 200.
- **Default Nickname:** `Plower` followed by four random digits (`Plower0000`–`Plower9999`); collisions are allowed.
- **Nickname rules** (pure, in rules): normalise to NFC, trim surrounding whitespace, then require length 3–16; every character is a letter (Python's notion of a letter, so Cyrillic and other scripts pass), an ASCII digit, a space, `_`, or `-`; no two consecutive spaces. Case is preserved. Not unique, no profanity filter, no change cooldown.
- **Validation errors:** the request schema runs the Nickname rules in a validator, so a rejected Nickname is a standard FastAPI 422 whose `msg` names the broken rule.
- **API contract:**
  - `POST /accounts/login` — body `{device_id}` → `{account_id, nickname, token}`.
  - `GET /accounts/me` — bearer → `{account_id, nickname}`.
  - `PATCH /accounts/me` — bearer, body `{nickname}` → `{account_id, nickname}`.
  - Operation ids are the route function names.
- **Schema:** table `accounts`: `id` UUID primary key generated by the backend, `device_id` UUID unique, `nickname` text, `token_hash` text unique, `created_at` timestamp with time zone. One Alembic migration.
- **Cross-feature interface:** the accounts service exposes a `CurrentAccountDep` dependency that resolves the bearer token to the calling Account (id and Nickname). Other features import only the service.
- **No rate limiting** in the MVP; recorded as a known risk in ADR-0011.

## Testing Decisions

- Tests check external behaviour only: Nickname rules through their public functions, everything else through HTTP responses. No test reaches into the repository or the tables directly.
- **Rules seam:** unit tests for Nickname normalisation and validation and the default Nickname format; no database, no fixtures.
- **HTTP seam:** tests drive the app through the existing `client` fixture against a real Postgres from testcontainers with all migrations applied: login creates and then reuses an Account, Device Id case-insensitivity, token rotation invalidating the old token, 401 paths, 422 for bad Device Id and bad Nickname, Nickname change visible on `GET /accounts/me`, Device Id and hash absent from responses.
- Each test uses a fresh random Device Id, so tests share the database without interfering.
- Prior art: `tests/features/health/test_health.py` and the `client` fixture in `tests/conftest.py`.

## Out of Scope

- Linking an Account to Google Play or any real identity; moving an Account between devices.
- Medals and Tournament data on the Account (tournament feature).
- Match registration and Participant confirmation (matches feature), beyond exposing `CurrentAccountDep`.
- Rate limiting, profanity filtering, Nickname uniqueness.
- Any client code.

## Further Notes

The Auth Token is defence in depth, not strong authentication: whoever reads the Device Id off the device owns the Account. ADR-0011 states this so the README can describe it honestly alongside GDD 9.3.
