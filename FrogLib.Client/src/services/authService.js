import axios from 'axios';
import store from '@/store';

const apiClient = axios.create({
  baseURL: 'https://localhost:7295/api/Auth',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default {
  register(formData) {
    return axios.post('https://localhost:7295/api/Auth/register', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
  login(email, password) {
    return apiClient.post(`/login/${email}/${password}`);
  },
  resetPassword(userEmail) {
    return apiClient.post('/reset-password', {
      userEmail: userEmail,
    });
  },
  refreshToken(refreshToken) {
    return apiClient.post('/refresh-token', { refreshToken });
  },
  logout() {
    return Promise.resolve();
  },
  updateProfile(idUser, formData) {
    const authApiClient = axios.create({
      baseURL: 'https://localhost:7295/api/Auth',
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    authApiClient.interceptors.request.use((config) => {
      const accessToken = store.getters['auth/accessToken'];
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
      return config;
    });

    return authApiClient.put(`/update-profile/${idUser}`, formData);
  },
};
