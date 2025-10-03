<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
import { getAvatarUrl } from '@/utils/imageUser';
import dayjs from 'dayjs';
import 'dayjs/locale/ru';
import userWithEntitiesService from '@/services/userWithEntitiesService';

const props = defineProps({
  comment: { type: Object, required: true },
});

const emit = defineEmits(['refresh-data']);

const route = useRoute();
const store = useStore();
const typeEntity = route.meta.entityType;
const idEntity = route.params.id;
const entityComment = 'Комментарий';
const userReaction = ref(-1);
const replyText = ref('');
const activeReply = ref(null);
const user = computed(() => store.getters['auth/user']);
const idUser = computed(() => user.value?.idUser || null);
const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);

const formattedDate = (date) => {
  return dayjs(date).isValid()
    ? dayjs(date).format('DD MMMM YYYY в HH:mm')
    : 'Неверный формат даты';
};

const loadUserData = async () => {
  if (!isAuthenticated.value || !props.comment) return;

  try {
    const rating = await userWithEntitiesService.getEntityRating(
      idUser.value,
      props.comment.id,
      entityComment
    );

    userReaction.value = rating;
  } catch (error) {
    console.error('Ошибка при загрузке пользовательских данных:', error);
  }
};
loadUserData();

const updateReaction = async (newReaction) => {
  try {
    if (userReaction.value === newReaction) {
      await userWithEntitiesService.deleteEntityRating(
        idUser.value,
        props.comment.id,
        entityComment
      );
      userReaction.value = -1;

      console.log('Реакция на комментарий удалена.');

      emit('refresh-data');
    } else {
      const ratingForm = {
        idUser: idUser.value,
        idEntity: props.comment.id,
        typeEntity: entityComment,
        rating: newReaction,
      };

      await userWithEntitiesService.updateEntityRating(ratingForm);
      userReaction.value = newReaction;

      console.log('Реакция на комментарий изменена.');

      emit('refresh-data');
    }
  } catch (error) {
    console.error('Ошибка при изменении реакции на комментарий:', error);
  }
};

const toggleReply = (parentId) => {
  activeReply.value = activeReply.value === parentId ? null : parentId;
  replyText.value = '';
};

const addReply = async (parentId) => {
  try {
    const commentForm = {
      idUser: idUser.value,
      idEntity: idEntity,
      typeEntity: typeEntity,
      text: replyText.value,
      idParentComment: parentId,
    };

    await userWithEntitiesService.addReply(commentForm);
    emit('refresh-data');

    replyText.value = '';
    activeReply.value = null;

    console.log('Ответ на комментарий опубликован.');
  } catch (error) {
    console.error('Ошибка при отправке ответа на комментарий:', error);
  }
};

const handleRefresh = () => {
  emit('refresh-data');
};

onMounted(() => {
  loadUserData();
});

watch(
  () => store.getters['auth/isAuthenticated'],
  (isAuth) => {
    if (isAuth) {
      loadUserData();
    } else {
      userReaction.value = -1;
    }
  }
);
</script>

