import { ref } from 'vue';
import axios from 'axios';

var development = false;

// ---------- Create User ----------
export default async function createUser(userName) {
    try {
      var apiVersion = "/api/v1.0"
      var host = development ? "http://localhost:5000" : 'https://lol-api.agileastronaut.com';
      var defaultPath = host + apiVersion;

      const response = await axios.post(
        `${defaultPath}/user/${encodeURIComponent(userName)}`
      );
      
      console.log("DEBUG: user created:", response.data);
      return response.data;
    } catch (e) {
      const errorMsg = e.response?.data?.error || e.message;
      console.error("Failed to create user:", errorMsg);
      throw new Error(errorMsg);
    }
  }