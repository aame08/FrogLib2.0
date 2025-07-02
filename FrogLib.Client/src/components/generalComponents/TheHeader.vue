<script setup>
import { ref, onMounted } from 'vue';
import frogLogo from '@/assets/frog.png';
import frog2Logo from '@/assets/frog2.png';

const isPinkTheme = ref(false);
const logoText = ref('FrogLib');
const showLoginModal = ref(false);
const showRegisterModal = ref(false);
const showForgotPasswordModal = ref(false);
const isClosing = ref(false);

const loginForm = ref({
  email: '',
  password: '',
});

const registerForm = ref({
  name: '',
  email: '',
  password: '',
  password_confirmation: '',
  avatar: null,
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
  registerForm.value.avatar = event.target.files[0];
};

const submitLogin = () => {
  console.log('Login form submitted:', loginForm.value);
  // showLoginModal.value = false;
};

const submitRegister = () => {
  console.log('Register form submitted:', registerForm.value);
  // showRegisterModal.value = false;
};

const submitForgotPassword = () => {
  console.log('Forgot password form submitted:', forgotPasswordForm.value);
  // showForgotPasswordModal.value = false;
};

const switchToLogin = () => {
  showRegisterModal.value = false;
  showForgotPasswordModal.value = false;
  setTimeout(() => (showLoginModal.value = true), 300);
};

const switchToRegister = () => {
  showLoginModal.value = false;
  setTimeout(() => (showRegisterModal.value = true), 300);
};

const openForgotPassword = () => {
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
</script>

<template>
  <div class="header">
    <div class="logo">
      <img :src="isPinkTheme ? frog2Logo : frogLogo" alt="logo" />
      <RouterLink to="/">{{ logoText }}</RouterLink>
    </div>

    <div class="nav-container">
      <div class="nav-links">
        <RouterLink to="/">Главная</RouterLink>
        <RouterLink to="/">Книги</RouterLink>
        <RouterLink to="/">Подборки</RouterLink>
        <RouterLink to="/">Рецензии</RouterLink>
      </div>
    </div>

    <div class="right-container">
      <button @click="showLoginModal = true" class="transparent-btn">
        Войти
      </button>
      <button @click="showRegisterModal = true" class="transparent-btn">
        Зарегистрироваться
      </button>
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
            <form @submit.prevent="submitLogin">
              <div class="form-group">
                <label for="login-email">Электронная почта</label>
                <input
                  id="login-email"
                  v-model="loginForm.email"
                  type="email"
                  required
                  placeholder="Введите вашу электронную почту"
                />
              </div>
              <div class="form-group">
                <label for="login-password">Пароль</label>
                <input
                  id="login-password"
                  v-model="loginForm.password"
                  type="password"
                  required
                  placeholder="Введите ваш пароль"
                />
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
            <form @submit.prevent="submitRegister">
              <div class="form-group">
                <label for="register-name">Имя пользователя</label>
                <input
                  id="register-name"
                  v-model="registerForm.name"
                  type="text"
                  required
                  placeholder="Введите ваше имя"
                />
              </div>
              <div class="form-group">
                <label for="register-email">Электронная почта</label>
                <input
                  id="register-email"
                  v-model="registerForm.email"
                  type="email"
                  required
                  placeholder="Введите вашу электронную почту"
                />
              </div>
              <div class="form-group">
                <label for="register-password">Пароль</label>
                <input
                  id="register-password"
                  v-model="registerForm.password"
                  type="password"
                  required
                  placeholder="Введите пароль"
                />
              </div>
              <div class="form-group">
                <label for="register-password-confirm"
                  >Подтверждение пароля</label
                >
                <input
                  id="register-password-confirm"
                  v-model="registerForm.password_confirmation"
                  type="password"
                  required
                  placeholder="Повторите пароль"
                />
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
            <form @submit.prevent="submitForgotPassword">
              <div class="form-group">
                <label for="forgot-email">Электронная почта</label>
                <input
                  id="forgot-email"
                  v-model="forgotPasswordForm.email"
                  type="email"
                  required
                  placeholder="Введите вашу электронную почту"
                />
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
  background-color: #ffffff;
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

/* Стили для модальных окон с анимациями */
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
  background-color: #00000080;
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background: #ffffff;
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
  color: #666666;
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
  color: #ffffff;
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
</style>