<template>
  <div
    :class="[
      'comment',
      {
        'reply-comment': comment.isReply,
        violation: comment.status === 'Обнаружено нарушение',
      },
    ]"
  >
    <div class="comment-header">
      <div class="comment-author">
        <img
          :src="getAvatarUrl(comment.authorURL)"
          :alt="comment.authorName"
          class="comment-avatar"
        />
        <div class="comment-meta">
          <div class="comment-author-name">
            {{ comment.authorName }}
            <span
              v-if="comment.status === 'Обнаружено нарушение'"
              class="comment-violation-badge"
              >Нарушение</span
            >
          </div>
          <div class="comment-date">{{ formattedDate(comment.date) }}</div>
        </div>
      </div>
    </div>

    <div class="comment-content" v-html="comment.text"></div>

    <div class="comment-actions">
      <button
        class="comment-action-btn"
        :class="{ active: userReaction === 0 }"
        title="Не понравилось"
        @click="updateReaction(0)"
        :disabled="!isAuthenticated || comment.authorId == idUser"
      >
        <span>↓</span><span>{{ comment.rating.dislikes }}</span>
      </button>

      <button
        class="comment-action-btn"
        title="Понравилось"
        :class="{ active: userReaction === 1 }"
        @click="updateReaction(1)"
        :disabled="!isAuthenticated || comment.authorId == idUser"
      >
        <span>↑</span><span>{{ comment.rating.likes }}</span>
      </button>

      <button
        class="comment-action-btn reply-btn"
        @click="toggleReply(comment.id)"
        :disabled="!isAuthenticated"
      >
        <span>💬</span><span>Ответить</span>
      </button>
    </div>

    <div v-if="activeReply === comment.id" class="new-comment">
      <form class="comment-form" @submit.prevent="addReply(comment.id)">
        <textarea
          class="comment-input"
          placeholder="Напишите ответ..."
          v-model="replyText"
          maxlength="1000"
        ></textarea>
        <div class="comment-actions">
          <div class="char-count">{{ replyText.length }}/1000</div>
          <div class="buttons-group">
            <button
              type="button"
              class="btn btn-outline"
              @click="activeReply = null"
            >
              Отмена
            </button>
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="!replyText.trim() || !isAuthenticated"
            >
              Ответить
            </button>
          </div>
        </div>
      </form>
    </div>

    <div v-if="comment.replies && comment.replies.length > 0" class="replies">
      <CommentCard
        v-for="reply in comment.replies"
        :key="reply.id"
        :comment="reply"
        @refresh-data="handleRefresh"
      />
    </div>
  </div>
</template>

<style scoped>
.comment {
  padding: 20px;
  background: var(--light-bg);
  border-radius: 8px;
  border-left: 4px solid transparent;
}

.violation {
  background: var(--light-red);
  border-left-color: var(--dark-red);
}

.comment-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 10px;
}

.comment-author {
  display: flex;
  align-items: center;
  gap: 10px;
}

.comment-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  object-fit: cover;
}

.comment-meta {
  display: flex;
  flex-direction: column;
}

.comment-author-name {
  font-weight: 600;
}

.comment-date {
  font-size: 0.8rem;
  color: var(--text-light);
}

.comment-violation-badge {
  padding: 2px 8px;
  margin-left: 10px;
  font-size: 0.7rem;
  font-weight: 600;
  background: var(--red);
  color: #ffffff;
  border-radius: 12px;
}

.comment-content {
  margin-bottom: 15px;
  line-height: 1.6;
}

.comment-actions {
  display: flex;
  align-items: center;
  gap: 15px;
}

.comment-action-btn {
  display: flex;
  align-items: center;
  gap: 5px;
  background: none;
  border: none;
  font-size: 0.9rem;
  color: var(--text-light);
  transition: color 0.2s;
}

.comment-action-btn:hover span {
  color: var(--accent-color);
}

.comment-action-btn.active span {
  color: var(--accent-color);
}

.reply-btn {
  margin-left: auto;
}

.new-comment {
  padding: 20px;
  margin-bottom: 30px;
  background: var(--light-bg);
  border-radius: 8px;
}

.comment-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.comment-input {
  width: 100%;
  min-height: 100px;
  padding: 15px;
  font-size: 1rem;
  border: 1px solid var(--border);
  border-radius: 6px;
}

.comment-input:focus {
  outline: none;
  border-color: var(--accent-color);
}

.comment-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.char-count {
  font-size: 0.9rem;
  color: var(--text-light);
}

.buttons-group {
  display: flex;
  gap: 5px;
}

.btn {
  padding: 8px 16px;
  font-weight: 500;
  border: none;
  border-radius: 4px;
  transition: all 0.2s;
}

.btn-outline {
  background: transparent;
  border: 1px solid var(--accent-color);
  color: var(--accent-color);
}

.btn-outline:hover {
  background: var(--light-bg);
}

.btn-primary {
  background: var(--accent-color);
  color: #ffffff;
}

.btn-primary:hover {
  background: var(--hover-color);
}

.replies {
  margin-top: 15px;
  margin-left: 40px;
  padding-left: 20px;
  border-left: 2px solid var(--border);
}
</style>
