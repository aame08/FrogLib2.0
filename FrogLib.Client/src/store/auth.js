import authService from '@/services/authService';
import axios from 'axios';

const API_URL = 'https://localhost:7295/api/Auth';

const state = () => ({
  accessToken: localStorage.getItem('accessToken') || null,
  refreshToken: localStorage.getItem('refreshToken') || null,
  user: JSON.parse(localStorage.getItem('user')) || null,
  lastActivity: Date.now(),
  activityCheckInterval: null,
});

const mutations = {
  setTokens(state, { accessToken, refreshToken }) {
    state.accessToken = accessToken;
    state.refreshToken = refreshToken;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  },
  setUser(state, user) {
    state.user = user;
    localStorage.setItem('user', JSON.stringify(user));
  },
  clearAuth(state) {
    state.accessToken = null;
    state.refreshToken = null;
    state.user = null;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },
  resetActivity(state) {
    state.lastActivity = Date.now();
  },
  startActivityTracker(state) {
    state.activityCheckInterval = setInterval(() => {
      const minutesInactive = (Date.now() - state.lastActivity) / 1000 / 60;
      if (minutesInactive > 15) {
        console.log('Пользователь вышел из системы из-за неактивности.');
        this.commit('auth/clearAuth');
      }
    }, 60000);
  },
  stopActivityTracker(state) {
    clearInterval(state.activityCheckInterval);
  },
};

const actions = {
  async login({ commit }, credentials) {
    try {
      const response = await authService.login(credentials);
      commit('setTokens', {
        accessToken: response.data.accessToken,
        refreshToken: response.data.refreshToken,
      });
      commit('setUser', response.data.user);
      commit('startActivityTracker');
    } catch (error) {
      throw error;
    }
  },

  async refreshToken({ state, commit }) {
    try {
      const response = await axios.post(`${API_URL}/refresh-token`, {
        refreshToken: state.refreshToken,
      });
      commit('setTokens', {
        accessToken: response.data.accessToken,
        refreshToken: response.data.refreshToken,
      });
      return response.data.accessToken;
    } catch (error) {
      console.error(
        'Ошибка при обновлении токена:',
        error.response?.data || error.message
      );
      commit('clearAuth');
      throw error;
    }
  },

  logout({ commit }) {
    console.log('Пользователь вышел из системы.');
    commit('clearAuth');
    commit('stopActivityTracker');
  },
};

const getters = {
  isAuthenticated: (state) => !!state.accessToken,
  user: (state) => state.user,
  accessToken: (state) => state.accessToken,
};

export default {
  namespaced: true,
  state,
  mutations,
  actions,
  getters,
};
