import { ref } from 'vue';
import axios from 'axios';

var development = true;

export default function useSummoner() {

  
  const summoner = ref(null);
  const loading  = ref(false);
  const error    = ref(null);
  const winRate  = ref(null);

  // ---------- The actual request ----------
  async function fetchSummoner(gameName, tagLine) {
    loading.value = true;
    error.value   = null;
    summoner.value = null;
    winRate.value = null;
    
    try {
      
      var apiVersion = "/api/v1.0"
      var host = development ? "http://localhost:5000" : 'https://lol-api.agileastronaut.com';
      var defaultPath = host + apiVersion;
      
      const { data } = await axios.get(
         `${defaultPath}/summoner/${encodeURIComponent(gameName)}/${encodeURIComponent(tagLine)}`
      );
      summoner.value = data;  
      console.log("DEBUG: summoner response:", summoner.value);
      
      const response  = await axios.get(
        `${defaultPath}/winrate/${tagLine}/${summoner.value.puuid}`
      );
      console.log("DEBUG: winrate response:", response.data);
      winRate.value = response.data;
    } catch (e) {
      error.value = e.response?.data?.error || e.message;
    } finally {
      loading.value = false;
    }
  }

  // ---------- What the component receives ----------
  return {
    summoner,
    winRate,
    loading,
    error,
    fetchSummoner,
  };
}