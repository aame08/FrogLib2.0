import { watch } from 'vue';
import { useStore } from 'vuex';
import userActivityService from '@/services/userActivityService';

export default function useViewHistory() {
  const store = useStore();

  const addView = async (idEntity, typeEntity) => {
    const isAuthenticated = store.getters['auth/isAuthenticated'];
    const user = store.getters['auth/user'];

    if (!isAuthenticated || !user?.idUser) {
      store.commit('viewHistory/ADD_PENDING_VIEW', { idEntity, typeEntity });
    }

    try {
      await userActivityService.addView(user.idUser, typeEntity, idEntity);
      console.log('Просмотр добавлен успешно.');
    } catch (error) {
      console.error('Ошибка при добавлении просмотра:', error);
    }
  };

  watch(
    () => store.getters['auth/isAuthenticated'],
    (isAuth) => {
      if (isAuth) {
        store.dispatch('viewHistory/processPendingViews');
      }
    }
  );

  return { addView };
}
