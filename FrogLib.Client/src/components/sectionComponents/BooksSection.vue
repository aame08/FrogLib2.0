<script setup>
import { ref, computed } from 'vue';

import BookCard from '../cards/BookCard.vue';

const props = defineProps({
  titleSection: { type: String, required: true },
  books: { type: Array, required: true },
  isRecommended: { type: Boolean, default: false },
});

const currentIndex = ref(0);
const showTooltip = ref(false);

const visibleBooks = computed(() => {
  return props.books.slice(currentIndex.value, currentIndex.value + 5);
});

const isPrevDisabled = computed(() => {
  return currentIndex.value === 0;
});

const isNextDisabled = computed(() => {
  return currentIndex.value + 5 >= props.books.length;
});

const prevSlide = () => {
  if (!isPrevDisabled.value) {
    currentIndex.value -= 1;
  }
};

const nextSlide = () => {
  if (!isNextDisabled.value) {
    currentIndex.value += 1;
  }
};
</script>

<template>
  <section class="section">
    <div class="section-title">
      <h2>
        ✧ {{ titleSection }} ✧
        <button
          v-if="isRecommended"
          class="recommendation-info-button"
          @mouseenter="showTooltip = true"
          @mouseleave="showTooltip = false"
          @focus="showTooltip = true"
          @blur="showTooltip = false"
        >
          ⓘ
        </button>
      </h2>

      <div v-if="showTooltip && isRecommended" class="recommendation-tooltip">
        <h3>Персональные рекомендации</h3>
        <p>
          Эту подборку сделал для вас искусственный интеллект по имени Frog-Ai.
          Его создали и обучили наши программисты и специалисты по работе с
          данными.
        </p>

        <h4>Как это работает</h4>
        <p>В основе работы Frog-Ai лежит гибридная система рекомендаций:</p>
        <ul>
          <li>
            <strong>Контентная фильтрация:</strong> Анализирует содержание книг,
            которые вам нравятся, и ищет похожие
          </li>
          <li>
            <strong>Коллаборативная фильтрация:</strong> Сравнивает ваши
            предпочтения с другими читателями
          </li>
        </ul>

        <h4>Как улучшить рекомендации</h4>
        <ul>
          <li>Оценивайте прочитанные книги</li>
          <li>Добавляйте книги в свои коллекции</li>
          <li>Отмечайте статусы чтения ("Читаю", "Прочитано" и другие)</li>
        </ul>
        <p>
          Система обновляет рекомендации ежедневно, поэтому заглядывайте сюда
          почаще!
        </p>
      </div>
    </div>
    <div class="books-container-wrapper">
      <button
        class="slide-buttons prev-button"
        @click="prevSlide"
        :disabled="isPrevDisabled"
      >
        ←
      </button>
      <div class="books-container">
        <div class="books-grid">
          <BookCard
            v-for="(book, index) in visibleBooks"
            :key="index"
            :id="book.id || index"
            :title="book.title"
            :image-u-r-l="book.imageURL"
            :average-rating="book.averageRating"
          />
        </div>
      </div>
      <button
        class="slide-buttons next-button"
        @click="nextSlide"
        :disabled="isNextDisabled"
      >
        →
      </button>
    </div>
  </section>
</template>

<style scoped>
.recommendation-info-button {
  margin-left: 0.5rem;
  vertical-align: middle;
  padding: 0.2rem 0.5rem;
  font-size: 1.2rem;
  color: var(--accent-color);
  background: none;
  border: none;
  border-radius: 50%;
  cursor: help;
  transition: all 0.2s;
}

.recommendation-info-button:hover {
  color: var(--hover-color);
}

.recommendation-tooltip {
  position: absolute;
  top: 80%;
  left: 50%;
  z-index: 100;
  width: 550px;
  height: fit-content;
  padding: 1.5rem;
  text-align: left;
  transform: translateX(-50%);
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  box-shadow: 0 4px 12px #0000001a;
}

.recommendation-tooltip h3 {
  margin-top: 0;
  margin-bottom: 1rem;
  color: var(--accent-color);
}

.recommendation-tooltip h4 {
  margin-top: 1.5rem;
  margin-bottom: 0.5rem;
  font-size: 1rem;
}

.recommendation-tooltip p {
  margin: 0.5rem 0;
  line-height: 1.5;
}

.recommendation-tooltip ul {
  margin: 0.5rem 0;
  padding-left: 1.5rem;
}

.recommendation-tooltip li {
  margin-bottom: 0.3rem;
}

.books-container-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  gap: 20px;
}

.books-container {
  position: relative;
  width: 100%;
  overflow: hidden;
  flex-grow: 1;
}

.books-grid {
  display: flex;
  gap: 1.5rem;
  padding: 10px 0;
  scroll-behavior: smooth;
  transition: transform 0.5s ease;
}

.prev-button {
  order: 1;
}

.next-button {
  order: 3;
}

.books-container {
  order: 2;
}
</style>
