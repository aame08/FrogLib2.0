<!-- СДЕЛАТЬ РЕДАКТИРОВАНИЕ -->

<script setup>
import { ref } from 'vue';
import { QuillEditor } from '@vueup/vue-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css';
import booksService from '@/services/booksService';
import userWithEntitiesService from '@/services/userWithEntitiesService';

const props = defineProps({
  book: { type: Object, required: true },
  idUser: { type: Number, default: 0 },
});

const emit = defineEmits(['refresh-data', 'close']);

const editorOptions = ref({
  theme: 'snow',
  placeholder: 'Опишите вашу подборку: тема, идея, почему выбрали эти книги...',
  modules: {
    toolbar: [
      ['bold', 'italic', 'underline'],
      [{ list: 'ordered' }, { list: 'bullet' }],
    ],
  },
});

const collectionTitle = ref('');
const collectionDescription = ref('');
const allBooks = ref([]);
const searchQuery = ref('');
const searchResults = ref([]);
const selectedBooks = ref([]);
const errors = ref({});

const getAllBooks = async () => {
  try {
    const response = await booksService.getAllBooks();
    allBooks.value = response;
  } catch (error) {
    console.error('Ошибка при загрузке книг:', error);
  }
};
getAllBooks();

const searchBooks = () => {
  if (!searchQuery.value.trim()) {
    searchResults.value = [];
    return;
  }

  const query = searchQuery.value.toLowerCase();
  searchResults.value = allBooks.value.filter((book) =>
    book.title.toLowerCase().includes(query)
  );
};

const toggleBookSelection = (book) => {
  const index = selectedBooks.value.findIndex((b) => b.id === book.id);

  if (index > -1) {
    selectedBooks.value.splice(index, 1);
  } else {
    selectedBooks.value.push(book);
  }
};

const removeBook = (idBook) => {
  selectedBooks.value = selectedBooks.value.filter(
    (book) => book.id !== idBook
  );
};

const isBookSelected = (idBook) => {
  return selectedBooks.value.some((book) => book.id === idBook);
};

const submitCollection = async () => {
  errors.value = {};
  let isValid = true;

  if (!collectionTitle.value.trim()) {
    errors.value.titleCollection = 'Заголовок подборки не может быть пустым.';
    isValid = false;
  }

  if (selectedBooks.value.length < 5) {
    errors.value.countBooks = 'В подборке не может быть меньше 5 книг.';
    isValid = false;
  }

  if (!isValid) {
    return;
  }

  try {
    const collectionForm = {
      idUser: props.idUser,
      titleCollection: collectionTitle.value,
      descriptionCollection: collectionDescription.value,
      plaintDescription: collectionDescription.value.replace(/<[^>]*>/g, ''),
      idBooks: selectedBooks.value.map((book) => book.id),
    };

    await userWithEntitiesService.addCollection(collectionForm);

    // emit('refresh-data')
    emit('close');
  } catch (error) {
    console.error('Ошибка при отправке подборке:', error);
  }
};

const closeForm = () => {
  emit('close');
};
</script>

