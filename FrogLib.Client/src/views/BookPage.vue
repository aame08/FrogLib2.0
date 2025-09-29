<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
import { pluralize } from '@/utils/pluralize';
import useViewHistory from '@/composables/useViewHistory';
import booksService from '@/services/booksService';
import userWithBookService from '@/services/userWithBookService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import InfoSection from '@/components/bookComponents/InfoSection.vue';
import EmptySection from '@/components/bookComponents/EmptySection.vue';
import ReviewCard from '@/components/cards/ReviewCard.vue';
import CollectionCard from '@/components/cards/CollectionCard.vue';
import BookCard from '@/components/cards/BookCard.vue';
import ReviewForm from '@/components/generalComponents/ReviewForm.vue';
import CollectionForm from '@/components/generalComponents/CollectionForm.vue';

const { addView } = useViewHistory();
const route = useRoute();
const store = useStore();
const book = ref(null);
const currentUserRating = ref(0);
const selectedUserRating = ref(0);
const hoverRating = ref(null);
const ratingTexts = ['', 'Ужасно', 'Плохо', 'Нормально', 'Хорошо', 'Отлично'];
const listTypes = ['Читаю', 'В планах', 'Брошено', 'Прочитано', 'Любимые'];
const currentListType = ref('');
const selectedListType = ref('');
const activeSection = ref('book');
const isReviewFormVisible = ref(false);
const isCollectionFormVisible = ref(false);
const user = computed(() => store.getters['auth/user']);
const idUser = computed(() => user.value?.idUser || null);
const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);

const loadUserData = async () => {
  if (!isAuthenticated.value || !book.value) return;

  try {
    const rating = await userWithBookService.getBookEvaluation(
      idUser.value,
      book.value.id
    );
    currentUserRating.value = rating || '';
    selectedUserRating.value = currentUserRating.value;

    const userBook = await userWithBookService.getUserBook(
      idUser.value,
      book.value.id
    );
    currentListType.value = userBook;
    selectedListType.value = currentListType.value;
  } catch (error) {
    console.error('Ошибка при загрузке пользовательских данных:', error);
  }
};

const getBookInfo = async () => {
  try {
    const response = await booksService.getBookInfo(route.params.id);
    book.value = response;

    if (isAuthenticated.value) {
      await loadUserData();
    }
  } catch (error) {
    console.error('Ошибка при загрузке информации о книге:', error);
  }
};
getBookInfo();

const setRating = (rating) => {
  selectedUserRating.value = rating;
};

const changeSection = (section) => {
  activeSection.value = section;
};

const ratingText = computed(() => {
  const rating = hoverRating.value || selectedUserRating.value;
  return rating ? ratingTexts[rating] : 'Поставьте оценку';
});

const starRating = computed(() => {
  if (!book.value?.ratingStats?.averageRating) return Array(5).fill('empty');

  const rating = book.value.ratingStats.averageRating;
  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;

  return Array.from({ length: 5 }, (_, i) => {
    if (i < fullStars) return 'full';
    if (i === fullStars && hasHalfStar) return 'half';
    return 'empty';
  });
});

const submitRating = async () => {
  try {
    if (selectedUserRating.value > 0) {
      await userWithBookService.updateBookEvaluation(
        idUser.value,
        book.value.id,
        selectedUserRating.value
      );

      currentUserRating.value = selectedUserRating.value;

      getBookInfo();
    }
  } catch (error) {
    console.error('Ошибка при отправке оценки:', error);
  }
};

const removeRating = async () => {
  try {
    await userWithBookService.deleteBookEvaluation(idUser.value, book.value.id);

    currentUserRating.value = 0;
    selectedUserRating.value = 0;
  } catch (error) {
    console.error('Ошибка при удалении оценки:', error);
  }
};

const updateUserBook = async () => {
  try {
    if (selectedListType.value === 'remove') {
      await userWithBookService.deleteUserBook(idUser.value, book.value.id);

      currentListType.value = '';
      selectedListType.value = '';
    } else if (
      selectedListType.value &&
      selectedListType.value !== currentListType.value
    ) {
      await userWithBookService.updateUserBook(
        idUser.value,
        book.value.id,
        selectedListType.value
      );

      currentListType.value = selectedListType.value;
    }

    getBookInfo();
  } catch (error) {
    console.error('Ошибка при обновлении списка:', error);
  }
};

