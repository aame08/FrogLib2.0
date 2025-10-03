<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
import { pluralize } from '@/utils/pluralize';
import { getAvatarUrl } from '@/utils/imageUser';
import { formattedDate } from '@/utils/dateUtils';
import useViewHistory from '@/composables/useViewHistory';
import collectionsService from '@/services/collectionsService';
import userWithEntitiesService from '@/services/userWithEntitiesService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import BookCollectionCard from '@/components/cards/BookCollectionCard.vue';
import CommentsSection from '@/components/sectionComponents/CommentsSection.vue';

const { addView } = useViewHistory();
const route = useRoute();
const store = useStore();
const typeEntity = route.meta.entityType;
const collection = ref(null);
const userReaction = ref(-1);
const isLiked = ref(false);
const user = computed(() => store.getters['auth/user']);
const idUser = computed(() => user.value?.idUser || null);
const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);

const loadUserData = async () => {
  if (!isAuthenticated.value || !collection.value) return;

  try {
    const rating = await userWithEntitiesService.getEntityRating(
      idUser.value,
      collection.value.id,
      typeEntity
    );
    const isSaved = await userWithEntitiesService.isCollectionSaved(
      idUser.value,
      collection.value.id
    );

    userReaction.value = rating;
    isLiked.value = isSaved;
  } catch (error) {
    console.error('Ошибка при загрузке пользовательских данных:', error);
  }
};

const getCollectionInfo = async () => {
  try {
    const response = await collectionsService.getCollectionInfo(
      route.params.id
    );
    collection.value = response;

    if (isAuthenticated.value) {
      await loadUserData();
    }
  } catch (error) {
    console.error('Ошибка при загрузке информации о подборке:', error);
  }
};
getCollectionInfo();

const updateReaction = async (newReaction) => {
  try {
    if (userReaction.value === newReaction) {
      await userWithEntitiesService.deleteEntityRating(
        idUser.value,
        collection.value.id,
        typeEntity
      );
      userReaction.value = -1;

      console.log('Реакция на подборку удалена.');
    } else {
      const ratingForm = {
        idUser: idUser.value,
        idEntity: collection.value.id,
        typeEntity: typeEntity,
        rating: newReaction,
      };
      await userWithEntitiesService.updateEntityRating(ratingForm);
      userReaction.value = newReaction;

      console.log('Реакция на подборку изменена.');
    }

    getCollectionInfo();
  } catch (error) {
    console.error('Ошибка при изменении реакции на подборку:', error);
  }
};

const toggleSavedCollection = async () => {
  try {
    if (isLiked.value) {
      await userWithEntitiesService.unlikeCollection(
        idUser.value,
        collection.value.id
      );
      isLiked.value = false;

      console.log('Подборка удалена из избранного.');
    } else {
      await userWithEntitiesService.likeCollection(
        idUser.value,
        collection.value.id
      );
      isLiked.value = true;

      console.log('Подборка добавлена в избранное.');
    }
  } catch (error) {
    console.error('Ошибка при добавлении/удалении подборки:', error);
  }
};

const handleView = async () => {
  const idEntity = route.params.id ? parseInt(route.params.id) : null;

  await addView(idEntity, typeEntity);
  await getCollectionInfo();
  await loadUserData();
};

onMounted(() => {
  getCollectionInfo();
  handleView();
  loadUserData();
});

watch(
  () => store.getters['auth/isAuthenticated'],
  (isAuth) => {
    if (isAuth) {
      handleView();
      getCollectionInfo();
      loadUserData();
    } else {
      userReaction.value = -1;
      isLiked.value = false;
    }
  }
);
</script>

