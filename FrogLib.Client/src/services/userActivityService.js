import axios from 'axios';

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
};
