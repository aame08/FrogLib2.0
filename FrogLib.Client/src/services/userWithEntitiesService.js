import axios from 'axios';
import store from '@/store';

const apiClient = axios.create({
  baseURL: 'https://localhost:7295/api/UserInteractionWithEntities',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const accessToken = store.getters['auth/accessToken'];
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default {
  addReview(reviewDTO) {
    return apiClient.post('/add-review', reviewDTO);
  },
  addCollection(collectionDTO) {
    return apiClient.post('/add-collection', collectionDTO);
  },
};
