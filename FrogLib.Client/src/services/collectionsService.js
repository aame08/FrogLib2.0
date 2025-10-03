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
  getAllCollections() {
    return apiClient.get('/all-collections').then((response) => response.data);
  },
  getCollectionInfo(idCollection) {
    return apiClient
      .get(`/collections/${idCollection}`)
      .then((response) => response.data);
  },
};
