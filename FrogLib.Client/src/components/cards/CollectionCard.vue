<script setup>
import { ref, computed } from 'vue';
import { formattedDate } from '@/utils/dateUtils';
import { truncateText } from '@/utils/truncateText';
import { getAvatarUrl } from '@/utils/imageUser';

const props = defineProps({
  id: { type: Number, required: true },
  title: { type: String, required: true },
  description: { type: String, required: true },
  books: { type: Array, required: true },
  createdDate: { type: String, required: true },
  userName: { type: String, required: true },
  userURL: { type: String, default: null },
});

const currentSlide = ref(0);

const truncatedDescription = computed(() => {
  return truncateText(props.description, 100);
});

const nextSlide = () => {
  currentSlide.value = (currentSlide.value + 1) % props.books.length;
};

const prevSlide = () => {
  currentSlide.value =
    (currentSlide.value - 1 + props.books.length) % props.books.length;
};
</script>

<template>
  <div class="collection-card">
    <h3 class="collection-title">
      {{ title }}
    </h3>

    <div class="collection-carousel">
      <button class="carousel-button prev" @click="prevSlide">‹</button>

      <div class="carousel-container">
        <div
          class="carousel-track"
          :style="{ transform: `translateX(-${currentSlide * 100}%)` }"
        >
          <div
            v-for="(book, index) in books"
            :key="index"
            class="carousel-slide"
          >
            <img :src="book.imageURL" :alt="book.title" class="book-cover" />
          </div>
        </div>
      </div>

      <button class="carousel-button next" @click="nextSlide">›</button>
    </div>

    <div class="collection-indicators">
      <span
        v-for="(book, index) in books"
        :key="index"
        :class="{ active: index === currentSlide }"
        @click="currentSlide = index"
      ></span>
    </div>

    <div class="collection-content">
      <p class="collection-text" v-html="truncatedDescription"></p>
      <button class="read-more-link">Посмотреть подборку →</button>
    </div>

    <div class="collection-footer">
      <div class="collection-author">
        <img class="avatar" :src="getAvatarUrl(userURL)" :alt="userName" />
        <span>{{ userName }}</span>
      </div>
      <div class="collection-date">{{ formattedDate(props.createdDate) }}</div>
    </div>
  </div>
</template>

<style scoped>
.collection-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-width: 400px;
  padding: 20px;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px #0000001a;
  transition: transform 0.3s;
}

.collection-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 6px 16px #00000026;
}

.collection-title {
  margin: 0 0 8px;
  font-size: 1.3rem;
  font-weight: 600;
  color: var(--dark-text);
}

.collection-carousel {
  position: relative;
  display: flex;
  align-items: center;
  margin-bottom: 10px;
}

.carousel-container {
  width: 100%;
  overflow: hidden;
}

.carousel-track {
  display: flex;
  transition: transform 0.3s ease;
}

.carousel-slide {
  display: flex;
  flex-direction: column;
  gap: 10px;
  min-width: 100%;
}

.book-cover {
  width: 100%;
  height: 180px;
  border-radius: 6px;
  box-shadow: 0 2px 8px #0000001a;
}

.carousel-button {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: #ffffffcc;
  border: none;
  border-radius: 50%;
  font-size: 1.2rem;
  box-shadow: 0 2px 4px #0000001a;
}

.carousel-button.prev {
  left: 8px;
}

.carousel-button.next {
  right: 8px;
}

.carousel-button:hover {
  background: #ffffff;
}

.collection-indicators {
  display: flex;
  justify-content: center;
  gap: 6px;
  margin: 8px 0;
}

.collection-indicators span {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #dddddd;
}

.collection-indicators span.active {
  background: var(--accent-color);
}

.collection-content {
  flex-grow: 1;
}

.collection-text {
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
}

.read-more-link:hover {
  text-decoration: underline;
}

.collection-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 15px;
  border-top: 1px solid #eeeeee;
}

.collection-author {
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

.collection-date {
  font-size: 0.8rem;
  color: #888888;
}
</style>
