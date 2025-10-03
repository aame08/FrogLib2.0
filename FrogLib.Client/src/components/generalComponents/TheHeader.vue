<script setup>
import { ref, onMounted, computed } from 'vue';
import { useStore } from 'vuex';
import { useRoute } from 'vue-router';
import { useRouter } from 'vue-router';
import authService from '@/services/authService';
import { getAvatarUrl } from '@/utils/imageUser';

import frogLogo from '@/assets/frog.png';
import frog2Logo from '@/assets/frog2.png';

const isPinkTheme = ref(false);
const logoText = ref('FrogLib');
const showLoginModal = ref(false);
const showRegisterModal = ref(false);
const showForgotPasswordModal = ref(false);
const isClosing = ref(false);
const message = ref('');
const errors = ref({});
const showUserMenu = ref(false);

const store = useStore();
const route = useRoute();
const router = useRouter();

const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);
const user = computed(() => store.getters['auth/user']);
const userRole = computed(() => user.value?.nameRole || '');

const loginForm = ref({
  email: '',
  password: '',
});

const registerForm = ref({
  nameUser: '',
  loginUser: '',
  newPassword: '',
  confirmPassword: '',
  profileImageUrl: null,
});

const forgotPasswordForm = ref({
  email: '',
});

const toggleTheme = () => {
  isPinkTheme.value = !isPinkTheme.value;

  if (isPinkTheme.value) {
    document.body.classList.add('pink-theme');
    logoText.value = 'PickMeLib';
    document.title = 'PickMeLib';
    document.querySelector('link[rel="icon"]').href = '/frog2.ico';
  } else {
    document.body.classList.remove('pink-theme');
    logoText.value = 'FrogLib';
    document.title = 'FrogLib';
    document.querySelector('link[rel="icon"]').href = '/frog.ico';
  }

  localStorage.setItem('theme', isPinkTheme.value ? 'pink' : 'green');
};

const handleAvatarUpload = (event) => {
  const file = event.target.files[0];
  if (file) {
    registerForm.value.profileImageUrl = file;
  }
};

const submitLogin = async () => {
  errors.value = {};

  let isValid = true;

  if (loginForm.value.email === '') {
    errors.value.email = 'Электронная почта не может быть пустой.';
    isValid = false;
  }
  if (loginForm.value.password === '') {
    errors.value.password = 'Пароль не может быть пустым.';
    isValid = false;
  }

  if (!isValid) {
    return;
  }

  try {
    const response = await authService.login(
      loginForm.value.email,
      loginForm.value.password
    );

    store.commit('auth/setTokens', {
      accessToken: response.data.accessToken,
      refreshToken: response.data.refreshToken,
    });

    store.commit('auth/setUser', response.data.user);

    message.value = 'С возвращением.';

    loginForm.value.email = '';
    loginForm.value.password = '';
  } catch (error) {
    if (
      error.response &&
      error.response.status === 400 &&
      error.response.data.errors
    ) {
      const serverErrors = error.response.data.errors;
      if (serverErrors.Unauthorized) {
        errors.value.email = serverErrors.Unauthorized[0];
      }
      if (serverErrors.InvalidPassword) {
        errors.value.password = serverErrors.InvalidPassword[0];
      }
      if (serverErrors.BannedUser) {
        errors.value.ban = serverErrors.BannedUser[0];
      }
    } else {
      message.value = 'Ошибка аутентификации.';
    }
  }
};

const submitRegister = async () => {
  errors.value = {};

  let isValid = true;

  if (registerForm.value.newPassword !== registerForm.value.confirmPassword) {
    errors.value.confirmPassword = 'Пароли не совпадают.';
    isValid = false;
  }
  if (registerForm.value.nameUser === '') {
    errors.value.username = 'Имя пользователя не может быть пустым.';
    isValid = false;
  }
  if (registerForm.value.loginUser === '') {
    errors.value.email = 'Электронная почта не может быть пустой.';
    isValid = false;
  }
  if (registerForm.value.newPassword === '') {
    errors.value.password = 'Пароль не может быть пустым.';
    isValid = false;
  }

  if (registerForm.value.newPassword.length < 8) {
    errors.value.password = 'Пароль должен состоять от 8 символов.';
    isValid = false;
  }

  if (!isValid) {
    return;
  }

  try {
    const formData = new FormData();
    formData.append('NameUser', registerForm.value.nameUser);
    formData.append('LoginUser', registerForm.value.loginUser);
    formData.append('NewPassword', registerForm.value.newPassword);
    formData.append('ConfirmPassword', registerForm.value.confirmPassword);

    if (registerForm.value.profileImageUrl) {
      formData.append('ProfileImageUrl', registerForm.value.profileImageUrl);
    }

    const response = await authService.register(formData);

    if (response.status === 201) {
      message.value = 'Регистрация прошла успешно.';

      registerForm.value.nameUser = '';
      registerForm.value.loginUser = '';
      registerForm.value.newPassword = '';
      registerForm.value.confirmPassword = '';
      registerForm.value.profileImageUrl = null;
    }
  } catch (error) {
    if (
      error.response &&
      error.response.status === 400 &&
      error.response.data.errors
    ) {
      const serverErrors = error.response.data.errors;
      if (serverErrors.NameUser) {
        errors.value.username = serverErrors.NameUser[0];
      }
      if (serverErrors.LoginUser) {
        errors.value.email = serverErrors.LoginUser[0];
      }
      if (serverErrors.ConfirmPassword) {
        errors.value.confirmPassword = serverErrors.ConfirmPassword[0];
      }
    } else {
      message.value = 'Ошибка регистрации.';
    }
  }
};

