// src/assets/useSummoner.js
import { ref } from 'vue';
import axios from 'axios';

/**
 * This composable talks to the tiny Express proxy, server.js:
 *   GET /summoner/by-riot-id/:gameName/:tagLine
 *
 * It returns four reactive values:
 *   - summoner : the object we get back from Riot (or null)
 *   - loading  : true while the request is in flight
 *   - error    : string with an error message (or null)
 *   - fetchSummoner : function you call with (gameName, tagLine)
 */
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

      var development = true; //change to false when deploying
      var apiVersion = "/api/v1.0" // update when the backend version is updated
      var host = development ? "http://localhost:5173" : 'https://lol-api.agileastronaut.com';
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
                   // the object the proxy returns
    } catch (e) {
      // Preserve a helpful message for the UI
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