<script setup>
import { ref, computed } from 'vue';
import { useStore } from 'vuex';
import { watch } from 'vue';
import authService from '@/services/authService';

import TheHeader from '@/components/generalComponents/TheHeader.vue';

const store = useStore();
const user = computed(() => store.getters['auth/user']);

const formData = ref({
  nameUser: user.value?.nameUser || '',
  loginUser: user.value?.loginUser || '',
  oldPassword: '',
  newPassword: '',
  confirmPassword: '',
  profileImage: null,
});
const previewImageUrl = ref(
  user.value?.profileImageUrl
    ? `https://localhost:7295${user.value.profileImageUrl}`
    : null
);
const deleteCurrentImage = ref(false);
const message = ref('');
const errors = ref({});

const handleDeleteImage = () => {
  deleteCurrentImage.value = true;
  formData.value.profileImage = null;
};

const handleFileChange = (e) => {
  formData.value.profileImage = e.target.files[0];
  deleteCurrentImage.value = false;
};

const saveChanges = async () => {
  errors.value = {};
  message.value = '';

  try {
    const formDataObj = new FormData();

    if (formData.value.nameUser !== user.value?.nameUser) {
      formDataObj.append('NameUser', formData.value.nameUser);
    }

    if (formData.value.loginUser !== user.value?.loginUser) {
      formDataObj.append('LoginUser', formData.value.loginUser);
    }

    if (formData.value.newPassword) {
      if (!formData.value.oldPassword) {
        errors.value.oldPassword = 'Введите текущий пароль';
        return;
      }
      if (formData.value.newPassword !== formData.value.confirmPassword) {
        errors.value.confirmPassword = 'Пароли не совпадают';
        return;
      }
      formDataObj.append('OldPassword', formData.value.oldPassword);
      formDataObj.append('NewPassword', formData.value.newPassword);
      formDataObj.append('ConfirmPassword', formData.value.confirmPassword);
    }

    if (deleteCurrentImage.value) {
      formDataObj.append('DeleteImage', 'true');
    } else if (formData.value.profileImage) {
      formDataObj.append('ProfileImageUrl', formData.value.profileImage);
    }

    const response = await authService.updateProfile(
      user.value.idUser,
      formDataObj
    );

    store.commit('auth/setUser', {
      ...user.value,
      nameUser: formData.value.nameUser,
      loginUser: formData.value.loginUser,
      profileImageUrl: response.data.profileImageUrl,
    });

    message.value = 'Профиль успешно обновлен.';
  } catch (error) {
    if (
      error.response &&
      error.response.status === 400 &&
      error.response.data.errors
    ) {
      const serverErrors = error.response.data.errors;
      if (serverErrors.OldPassword) {
        errors.value.password = serverErrors.OldPassword[0];
      }
      if (serverErrors.ConfirmPassword) {
        errors.value.confirmPassword = serverErrors.ConfirmPassword[0];
      }
      if (serverErrors.LoginUser) {
        errors.value.email = serverErrors.LoginUser[0];
      }
      if (serverErrors.ProfileImageUrl) {
        errors.value.image = serverErrors.ProfileImageUrl[0];
      }
    } else {
      message.value = 'Ошибка обновления профиля.';
    }
  }
};

watch(
  () => formData.value.profileImage,
  (newFile) => {
    if (newFile) {
      previewImageUrl.value = URL.createObjectURL(newFile);
    } else if (user.value?.profileImageUrl && !deleteCurrentImage.value) {
      previewImageUrl.value = `https://localhost:7295${user.value.profileImageUrl}`;
    } else {
      previewImageUrl.value = null;
    }
  }
);

watch(deleteCurrentImage, (deleted) => {
  if (deleted) {
    previewImageUrl.value = null;
  }
});
</script>

