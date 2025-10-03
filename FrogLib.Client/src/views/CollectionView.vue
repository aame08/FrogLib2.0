<script setup>
import { ref, computed } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
import collectionsService from '@/services/collectionsService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import CollectionCard from '@/components/cards/CollectionCard.vue';
import CollectionForm from '@/components/generalComponents/CollectionForm.vue';

const route = useRoute();
const store = useStore();
const collections = ref([]);
const searchQuery = ref('');
const selectedSort = ref('newest');
const currentPage = ref(1);
const itemsPerPage = 12;
const isCollectionFormVisible = ref(false);
const user = computed(() => store.getters['auth/user']);
const idUser = computed(() => user.value?.idUser || null);
const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);

const getCollections = async () => {
  try {
    const response = await collectionsService.getAllCollections();
    collections.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке подборок:', error);
  }
};
getCollections();

const filteredCollections = computed(() => {
  let result = collections.value;

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    result = result.filter((collection) =>
      collection.title.toLowerCase().includes(query)
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
    case 'title':
      result = result.sort((a, b) => a.title.localeCompare(b.title));
      break;
    case 'titleDesc':
      result = result.sort((a, b) => b.title.localeCompare(a.title));
      break;
  }

  return result;
});

const paginatedCollections = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return filteredCollections.value.slice(start, end);
});

const totalPages = computed(() => {
  return Math.ceil(filteredCollections.value.length / itemsPerPage);
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

const openForm = () => {
  isCollectionFormVisible.value = true;
};

const closeForm = () => {
  isCollectionFormVisible.value = false;
};
</script>

<template>
  <TheHeader />

  <div class="container" v-if="!isCollectionFormVisible">
    <div class="page-header">
      <div>
        <h1 class="page-title">✧ Каталог подборок ✧</h1>
        <p>Откройте для себя тематические коллекции книг</p>
      </div>
      <button
        class="btn btn-primary create-collection-btn"
        @click="openForm"
        :disabled="!isAuthenticated"
      >
        <span style="color: #ffffff">+</span> Создать подборку
      </button>
    </div>

    <div class="filters-section">
      <div class="filters-row">
        <div class="filter-group">
          <div class="search-input">
            <input
              type="text"
              class="filter-input"
              placeholder="Введите название..."
              v-model="searchQuery"
            />
            <div class="search-icon">⌕</div>
          </div>
        </div>

        <div class="filter-group">
          <select class="filter-input" v-model="selectedSort">
            <option value="newest">Сначала новые</option>
            <option value="oldest">Сначала старые</option>
            <option value="title">По названию (А-Я)</option>
            <option value="titleDesc">По названию (Я-А)</option>
          </select>
        </div>
      </div>
    </div>

    <div class="collections-grid">
      <CollectionCard
        v-for="(collection, index) in paginatedCollections"
        :key="index"
        :id="collection.id"
        :title="collection.title"
        :description="collection.description"
        :books="collection.books"
        :created-date="collection.createdDate"
        :user-name="collection.userName"
        :user-u-r-l="collection.userURL"
      />
    </div>

    <div v-if="filteredCollections.length > itemsPerPage" class="pagination">
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

  <CollectionForm
    v-if="isCollectionFormVisible"
    :id-user="idUser"
    @close="() => closeForm()"
  />
</template>

<style scoped>
.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.btn-primary {
  background: var(--accent-color);
  color: #ffffff;
}

.btn-primary:hover {
  background: var(--hover-color);
  transform: translateY(-2px);
  box-shadow: var(--shadow-hover);
}

.collections-page {
  padding: 40px 0;
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

.create-collection-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 1rem;
}

.filters-section {
  position: sticky;
  top: 76px;
  z-index: 100;
  background: var(--bg);
  padding: 25px;
  border-radius: 8px;
  border: 1px solid var(--accent-color);
  box-shadow: var(--shadow);
  margin-bottom: 30px;
}

.filters-row {
  display: flex;
  gap: 20px;
  align-items: end;
}

.filter-group {
  flex: 1;
}

.filter-input {
  width: 95%;
  padding: 10px 12px;
  border: 1px solid var(--border);
  font-size: 0.9rem;
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

.collections-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: 25px;
  margin-bottom: 40px;
}
</style>
