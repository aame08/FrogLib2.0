import axios from 'axios';
import store from '@/store';
import { jwtDecode } from 'jwt-decode';

const api = axios.create({
  baseURL: 'https://localhost:7295',
});

api.interceptors.request.use(async (config) => {
  const token = store.state.auth.accessToken;

  if (token) {
    const decodedToken = jwtDecode(token);
    const isTokenExpired = decodedToken.exp * 1000 < Date.now();

    console.log('Текущий access token:', token);
    console.log('Токен истек:', isTokenExpired);

    if (isTokenExpired) {
      try {
        const newToken = await store.dispatch('auth/refreshToken');
        config.headers.Authorization = `Bearer ${newToken}`;
      } catch (error) {
        console.error('Ошибка при обновлении токена:', error);
        store.dispatch('auth/logout');
        return Promise.reject(error);
      }
    } else {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const newToken = await store.dispatch('auth/refreshToken');
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        console.error('Ошибка при обновлении токена:', refreshError);
        store.dispatch('auth/logout');
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
