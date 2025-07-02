<script setup>
import { ref, computed } from 'vue';

import BookCard from '../cards/BookCard.vue';

const props = defineProps({
  titleSection: { type: String, required: true },
  books: { type: Array, required: true },
});

const currentIndex = ref(0);

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
      <h2>{{ titleSection }}</h2>
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