const handleView = async () => {
  const idEntity = route.params.id ? parseInt(route.params.id) : null;
  const typeEntity = 'Книга';

  await addView(idEntity, typeEntity);
  await getBookInfo();
  await loadUserData();
};

const openForm = (formName) => {
  if (formName === 'review') {
    isReviewFormVisible.value = true;
  } else if (formName === 'collection') {
    isCollectionFormVisible.value = true;
  }
};

const closeForm = (formName) => {
  if (formName === 'review') {
    isReviewFormVisible.value = false;
  } else if (formName === 'collection') {
    isCollectionFormVisible.value = false;
  }
};

onMounted(() => {
  getBookInfo();
  handleView();
  loadUserData();
});

watch(
  () => store.getters['auth/isAuthenticated'],
  (isAuth) => {
    if (isAuth) {
      handleView();
      getBookInfo();
      loadUserData();
    } else {
      currentUserRating.value = 0;
      selectedUserRating.value = 0;
      currentListType.value = '';
      selectedListType.value = '';
    }
  }
);
</script>

<template>
  <TheHeader />
  <div class="book-page">
    <div
      class="container"
      v-if="!isReviewFormVisible && !isCollectionFormVisible"
    >
      <div class="book-layout">
        <div class="book-cover-container">
          <img :src="book.imageURL" :alt="book.title" class="book-cover" />

          <div class="rating-widget">
            <div class="rating-title">Ваша оценка</div>
            <div class="rating-stars">
              <span
                class="rating-star"
                v-for="star in 5"
                :key="star"
                :class="{ active: currentUserRating >= star }"
                @click="setRating(star)"
                @mouseover="hoverRating = star"
                @mouseleave="hoverRating = null"
                >{{
                  hoverRating >= star ||
                  (!hoverRating && selectedUserRating >= star)
                    ? '★'
                    : '☆'
                }}</span
              >
            </div>
            <div class="rating-value">
              {{ ratingText }}
            </div>
            <button
              class="rating-submit"
              :disabled="!isAuthenticated"
              v-if="selectedUserRating !== currentUserRating"
              @click="submitRating"
            >
              ✧
              {{
                currentUserRating > 0 ? 'Изменить оценку' : 'Отправить оценку'
              }}
            </button>
            <button
              v-if="currentUserRating > 0"
              class="rating-submit"
              @click="removeRating"
            >
              ✧ Удалить оценку
            </button>
          </div>

          <div class="book-actions">
            <select
              class="book-select"
              v-model="selectedListType"
              @change="updateUserBook"
            >
              <option value="" disabled>Добавить в список</option>
              <option
                v-if="isAuthenticated"
                v-for="(type, index) in listTypes"
                :key="index"
                :value="type"
                :selected="type === currentListType"
              >
                {{ type }}
              </option>
              <option v-if="currentListType" value="remove">
                Удалить из списка
              </option>
            </select>
            <button
              class="btn btn-accent"
              :disabled="!isAuthenticated"
              @click="openForm('review')"
            >
              ✧ Написать рецензию
            </button>
            <button
              class="btn btn-outline"
              :disabled="!isAuthenticated"
              @click="openForm('collection')"
            >
              ✧ Создать подборку
            </button>
          </div>
        </div>

        <div class="book-info">
          <header class="book-header">
            <h1 class="book-title">{{ book.title }}</h1>
            <p class="book-author">{{ book.authors }}</p>

            <div class="book-meta">
              <div class="meta-item" v-if="book">
                <div class="star-rating">
                  <span
                    v-for="(starType, index) in starRating"
                    :key="index"
                    :class="['star', starType]"
                  >
                    ★
                  </span>
                  <span class="rating-value">
                    {{ book.ratingStats?.averageRating?.toFixed(1) || '0.0' }}
                  </span>
                </div>
              </div>
              <div class="meta-item">
                <span>👁</span>
                <span
                  >{{ book.countView }}
                  {{ pluralize(book.countView, 'просмотр') }}</span
                >
              </div>
            </div>
          </header>

          <div class="book-tabs">
            <button
              class="tab-btn"
              :class="{ active: activeSection === 'book' }"
              @click="changeSection('book')"
            >
              О книге
            </button>
            <button
              class="tab-btn"
              :class="{ active: activeSection === 'reviews' }"
              @click="changeSection('reviews')"
            >
              Рецензии
            </button>
            <button
              class="tab-btn"
              :class="{ active: activeSection === 'collections' }"
              @click="changeSection('collections')"
            >
              Подборки
            </button>
          </div>

          <div v-if="activeSection === 'book'">
            <InfoSection v-bind="book" />
          </div>
          <div v-else-if="activeSection === 'reviews'">
            <EmptySection
              type="reviews"
              :count="book.countReviews"
              :items="book.reviews"
              empty-message="Пока нет рецензий на эту книгу."
              button-text="Написать первую рецензию"
              :card-component="ReviewCard"
              @open-form="openForm"
            />
          </div>
          <div v-else-if="activeSection === 'collections'">
            <EmptySection
              type="collections"
              :count="book.countCollections"
              :items="book.collections"
              empty-message="Эта книга пока не добавлена в подборки."
              button-text="Создать подборку"
              :card-component="CollectionCard"
              @open-form="openForm"
            />
          </div>
        </div>
      </div>

      <section class="similar-books">
        <h3 class="section-title">Похожие книги</h3>
        <div class="books-grid">
          <BookCard
            v-for="(similar, index) in book.similarBooks"
            :key="index"
            :id="similar.id || index"
            :title="similar.title"
            :image-u-r-l="similar.imageURL"
            :average-rating="similar.averageRating"
          />
        </div>
      </section>
    </div>
    <ReviewForm
      v-if="isReviewFormVisible"
      :book="book"
      :user-rating="currentUserRating"
      :id-user="idUser"
      @close="() => closeForm('review')"
      @refresh-data="getBookInfo"
    />
    <CollectionForm
      v-if="isCollectionFormVisible"
      :book="book"
      :id-user="idUser"
      @close="() => closeForm('collection')"
    />
  </div>