const submitForgotPassword = async () => {
  errors.value = {};

  if (forgotPasswordForm.value.email === '') {
    errors.value.email = 'Введите электронную почту.';
    return;
  }

  try {
    const response = await authService.resetPassword(
      forgotPasswordForm.value.email
    );

    message.value =
      response.data.message || 'Новый пароль отправлен на электронную почту.';

    forgotPasswordForm.value.email = '';
  } catch (error) {
    if (error.response) {
      if (error.response.data?.errors) {
        errors.value = error.response.data.errors;
      } else {
        message.value =
          error.response.data?.message || 'Произошла ошибка при сбросе пароля';
      }
    } else {
      message.value = 'Не удалось подключиться к серверу';
    }
    console.error('Ошибка сброса пароля:', error);
  }
};

const switchToLogin = () => {
  message.value = '';
  showRegisterModal.value = false;
  showForgotPasswordModal.value = false;
  setTimeout(() => (showLoginModal.value = true), 300);
};

const switchToRegister = () => {
  message.value = '';
  showLoginModal.value = false;
  setTimeout(() => (showRegisterModal.value = true), 300);
};

const openForgotPassword = () => {
  message.value = '';
  showLoginModal.value = false;
  setTimeout(() => (showForgotPasswordModal.value = true), 300);
};

const closeModal = (modalType) => {
  isClosing.value = true;
  setTimeout(() => {
    if (modalType === 'login') showLoginModal.value = false;
    if (modalType === 'register') showRegisterModal.value = false;
    if (modalType === 'forgot') showForgotPasswordModal.value = false;
    isClosing.value = false;
  }, 300);
};

onMounted(() => {
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'pink') {
    isPinkTheme.value = true;
    document.body.classList.add('pink-theme');
    logoText.value = 'PickMeLib';
    document.title = 'PickMeLib';
    document.querySelector('link[rel="icon"]').href = '/frog2.ico';
  }
});

const logout = () => {
  message.value = '';
  store.dispatch('auth/logout');
  router.push('/');
};
</script>

