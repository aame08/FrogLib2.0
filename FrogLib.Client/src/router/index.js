import { createRouter, createWebHistory } from 'vue-router';
import store from '@/store';

const routes = [
  { path: '/', component: () => import('@/views/HomeView.vue') },
  { path: '/books', component: () => import('@/views/BookView.vue') },
  {
    path: '/books/:id',
    component: () => import('@/views/BookPage.vue'),
    props: true,
    meta: { entityType: 'Книга', trackView: true },
  },
  {
    path: '/settings',
    component: () => import('@/views/UserSettings.vue'),
    meta: { requiresAuth: true },
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const isAuthenticated = store.getters['auth/isAuthenticated'];
  const user = store.getters['auth/user'];
  const userRole = user?.nameRole;

  if (to.matched.some((record) => record.meta.requiresAuth)) {
    if (!isAuthenticated) {
      console.warn('Пользователь не авторизован.');
      router.push('/');
      return;
    }

    // if (
    //   to.matched.some((record) => record.meta.requiresAdmin) &&
    //   userRole !== 'Администратор'
    // ) {
    //   console.warn('Пользователь не имеет достаточных прав.');
    //   router.push('/');
    //   return;
    // }

    // if (
    //   to.matched.some((record) => record.meta.requiresModer) &&
    //   userRole !== 'Модератор'
    // ) {
    //   console.warn('Пользователь не имеет достаточных прав.');
    //   router.push('/');
    //   return;
    // }
  }

  next();
});

// router.beforeEach(async (to, from) => {
//   if (!to.meta.trackView) {
//     return;
//   }

//   const isAuthenticated = store.getters['auth/isAuthenticated'];
//   const user = store.getters['auth/user'];

//   if (!isAuthenticated || !user?.idUser) {
//     console.warn('Пользователь не авторизован или данные отсутствуют.');
//     return;
//   }

//   const idUser = user.idUser;
//   const idEntity = to.params.id ? parseInt(to.params.id) : null;
//   const typeEntity = to.meta.entityType || null;

//   if (!idEntity || !typeEntity) {
//     console.log('Нет данных для добавления просмотра.');
//     return;
//   }

//   try {
//     await userActivityService.addView(idUser, idEntity, typeEntity);
//     console.log('Просмотр добавлен успешно.');
//   } catch (error) {
//     console.error('Ошибка при добавлении просмотра:', error);
//   }
// });

export default router;
