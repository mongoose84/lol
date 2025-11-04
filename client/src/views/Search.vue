<template>
  <section class="search">

    <!-- User Creation Section -->
    <div class="user-creation">
      <button @click="showUserForm = !showUserForm" class="toggle-user-btn">
        {{ showUserForm ? 'Cancel' : 'Create User' }}
      </button>

      <CreateUserPopup
        v-if="showUserForm"
        :onClose="() => (showUserForm = false)"
        :onCreate="handleCreateUser"
      />
    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import CreateUserPopup from './CreateUserPopup.vue' // Import the CreateUserPopup component

// ----- Options for the dropdown ---------------------------------------
const options = [
  { value: 'NA', label: 'NA' },
  { value: 'EUW', label: 'EUW' },
  { value: 'EUNE', label: 'EUNE' },
  { value: 'KR', label: 'KR' },
  { value: 'JP', label: 'JP' },
  { value: 'LAN', label: 'LAN' },
  { value: 'LAS', label: 'LAS' },
  { value: 'OCE', label: 'OCE' },
  { value: 'RU', label: 'RU' },
  { value: 'TR', label: 'TR' },
]

// ----- Reactive state -------------------------------------------------
const query = ref('')
const tagLine = ref('EUNE')
const router = useRouter()

// User creation state
const showUserForm = ref(false)

// ----- Methods --------------------------------------------------------
function goToChampionView() {
  const trimmed = query.value.trim()
  if (!trimmed) return

  const queryParams = { gameName: trimmed }
  if (tagLine.value) {
    queryParams.tagLine = tagLine.value
  }

  router.push({ name: 'ChampionStats', query: queryParams })
}

async function handleCreateUser(userNames) {
  // Handle user creation logic here
  for (const userName of userNames) {
    const trimmedGameName = userName.gameName.trim();
    const trimmedTagline = userName.tagline.trim();
    
    if (trimmedGameName && trimmedTagline) {
      try {
        await createUser(trimmedGameName, trimmedTagline); // Call your API to create the user
        console.log(`User "${trimmedGameName}" created successfully!`);
      } catch (err) {
        console.error(`Failed to create user "${trimmedGameName}":`, err.message);
      }
    }
  }
}
</script>

<style scoped>

.search {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  margin-top: -12%;
}

.search-form {  
  display: flex;
  gap: 0.1rem;
  margin-top: 1rem;
  align-items: center;
  justify-content: center;
}

.tagLine-select {
  padding: 0.5rem;
  font-size: 1rem;
  width: 90px;
}

.search-input {
  flex: 1;
  min-width: 0;
  width: 300px;
  padding: 0.5rem;
  font-size: 1rem;
}

.user-creation {
  margin-top: 2rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.toggle-user-btn,
.create-user-btn {
  padding: 0.5rem 1rem;
  font-size: 1rem;
  cursor: pointer;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
}

.toggle-user-btn:hover,
.create-user-btn:hover {
  background-color: #0056b3;
}

.user-form {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  align-items: center;
}

.error {
  color: #dc3545;
  font-size: 0.9rem;
}

.success {
  color: #28a745;
  font-size: 0.9rem;
}
</style>