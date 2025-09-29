import asyncio
import re
from contextlib import asynccontextmanager
from datetime import datetime
from pathlib import Path

from fastapi import FastAPI

from app.db import load_data
from app.recommender.collaborative import collaborative_filtering
from app.recommender.content_based import content_based_recommendation

# Глобальные переменные
books = None
user_interactions = None
content_similarity = None
predicted_ratings = None
user_ids = None
book_ids = None
last_update_time = None
update_interval = 3600  # 1 час
forbidden_words_list = None

letter_map = {
    "а": ["a", "@", "4"],
    "б": ["6", "b"],
    "в": ["b", "v", "8"],
    "г": ["g"],
    "д": ["d"],
    "е": ["e", "€"],
    "ё": ["e", "yo"],
    "ж": ["zh", "*"],
    "з": ["3", "z", "2"],
    "и": ["i", "1", "!"],
    "й": ["i", "y"],
    "к": ["k", "c", "q"],
    "л": ["l"],
    "м": ["m"],
    "н": ["h", "n"],
    "о": ["o", "0"],
    "п": ["n", "p"],
    "р": ["r", "p"],
    "с": ["c", "$", "s", "5"],
    "т": ["t", "7"],
    "у": ["y", "u"],
    "ф": ["f", "ph"],
    "х": ["x", "h", "kh"],
    "ц": ["c", "ts"],
    "ч": ["ch", "4"],
    "ш": ["sh"],
    "щ": ["sch", "shh"],
    "ы": ["y", "i"],
    "ь": ["'", "`"],
    "э": ["e"],
    "ю": ["yu", "u"],
    "я": ["ya", "ja"],
}


@asynccontextmanager
async def lifespan(app: FastAPI):
    global books, user_interactions, content_similarity, predicted_ratings, user_ids, book_ids, last_update_time
    global forbidden_words_list

    books, user_interactions = load_data()
    content_similarity = content_based_recommendation(books)
    predicted_ratings, user_ids, book_ids = collaborative_filtering(user_interactions, books)
    forbidden_words_list = load_forbidden_words()
    last_update_time = datetime.now()

    asyncio.create_task(update_data())
    yield

    print("[INFO] Приложение останавливается")


async def update_data():
    global books, user_interactions, content_similarity, predicted_ratings, user_ids, book_ids, last_update_time
    global forbidden_words_list

    while True:
        try:
            print(f'[DEBUG] Обновление данных в {datetime.now()}')
            books, user_interactions = load_data()
            content_similarity = content_based_recommendation(books)
            predicted_ratings, user_ids, book_ids = collaborative_filtering(user_interactions, books)
            forbidden_words_list = load_forbidden_words()
            last_update_time = datetime.now()
            print(f'[DEBUG] Загружено {len(forbidden_words_list)} запрещенных слов')
        except Exception as e:
            print(f'[ERROR] Ошибка при обновлении данных: {e}')

        await asyncio.sleep(update_interval)


def normalize_text(text: str) -> str:
    text = text.lower()
    for target, variants in letter_map.items():
        for v in variants:
            text = re.sub(re.escape(v), target, text)
    return text


def build_pattern_for_word(word: str) -> re.Pattern:
    return re.compile(".*?".join(map(re.escape, word)), re.IGNORECASE | re.UNICODE)


def load_forbidden_words():
    try:
        file_path = Path("badwords.txt")
        if file_path.exists():
            with open(file_path, 'r', encoding='utf-8') as f:
                words = [line.strip().lower() for line in f if line.strip()]
            return words
        return []
    except Exception as e:
        print(f"[ERROR] Ошибка загрузки запрещенных слов: {e}")
        return []


def check_text_for_forbidden_words(text: str, forbidden_words: list) -> dict:
    if not text or not forbidden_words:
        return {"has_forbidden": False, "found_words": [], "forbidden_count": 0}

    normalized = normalize_text(text)

    found_words = []
    for word in forbidden_words:
        pattern = build_pattern_for_word(word)
        if pattern.search(normalized):
            found_words.append(word)

    return {
        "has_forbidden": len(found_words) > 0,
        "found_words": found_words,
        "forbidden_count": len(found_words)
    }


def highlight_forbidden_words(text: str, forbidden_words: list) -> str:
    if not text or not forbidden_words:
        return text

    normalized = normalize_text(text)
    highlighted_text = text

    for word in forbidden_words:
        pattern = build_pattern_for_word(word)

        matches = list(pattern.finditer(normalized))
        for match in matches:
            start, end = match.span()
            original_fragment = text[start:end]
            highlighted_text = highlighted_text.replace(
                original_fragment, f'<span class="forbidden-word">{original_fragment}</span>'
            )

    return highlighted_text
