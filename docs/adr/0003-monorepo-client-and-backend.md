# Monorepo with client/ and backend/

The Unity project lives in `client/`, the tournament API in `backend/`, CI in `.github/workflows/`, all in one repository. One repo lets a reviewer see the whole system and lets one commit change an API contract on both sides. Keeping Unity out of the root keeps its generated files (`Library/`, `.csproj`, `.sln`) away from Python tooling, gives CI simple path filters (`client/**`, `backend/**`), and lets each side carry its own `CLAUDE.md` loaded only when an agent works there. Cost: Unity Hub and the Unity CLI need the `client/` path.
