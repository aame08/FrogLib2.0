<script setup>
import { computed } from 'vue';
import { formattedDate } from '@/utils/dateUtils';
import { truncateText } from '@/utils/truncateText';
import { getAvatarUrl } from '@/utils/imageUser';

const props = defineProps({
  id: { type: Number, required: true },
  title: { type: String, required: true },
  content: { type: String, required: true },
  book: { type: Object, required: true },
  rating: { type: Number, required: true },
  createdDate: { type: String, required: true },
  userName: { type: String, required: true },
  userURL: { type: String, default: null },
});

const truncatedDescription = computed(() => {
  return truncateText(props.content, 100);
});
</script>

<template>
  <div class="review-card">
    <h3 class="review-title">
      {{ title }}
    </h3>

    <div class="review-header">
      <div class="review-book-cover">
        <img :src="book.imageURL" :alt="book.title" />
      </div>
      <div class="review-meta">
        <h4 class="book-title">{{ book.title }}</h4>
        <p class="book-author">{{ book.authors }}</p>
        <div class="review-rating">
          <span v-for="i in 5" :key="i" :class="{ filled: i <= rating }"
            >★</span
          >
        </div>
      </div>
    </div>

    <div class="review-content">
      <p class="review-text" v-html="truncatedDescription"></p>
      <RouterLink :to="`/reviews/${id}`" class="read-more-link"
        >Читать полностью →</RouterLink
      >
    </div>

    <div class="review-footer">
      <div class="review-author">
        <img class="avatar" :src="getAvatarUrl(userURL)" :alt="userName" />
        <span>{{ userName }}</span>
      </div>
      <div class="review-date">{{ formattedDate(props.createdDate) }}</div>
    </div>
  </div>
</template>

<style scoped>
.review-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 400px;
  padding: 20px;
  background: var(--bg);
  border-radius: 12px;
  box-shadow: 0 4px 12px #0000001a;
  transition: transform 0.3s;
}

.review-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 6px 16px #00000026;
}

.review-title {
  margin: 0 0 8px;
  font-size: 1.3rem;
  font-weight: 600;
  color: var(--dark-text);
}

.review-header {
  display: flex;
  gap: 15px;
  margin-bottom: 10px;
}

.review-book-cover img {
  width: 80px;
  height: 120px;
  object-fit: cover;
  border-radius: 4px;
  box-shadow: 0 2px 8px #0000001a;
}

.book-title {
  margin: 0 0 5px;
  font-size: 1.1rem;
  color: var(--accent-color);
}

.book-author {
  margin: 0 0 10px;
  font-size: 0.9rem;
  color: var(--text-light);
}

.review-rating {
  font-size: 1.1rem;
}

.review-rating .filled {
  color: var(--accent-color);
}

.review-content {
  flex-grow: 1;
}

.review-text {
  margin: 0 0 12px;
  line-height: 1.6;
  color: var(--dark-text);
}

.read-more-link {
  display: inline-block;
  padding: 4px 0;
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--accent-color);
  background: none;
  border: none;
  text-decoration: none;
  cursor: pointer;
}

.read-more-link:hover {
  text-decoration: underline;
}

.review-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 15px;
  border-top: 1px solid #eeeeee;
}

.review-author {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
}

.avatar {
  width: 32px;
  height: 32px;
  object-fit: cover;
  border-radius: 50%;
}

.review-date {
  font-size: 0.8rem;
  color: var(--grey);
}
</style>
