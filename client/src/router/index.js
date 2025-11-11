import { createRouter, createWebHistory } from 'vue-router';
import { authGuard } from './guard.js';
import Login from '../views/Login.vue';
import Search from '../views/Search.vue';

const routes = [
  { path: '/', name: 'Search', component: Search, meta: { requiresAuth: true } },
  { path: '/login', name: 'Login', component: Login }
];

const router = createRouter({ history: createWebHistory(), routes });
router.beforeEach(authGuard);
export default router;