<script setup>
import { ref, computed } from 'vue';
import { useRoute } from 'vue-router';
import { useStore } from 'vuex';
import userWithEntitiesService from '@/services/userWithEntitiesService';

import CommentCard from '../cards/CommentCard.vue';

const props = defineProps({
  countComments: { type: Number, required: true },
  comments: { type: Array, required: true },
});

const emit = defineEmits(['refresh-data']);

const route = useRoute();
const store = useStore();
const typeEntity = route.meta.entityType;
const idEntity = route.params.id;
const selectedSort = ref('newest');
const newComment = ref('');
const user = computed(() => store.getters['auth/user']);
const idUser = computed(() => user.value?.idUser || null);
const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);

const addComment = async () => {
  try {
    await userWithEntitiesService.addComment(
      idUser.value,
      idEntity,
      typeEntity,
      newComment.value
    );
    emit('refresh-data');

    newComment.value = '';

    console.log('Комментарий опубликован.');
  } catch (error) {
    console.error('Ошибка при отправке комментария:', error);
  }
};

const filteredComments = computed(() => {
  let result = [...props.comments];

  switch (selectedSort.value) {
    case 'newest':
      result.sort((a, b) => new Date(b.date) - new Date(a.date));
      break;
    case 'oldest':
      result.sort((a, b) => new Date(a.date) - new Date(b.date));
      break;
    case 'popular':
      result.sort((a, b) => b.rating.rating - a.rating.rating);
      break;
  }

  return result;
});

const handleRefresh = (event) => {
  emit('refresh-data');
};
</script>

<template>
  <section class="comments-section">
    <div class="comments-header">
      <h2 class="section-title">✧ Комментарии ({{ countComments }}) ✧</h2>

      <div class="comments-sort">
        <!-- <label class="text-sm">Сортировка:</label> -->
        <select class="sort-select" v-model="selectedSort">
          <option value="newest">Сначала новые</option>
          <option value="oldest">Сначала старые</option>
          <option value="popular">По популярности</option>
        </select>
      </div>
    </div>

    <div class="new-comment">
      <form class="comment-form" @submit.prevent="addComment">
        <textarea
          class="comment-input"
          placeholder="Напишите ваш комментарий..."
          v-model="newComment"
          maxlength="1000"
        ></textarea>
        <div class="comment-actions">
          <div class="char-count">{{ newComment.length }}/1000</div>
          <button
            type="submit"
            class="btn btn-primary"
            :disabled="!newComment.trim() || !isAuthenticated"
          >
            Отправить
          </button>
        </div>
      </form>
    </div>

    <div class="comments-list">
      <CommentCard
        v-for="comment in filteredComments"
        :key="comment.id"
        :comment="comment"
        @refresh-data="handleRefresh"
      />
    </div>
  </section>
</template>

<style scoped>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.comments-section {
  padding: 30px;
  background: var(--bg);
  border-radius: 12px;
  box-shadow: var(--shadow);
}

.comments-header {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  border-bottom: 2px solid var(--accent-color);
}

.section-title {
  padding-bottom: 10px;
  font-size: 1.5rem;
  color: var(--accent-color);
}

.section-title::after {
  display: none !important;
}

.comments-sort {
  display: flex;
  align-items: center;
  gap: 10px;
}

.sort-select {
  padding: 8px 12px;
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 4px;
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

.btn {
  padding: 8px 16px;
  font-weight: 500;
  border: none;
  border-radius: 4px;
  transition: all 0.2s;
}

.btn-primary {
  background: var(--accent-color);
  color: #ffffff;
}

.btn-primary:hover {
  background: var(--hover-color);
}

.comments-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
</style>
