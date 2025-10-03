<script setup>
import { ref, computed } from 'vue';
import reviewsService from '@/services/reviewsService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import ReviewCard from '@/components/cards/ReviewCard.vue';

const allReviews = ref([]);
const searchQuery = ref('');
const selectedSort = ref('newest');
const currentPage = ref(1);
const itemsPerPage = 12;

const getReviews = async () => {
  try {
    const response = await reviewsService.getAllReviews();
    allReviews.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке рецензий:', error);
  }
};
getReviews();

const filteredReviews = computed(() => {
  let result = allReviews.value;

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    result = result.filter(
      (review) =>
        review.title.toLowerCase().includes(query) ||
        review.book.title.toLowerCase().includes(query)
    );
  }

  switch (selectedSort.value) {
    case 'newest':
      result = result.sort(
        (a, b) => new Date(b.createdDate) - new Date(a.createdDate)
      );
      break;
    case 'oldest':
      result = result.sort(
        (a, b) => new Date(a.createdDate) - new Date(b.createdDate)
      );
      break;
    case 'rating-high':
      result = result.sort((a, b) => b.rating - a.rating);
      break;
    case 'rating-low':
      result = result.sort((a, b) => a.rating - b.rating);
      break;
  }

  return result;
});

const paginatedCollections = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return filteredReviews.value.slice(start, end);
});

const totalPages = computed(() => {
  return Math.ceil(filteredReviews.value.length / itemsPerPage);
});

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
  }
};

const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
  }
};

const goToPage = (page) => {
  currentPage.value = page;
};
</script>

<template>
  <TheHeader />
  <div class="container">
    <div class="page-header">
      <div>
        <h1 class="page-title">✧ Каталог рецензий ✧</h1>
        <p>Мнения пользователей о прочитанных книгах</p>
      </div>
    </div>

    <section class="filters-section">
      <div class="filters-row">
        <div class="filter-group">
          <div class="search-input">
            <input
              type="text"
              class="filter-input"
              placeholder="Введите название рецензии или книги..."
              v-model="searchQuery"
            />
            <div class="search-icon">⌕</div>
          </div>
        </div>

        <div class="filter-group">
          <select class="filter-input" v-model="selectedSort">
            <option value="newest">Сначала новые</option>
            <option value="oldest">Сначала старые</option>
            <option value="rating-high">Высокий рейтинг</option>
            <option value="rating-low">Низкий рейтинг</option>
          </select>
        </div>
      </div>
    </section>

    <div class="reviews-grid">
      <ReviewCard
        v-for="(review, index) in paginatedCollections"
        :key="index"
        :id="review.id"
        :title="review.title"
        :content="review.content"
        :book="review.book"
        :rating="review.userRating"
        :created-date="review.createdDate"
        :user-name="review.userName"
        :user-u-r-l="review.userURL"
      />
    </div>

    <div v-if="filteredReviews.length > itemsPerPage" class="pagination">
      <button
        @click="prevPage"
        :disabled="currentPage === 1"
        title="Предыдущая страница"
      >
        ←
      </button>
      <button
        v-for="page in totalPages"
        :key="page"
        @click="goToPage(page)"
        :class="{ active: currentPage === page }"
      >
        {{ page }}
      </button>
      <button
        @click="nextPage"
        :disabled="currentPage === totalPages"
        title="Следующая страница"
      >
        →
      </button>
    </div>
  </div>
</template>

<style scoped>
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.page-title {
  color: var(--accent-color);
}

.filters-section {
  position: sticky;
  top: 76px;
  z-index: 100;
  margin-bottom: 30px;
  padding: 25px;
  background: var(--bg);
  border: 1px solid var(--accent-color);
  border-radius: 8px;
  box-shadow: var(--shadow);
}

.filters-row {
  display: flex;
  align-items: end;
  gap: 20px;
}

.filter-group {
  flex: 1;
}

.filter-input {
  width: 95%;
  padding: 10px 12px;
  font-size: 0.9rem;
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 4px;
  transition: border-color 0.2s;
}

.filter-input:focus {
  outline: none;
  border-color: var(--accent-color);
}

.search-input {
  position: relative;
}

.search-icon {
  position: absolute;
  top: 50%;
  right: 12px;
  transform: translateY(-50%);
  color: var(--text-light);
}

.reviews-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: 25px;
  margin-bottom: 40px;
}
</style>
