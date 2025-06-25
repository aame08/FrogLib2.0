<script setup>
import { ref, onMounted } from 'vue';
import frogLogo from '@/assets/frog.png';
import frog2Logo from '@/assets/frog2.png';

const isPinkTheme = ref(false);
const logoText = ref('FrogLib');

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
      <button class="transparent-btn">Войти</button>
      <button class="transparent-btn">Зарегистрироваться</button>
      <button @click="toggleTheme" class="theme-toggle" title="Поменять тему">
        {{ isPinkTheme ? '🌸' : '🌿' }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.header {
  background-color: #ffffff;
  box-shadow: 0 2px 5px #0000001a;
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 1rem;
}

.nav-container {
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
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
</style>
