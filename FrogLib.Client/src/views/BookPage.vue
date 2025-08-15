<script setup>
import { ref, computed } from 'vue';
import { useRoute } from 'vue-router';
import booksService from '@/services/booksService';
import { pluralize } from '@/utils/pluralize';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import InfoSection from '@/components/bookComponents/InfoSection.vue';
import ReviewsSection from '@/components/bookComponents/ReviewsSection.vue';
import CollectionsSection from '@/components/bookComponents/CollectionsSection.vue';

const route = useRoute();
const book = ref(null);
const userRating = ref(0);
const hoverRating = ref(null);
const ratingSubmitted = ref(false);
const ratingTexts = ['', 'Ужасно', 'Плохо', 'Нормально', 'Хорошо', 'Отлично'];
const ratingText = computed(() => {
  const rating = hoverRating.value || userRating.value;
  return rating ? ratingTexts[rating] : 'Поставьте оценку';
});
const listTypes = ['Читаю', 'В планах', 'Брошено', 'Прочитано', 'Любимые'];
const selectedListType = ref('');
const activeSection = ref('book');

const getBookInfo = async () => {
  try {
    const response = await booksService.getBookInfo(route.params.id);
    book.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке информации о книге:', error);
  }
};
getBookInfo();

const setRating = (rating) => {
  userRating.value = rating;
};

const changeSection = (section) => {
  activeSection.value = section;
};

const starRating = computed(() => {
  if (!book.value?.ratingStats?.averageRating) return Array(5).fill('empty');

  const rating = book.value.ratingStats.averageRating;
  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;

  return Array.from({ length: 5 }, (_, i) => {
    if (i < fullStars) return 'full';
    if (i === fullStars && hasHalfStar) return 'half';
    return 'empty';
  });
});
</script>

<template>
  <TheHeader />
  <div class="book-page">
    <div class="container">
      <div class="book-layout">
        <div class="book-cover-container">
          <img :src="book.imageURL" :alt="book.title" class="book-cover" />

          <div class="rating-widget">
            <div class="rating-title">Ваша оценка</div>
            <div class="rating-stars">
              <span
                class="rating-star"
                v-for="star in 5"
                :key="star"
                :class="{ active: userRating >= star }"
                @click="setRating(star)"
                @mouseover="hoverRating = star"
                @mouseleave="hoverRating = null"
                >{{
                  hoverRating >= star || (!hoverRating && userRating >= star)
                    ? '★'
                    : '☆'
                }}</span
              >
            </div>
            <div class="rating-value">{{ ratingText }}</div>
            <button class="rating-submit">Отправить оценку</button>
            <!-- <button
                class="rating-submit"
              >
                Удалить оценку
              </button> -->
          </div>

          <div class="book-actions">
            <select class="book-select">
              <option value="">Добавить в список</option>
              <option
                v-for="(type, index) in listTypes"
                :key="index"
                :value="type"
              >
                {{ type }}
              </option>
            </select>
            <button class="btn btn-accent">✧ Написать рецензию</button>
            <button class="btn btn-outline">✧ Создать подборку</button>
          </div>
        </div>

        <div class="book-info">
          <header class="book-header">
            <h1 class="book-title">{{ book.title }}</h1>
            <p class="book-author">{{ book.authors }}</p>

            <div class="book-meta">
              <div class="meta-item" v-if="book">
                <div class="star-rating">
                  <span
                    v-for="(starType, index) in starRating"
                    :key="index"
                    :class="['star', starType]"
                  >
                    ★
                  </span>
                  <span class="rating-value">
                    {{ book.ratingStats?.averageRating?.toFixed(1) || '0.0' }}
                  </span>
                </div>
              </div>
              <div class="meta-item">
                <span>👁</span>
                <span
                  >{{ book.countView }}
                  {{ pluralize(book.countView, 'просмотр') }}</span
                >
              </div>
            </div>
          </header>

          <div class="book-tabs">
            <button
              class="tab-btn"
              :class="{ active: activeSection === 'book' }"
              @click="changeSection('book')"
            >
              О книге
            </button>
            <button
              class="tab-btn"
              :class="{ active: activeSection === 'reviews' }"
              @click="changeSection('reviews')"
            >
              Рецензии
            </button>
            <button
              class="tab-btn"
              :class="{ active: activeSection === 'collections' }"
              @click="changeSection('collections')"
            >
              Подборки
            </button>
          </div>

          <div v-if="activeSection === 'book'">
            <InfoSection v-bind="book" />
          </div>
          <div v-else-if="activeSection === 'reviews'">
            <ReviewsSection
              :count-reviews="book.countReviews"
              :reviews="book.reviews"
            />
          </div>
          <div v-else-if="activeSection === 'collections'">
            <CollectionsSection
              :count-collections="book.countCollections"
              :collections="book.collections"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.book-page {
  padding: 40px 0;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.book-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 40px;
}

