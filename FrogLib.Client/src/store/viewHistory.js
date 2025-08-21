import userActivityService from '@/services/userActivityService';

export default {
  namespaced: true,
  state: {
    pendingViews: [],
  },
  mutations: {
    ADD_PENDING_VIEW(state, view) {
      state.pendingViews.push(view);
    },
    CLEAR_PENDING_VIEWS(state) {
      state.pendingViews = [];
    },
  },
  actions: {
    async processPendingViews({ state, commit, rootGetters }) {
      const idUser = rootGetters['auth/user']?.idUser;
      if (!idUser) return;

      for (const view of state.pendingViews) {
        try {
          await userActivityService.addView(
            idUser,
            view.typeEntity,
            view.idEntity
          );
        } catch (error) {
          console.error('Ошибка при добавлении отложенного просмотра:', error);
        }
      }

      commit('CLEAR_PENDING_VIEWS');
    },
  },
};
