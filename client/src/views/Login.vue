<!-- filepath: /home/thread/Documents/lol/client/src/views/Login.vue -->
<template>
  <form class="login" @submit.prevent="doLogin">
    <input v-model="userName" placeholder="Username" />
    <input v-model="password" type="password" placeholder="Password" />
    <button :disabled="busy">Login</button>
    <p v-if="error" class="error">{{ error }}</p>
  </form>
</template>

<script setup>
import { ref } from 'vue';
import api from '../axios.js';
import { saveToken } from '../services/auth.js';
import { useRouter } from 'vue-router';

const userName = ref('');
const password = ref('');
const busy = ref(false);
const error = ref('');
const router = useRouter();

async function doLogin() {
  error.value = '';
  busy.value = true;
  try {
    const res = await api.post('/auth/login', { userName: userName.value, password: password.value });
    saveToken(res.data.token);
    router.push({ name: 'Home' });
  } catch (e) {
    error.value = 'Login failed';
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.login { display:flex; flex-direction:column; gap:.5rem; width:220px; }
.error { color:#f44; }
</style>