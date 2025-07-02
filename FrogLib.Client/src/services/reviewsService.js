import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7295/api/Reviews',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default {
  getPopularReviews() {
    return apiClient.get('/popular-reviews').then((response) => response.data);
  },
};
