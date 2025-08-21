from fastapi import APIRouter
import numpy as np
import pandas as pd
import app.core as core

router = APIRouter()


@router.get("/recommend/{user_id}")
async def recommend(user_id: int):
    # Если данные ещё не загружены
    if core.books is None or core.user_interactions is None or core.predicted_ratings is None:
        return [{"book_id": 0, "title": "Данные загружаются, повторите позже", "category": "-"}]

    try:
        # Если пользователь найден
        if core.user_ids is not None and user_id in core.user_ids:
            user_idx = np.where(core.user_ids == user_id)[0][0]
            collab_scores = core.predicted_ratings[user_idx]

            interacted_books = core.user_interactions[
                core.user_interactions['id_user'] == user_id]['id_book'].unique()

            # Контентные рекомендации
            favorite_books = core.user_interactions[
                (core.user_interactions['id_user'] == user_id) &
                (core.user_interactions['weight'] >= 4)
                ]['id_book'].values

            if len(favorite_books) > 0:
                favorite_indices = core.books[core.books['id_book'].isin(favorite_books)].index
                content_scores = core.content_similarity[favorite_indices].mean(axis=0)
            else:
                interacted_indices = core.books[core.books['id_book'].isin(interacted_books)].index
                content_scores = core.content_similarity[interacted_indices].mean(axis=0) \
                    if len(interacted_indices) > 0 else np.zeros(len(core.books))

            # Нормализация
            content_scores = (content_scores - content_scores.min()) / \
                             (content_scores.max() - content_scores.min() + 1e-10)
            collab_scores = (collab_scores - collab_scores.min()) / \
                            (collab_scores.max() - collab_scores.min() + 1e-10)

            # Выравнивание длин
            min_length = min(len(core.book_ids), len(content_scores), len(collab_scores))
            book_ids_aligned = core.book_ids[:min_length]
            content_scores_aligned = content_scores[:min_length]
            collab_scores_aligned = collab_scores[:min_length]

            recommendations = pd.DataFrame({
                'book_id': book_ids_aligned,
                'content_score': content_scores_aligned,
                'collab_score': collab_scores_aligned,
                'score': 0.6 * content_scores_aligned + 0.4 * collab_scores_aligned
            })

            # Исключаем книги, с которыми уже взаимодействовал пользователь
            recommendations = recommendations[~recommendations['book_id'].isin(interacted_books)]

            if len(recommendations) == 0:
                popular_books = core.user_interactions['id_book'].value_counts().head(5).index
                result = core.books[core.books['id_book'].isin(popular_books)][
                    ['id_book', 'title_book', 'name_category']]
                return [{"book_id": row.id_book, "title": row.title_book, "category": row.name_category}
                        for row in result.itertuples()]

            top_books = recommendations.sort_values('score', ascending=False).head(5)
            result = pd.merge(
                top_books,
                core.books[['id_book', 'title_book', 'name_category']],
                left_on='book_id',
                right_on='id_book',
                how='left'
            )

            return [{
                "book_id": row.book_id,
                "title": row.title_book,
                "category": row.name_category,
                "score": round(row.score, 2)
            } for row in result.itertuples()]

        # Если пользователь новый
        popular_books = core.user_interactions['id_book'].value_counts().head(5).index
        result = core.books[core.books['id_book'].isin(popular_books)][['id_book', 'title_book', 'name_category']]
        return [{"book_id": row.id_book, "title": row.title_book, "category": row.name_category}
                for row in result.itertuples()]

    except Exception as e:
        print(f"[ERROR] Ошибка в recommend: {e}")
        return [{"book_id": 0, "title": "Ошибка генерации рекомендаций", "category": "-"}]


@router.get("/similar_books/{book_id}")
async def similar_books(book_id: int, top_n: int = 5):
    if core.books is None or core.content_similarity is None:
        return [{"book_id": 0, "title": "Данные загружаются, повторите позже", "category": "-", "language": "-"}]

    try:
        book_idx = core.books[core.books['id_book'] == book_id].index
        if len(book_idx) == 0:
            return [{"book_id": 0, "title": "Книга не найдена", "category": "-", "language": "-"}]

        book_idx = book_idx[0]
        sim_scores = core.content_similarity[book_idx]

        similar_idx = np.argsort(sim_scores)[::-1]

        book_language = core.books.iloc[book_idx]['language_book']
        filtered_idx = [
                           i for i in similar_idx
                           if i != book_idx and core.books.iloc[i]['language_book'] == book_language
                       ][:top_n]

        result = core.books.iloc[filtered_idx][['id_book', 'title_book', 'name_category', 'language_book']]
        return [
            {
                "book_id": row.id_book,
                "title": row.title_book,
                "category": row.name_category,
                "language": row.language_book,
                "similarity_score": round(sim_scores[idx], 2)
            }
            for idx, row in zip(filtered_idx, result.itertuples())
        ]

    except Exception as e:
        print(f"[ERROR] Ошибка в similar_books: {e}")
        return [{"book_id": 0, "title": "Ошибка генерации похожих книг", "category": "-", "language": "-"}]
