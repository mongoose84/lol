<!-- src/App.vue -->
<template>
  <!-- Optional global layout -->
  <div id="app">
    

    <!-- This is where the matched route component (Home or Results) is rendered -->
    <router-view />
  </div>
</template>

<script setup>
// No script logic needed for the root component in this simple case
</script>

<style scoped>
#app {
  font-family: system-ui, sans-serif;
  color: #222;
  margin: 0;
  padding: 0;
}

/* Simple styling for the optional header */
.site-header {
  background: #3b82f6;       /* Tailwind blue‑500 */
  color: white;
  padding: 1rem;
  text-align: center;
}
</style>