<!-- СДЕЛАТЬ РЕДАКТИРОВАНИЕ -->

<script setup>
import { ref } from 'vue';
import { QuillEditor } from '@vueup/vue-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css';
import userWithBookService from '@/services/userWithBookService';
import userWithEntitiesService from '@/services/userWithEntitiesService';

const props = defineProps({
  book: { type: Object, required: true },
  userRating: { type: Number, default: 0 },
  idUser: { type: Number, default: 0 },
});

const emit = defineEmits(['refresh-data', 'close']);

const editorOptions = ref({
  theme: 'snow',
  placeholder: 'Напишите вашу рецензию здесь...',
  modules: {
    toolbar: [
      ['bold', 'italic', 'underline'],
      [{ list: 'ordered' }, { list: 'bullet' }],
    ],
  },
});

const hoverRating = ref(0);
const selectedRating = ref(props.userRating);
const reviewTitle = ref('');
const reviewContent = ref('');
const errors = ref({});

const setRating = (stars) => {
  selectedRating.value = stars;
};

const submitReview = async () => {
  errors.value = {};
  let isValid = true;

  if (!reviewTitle.value.trim()) {
    errors.value.titleReview = 'Заголовок рецензии не может быть пустым.';
    isValid = false;
  }

  if (!reviewContent.value.trim()) {
    errors.value.textReview = 'Текст рецензии не может быть пустым.';
    isValid = false;
  }

  if (selectedRating.value === 0) {
    errors.value.evaluationBook = 'Оценка не может быть пустой.';
    isValid = false;
  }

  if (!isValid) {
    return;
  }

  try {
    const reviewForm = {
      idUser: props.idUser,
      idBook: props.book.id,
      titleReview: reviewTitle.value,
      contentReview: reviewContent.value,
      plaintText: reviewContent.value.replace(/<[^>]*>/g, ''),
    };

    await userWithBookService.updateBookEvaluation(
      props.idUser,
      props.book.id,
      selectedRating.value
    );

    await userWithEntitiesService.addReview(reviewForm);

    emit('refresh-data');
    emit('close');
  } catch (error) {
    console.error('Ошибка при отправке рецензии:', error);
  }
};

const closeForm = () => {
  emit('close');
};
</script>

<template>
  <div class="container">
    <div class="form-header">
      <h1>Рецензия на книгу</h1>
      <p>Поделитесь своими впечатлениями о книге</p>
    </div>

    <div class="book-info">
      <div
        class="blurred-background"
        :style="{ backgroundImage: `url(${book.imageURL})` }"
      ></div>
      <div class="book-container">
        <div class="content-wrapper">
          <img class="book-image" :src="book.imageURL" :alt="book.title" />
          <div class="text-content">
            <h1 class="book-title">{{ book.title }}</h1>
            <p class="book-author">{{ book.authors }}</p>
            <p class="rating">
              ☆ {{ book.ratingStats?.averageRating?.toFixed(1) || '0.0' }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <div class="rating-section">
      <h3 class="rating-title">Ваша оценка</h3>
      <div class="star-rating">
        <span
          v-for="star in 5"
          :key="star"
          class="star"
          :class="{ active: star <= selectedRating }"
          @click="setRating(star)"
          @mouseover="hoverRating = star"
          @mouseleave="hoverRating = 0"
          >★</span
        >
      </div>
      <div class="rating-value">Оценка: {{ selectedRating }} из 5</div>
      <div v-if="errors.evaluationBook" class="error-message">
        {{ errors.evaluationBook }}
      </div>
    </div>

    <form class="review-form" @submit.prevent="submitReview">
      <div class="form-group">
        <label class="form-label">Заголовок рецензии</label>
        <input
          type="text"
          class="form-input"
          placeholder="Введите заголовок..."
          v-model="reviewTitle"
          :class="{ 'input-error': errors.titleReview }"
        />
        <div
          class="character-count"
          :style="{
            color:
              reviewTitle.length > 255 ? 'var(--red)' : 'var(--text-light)',
          }"
        >
          {{ reviewTitle.length }}/255 символов
        </div>
        <div v-if="errors.titleReview" class="error-message">
          {{ errors.titleReview }}
        </div>
      </div>

      <div class="form-group">
        <label class="form-label">Текст рецензии</label>
        <QuillEditor
          v-model:content="reviewContent"
          contentType="html"
          :options="editorOptions"
        />
        <div v-if="errors.textReview" class="error-message">
          {{ errors.textReview }}
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn-outline" @click="closeForm">
          ✧ Закрыть форму
        </button>
        <button type="submit" class="btn btn-accent">
          ✧ Отправить рецензию
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
  max-width: 800px;
  margin: 0 auto;
  padding: 0 20px;
}

.form-header {
  margin-bottom: 30px;
  text-align: center;
}

.form-header h1 {
  margin-bottom: 10px;
  color: var(--accent-color);
}

.book-info {
  position: relative;
  display: flex;
  align-self: center;
  padding: 10px;
  border-radius: 5px;
}

.blurred-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
  filter: blur(10px);
  z-index: 1;
}

.book-container {
  position: relative;
  z-index: 2;
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
}

.content-wrapper {
  display: flex;
  align-items: center;
  gap: 30px;
  padding: 20px;
  border-radius: 8px;
  background-color: #ffffff4d;
  backdrop-filter: blur(5px);
}

.book-image {
  height: 250px;
}

.text-content {
  color: var(--bg);
  text-shadow: 0 2px 4px #00000080;
}

.book-title {
  margin: 0;
  max-width: 600px;
}

.book-author {
  margin-bottom: 20px;
  font-size: 20px;
}

.rating {
  font-size: 20px;
  font-weight: 600;
}

.rating-section {
  margin-top: 25px;
  margin-bottom: 20px;
  padding: 20px;
  border-radius: 8px;
  background: var(--bg);
  box-shadow: var(--shadow);
}

.rating-title {
  margin-bottom: 15px;
  color: var(--accent-color);
}

.star-rating {
  display: flex;
  gap: 5px;
  margin-bottom: 10px;
}

.star {
  font-size: 2rem;
  cursor: pointer;
  color: #dddddd;
  transition: color 0.2s;
}

.star:hover,
.star.active {
  color: var(--hover-color);
}

.rating-value {
  font-weight: 600;
  color: var(--hover-color);
}

.review-form {
  padding: 30px;
  border-radius: 8px;
  background: var(--bg);
  box-shadow: var(--shadow);
}

.form-group {
  margin-bottom: 20px;
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
  margin-top: 5px;
  text-align: right;
  font-size: 0.9rem;
  color: var(--text-light);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  margin-top: 30px;
}

.form-actions .btn {
  padding: 8px 16px;
}
</style>
