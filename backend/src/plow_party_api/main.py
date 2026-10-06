from collections.abc import AsyncGenerator
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.routing import APIRoute

from plow_party_api.core.database import Database
from plow_party_api.core.settings import Settings
from plow_party_api.features.accounts.router import router as accounts_router
from plow_party_api.features.health.router import router as health_router


def operation_id(route: APIRoute) -> str:
    return route.name


def create_app(settings: Settings | None = None) -> FastAPI:
    database = Database((settings or Settings()).database_url)

    @asynccontextmanager
    async def lifespan(_: FastAPI) -> AsyncGenerator[None]:
        yield
        await database.dispose()

    app = FastAPI(
        title="Plow Party API",
        version="0.1.0",
        lifespan=lifespan,
        generate_unique_id_function=operation_id,
    )
    app.state.database = database
    app.include_router(health_router)
    app.include_router(accounts_router)
    return app
