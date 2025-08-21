import { createStore } from 'vuex';
import auth from './auth';
import viewHistory from './viewHistory';

const store = createStore({
  modules: {
    auth,
    viewHistory,
  },
});

export default store;