<template>
  <div class="header">
    <div class="logo">
      <img :src="isPinkTheme ? frog2Logo : frogLogo" alt="logo" />
      <RouterLink to="/">{{ logoText }}</RouterLink>
    </div>

    <div class="nav-container">
      <div class="nav-links">
        <template v-if="!isAuthenticated || userRole === 'Пользователь'">
          <RouterLink to="/">Главная</RouterLink>
          <RouterLink to="/books">Книги</RouterLink>
          <RouterLink to="/collections">Подборки</RouterLink>
          <RouterLink to="/reviews">Рецензии</RouterLink>
        </template>
      </div>
    </div>

    <div class="right-container">
      <template v-if="!isAuthenticated">
        <button @click="showLoginModal = true" class="transparent-btn">
          Войти
        </button>
        <button @click="showRegisterModal = true" class="transparent-btn">
          Зарегистрироваться
        </button>
      </template>

      <template v-else>
        <div class="user-menu-container">
          <button
            class="user-profile-btn"
            @click="showUserMenu = !showUserMenu"
          >
            <span class="user-name">{{ user.nameUser }}</span>
            <img
              :src="getAvatarUrl(user.profileImageUrl)"
              class="user-avatar"
              alt="Аватар"
            />
          </button>

          <transition name="fade">
            <div v-if="showUserMenu" class="user-menu">
              <!-- <button @click="router.push('/account')" class="user-menu-item">
                Аккаунт
              </button> -->
              <button class="user-menu-item" v-if="userRole === 'Пользователь'">
                Аккаунт
              </button>
              <button @click="router.push('/settings')" class="user-menu-item">
                Настройки
              </button>
              <button @click="logout" class="user-menu-item logout">
                Выйти
              </button>
            </div>
          </transition>
        </div>
      </template>

      <button @click="toggleTheme" class="theme-toggle" title="Поменять тему">
        {{ isPinkTheme ? '🌸' : '🌿' }}
      </button>
    </div>

    <Transition name="modal">
      <div
        v-if="showLoginModal"
        class="modal-overlay"
        @click.self="closeModal('login')"
      >
        <div class="modal" :class="{ closing: isClosing && !showLoginModal }">
          <div class="modal-header">
            <h3>Вход</h3>
            <button class="close-btn" @click="closeModal('login')">×</button>
          </div>
          <div class="modal-body">
            <div v-if="message" class="message">{{ message }}</div>
            <form @submit.prevent="submitLogin">
              <div class="form-group">
                <label for="login-email">Электронная почта</label>
                <input
                  id="login-email"
                  v-model="loginForm.email"
                  type="email"
                  required
                  placeholder="Введите вашу электронную почту"
                  :class="{ 'input-error': errors.login }"
                />
                <div v-if="errors.email" class="error-message">
                  {{ errors.email }}
                </div>
              </div>
              <div class="form-group">
                <label for="login-password">Пароль</label>
                <input
                  id="login-password"
                  v-model="loginForm.password"
                  type="password"
                  required
                  placeholder="Введите ваш пароль"
                  :class="{ 'input-error': errors.password }"
                />
                <div v-if="errors.password" class="error-message">
                  {{ errors.password }}
                </div>
              </div>
              <div v-if="errors.ban" class="error-message">
                {{ errors.ban }}
              </div>
              <button type="submit" class="submit-btn">Войти</button>
              <div class="form-footer">
                <span
                  >Нет аккаунта?
                  <a href="#" @click.prevent="switchToRegister"
                    >Зарегистрируйтесь</a
                  ></span
                >
                <a href="#" @click.prevent="openForgotPassword"
                  >Забыли пароль?</a
                >
              </div>
            </form>
          </div>
        </div>
      </div>
    </Transition>

    <Transition name="modal">
      <div
        v-if="showRegisterModal"
        class="modal-overlay"
        @click.self="closeModal('register')"
      >
        <div
          class="modal"
          :class="{ closing: isClosing && !showRegisterModal }"
        >
          <div class="modal-header">
            <h3>Регистрация</h3>
            <button class="close-btn" @click="closeModal('register')">×</button>
          </div>
          <div class="modal-body">
            <div v-if="message" class="message">{{ message }}</div>
            <form @submit.prevent="submitRegister">
              <div class="form-group">
                <label for="register-name">Имя пользователя</label>
                <input
                  id="register-name"
                  v-model="registerForm.nameUser"
                  type="text"
                  required
                  placeholder="Введите ваше имя"
                  :class="{ 'input-error': errors.username }"
                />
                <div v-if="errors.username" class="error-message">
                  {{ errors.username }}
                </div>
              </div>

              <div class="form-group">
                <label for="register-email">Электронная почта</label>
                <input
                  id="register-email"
                  v-model="registerForm.loginUser"
                  type="email"
                  required
                  placeholder="Введите вашу электронную почту"
                  :class="{ 'input-error': errors.email }"
                />
                <div v-if="errors.email" class="error-message">
                  {{ errors.email }}
                </div>
              </div>
              <div class="form-group">
                <label for="register-password">Пароль</label>
                <input
                  id="register-password"
                  v-model="registerForm.newPassword"
                  type="password"
                  required
                  placeholder="Введите пароль"
                  :class="{ 'input-error': errors.password }"
                />
                <div v-if="errors.password" class="error-message">
                  {{ errors.password }}
                </div>
              </div>
              <div class="form-group">
                <label for="register-password-confirm"
                  >Подтверждение пароля</label
                >
                <input
                  id="register-password-confirm"
                  v-model="registerForm.confirmPassword"
                  type="password"
                  required
                  placeholder="Повторите пароль"
                  :class="{ 'input-error': errors.confirmPassword }"
                />
                <div v-if="errors.confirmPassword" class="error-message">
                  {{ errors.confirmPassword }}
                </div>
              </div>
              <div class="form-group">
                <label for="register-avatar">Изображение</label>
                <input
                  id="register-avatar"
                  type="file"
                  accept="image/*"
                  class="input-image"
                  @change="handleAvatarUpload"
                />
                <div v-if="errors.profilePicture" class="error-message">
                  {{ errors.profilePicture }}
                </div>
              </div>
              <button type="submit" class="submit-btn">
                Зарегистрироваться
              </button>
              <div class="form-footer">
                <span
                  >Уже есть аккаунт?
                  <a href="#" @click.prevent="switchToLogin">Войдите</a></span
                >
              </div>
            </form>
          </div>
        </div>
      </div>
    </Transition>

    <Transition name="modal">
      <div
        v-if="showForgotPasswordModal"
        class="modal-overlay"
        @click.self="closeModal('forgot')"
      >
        <div
          class="modal"
          :class="{ closing: isClosing && !showForgotPasswordModal }"
        >
          <div class="modal-header">
            <h3>Восстановление пароля</h3>
            <button class="close-btn" @click="closeModal('forgot')">×</button>
          </div>
          <div class="modal-body">
            <div v-if="message" class="message">{{ message }}</div>
            <form @submit.prevent="submitForgotPassword">
              <div class="form-group">
                <label for="forgot-email">Электронная почта</label>
                <input
                  id="forgot-email"
                  v-model="forgotPasswordForm.email"
                  type="email"
                  required
                  placeholder="Введите вашу электронную почту"
                  :class="{ 'input-error': errors.email }"
                />
                <div v-if="errors.email" class="error-message">
                  {{ errors.email }}
                </div>
              </div>
              <button type="submit" class="submit-btn">
                Восстановить пароль
              </button>
              <div class="form-footer">
                <a href="#" @click.prevent="switchToLogin">Вернуться к входу</a>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.header {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 1rem;
  background-color: var(--bg);
  box-shadow: 0 2px 5px #0000001a;
}