<template>
  <TheHeader />

  <div class="collection-page">
    <div class="container">
      <div class="collection-header">
        <h1 class="collection-title">{{ collection?.title }}</h1>

        <div class="collection-meta">
          <div class="meta-row">
            <div class="collection-author">
              <img
                class="author-avatar"
                :src="getAvatarUrl(collection.userUrl)"
                :alt="collection.userName"
              />
              <div class="author-info">
                <div class="author-name">{{ collection.userName }}</div>
                <div class="text-sm">
                  {{ formattedDate(collection.createdDate) }}
                </div>
              </div>
            </div>

            <div class="collection-stats">
              <div class="stat-item">
                {{ collection.countBooks }}
                {{ pluralize(collection.countBooks, 'книга') }}
              </div>
              <div class="stat-item">
                👁 {{ collection.countView }}
                {{ pluralize(collection.countView, 'просмотр') }}
              </div>
            </div>
          </div>

          <p class="collection-description" v-html="collection.description"></p>

          <div class="action-buttons">
            <div class="rating-info">
              <button
                class="action-btn dislike-btn"
                :class="{ active: userReaction === 0 }"
                title="Не понравилось"
                :disabled="!isAuthenticated"
                @click="updateReaction(0)"
              >
                <span>↓</span>
              </button>
              <span
                class="rating-value"
                :class="{
                  positive: collection.rating?.rating > 0,
                  negative: collection.rating?.rating < 0,
                }"
              >
                {{ collection.rating?.rating || 0 }}
              </span>
              <button
                class="action-btn like-btn"
                :class="{ active: userReaction === 1 }"
                title="Понравилось"
                :disabled="!isAuthenticated"
                @click="updateReaction(1)"
              >
                <span>↑</span>
              </button>
            </div>

            <button
              class="action-btn favorite-btn"
              :class="{ active: isLiked }"
              :disabled="!isAuthenticated || collection.userId === idUser"
              @click="toggleSavedCollection"
            >
              <span>⛉</span>
              <span>{{ isLiked ? 'В избранном' : 'В избранное' }}</span>
            </button>
          </div>
        </div>
      </div>

      <section class="books-section">
        <h2 class="section-title">
          ✧ Книги в подборке: {{ collection.countBooks }} ✧
        </h2>
        <div class="books-grid">
          <BookCollectionCard
            v-for="(book, index) in collection.books"
            :key="index"
            :book="book"
          />
        </div>
      </section>

      <CommentsSection
        :count-comments="collection.countComments"
        :comments="collection.comments"
        @refresh-data="getCollectionInfo"
      />
    </div>
  </div>
</template>

<style scoped>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.collection-page {
  padding: 40px 0;
}

.collection-header {
  margin-bottom: 30px;
  padding: 30px;
  background: var(--bg);
  border-radius: 12px;
  box-shadow: var(--shadow);
}

.collection-title {
  margin-bottom: 20px;
  font-size: 2.5rem;
  line-height: 1.2;
  color: var(--accent-color);
}

.collection-meta {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.meta-row {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  justify-content: space-between;
  align-items: center;
}

.collection-author {
  display: flex;
  align-items: center;
  gap: 12px;
}

.author-avatar {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 50%;
}

.author-info {
  display: flex;
  flex-direction: column;
}

.author-name {
  margin-bottom: 4px;
  font-weight: 600;
  color: var(--dark-text);
}

.text-sm {
  font-size: 0.9rem;
  color: var(--text-light);
}

.collection-stats {
  display: flex;
  gap: 20px;
  font-size: 0.9rem;
  color: var(--text-light);
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 5px;
}

.collection-description {
  margin: 0;
  padding: 15px 0;
  font-size: 1.1rem;
  line-height: 1.7;
  color: var(--dark-text);
  border-top: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
}

.action-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  justify-content: space-between;
  align-items: center;
}

.rating-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.rating-value {
  min-width: 20px;
  font-weight: 600;
  text-align: center;
}

.positive {
  color: var(--green);
}

.negative {
  color: var(--red);
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  font-size: 0.9rem;
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.action-btn:hover:not(:disabled) {
  background: var(--hover-color);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.action-btn.active,
.like-btn.active,
.dislike-btn.active,
.favorite-btn.active {
  background: var(--accent-color);
  color: #ffffff;
  border-color: var(--hover-color);
}

.books-section {
  margin-bottom: 30px;
  padding: 30px;
  background: var(--bg);
  border-radius: 12px;
  box-shadow: var(--shadow);
}

.section-title {
  margin-bottom: 20px;
  padding-bottom: 10px;
  font-size: 1.5rem;
  color: var(--accent-color);
  border-bottom: 2px solid var(--accent-color);
}

.section-title::after {
  display: none !important;
}

.books-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}
</style>