<template>
  <div class="container">
    <div class="form-header">
      <h1>Подборка книг</h1>
      <p>Соберите любимые книги в тематическую подборку</p>
    </div>

    <form class="collection-form" @submit.prevent="submitCollection">
      <div class="form-group">
        <label class="form-label">Название подборки</label>
        <input
          type="text"
          class="form-input"
          placeholder="Введите название подборки..."
          v-model="collectionTitle"
          :class="{ 'input-error': errors.titleCollection }"
        />
        <div class="character-count">
          {{ collectionTitle.length }}/255 символов
        </div>
        <div v-if="errors.titleCollection" class="error-message">
          {{ errors.titleCollection }}
        </div>
      </div>

      <div class="form-group">
        <label class="form-label">Описание подборки</label>
        <QuillEditor
          v-model:content="collectionDescription"
          contentType="html"
          :options="editorOptions"
        />
      </div>

      <div class="form-group">
        <label class="form-label">Книги в подборке</label>
        <div v-if="errors.countBooks" class="error-message">
          {{ errors.countBooks }}
        </div>
        <div class="selected-books" v-if="selectedBooks.length > 0">
          <div class="form-label">Выбрано книг: {{ selectedBooks.length }}</div>
          <div class="selected-books-list">
            <div
              v-for="book in selectedBooks"
              :key="book.id"
              class="selected-book"
            >
              <span>{{ book.title }}</span>
              <button
                type="button"
                class="remove-book"
                @click="removeBook(book.id)"
              >
                ×
              </button>
            </div>
          </div>
        </div>

        <div class="book-search">
          <div class="search-input">
            <input
              type="text"
              class="form-input"
              placeholder="Начните вводить название книги..."
              v-model="searchQuery"
              @input="searchBooks"
            />
            <div class="search-icon">⌕</div>
          </div>

          <div
            class="book-results"
            v-if="searchResults.length > 0 && searchQuery"
          >
            <div
              v-for="book in searchResults"
              :key="book.id"
              class="book-result-item"
              @click="toggleBookSelection(book)"
            >
              <img
                :src="book.imageURL"
                :alt="book.title"
                class="book-result-cover"
              />
              <div class="book-result-info">
                <div class="book-result-title">{{ book.title }}</div>
                <div class="book-result-author">{{ book.authors }}</div>
                <div class="book-result-rating">
                  ★ {{ book.averageRating.toFixed(1) }}
                </div>
              </div>
              <div class="book-result-checkbox">
                <input
                  type="checkbox"
                  :checked="isBookSelected(book.id)"
                  @click.stop="toggleBookSelection(book)"
                />
              </div>
            </div>
          </div>

          <div
            class="empty-state"
            v-else-if="searchQuery && searchResults.length === 0"
          >
            <div class="empty-state-icon">🕮</div>
            <p>Книги не найдены.</p>
            <p class="small">Попробуйте изменить запрос.</p>
          </div>

          <div class="empty-state" v-else-if="!searchQuery">
            <div class="empty-state-icon">⌕</div>
            <p>Начните поиск книг для добавления в подборку</p>
          </div>
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn-outline" @click="closeForm">
          ✧ Закрыть форму
        </button>
        <button type="submit" class="btn btn-accent">
          ✧ Отправить подборку
        </button>
      </div>
    </form>
  </div>
</template>

<style scoped>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 0 20px;
}

.form-header {
  text-align: center;
  margin-bottom: 30px;
}

.form-header h1 {
  color: var(--accent-color);
  margin-bottom: 10px;
}

.collection-form {
  background: var(--bg);
  padding: 30px;
  border-radius: 8px;
  box-shadow: var(--shadow);
}

.form-group {
  margin-bottom: 25px;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: var(--accent-color);
}

.form-input {
  width: 100%;
  padding: 12px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.character-count {
  text-align: right;
  color: var(--text-light);
  font-size: 0.9rem;
  margin-top: 5px;
}

.selected-books {
  margin-bottom: 20px;
}

.selected-books-list {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 10px;
}

.selected-book {
  display: flex;
  align-items: center;
  gap: 8px;
  background: var(--light-bg);
  padding: 8px 12px;
  border-radius: 20px;
  font-size: 0.9rem;
}

.remove-book {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  background: none;
  border: none;
  color: var(--accent-color);
  cursor: pointer;
  font-size: 1.2rem;
  line-height: 1;
  border-radius: 50%;
  padding: 0;
}

.remove-book:hover {
  background: var(--hover-color);
  color: var(--bg);
}

.book-search {
  margin-bottom: 20px;
}

.search-input {
  position: relative;
}

.search-input input {
  padding-right: 40px;
}

.search-icon {
  position: absolute;
  top: 50%;
  right: 12px;
  transform: translateY(-50%);
  color: var(--text-light);
}

.book-results {
  max-height: 300px;
  overflow-y: auto;
  background: var(--bg);
  border: 1px solid #e0e0e0;
  border-radius: 4px;
}

.book-result-item {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  cursor: pointer;
  transition: background-color 0.2s;
  border-bottom: 1px solid #e0e0e0;
}

.book-result-item:hover {
  background: var(--light-bg);
}

.book-result-item:last-child {
  border-bottom: none;
}

.book-result-cover {
  flex-shrink: 0;
  width: 50px;
  height: 75px;
  object-fit: cover;
  border-radius: 3px;
}

.book-result-info {
  flex: 1;
}

.book-result-title {
  font-weight: 600;
  margin-bottom: 4px;
}

.book-result-author {
  color: var(--text-light);
  font-size: 0.9rem;
  margin-bottom: 4px;
}

.book-result-rating {
  color: var(--hover-color);
  font-size: 0.9rem;
}

.book-result-checkbox {
  margin-left: auto;
}

.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: var(--text-light);
}

.empty-state-icon {
  font-size: 3rem;
  margin-bottom: 15px;
  opacity: 0.5;
}

.form-actions {
  display: flex;
  gap: 15px;
  justify-content: flex-end;
  margin-top: 30px;
}

.form-actions .btn {
  padding: 8px 16px;
}
</style>
