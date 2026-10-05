from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="PLOW_", env_file=".env", extra="ignore")

    database_url: str = "postgresql+asyncpg://plow:plow@localhost:5432/plow_party"
