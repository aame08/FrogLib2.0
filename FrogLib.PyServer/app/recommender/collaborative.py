import pandas as pd
import numpy as np
from sklearn.decomposition import TruncatedSVD


def collaborative_filtering(user_interactions: pd.DataFrame, books: pd.DataFrame):
    try:
        user_book_matrix = user_interactions.pivot_table(
            index='id_user',
            columns='id_book',
            values='weight',
            fill_value=0
        )

        if user_book_matrix.empty:
            dummy_books = books['id_book'].unique()
            return np.zeros((1, len(dummy_books))), np.array([0]), dummy_books

        user_means = user_book_matrix.mean(axis=1)
        user_book_matrix = user_book_matrix.sub(user_means, axis=0)

        n_components = min(50, user_book_matrix.shape[1])
        svd = TruncatedSVD(n_components=n_components, random_state=42)
        user_factors = svd.fit_transform(user_book_matrix)
        book_factors = svd.components_.T

        predicted_ratings = np.dot(user_factors, book_factors.T)
        return predicted_ratings, user_book_matrix.index, user_book_matrix.columns

    except Exception as e:
        print(f"[ERROR] Ошибка в collaborative_filtering(): {e}")
        dummy_books = books['id_book'].unique()
        return np.zeros((1, len(dummy_books))), np.array([0]), dummy_books
