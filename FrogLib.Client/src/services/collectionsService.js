import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7295/api/Collections',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default {
  getPopularCollections() {
    return apiClient
      .get('/popular-collections')
      .then((response) => response.data);
  },
};
