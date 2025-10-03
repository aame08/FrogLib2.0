<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
import { pluralize } from '@/utils/pluralize';
import { getAvatarUrl } from '@/utils/imageUser';
import { formattedDate } from '@/utils/dateUtils';
import { truncateText } from '@/utils/truncateText';
import useViewHistory from '@/composables/useViewHistory';
import reviewsService from '@/services/reviewsService';
import userWithEntitiesService from '@/services/userWithEntitiesService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';
import CommentsSection from '@/components/sectionComponents/CommentsSection.vue';

const { addView } = useViewHistory();
const route = useRoute();
const store = useStore();
const typeEntity = route.meta.entityType;
const review = ref(null);
const userReaction = ref(-1);
const user = computed(() => store.getters['auth/user']);
const idUser = computed(() => user.value?.idUser || null);
const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);

const loadUserData = async () => {
  if (!isAuthenticated.value || !review.value) return;

  try {
    const rating = await userWithEntitiesService.getEntityRating(
      idUser.value,
      review.value.id,
      typeEntity
    );

    userReaction.value = rating;
  } catch (error) {
    console.error('Ошибка при загрузке пользовательских данных:', error);
  }
};

const getReviewInfo = async () => {
  try {
    const response = await reviewsService.getReviewInfo(route.params.id);
    review.value = response;

    if (isAuthenticated.value) {
      await loadUserData();
    }
  } catch (error) {
    console.error('Ошибка при загрузке информации о рецензии:', error);
  }
};
getReviewInfo();

const updateReaction = async (newReaction) => {
  try {
    if (userReaction.value === newReaction) {
      await userWithEntitiesService.deleteEntityRating(
        idUser.value,
        review.value.id,
        typeEntity
      );
      userReaction.value = -1;

      console.log('Реакция на рецензию удалена.');
    } else {
      const ratingForm = {
        idUser: idUser.value,
        idEntity: review.value.id,
        typeEntity: typeEntity,
        rating: newReaction,
      };
      await userWithEntitiesService.updateEntityRating(ratingForm);
      userReaction.value = newReaction;

      console.log('Реакция на рецензию изменена.');
    }

    getReviewInfo();
  } catch (error) {
    console.error('Ошибка при изменении реакции на рецензию:', error);
  }
};

const handleView = async () => {
  const idEntity = route.params.id ? parseInt(route.params.id) : null;

  await addView(idEntity, typeEntity);
  await getReviewInfo();
  await loadUserData();
};

const truncatedDescription = computed(() => {
  return truncateText(review.value.book.description, 300);
});

onMounted(() => {
  getReviewInfo();
  handleView();
  loadUserData();
});

watch(
  () => store.getters['auth/isAuthenticated'],
  (isAuth) => {
    if (isAuth) {
      handleView();
      getReviewInfo();
      loadUserData();
    } else {
      userReaction.value = -1;
    }
  }
);
</script>

