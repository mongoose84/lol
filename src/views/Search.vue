<template>
  <section class="search">
     <h1>🔎 Search Champions</h1>

    <form @submit.prevent="goToChampionView">
      
      <!-- ▼ Dropdown ---------------------------------------------------- -->
      <select v-model="selectedRegion" class="region-select">
        <option disabled value="">Select a region</option>
        <!-- Loop through the options array -->
        <option v-for="opt in options" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
      
      <!-- Text input ---------------------------------------------------- -->
      <input
        v-model="query"
        type="text"
        placeholder="Search for your champion…"
        required
        class="search-input"
      />
      <button type="submit" class="search-btn">Search</button>
    </form>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

// ----- Options for the dropdown ---------------------------------------
const options = [
  { value: 'NA',   label: 'NA' },
  { value: 'EUW',   label: 'EUW' },
  { value: 'EUNE',label: 'EUNE' },
  { value: 'KR',   label: 'KR' },
  { value: 'JP',   label: 'JP' },
  { value: 'LAN',  label: 'LAN' },
  { value: 'LAS',  label: 'LAS' },
  { value: 'OCE',  label: 'OCE' },
  { value: 'RU',   label: 'RU' },
  { value: 'TR',   label: 'TR' },
]

// ----- Reactive state -------------------------------------------------
const query = ref('')
const selectedRegion = ref('')   // will hold the dropdown value
const router = useRouter()

// ----- Methods --------------------------------------------------------
function goToChampionView() {
  const trimmed = query.value.trim()
  if (!trimmed) return

  // Build the champion-string. We include both the search term and the
  // selected region. The receiving page can read them via
  // `this.$route.query.champion` and `this.$route.query.region`.
  const queryParams = { champion: trimmed }
  if (selectedRegion.value) {
    queryParams.region = selectedRegion.value
  }

  router.push({ name: 'ChampionStats', query: queryParams })
}
</script>

<style scoped>
.search-page {
  max-width: 600px;
  margin: 4rem auto;
  text-align: center;
}

.search-form {
  display: flex;
  gap: 0.5rem;
  justify-content: center;
  align-items: stretch;
}

/* Dropdown */
.region-select {
  padding: 0.5rem;
  font-size: 1rem;
}

/* Text input */
.search-input {
  flex: 1;
  min-width: 0;
  padding: 0.5rem;
  font-size: 1rem;
}

/* Button */
.search-btn {
  padding: 0.5rem 1rem;
  font-size: 1rem;
}
</style>