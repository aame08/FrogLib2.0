<script setup>
import { ref, computed } from 'vue';
import { truncateText } from '@/utils/truncateText';

const props = defineProps({
  book: { type: Object, required: true },
});

const starRating = computed(() => {
  if (!props.book.averageRating) return Array(5).fill('empty');

  const rating = props.book.averageRating;
  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;

  return Array.from({ length: 5 }, (_, i) => {
    if (i < fullStars) return 'full';
    if (i === fullStars && hasHalfStar) return 'half';
    return 'empty';
  });
});

const truncatedDescription = computed(() => {
  return truncateText(props.book.description, 200);
});
</script>

<template>
  <div class="book-card">
    <img :src="book.imageURL" :alt="book.title" />
    <div class="book-info">
      <p class="book-author">{{ book.authors }}</p>
      <RouterLink :to="`/books/${book.id}`" class="book-title">{{
        book.title
      }}</RouterLink>
      <div class="star-rating">
        <span
          v-for="(starType, index) in starRating"
          :key="index"
          :class="['star', starType]"
        >
          ★
        </span>
        <span class="rating-value">
          {{ book.averageRating?.toFixed(1) || '0.0' }}
        </span>
      </div>
      <p>{{ truncatedDescription }}</p>
    </div>
  </div>
</template>

<style scoped>
.book-card {
  display: flex;
  gap: 15px;
  padding: 15px;
  background-color: var(--light-bg);
  border-radius: 4px;
  box-shadow: 0 2px 5px #0000001a;
  transition: transform 0.3s, box-shadow 0.3s;
}

.book-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px #0000001a;
}

.book-card img {
  height: 250px;
  align-self: center;
}

.book-info {
  display: flex;
  flex-direction: column;
}

.book-author {
  margin-bottom: 0;
  color: var(--text-light);
}

.book-title {
  color: var(--dark-text);
  font-size: 1.3rem;
  font-weight: 600;
  letter-spacing: 1px;
}

.book-title:hover {
  text-decoration: underline;
  text-decoration-color: var(--accent-color);
}

.star-rating {
  display: flex;
  align-items: center;
  gap: 2px;
}

.star {
  position: relative;
  font-size: 1.2em;
  color: var(--light-grey);
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
</style>
