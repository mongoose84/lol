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
        <option v-for="opt in options" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>
    </form>

    <!-- User Creation Section -->
    <div class="user-creation">
      <button @click="showUserForm = !showUserForm" class="toggle-user-btn">
        {{ showUserForm ? 'Cancel' : 'Create User' }}
      </button>

      <div v-if="showUserForm" class="user-form">
        <input
          v-model="newUserName"
          type="text"
          placeholder="Enter username…"
          class="search-input"
        />
        <button @click="handleCreateUser" class="create-user-btn">
          Submit
        </button>
        <p v-if="userCreationError" class="error">{{ userCreationError }}</p>
        <p v-if="userCreationSuccess" class="success">{{ userCreationSuccess }}</p>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import createUser from '@/assets/createUser.js'

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
const tagLine = ref('EUNE')
const router = useRouter()

// User creation state
const showUserForm = ref(false)
const newUserName = ref('')
const userCreationError = ref(null)
const userCreationSuccess = ref(null)


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

async function handleCreateUser() {
  userCreationError.value = null
  userCreationSuccess.value = null

  const trimmed = newUserName.value.trim()
  if (!trimmed) {
    userCreationError.value = 'Username cannot be empty'
    return
  }

  try {
    await createUser(trimmed)
    userCreationSuccess.value = `User "${trimmed}" created successfully!`
    newUserName.value = ''
    setTimeout(() => {
      showUserForm.value = false
      userCreationSuccess.value = null
    }, 2000)
  } catch (err) {
    userCreationError.value = err.message || 'Failed to create user'
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