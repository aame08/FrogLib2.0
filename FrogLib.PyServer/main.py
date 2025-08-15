#  uvicorn main:app --reload

from fastapi import FastAPI
from app.core import lifespan
from app.api import router as api_router

app = FastAPI(lifespan=lifespan)
app.include_router(api_router)
