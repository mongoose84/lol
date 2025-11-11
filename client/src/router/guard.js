import { isLoggedIn } from '../services/auth.js';

export function authGuard(to, from, next) {
  if (to.meta.requiresAuth && !isLoggedIn()) return next({ name: 'Login' });
  next();
}