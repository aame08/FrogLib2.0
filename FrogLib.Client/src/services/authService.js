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
      senderEmail: 'vika.korsakova.2016@gmail.com',
      senderPassword: '2412vn2412',
      userEmail: userEmail,
    });
  },
  refreshToken(refreshToken) {
    return apiClient.post('/refresh-token', { refreshToken });
  },
  logout() {
    return Promise.resolve();
  },
};
