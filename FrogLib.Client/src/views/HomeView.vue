<script setup>
import { ref, onMounted } from 'vue';
import booksService from '@/services/booksService';
import reviewsService from '@/services/reviewsService';
import collectionsService from '@/services/collectionsService';
import userActivityService from '@/services/userActivityService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import BooksSection from '@/components/sectionComponents/BooksSection.vue';
import ReviewsSection from '@/components/sectionComponents/ReviewsSection.vue';
import CollectionsSection from '@/components/sectionComponents/CollectionsSection.vue';

const newBooks = ref([]);
const bestsellers = ref([]);
const popularBooks = ref([]);
const popularReviews = ref([]);
const popularCollections = ref([]);
const statistics = ref(null);

const getNewBooks = async () => {
  try {
    const response = await booksService.getNewBooks();
    newBooks.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке новых книг:', error);
  }
};

const getBestselling = async () => {
  try {
    const response = await booksService.getBestselling();
    bestsellers.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке бестселлеров:', error);
  }
};

const getPopularBooks = async () => {
  try {
    const response = await booksService.getPopularBooks();
    popularBooks.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке популярных книг:', error);
  }
};

const getPopularReviews = async () => {
  try {
    const response = await reviewsService.getPopularReviews();
    popularReviews.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке популярных реценций:', error);
  }
};

const getPopularCollections = async () => {
  try {
    const response = await collectionsService.getPopularCollections();
    popularCollections.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке популярных подборок:', error);
  }
};

const getStatistics = async () => {
  try {
    const response = await userActivityService.getStatistics();
    statistics.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке статистики:', error);
  }
};

getNewBooks();
getBestselling();
getPopularBooks();
getPopularReviews();
getPopularCollections();
getStatistics();

onMounted(() => {
  getStatistics();

  setInterval(getStatistics, 60000);
});
</script>

<template>
  <TheHeader />

  <section class="quote-section">
    <div class="container quote-container">
      <div class="quote-text">
        "Книги напоминают, что даже в самых темных страницах жизни можно найти
        светлые слова и мысли. Они дают нам укрытие, когда реальность становится
        невыносимой."
      </div>
      <div class="quote-image">
        <img src="@/assets/book2.png" alt="image" />
      </div>
    </div>
  </section>

  <section class="features-container">
    <h1>Почему именно мы?</h1>
    <div class="inner-container">
      <div class="features-card">
        <img src="@/assets/search.png" alt="Поиск книг" />
        <h2>Поиск книг</h2>
        <p>Наш удобный поиск поможет вам найти именно то, что вы ищете.</p>
      </div>
      <div class="features-card">
        <img src="@/assets/collection.png" alt="Создание подборок" />
        <h2>Создание подборок</h2>
        <p>
          Создавайте свои собственные подборки книг и просматривайте другие.
        </p>
      </div>
      <div class="features-card">
        <img src="@/assets/recommend.png" alt="Получение рекомендаций" />
        <h2>Получение рекомендаций</h2>
        <p>
          Получайте персонализированные рекомендации книг на основе ваших
          предпочтений.
        </p>
      </div>
    </div>
  </section>

  <div class="container">
    <BooksSection
      v-if="newBooks.length > 0"
      title-section="Книжные новинки"
      :books="newBooks"
    />
    <BooksSection title-section="Бестселлеры" :books="bestsellers" />
    <BooksSection title-section="Популярные книги" :books="popularBooks" />
    <ReviewsSection :reviews="popularReviews" />
    <CollectionsSection :collections="popularCollections" />
    <section class="section">
      <div class="section-title">
        <h2>✧ Статистика сайта ✧</h2>
      </div>
      <div class="forum-stats">
        <div class="stat-item">
          <h3>{{ statistics.countUsers }}</h3>
          <p>Пользователей</p>
        </div>
        <div class="stat-item">
          <h3>{{ statistics.countBooks }}</h3>
          <p>Книг</p>
        </div>
        <div class="stat-item">
          <h3>{{ statistics.countReviews }}</h3>
          <p>Рецензий</p>
        </div>
        <div class="stat-item">
          <h3>{{ statistics.countCollections }}</h3>
          <p>Подборок</p>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.quote-section {
  background-color: var(--accent-color);
  padding: 1.5rem 0;
  text-align: center;
}

.quote-container {
  max-width: 800px;
  margin: 0 auto;
}

.quote-text {
  font-size: 1.8rem;
  font-style: italic;
  line-height: 1.4;
  margin-bottom: 1.5rem;
  color: #ffffff;
}

.quote-image img {
  height: 300px;
  margin-top: -80px;
}

.features-container {
  padding: 4rem 0;
  background-color: var(--light-bg);
  text-align: center;
}

.features-container h1 {
  font-size: 2.5rem;
  margin-bottom: 2rem;
  color: var(--accent-color);
}

.inner-container {
  display: flex;
  justify-content: center;
  flex-wrap: wrap;
  gap: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.features-card {
  width: 280px;
  padding: 2rem;
  background-color: #ffffff;
  border-radius: 8px;
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s;
}

.features-card:hover {
  transform: translateY(-5px);
}

.features-card img {
  width: 80px;
  height: 80px;
  object-fit: contain;
  margin-bottom: 1rem;
}

.features-card h2 {
  font-size: 1.4rem;
  font-weight: 700;
  margin-bottom: 1rem;
  color: var(--accent-color);
}

.features-card p {
  font-size: 0.95rem;
  color: #666;
}

.container {
  width: 85%;
  max-width: 1300px;
  margin: 0 auto;
}

.forum-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  text-align: center;
  margin-bottom: 2rem;
  padding: 2rem;
  background-color: var(--light-bg);
  border-radius: 6px;
}

.stat-item h3 {
  margin-bottom: 0.5rem;
  font-size: 2rem;
  color: var(--accent-color);
}

.stat-item p {
  color: #666666;
}
</style>
