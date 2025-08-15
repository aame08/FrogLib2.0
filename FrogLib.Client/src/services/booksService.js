import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7295/api/Books',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default {
  getNewBooks() {
    return apiClient.get('/new-books').then((response) => response.data);
  },
  getBestselling() {
    return apiClient.get('/best-selling').then((response) => response.data);
  },
  getPopularBooks() {
    return apiClient.get('/popular-books').then((response) => response.data);
  },
  getAllBooks() {
    return apiClient.get('/all-books').then((response) => response.data);
  },
  getCategories() {
    return apiClient.get('/categories').then((response) => response.data);
  },
  getBookInfo(idBook) {
    return apiClient.get(`/books/${idBook}`).then((response) => response.data);
  },
};
