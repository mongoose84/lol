<!-- src/App.vue -->
<template>
  <!-- Optional global layout -->
  <div id="app">
    <!-- snippet inside App.vue -->
    <button v-if="loggedIn" @click="handleLogout">Logout</button>

    <!-- This is where the matched route component (Home or Results) is rendered -->
    <router-view />
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { isLoggedIn, logout } from './services/auth.js';
import { useRouter } from 'vue-router';

const router = useRouter();
const loggedIn = computed(() => isLoggedIn());

function handleLogout() {
  logout();
  router.push({ name: 'Login' });
}
</script>