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
  getAllReviews() {
    return apiClient.get('/all-reviews').then((response) => response.data);
  },
  getReviewInfo(idReview) {
    return apiClient
      .get(`/reviews/${idReview}`)
      .then((response) => response.data);
  },
};