</template>

<style scoped>
.book-page {
  padding: 40px 0;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.book-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 40px;
}

.book-cover-container {
  position: relative;
}

.book-cover {
  width: 100%;
  height: auto;
  border-radius: 4px;
  box-shadow: var(--shadow);
  transition: transform 0.3s;
}

.book-cover:hover {
  transform: scale(1.02);
}

.rating-widget {
  margin-top: 15px;
  padding: 15px;
  border-radius: 4px;
  box-shadow: var(--shadow);
  background: var(--light-bg);
}

.rating-title {
  margin-bottom: 10px;
  font-weight: 600;
  color: var(--accent-color);
}

.rating-stars {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
}

.rating-star {
  font-size: 24px;
  cursor: pointer;
  color: var(--light-grey);
  transition: all 0.2s;
}

.rating-star:hover,
.rating-star.active {
  color: var(--hover-color);
  transform: scale(1.2);
}

.rating-value {
  text-align: center;
  font-size: 0.9rem;
  color: var(--text-light);
}

.rating-submit {
  width: 100%;
  padding: 8px;
  margin-top: 10px;
  border: none;
  border-radius: 4px;
  color: var(--bg);
  background: var(--accent-color);
  transition: background 0.2s;
}

.rating-submit:hover {
  background: var(--hover-color);
}

.book-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 20px;
}

.book-actions select {
  padding: 8px 12px;
}

.book-header {
  margin-bottom: 30px;
}

.book-title {
  margin-bottom: 8px;
  font-size: 2.2rem;
  line-height: 1.2;
  color: var(--accent-color);
}

.book-author {
  margin-bottom: 20px;
  font-size: 1.2rem;
  font-style: italic;
  color: var(--dark-text);
}

.book-meta {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.95rem;
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

.book-tabs {
  display: flex;
  margin-bottom: 30px;
  border-bottom: 1px solid #e0e0e0;
}

.tab-btn {
  position: relative;
  padding: 12px 20px;
  border: none;
  cursor: pointer;
  font-weight: 500;
  color: var(--text-light);
  background: none;
  transition: all 0.2s;
}

.tab-btn:hover {
  color: var(--hover-color);
}

.tab-btn.active {
  font-weight: 600;
  color: var(--hover-color);
}

.tab-btn.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background: var(--hover-color);
}

.section {
  margin: 0;
  margin-bottom: 30px;
  padding: 0;
}

.section-title {
  margin-bottom: 15px;
  margin-left: 20px;
  padding-bottom: 8px;
  font-size: 1.4rem;
  display: inline-block;
  border-bottom: 2px solid var(--accent-color);
}

.section-title::after {
  display: none !important;
}

.similar-books {
  margin-top: 50px;
  padding: 30px 0;
  border-radius: 8px;
  box-shadow: var(--shadow);
  background: var(--bg);
}

.books-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 50px;
  padding: 0 20px;
}
</style>
