import axios from 'axios';
import store from '@/store';

const apiClient = axios.create({
  baseURL: 'https://localhost:7295/api/UserInteractionWithBooks',
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
  getBookEvaluation(idUser, idBook) {
    return apiClient
      .get(`/book-evaluation/${idUser}/${idBook}`)
      .then((response) => response.data);
  },
  updateBookEvaluation(idUser, idBook, evaluation) {
    return apiClient.post(
      `/update-book-evaluation/${idUser}/${idBook}/${evaluation}`
    );
  },
  deleteBookEvaluation(idUser, idBook) {
    return apiClient.delete(`/delete-book-evaluation/${idUser}/${idBook}`);
  },
  getUserBook(idUser, idBook) {
    return apiClient
      .get(`/user-book/${idUser}/${idBook}`)
      .then((response) => response.data);
  },
  updateUserBook(idUser, idBook, listType) {
    const encodedListType = encodeURIComponent(listType);

    return apiClient.post(
      `/update-user-book/${idUser}/${idBook}/${encodedListType}`
    );
  },
  deleteUserBook(idUser, idBook) {
    return apiClient.delete(`/delete-user-book/${idUser}/${idBook}`);
  },
};
