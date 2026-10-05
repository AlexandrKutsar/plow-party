from fastapi import APIRouter
from sqlalchemy import text

from plow_party_api.core.dependencies import SessionDep
from plow_party_api.features.health.schemas import HealthResponse

router = APIRouter(tags=["health"])


@router.get(
    "/health",
    summary="Liveness of the API and its database",
    response_model=HealthResponse,
)
async def get_health(session: SessionDep) -> HealthResponse:
    await session.execute(text("SELECT 1"))
    return HealthResponse(status="ok")
