import axios from 'axios';

var development = false;

// POST /api/v1.0/user/{username} with JSON body { accounts: [{ gameName, tagLine }, ...] }
export default async function createUser(username, accounts) {
  const apiVersion = '/api/v1.0';
  const host = development ? 'http://localhost:5000' : 'https://lol-api.agileastronaut.com';
  const base = host + apiVersion;

  try {
    const response = await axios.post(
      `${base}/user/${encodeURIComponent(username)}`,
      { accounts },
      { headers: { 'Content-Type': 'application/json' } }
    );
    return response.data;
  } catch (e) {
    const errorMsg = e?.response?.data?.error || e.message || 'Request failed';
    throw new Error(errorMsg);
  }
}