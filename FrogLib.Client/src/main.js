import './assets/style.css';
import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import store from './store';

const app = createApp(App);

app.use(store);
app.use(router);

const initializeApp = async () => {
  if (store.state.auth.refreshToken) {
    try {
      await store.dispatch('auth/refreshToken');

      const user = store.getters['auth/user'];
      const currentPath = router.currentRoute.value.path;

      if (
        user?.nameRole === 'Администратор' &&
        !currentPath.startsWith('/admin')
      ) {
        await router.push('/admin/management');
      } else if (
        user?.nameRole === 'Модератор' &&
        !currentPath.startsWith('moder')
      ) {
        await router.push('/moder/management');
      } else if (user?.nameRole === 'Пользователь' && currentPath !== '/') {
        await router.push('/');
      }
    } catch (error) {
      console.error('Ошибка при обновлении токена:', error);
      store.dispatch('auth/logout');
    }
  }

  app.mount('#app');
};

initializeApp();
