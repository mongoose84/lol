import axios from 'axios';
import { getToken } from '../services/auth.js';

var development = true;


const host = development ? 'http://localhost:5000' : 'https://lol-api.agileastronaut.com';

const api = axios.create({ baseURL: host });

api.interceptors.request.use(cfg => {
  const token = getToken();
  if (token) cfg.headers.Authorization = `Bearer ${token}`;
  return cfg;
});

export default api;