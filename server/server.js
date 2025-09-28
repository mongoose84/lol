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
const REGION = 'EUN1'; // change to the region you need (e.g. euw1, ap2, …)
const RIOT_API_KEY = process.env.RIOT_API_KEY;
if (!RIOT_API_KEY) {
  console.error('❌ Missing RIOT_API_KEY in environment variables');
  process.exit(1);
}
function riotUrl(path) {
  // All Riot endpoints are HTTPS and require the api_key query param
  return `https://${REGION}.api.riotgames.com${path}?api_key=${RIOT_API_KEY}`;
}

// ---------------------------------------------------
// 1️⃣ Convert Riot ID (gameName + tagLine) → PUUID
// ---------------------------------------------------
app.get('/account/by-riot-id/:gameName/:tagLine', async (req, res) => {
  const { gameName, tagLine } = req.params;
  try {

    const url = riotUrl(`/riot/account/v1/accounts/by-riot-id/${encodeURIComponent(gameName)}/${encodeURIComponent(tagLine)}`);
    const { data } = await axios.get(url);
    // data.puuid is the identifier we need for the next step
    res.json({ puuid: data.puuid, gameName: data.gameName, tagLine: data.tagLine });
  } catch (err) {
    console.error('Account-by-RiotId error:', err.response?.data || err.message);
    res.status(err.response?.status || 500).json({ error: 'Failed to resolve Riot ID' });
  }
});

// ---------------------------------------------------
// 2️⃣ Get full summoner data from PUUID
// ---------------------------------------------------
app.get('/summoner/by-puuid/:puuid', async (req, res) => {
  const { puuid } = req.params;
  try {
    const url = riotUrl(`/lol/summoner/v4/summoners/by-puuid/${encodeURIComponent(puuid)}`);
    const { data } = await axios.get(url);
    // data contains: id (summonerId), accountId, puuid, name (legacy name), profileIconId, revisionDate, summonerLevel
    res.json(data);
  } catch (err) {
    console.error('Summoner-by-PUUID error:', err.response?.data || err.message);
    res.status(err.response?.status || 500).json({ error: 'Failed to fetch summoner data' });
  }
});

// ---------------------------------------------------
// Convenience endpoint: Riot ID → full summoner object
// ---------------------------------------------------
app.get('/summoner/by-riot-id/:gameName/:tagLine', async (req, res) => {
  const { gameName, tagLine } = req.params;
  try {
    // 1️⃣ Resolve Riot ID → PUUID
    const acctUrl = riotUrl(`/riot/account/v1/accounts/by-riot-id/${encodeURIComponent(gameName)}/${encodeURIComponent(tagLine)}`);
    const acctRes = await axios.get(acctUrl);
    const puuid   = acctRes.data.puuid;

    // 2️⃣ Fetch summoner record by PUUID
    const sumUrl = riotUrl(`/lol/summoner/v4/summoners/by-puuid/${encodeURIComponent(puuid)}`);
    const sumRes = await axios.get(sumUrl);

    // Merge useful bits together for the front‑end
    const result = {
      puuid,
      summonerId: sumRes.data.id,
      summonerLevel: sumRes.data.summonerLevel,
      profileIconId: sumRes.data.profileIconId,
      gameName: acctRes.data.gameName,
      tagLine: acctRes.data.tagLine,
    };
    res.json(result);
  } catch (err) {
    console.error('Combined endpoint error:', err.response?.data || err.message);
    res.status(err.response?.status || 500).json({ error: 'Failed to resolve Riot ID → summoner' });
  }
});

const PORT = 4000;
app.listen(PORT, () => console.log(`⚡️ Proxy listening on http://localhost:${PORT}`));