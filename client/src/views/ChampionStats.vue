<template>
  <section class="championstats">
    <h2>Results for {{ gameName }}#{{ tagLine }}</h2>

    <div v-if="loading">Loading…</div>
    <div v-else-if="error">{{ error }}</div>

    <!-- Pretty‑print the returned object -->
    <pre v-else-if="summoner">{{ JSON.stringify(summoner, null, 2) }}</pre>
  </section>
</template>

<script setup>
import { onMounted, watch } from 'vue';
import useSummoner from '../assets/useSummoner';

// ----- Props coming from the parent (router, other component, etc.) -----
const props = defineProps({
  gameName: {
    type: String,
    required: true,
  },
  tagLine: {
    type: String,
    required: true,
  },
});

const { summoner, loading, error, fetchSummoner } = useSummoner();

function load() {
  if (props.gameName.trim() && props.tagLine.trim()) {
    fetchSummoner(props.gameName.trim(), props.tagLine.trim());
  }
}

onMounted(() => {
  load();
});

watch(
  () => [props.gameName, props.tagLine],
  () => {
    load();
  }
);

// (Optional) expose `load` so a parent could call it manually
defineExpose({ load });
</script>

<style scoped>
.championstats {
  max-width: 800px;
  margin: 4rem auto;
}
</style>