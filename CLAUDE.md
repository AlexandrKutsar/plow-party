# Plow Party

Mobile (Android) top-down multiplayer arcade: animals on snowplows collect snow and deliver it to a drop-off zone. Portfolio project, 2-week scope, judged on code and architecture quality. Game design: `docs/GDD.md` (Russian).

## Monorepo

| Path | What | Agent context |
|---|---|---|
| `client/` | Unity 6 project (URP, Photon Fusion 2 host mode, VContainer, UniTask) | `client/CLAUDE.md` |
| `backend/` | Tournament API (FastAPI + PostgreSQL, Docker) — not started | `backend/CLAUDE.md` once created |
| `.github/workflows/` | CI/CD — not started | |
| `docs/` | GDD, architecture, standards, ADRs | |

## Read before changing code

- **Architecture** — `docs/architecture.md`: layers, lifetime scopes, Gameplay/Meta boundary, and the **feature index**. Read before adding a feature or crossing a module boundary.
- **Standards** — `docs/coding-standards.md`: binding rules for C#; `/code-review` checks against it.
- **Glossary** — `GLOSSARY.md`: code names for every game concept. Name types, tests, and issues with these terms.
- **Decisions** — `docs/adr/`: read the ADRs touching the area you change; flag any contradiction explicitly.

## Living docs

Every module folder carries a `CLAUDE.md` describing its purpose, entry points, state, and decisions. A change to a module updates its `CLAUDE.md` and its row in the feature index in the same commit. A new module starts by writing its `CLAUDE.md`. Specs and tickets in `.scratch/` are transient; module `CLAUDE.md` files are the lasting record.

Docs, code, and commit messages are in English. Talk to the user in the language they write in.

## Agent skills

### Issue tracker

Issues and specs live as local markdown files under `.scratch/<feature>/`. See `docs/agents/issue-tracker.md`.

### Triage labels

Default vocabulary: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
