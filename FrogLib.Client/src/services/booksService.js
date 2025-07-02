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
};
