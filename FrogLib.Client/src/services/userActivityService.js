import axios from 'axios';
import store from '@/store';

const apiClient = axios.create({
  baseURL: 'https://localhost:7295/api/UserActivity',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default {
  getStatistics() {
    return apiClient.get('/get-statistics').then((response) => response.data);
  },
  addView(idUser, typeEntity, idEntity) {
    const encodedTypeEntity = encodeURIComponent(typeEntity);

    const authApiClient = axios.create({
      baseURL: 'https://localhost:7295/api/UserActivity',
    });

    authApiClient.interceptors.request.use((config) => {
      const accessToken = store.getters['auth/accessToken'];
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
      return config;
    });

    return authApiClient.post(
      `/add-view/${idUser}/${encodedTypeEntity}/${idEntity}`
    );
  },
};
