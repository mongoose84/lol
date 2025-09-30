<template>
  <section class="search" >
     

    <form @submit.prevent="goToChampionView" class="search-form">
      
      <!-- Text input ---------------------------------------------------- -->
      <input
        v-model="query"
        type="text"
        placeholder="Search for your champion name…"
        required
        class="search-input"
      />

      <h1>#</h1>

      <!-- ▼ Dropdown ---------------------------------------------------- -->
      <select v-model="tagLine" class="tagLine-select select-dark">

        <!-- Loop through the options array -->
        <option v-for="opt in options" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
     
      

      
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
const tagLine = ref('EUNE')   // will hold the dropdown value
const router = useRouter()

// ----- Methods --------------------------------------------------------
function goToChampionView() {
  const trimmed = query.value.trim()
  if (!trimmed) return

  // Build the champion-string. We include both the search term and the
  // selected tagLine. The receiving page can read them via
  // `this.$route.query.gameName` and `this.$route.query.tagLine`.
  const queryParams = { gameName: trimmed }
  if (tagLine.value) {
    queryParams.tagLine = tagLine.value
  }

  router.push({ name: 'ChampionStats', query: queryParams })
}
</script>

<style scoped>
.search {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh; /* Full viewport height */
  margin-top: -12%; /* Move up by 50px */
}


.search-page {
  max-width: 600px;
  margin: 4rem auto;
  text-align: center;
}

.search-form {  
  display: flex;
  gap: 0.1rem;
  margin-top: 1rem;
  align-items: center;
  justify-content: center;
}

/* Dropdown */
.tagLine-select {
  padding: 0.5rem;
  font-size: 1rem;
  width: 90px;
}

/* Text input */
.search-input {
  flex: 1;
  min-width: 0;
  width: 300px;
  padding: 0.5rem;
  font-size: 1rem;
}

</style>