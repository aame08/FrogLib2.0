<script setup>
import { ref, computed } from 'vue';
import booksService from '@/services/booksService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import BookCard from '@/components/cards/BookCard.vue';

const books = ref([]);
const categories = ref([]);
const searchQuery = ref('');
const selectedSort = ref('rating');
const selectedCategory = ref([]);
const selectedRating = ref('');
const currentPage = ref(1);
const itemsPerPage = 20;

const getBooks = async () => {
  try {
    const response = await booksService.getAllBooks();
    books.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке книг:', error);
  }
};

const getCategories = async () => {
  try {
    const response = await booksService.getCategories();
    categories.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке категорий:', error);
  }
};

getBooks();
getCategories();

const filteredBooks = computed(() => {
  let result = books.value;

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    result = result.filter((book) => book.title.toLowerCase().includes(query));
  }

  if (selectedSort.value === 'rating') {
    result = result.sort((a, b) => b.averageRating - a.averageRating);
  } else if (selectedSort.value === 'news') {
    result = result.sort((a, b) => b.yearPublication - a.yearPublication);
  }

  if (selectedCategory.value.length > 0) {
    result = result.filter((book) =>
      selectedCategory.value.includes(book.idCategory)
    );
  }

  if (selectedRating.value) {
    const ratingValue = parseInt(selectedRating.value);
    result = result.filter((book) => book.averageRating >= ratingValue);
  }

  return result;
});

const paginatedBooks = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return filteredBooks.value.slice(start, end);
});

const totalPages = computed(() => {
  return Math.ceil(filteredBooks.value.length / itemsPerPage);
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
    <div class="header">
      <h1 class="page-title">Каталог книг</h1>
      <div class="search-sort-container">
        <input
          type="text"
          class="search-box"
          placeholder="Поиск по названию..."
          v-model="searchQuery"
        />
        <select class="sort-select" v-model="selectedSort">
          <option value="rating">По рейтингу</option>
          <option value="news">По новизне</option>
        </select>
      </div>
    </div>

    <main class="main-content">
      <aside class="filters-section">
        <h3>Фильтры</h3>
        <fieldset>
          <legend>Категории</legend>
          <div
            class="filter-option"
            v-for="category in categories"
            :key="category.idCategory"
          >
            <input
              type="checkbox"
              :id="`category-${category.idCategory}`"
              :value="category.idCategory"
              v-model="selectedCategory"
            />
            <label :for="`category-${category.idCategory}`">
              {{ category.nameCategoryRu }}
            </label>
          </div>
        </fieldset>
        <fieldset>
          <legend>Рейтинг</legend>
          <div class="filter-option">
            <input
              type="radio"
              id="rating-5"
              name="rating"
              value="5"
              v-model="selectedRating"
            />
            <label for="rating-5">★★★★★</label>
          </div>
          <div class="filter-option">
            <input
              type="radio"
              id="rating-4"
              name="rating"
              value="4"
              v-model="selectedRating"
            />
            <label for="rating-4">★★★★ и выше</label>
          </div>
          <div class="filter-option">
            <input
              type="radio"
              id="rating-3"
              name="rating"
              value="3"
              v-model="selectedRating"
            />
            <label for="rating-3">★★★ и выше</label>
          </div>
        </fieldset>
      </aside>

      <section class="books-section">
        <div class="books-grid">
          <BookCard
            v-for="(book, index) in paginatedBooks"
            :key="index"
            :id="book.id || index"
            :title="book.title"
            :image-u-r-l="book.imageURL"
            :average-rating="book.averageRating"
          />
        </div>

        <div v-if="filteredBooks.length > itemsPerPage" class="pagination">
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
      </section>
    </main>
  </div>
</template>

<style scoped>
.container {
  max-width: 1450px;
  margin: 0 auto;
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.page-title {
  font-size: 2rem;
  font-weight: 600;
  color: var(--accent-color);
}

.search-sort-container {
  display: flex;
  gap: 20px;
  align-items: center;
  height: 50px;
}

.search-box,
.sort-select {
  padding: 10px 15px;
  width: 250px;
  font-size: 1rem;
}

.main-content {
  display: flex;
  gap: 30px;
}

.filters-section {
  flex: 0 0 250px;
  align-self: flex-start;
  position: sticky;
  top: 20px;
  height: fit-content;
  max-height: calc(100vh - 40px);
  overflow-y: auto;

  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 5px #0000001a;
}

.filters-section h3 {
  margin-top: 0;
  padding-bottom: 10px;
  border-bottom: 1px solid #eeeeee;
  color: var(--accent-color);
}

.filter-option {
  margin-bottom: 10px;
}

.filter-option label {
  margin-left: 8px;
  cursor: pointer;
}

.filter-option input {
  cursor: pointer;
}

.books-section {
  flex: 1;
}

.books-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
}

.pagination {
  display: flex;
  justify-content: center;
  gap: 10px;
  margin-top: 30px;
}

.pagination button {
  padding: 8px 15px;
  border: 1px solid var(--accent-color);
  border-radius: 4px;
  background-color: #ffffff;
  cursor: pointer;
  transition: all 0.3s;
}

.pagination button:hover {
  background-color: var(--hover-color);
  border-color: var(--hover-color);
  color: #ffffff;
}

.pagination button.active {
  background-color: var(--accent-color);
  color: #ffffff;
  border-color: var(--accent-color);
}
</style>