<template>
  <TheHeader />
  <div class="profile-edit-container">
    <div v-if="message" class="message">{{ message }}</div>
    <h1 class="profile-edit-title">✧ Редактирование профиля ✧</h1>
    <div class="profile-edit-form">
      <div class="avatar-section">
        <div class="avatar-preview">
          <img
            v-if="previewImageUrl"
            :src="previewImageUrl"
            alt="Аватар пользователя"
            class="avatar-image"
          />
          <div v-else class="avatar-placeholder">
            <span>Нет фото</span>
          </div>
        </div>
        <div class="avatar-controls">
          <label
            v-if="!user?.profileImageUrl"
            for="avatar-upload"
            class="upload-button"
          >
            <input
              type="file"
              id="avatar-upload"
              accept="image/*"
              @change="handleFileChange"
              hidden
            />Добавить фото
          </label>
          <button
            type="button"
            class="delete-button"
            @click="handleDeleteImage"
            v-else
          >
            Удалить фото
          </button>
        </div>
        <div v-if="errors.image" class="error-message">
          {{ errors.image }}
        </div>
      </div>

      <div class="form-group">
        <label for="name">Имя</label>
        <input
          type="text"
          id="name"
          v-model="formData.nameUser"
          placeholder="Введите ваше имя"
        />
      </div>
      <div class="form-group">
        <label for="email">Электронная почта</label>
        <input
          type="email"
          id="email"
          v-model="formData.loginUser"
          placeholder="Введите ваш email"
          :class="{ 'input-error': errors.email }"
        />
        <div v-if="errors.email" class="error-message">
          {{ errors.email }}
        </div>
      </div>

      <div class="password-section">
        <h3>Смена пароля</h3>

        <div class="form-group">
          <label for="current-password">Текущий пароль</label>
          <input
            type="password"
            id="current-password"
            v-model="formData.oldPassword"
            placeholder="Введите текущий пароль"
            :class="{ 'input-error': errors.password }"
          />
          <div v-if="errors.password" class="error-message">
            {{ errors.password }}
          </div>
        </div>
        <div class="form-group">
          <label for="new-password">Новый пароль</label>
          <input
            type="password"
            id="new-password"
            v-model="formData.newPassword"
            placeholder="Введите новый пароль"
            :class="{ 'input-error': errors.confirmPassword }"
          />
          <div v-if="errors.confirmPassword" class="error-message">
            {{ errors.confirmPassword }}
          </div>
        </div>
        <div class="form-group">
          <label for="confirm-password">Подтвердите новый пароль</label>
          <input
            type="password"
            id="confirm-password"
            v-model="formData.confirmPassword"
            placeholder="Повторите новый пароль"
          />
        </div>
      </div>

      <div class="form-actions">
        <button class="save-button" @click="saveChanges">
          Сохранить изменения
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.profile-edit-container {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
  background-color: #ffffff;
}

.profile-edit-title {
  text-align: center;
  margin-bottom: 2rem;
  color: var(--dark-text);
}

.profile-edit-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
}

.avatar-preview {
  width: 150px;
  height: 150px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  border-radius: 50%;
  background-color: #f5f5f5;
}

.avatar-image {
  width: 100%;
  height: 100%;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #999999;
}

.avatar-controls {
  display: flex;
  gap: 1rem;
}

.upload-button,
.delete-button {
  padding: 0.5rem 1rem;
  font-size: 0.9rem;
  border-radius: 5px;
  cursor: pointer;
}

.upload-button {
  background-color: var(--accent-color);
  color: #ffffff;
  border: none;
}

.delete-button {
  background-color: #f8f8f8;
  border: 1px solid #ddd;
  color: #dc143c;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 500;
  color: var(--dark-text);
}

.form-group input {
  padding: 0.75rem;
  font-size: 1rem;
}

.password-section h3 {
  margin-bottom: 1rem;
  color: var(--accent-color);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
}

.save-button {
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
  background-color: var(--accent-color);
  color: #ffffff;
  border: none;
  border-radius: 4px;
}

.cancel-button {
  padding: 0.75rem 1.5rem;
  font-size: 1rem;
  background-color: #f8f8f8;
  border: 1px solid #dddddd;
  border-radius: 4px;
}
</style>
