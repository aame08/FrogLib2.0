import pandas as pd
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from scipy.sparse import hstack
import nltk
from nltk.corpus import stopwords

nltk.download('stopwords')
russian_stopwords = stopwords.words('russian')


def content_based_recommendation(books: pd.DataFrame):
    tfidf = TfidfVectorizer(stop_words=russian_stopwords, max_features=5000)
    tfidf_matrix = tfidf.fit_transform(books['clean_description'])

    category_dummies = pd.get_dummies(books['name_category'], sparse=True)
    content_features = hstack([tfidf_matrix, category_dummies])

    return cosine_similarity(content_features)
