import pandas as pd
import pymysql

db_config = {
    'host': 'localhost',
    'user': 'root',
    'password': 'ame0372',
    'database': 'froglib2'
}


def load_data():
    connection = pymysql.connect(**db_config)

    books_query = """
    SELECT b.id_book, b.title_book, b.description_book, b.id_category, c.name_category_ru AS name_category, b.language_book
    FROM books b
    JOIN categories c ON b.id_category = c.id_category
    """
    books = pd.read_sql(books_query, connection)
    books['clean_description'] = books['description_book'].fillna('').str.lower()

    user_books_query = "SELECT id_user, id_book, list_type FROM userbooks"
    user_books = pd.read_sql(user_books_query, connection)

    ratings_query = "SELECT id_user, id_book, rating FROM bookratings"
    ratings = pd.read_sql(ratings_query, connection)

    list_weights = {
        'Прочитано': 3,
        'В планах': 2,
        'Брошено': -1,
        'Любимые': 4,
        'Читаю': 2
    }
    user_books['weight'] = user_books['list_type'].map(list_weights)

    user_interactions = pd.concat([
        ratings.assign(weight=ratings['rating'] * 2),
        user_books[['id_user', 'id_book', 'weight']]
    ]).drop_duplicates(['id_user', 'id_book'], keep='first')

    connection.close()
    print(f"[DEBUG] Загружено {len(books)} книг и {len(user_interactions)} взаимодействий")
    return books, user_interactions