<template>
  <TheHeader />

  <div class="review-page">
    <div class="container">
      <div class="review-header">
        <h1 class="review-title">{{ review.title }}</h1>

        <div class="review-meta">
          <div class="author-info">
            <img
              :src="getAvatarUrl(review.userUrl)"
              :alt="review.userName"
              class="author-avatar"
            />
            <div class="author-details">
              <h4>{{ review.userName }}</h4>
              <div class="review-date">
                {{ formattedDate(review.createdDate) }}
              </div>
            </div>
          </div>

          <div class="review-rating">
            <span
              v-for="i in 5"
              :key="i"
              :class="{ filled: i <= review.userEvaluation }"
              >★</span
            >
            <span>{{ review.userEvaluation }}</span>
          </div>
        </div>

        <div class="book-info">
          <img
            :src="review.book.imageURL"
            :alt="review.book.title"
            class="book-cover"
          />
          <div class="book-details">
            <RouterLink :to="`/books/${review.book.id}`" class="book-title">{{
              review.book.title
            }}</RouterLink>
            <div class="book-authors">{{ review.book.authors }}</div>
            <div class="book-rating">
              <span>★ {{ review.book.averageRating.toFixed(1) }}</span>
            </div>
            <p class="book-description">{{ truncatedDescription }}</p>
          </div>
        </div>
      </div>

      <div class="review-content">
        <div class="review-text" v-html="review.text"></div>

        <div class="review-stats">
          <div class="views-count">
            <span>👁</span>
            <span
              >{{ review.countView }}
              {{ pluralize(review.countView, 'просмотр') }}</span
            >
          </div>

          <div class="reaction-buttons">
            <button
              class="reaction-btn dislike"
              :class="{ active: userReaction === 0 }"
              @click="updateReaction(0)"
              title="Не понравилось"
              :disabled="!isAuthenticated || review.userId === idUser"
            >
              <span>↓</span>
            </button>
            <span
              class="rating-value"
              :class="{
                positive: review.rating?.rating > 0,
                negative: review.rating?.rating < 0,
              }"
              :title="`${review.rating?.likes || 0} ${pluralize(
                review.rating?.likes || 0,
                'лайк'
              )}, ${review.rating?.dislikes || 0} ${pluralize(
                review.rating?.dislikes || 0,
                'дизлайк'
              )}`"
            >
              {{ review.rating?.rating || 0 }}
            </span>
            <button
              class="reaction-btn"
              :class="{ active: userReaction === 1 }"
              @click="updateReaction(1)"
              title="Понравилось"
              :disabled="!isAuthenticated || review.userId === idUser"
            >
              <span>↑</span>
            </button>
          </div>
        </div>
      </div>

      <CommentsSection
        :count-comments="review.countComments"
        :comments="review.comments"
        @refresh-data="getReviewInfo"
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

.review-page {
  padding: 40px 0;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.review-header {
  margin-bottom: 30px;
  padding: 30px;
  background: var(--bg);
  border-radius: 8px;
  box-shadow: 0 2px 8px #0000001a;
}

.review-title {
  margin-bottom: 15px;
  font-size: 2.2rem;
  line-height: 1.3;
  color: var(--accent-color);
}

.review-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid var(--border);
}

.author-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.author-avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
}

.author-details h4 {
  margin-bottom: 4px;
  color: var(--dark-text);
}

.review-date {
  font-size: 0.9rem;
  color: var(--text-light);
}

.review-rating {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 1.2rem;
}

.review-rating .filled {
  color: var(--accent-color);
}

.book-info {
  display: flex;
  gap: 20px;
  padding: 20px;
  background: var(--light-bg);
  border-radius: 6px;
}

.book-cover {
  width: 120px;
  height: 180px;
  object-fit: cover;
  border-radius: 4px;
  box-shadow: 0 2px 8px #0000001a;
}

.book-details {
  flex: 1;
}

.book-title {
  margin-bottom: 8px;
  font-size: 1.4rem;
  font-weight: 600;
  letter-spacing: 1px;
  color: var(--accent-color);
}

.book-title:hover {
  color: var(--dark-text);
  text-decoration: underline;
  text-decoration-color: var(--hover-color);
}

.book-authors {
  margin-bottom: 10px;
  font-style: italic;
  color: var(--text-light);
}

.book-rating {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 10px;
  color: var(--text-light);
}

.book-description {
  line-height: 1.5;
  color: var(--dark-text);
}

.review-content {
  margin-bottom: 30px;
  padding: 30px;
  background: var(--bg);
  border-radius: 8px;
  box-shadow: 0 2px 8px #0000001a;
}

.review-text {
  margin-bottom: 25px;
  font-size: 1.1rem;
  line-height: 1.7;
}

.review-text p {
  margin-bottom: 15px;
}

.review-stats {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 20px;
  border-top: 1px solid var(--border);
}

.views-count {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-light);
}

.reaction-buttons {
  display: flex;
  align-items: center;
  gap: 8px;
}

.reaction-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  background: var(--light-bg);
  border: 1px solid var(--border);
  border-radius: 20px;
  transition: all 0.2s;
}

.reaction-btn:hover {
  background: var(--hover-color);
  transform: translateY(-2px);
}

.reaction-btn.active,
.reaction-btn.dislike.active {
  background: var(--accent-color);
  color: #ffffff;
  border-color: var(--hover-color);
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
</style>
