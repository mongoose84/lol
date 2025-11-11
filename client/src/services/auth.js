const TOKEN_KEY = 'auth_token';

export function saveToken(t) { localStorage.setItem(TOKEN_KEY, t); }
export function getToken() { return localStorage.getItem(TOKEN_KEY); }
export function isLoggedIn() { return !!getToken(); }
export function logout() { localStorage.removeItem(TOKEN_KEY); }