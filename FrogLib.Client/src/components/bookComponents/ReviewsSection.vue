<script setup>
import ReviewCard from '../cards/ReviewCard.vue';

const props = defineProps({
  countReviews: { type: Number, required: true },
  reviews: { type: Array, required: true },
});
</script>

<template>
  <div v-if="countReviews === 0" class="empty-state">
    <p>Пока нет рецензий на эту книгу.</p>
    <button class="btn btn-accent" style="margin-top: 15px">
      Написать первую рецензию
    </button>
  </div>
  <div v-else class="reviews-container">
    <p class="reviews-count">
      Количество рецензий: {{ countReviews.toFixed(0) }}
    </p>
    <div class="reviews-grid">
      <ReviewCard v-for="review in reviews" :key="review.id" v-bind="review" />
    </div>
  </div>
</template>

<style scoped>
.empty-state {
  text-align: center;
  padding: 40px 0;
  color: var(--text-light);
}

.reviews-count {
  margin-bottom: 20px;
  font-size: 1.1rem;
  color: var(--text-light);
}

.reviews-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 24px;
  width: 100%;
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
</style>