.book-cover-container {
  position: relative;
}

.book-cover {
  width: 100%;
  height: auto;
  border-radius: 4px;
  box-shadow: 0 4px 12px #0000000d;
  transition: transform 0.3s;
}

.book-cover:hover {
  transform: scale(1.02);
}

.rating-widget {
  margin-top: 15px;
  padding: 15px;
  border-radius: 4px;
  box-shadow: 0 4px 12px #0000000d;
  background: var(--light-bg);
}

.rating-title {
  font-weight: 600;
  margin-bottom: 10px;
  color: var(--accent-color);
}

.rating-stars {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
}

.rating-star {
  font-size: 24px;
  cursor: pointer;
  color: #e0e0e0;
  transition: all 0.2s;
}

.rating-star:hover,
.rating-star.active {
  color: var(--hover-color);
  transform: scale(1.2);
}

.rating-value {
  font-size: 0.9rem;
  color: var(--text-light);
  text-align: center;
}

.rating-submit {
  width: 100%;
  padding: 8px;
  margin-top: 10px;
  background: var(--accent-color);
  color: #ffffff;
  border: none;
  border-radius: 4px;
  transition: background 0.2s;
}

.rating-submit:hover {
  background: var(--hover-color);
}

.book-actions {
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.book-actions select {
  padding: 8px 12px;
}

.btn {
  padding: 8px 16px;
  border-radius: 4px;
  font-weight: 500;
  transition: all 0.2s;
  border: none;
}

.btn-accent {
  background: var(--accent-color);
  color: white;
}

.btn-accent:hover {
  background: var(--hover-color);
  transform: translateY(-2px);
  box-shadow: 0 6px 16px #0000001a;
}

.btn-outline {
  background-color: var(--light-bg);
  border: 1px solid var(--accent-color);
  color: var(--accent-color);
}

.btn-outline:hover {
  background: var(--hover-color);
  color: #ffffff;
}

.book-header {
  margin-bottom: 30px;
}

.book-title {
  font-size: 2.2rem;
  line-height: 1.2;
  margin-bottom: 8px;
  color: var(--accent-color);
}

.book-author {
  font-size: 1.2rem;
  color: var(--dark-text);
  font-style: italic;
  margin-bottom: 20px;
}

.book-meta {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.95rem;
}

.star-rating {
  display: flex;
  align-items: center;
  gap: 2px;
}

.star {
  font-size: 1.2em;
  position: relative;
  color: #d3d3d3;
}

.star.full {
  color: var(--accent-color);
}

.star.half::before {
  content: '★';
  position: absolute;
  left: 0;
  width: 50%;
  overflow: hidden;
  color: var(--accent-color);
}

.rating-value {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--accent-color);
}

.book-tabs {
  display: flex;
  border-bottom: 1px solid #e0e0e0;
  margin-bottom: 30px;
}

.tab-btn {
  padding: 12px 20px;
  background: none;
  border: none;
  cursor: pointer;
  font-weight: 500;
  color: var(--text-light);
  position: relative;
  transition: all 0.2s;
}

.tab-btn:hover {
  color: var(--hover-color);
}

.tab-btn.active {
  color: var(--hover-color);
  font-weight: 600;
}

.tab-btn.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background: var(--hover-color);
}
</style>
