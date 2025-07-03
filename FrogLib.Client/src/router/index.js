import { createRouter, createWebHistory } from 'vue-router';
import store from '@/store';

const routes = [{ path: '/', component: () => import('@/views/HomeView.vue') }];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
