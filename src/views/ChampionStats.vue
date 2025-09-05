<template>
  <section class="championstats">
    <h2>Results for {{ region }}#{{ champion }}</h2>

    <!-- Placeholder – replace with API call or filtered data -->
    <p>Here you could render search results, call an API, etc.</p>

    <router-link :to="{ name: 'Search' }">← Back to search</router-link>
  </section>
</template>

<script setup>

defineProps({
  champion: {
    type: String,
    required: true,
  },
  region: {
    type: String,
    default: 'EUNE',
  },
})
</script>

<style scoped>
.results {
  max-width: 800px;
  margin: 4rem auto;
}
</style>