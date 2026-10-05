# OpenAPI as the client–backend contract

The FastAPI-generated OpenAPI document, exported to `docs/api/openapi.json` and committed, is the single source of truth for the HTTP contract between the Unity client and the backend. CI fails when the committed file is stale, so a change to the API shows up as a reviewable diff of the contract in the same commit. The client writes its C# DTOs and calls against this file by hand for now; generating a C# client from it stays an option once the API settles.
