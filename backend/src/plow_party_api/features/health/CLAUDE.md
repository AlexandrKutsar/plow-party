# health

Liveness probe for hosting and CI: `GET /health` runs `SELECT 1` and returns `{"status": "ok"}`, so a 200 means both the API and its database answer.

The reference slice for the layout, not for the layers: it has only `router.py` and `schemas.py` because it has no rules or tables.
