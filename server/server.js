require('dotenv').config();
const express = require('express');
const axios   = require('axios');
const cors    = require('cors');

const app = express();
app.use(cors());               // allow your Vue dev server to call it
app.use(express.json());

// ---------------------------------------------------
// Helper: build Riot URLs for a given region
// ---------------------------------------------------
const REGION = 'eun1'; // change to the region you need (e.g. euw1, ap2, …)
const RIOT_API_KEY = process.env.RIOT_API_KEY;
if (!RIOT_API_KEY) {
  console.error('❌ Missing RIOT_API_KEY in environment variables');
  process.exit(1);
}
function riotUrl(path) {
  // All Riot endpoints are HTTPS and require the api_key query param
  return 'https://europe.api.riotgames.com/riot' + `${path}?api_key=${RIOT_API_KEY}`;
}

function matchUrl(path) {
  return 'https://europe.api.riotgames.com/lol' + `${path}api_key=${RIOT_API_KEY}`;
}

function summonerUrl(path) {
  //https://https://eun1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/BeicbUttWzqGLM8cdA6rUt_1-fVWe3WFOQ0XUV2KNLawcdtjxE3GNaF2DC0sVZ80jHRw9dUvW6r1GA?api_key=RGAPI-996f191a-30ae-44e2-8283-b7b6d7ccb0f6.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/BeicbUttWzqGLM8cdA6rUt_1-fVWe3WFOQ0XUV2KNLawcdtjxE3GNaF2DC0sVZ80jHRw9dUvW6r1GA?api_key=RGAPI-996f191a-30ae-44e2-8283-b7b6d7ccb0f6
  return 'https://eun1.api.riotgames.com/lol' + `${path}?api_key=${RIOT_API_KEY}`;
}

async function getPuuid(gameName, tagLine) {
  const url = riotUrl(`/account/v1/accounts/by-riot-id/${encodeURIComponent(gameName)}/${encodeURIComponent(tagLine)}`);
  try {
    console.log('Fetching PUUID for:', gameName, tagLine);
    const response = await axios.get(url);
    return response.data.puuid;
  } catch (err) {
    console.error('Account-by-RiotId error:', err.response?.data || err.message);
    throw new Error('Failed to resolve Riot ID');
  }
}

async function getMatchHistory(puuid) {
  const url = matchUrl(`/match/v5/matches/by-puuid/${encodeURIComponent(puuid)}/ids?start=0&count=100&`);
  try {
    console.log('Fetching match history for PUUID:', puuid);
    const response = await axios.get(url);
    return response.data; // array of match IDs
  } catch (err) {
    console.error('Match history error:', err.response?.data || err.message);
    throw new Error('Failed to fetch match history');
  }
}

async function getMatchDetails(matchId) {
  const url = matchUrl(`/match/v5/matches/${encodeURIComponent(matchId)}?`);
  try {
    console.log('Fetching match details for match ID:', matchId);
    const response = await axios.get(url);
    return response.data; // full match details
  } catch (err) {
    console.error('Match details error:', err.response?.data || err.message);
    throw new Error('Failed to fetch match details');
  }
}
async function getSummonerByPuuid(puuid) {
  const url = summonerUrl(`/summoner/v4/summoners/by-puuid/${encodeURIComponent(puuid)}`);
  try {
    console.log('Fetching summoner data for PUUID:', puuid);
    const response = await axios.get(url);
    return response.data; // full summoner details
  } catch (err) {
    console.error('Summoner-by-PUUID error:', err.response?.data || err.message);
    throw new Error('Failed to fetch summoner data');
  }
}

// ---------------------------------------------------
// 1️⃣ Convert Riot ID (gameName + tagLine) → PUUID
// ---------------------------------------------------
app.get('/api/by-riot-id/:gameName/:tagLine', async (req, res) => {
  const { gameName, tagLine } = req.params;
  try {
    const puuid = await getPuuid(gameName, tagLine);
    console.log('Resolved PUUID:', puuid);

    //const matchIds = await getMatchHistory(puuid);
    //console.log(`Fetched ${matchIds.length} match IDs`);

    //const matchDetails = await getMatchDetails(matchIds[0]);
    //console.log('Fetched match details for first match ID');

    const summonerData = await getSummonerByPuuid(puuid);
    console.log('Fetched summoner data for PUUID');

    res.json( summonerData );
  } catch (err) {
    console.error('Account-by-RiotId error:', err.response?.data || err.message);
    res.status(err.response?.status || 500).json({ error: 'Failed to resolve Riot ID' });
  }
});

const PORT = 4000;
app.listen(PORT, () => console.log(`⚡️ Proxy listening on http://localhost:${PORT}`));
