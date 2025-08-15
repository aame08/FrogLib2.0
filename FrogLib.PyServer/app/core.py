import asyncio
from contextlib import asynccontextmanager
from datetime import datetime
from fastapi import FastAPI

from app.db import load_data
from app.recommender.content_based import content_based_recommendation
from app.recommender.collaborative import collaborative_filtering

# Глобальные переменные
books = None
user_interactions = None
content_similarity = None
predicted_ratings = None
user_ids = None
book_ids = None
last_update_time = None
update_interval = 3600  # 1 час


@asynccontextmanager
async def lifespan(app: FastAPI):
    global books, user_interactions, content_similarity, predicted_ratings, user_ids, book_ids, last_update_time

    books, user_interactions = load_data()
    content_similarity = content_based_recommendation(books)
    predicted_ratings, user_ids, book_ids = collaborative_filtering(user_interactions, books)
    last_update_time = datetime.now()

    asyncio.create_task(update_data())
    yield

    print("[INFO] Приложение останавливается")


async def update_data():
    global books, user_interactions, content_similarity, predicted_ratings, user_ids, book_ids, last_update_time
    while True:
        print(f'[DEBUG] Обновление данных в {datetime.now()}')
        books, user_interactions = load_data()
        content_similarity = content_based_recommendation(books)
        predicted_ratings, user_ids, book_ids = collaborative_filtering(user_interactions, books)
        last_update_time = datetime.now()
        await asyncio.sleep(update_interval)
