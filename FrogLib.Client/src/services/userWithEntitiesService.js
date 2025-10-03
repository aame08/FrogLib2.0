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
  getEntityRating(idUser, idEntity, typeEntity) {
    return apiClient
      .get('/get-entity-rating', {
        params: {
          IdUser: idUser,
          IdEntity: idEntity,
          TypeEntity: typeEntity,
        },
      })
      .then((response) => response.data);
  },
  updateEntityRating(ratingDTO) {
    return apiClient.post('/update-entity-rating', ratingDTO);
  },
  deleteEntityRating(idUser, idEntity, typeEntity) {
    return apiClient.delete('/delete-entity-rating', {
      params: {
        IdUser: idUser,
        IdEntity: idEntity,
        TypeEntity: typeEntity,
      },
    });
  },
  isCollectionSaved(idUser, idCollection) {
    return apiClient
      .get(`/is-collection-saved/${idUser}/${idCollection}`)
      .then((response) => response.data);
  },
  likeCollection(idUser, idCollection) {
    return apiClient
      .post(`/like-collection/${idUser}/${idCollection}`)
      .then((response) => response.data);
  },
  unlikeCollection(idUser, idCollection) {
    return apiClient
      .delete(`/unlike-collection/${idUser}/${idCollection}`)
      .then((response) => response.data);
  },
  addComment(idUser, idEntity, typeEntity, text) {
    return apiClient.post('/add-comment', null, {
      params: {
        IdUser: idUser,
        IdEntity: idEntity,
        TypeEntity: typeEntity,
        Text: text,
      },
    });
  },
  addReply(commentDTO) {
    return apiClient.post('/add-reply', commentDTO);
  },
};
