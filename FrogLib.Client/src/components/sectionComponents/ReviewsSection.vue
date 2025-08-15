<script setup>
import { ref, computed } from 'vue';
import ReviewCard from '../cards/ReviewCard.vue';

const props = defineProps({
  reviews: { type: Array, required: true },
});

const currentIndex = ref(0);

const visibleReviews = computed(() => {
  return props.reviews.slice(currentIndex.value, currentIndex.value + 3);
});

const isPrevDisabled = computed(() => {
  return currentIndex.value === 0;
});

const isNextDisabled = computed(() => {
  return currentIndex.value + 5 >= props.reviews.length;
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
      <h2>✧ Популярные рецензии ✧</h2>
    </div>
    <div class="reviews-container-wrapper">
      <button
        class="slide-buttons prev-button"
        @click="prevSlide"
        :disabled="isPrevDisabled"
      >
        ←
      </button>
      <div class="reviews-container">
        <div class="reviews-grid">
          <ReviewCard
            v-for="(review, index) in visibleReviews"
            :key="index"
            :id="review.id"
            :title="review.title"
            :content="review.content"
            :book="review.book"
            :rating="review.rating"
            :created-date="review.createdDate"
            :user-name="review.userName"
            :user-u-r-l="review.userURL"
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
.reviews-container-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 0 20px;
}

.reviews-container {
  position: relative;
  width: 100%;
  overflow: hidden;
  flex-grow: 1;
}

.reviews-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  padding: 10px 0;
  scroll-behavior: smooth;
}
</style>