.nav-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  margin: 0 auto;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0;
}

.logo img {
  height: 80px;
  margin-top: -5px;
}

.logo a {
  font-size: 1.5rem;
  font-weight: bold;
  color: var(--accent-color);
  letter-spacing: 2px;
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.nav-links a {
  text-decoration: none;
  color: var(--dark-text);
  font-weight: 500;
  transition: color 0.3s;
}

.nav-links a:hover {
  color: var(--accent-color);
}

.right-container {
  display: flex;
  align-items: center;
}

.theme-toggle {
  background: none;
  border: none;
  font-size: 1rem;
  transition: transform 0.3s;
}

.theme-toggle:hover {
  transform: scale(1.1);
}

.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--black);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background: var(--bg);
  border-radius: 8px;
  width: 100%;
  max-width: 550px;
  box-shadow: 0 4px 20px #00000026;
  transform: scale(1);
  transition: transform 0.3s ease, opacity 0.3s ease;
}

.modal.closing {
  transform: scale(0.95);
  opacity: 0;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #eeeeee;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.3rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: var(--text-light);
  transition: transform 0.2s ease;
}

.close-btn:hover {
  transform: scale(1.2);
}

.modal-body {
  padding: 20px;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: 500;
}

.form-group input {
  width: 95%;
  padding: 10px;
  border-radius: 5px;
  font-size: 1rem;
  transition: border-color 0.3s ease, box-shadow 0.3s ease;
}

.input-image {
  background: none;
  border: 1px solid var(--accent-color);
  border-radius: 5px;
  font-size: 14px;
}

.submit-btn {
  width: 100%;
  padding: 12px;
  background-color: var(--accent-color);
  color: var(--bg);
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  margin-top: 10px;
  transition: background-color 0.3s ease, transform 0.2s ease;
}

.submit-btn:hover {
  background-color: var(--hover-color);
  transform: translateY(-2px);
}

.submit-btn:active {
  transform: translateY(0);
}

.form-footer {
  margin-top: 16px;
  display: flex;
  justify-content: space-between;
  font-size: 0.9rem;
}

.form-footer a {
  color: var(--accent-color);
  text-decoration: none;
  transition: color 0.3s ease;
}

.form-footer a:hover {
  color: var(--hover-color);
  text-decoration: underline;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeOut {
  from {
    opacity: 1;
    transform: translateY(0);
  }
  to {
    opacity: 0;
    transform: translateY(10px);
  }
}

.user-menu-container {
  position: relative;
  margin-right: 10px;
}

.user-profile-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 5px 10px;
  border: none;
  border-radius: 20px;
  background: none;
  transition: background-color 0.2s;
}

.user-profile-btn:hover {
  background-color: #f0f0f0;
}

.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  object-fit: cover;
}

.user-name {
  font-weight: 500;
  color: var(--dark-text);
}

.user-menu {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 5px;
  min-width: 150px;
  z-index: 100;
  overflow: hidden;
  background: var(--bg);
  border-radius: 8px;
  box-shadow: 0 2px 10px #0000001a;
}

.user-menu-item {
  width: 100%;
  padding: 10px 15px;
  text-align: left;
  border: none;
  background: none;
  transition: background-color 0.2s;
}

.user-menu-item:hover {
  background-color: #f5f5f5;
}

.user-menu-item.logout {
  color: var(--red);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.transparent-btn {
  background: none;
  border: none;
  padding: 8px 16px;
  transition: color 0.2s;
}

.transparent-btn:hover {
  color: var(--accent-color);
}
</style>
