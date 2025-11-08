<template>
  <div class="popup-overlay">
    <div class="popup-content">
      <h2>Create Username</h2>

      <!-- Single username -->
      <div class="username-input-container">
        <input
          v-model="username"
          type="text"
          placeholder="Enter username…"
          required
          class="username-input"
        />
      </div>

      <!-- Multiple gameName/tagLine pairs -->
      <div class="username-fields">
        <div v-for="row in summoners" :key="row.id" class="username-row">
          <section class="search">
            <form class="search-form" @submit.prevent>
              <input
                v-model="row.gameName"
                type="text"
                placeholder="Search for your champion name…"
                required
                class="search-input"
              />
              <h1>#</h1>
              
              <!-- Custom combo box -->
              <div class="combo-box">
                <input 
                  v-model="row.tagLine" 
                  type="text"
                  placeholder="Tag"
                  class="tagLine-input"
                  @focus="row.showDropdown = true"
                  @blur="handleBlur(row)"
                />
                <button 
                  type="button"
                  class="dropdown-arrow"
                  @click="row.showDropdown = !row.showDropdown"
                >
                  ▼
                </button>
                <div v-if="row.showDropdown" class="dropdown-menu">
                  <div 
                    v-for="opt in options" 
                    :key="opt.value"
                    class="dropdown-item"
                    @click="selectOption(row, opt.value)"
                  >
                    {{ opt.label }}
                  </div>
                </div>
              </div>
            </form>
          </section>
        </div>
      </div>

      <button class="add-button" @click="handleAddRow">+</button>

      <div class="action-buttons">
        <button @click="handleCreate" :disabled="busy">Create</button>
        <button @click="onClose" :disabled="busy">Cancel</button>
      </div>

      <p v-if="error" class="error">{{ error }}</p>
      <p v-if="success" class="success">{{ success }}</p>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue'
import createUser from '../assets/createUser.js'

interface SummonerField {
  id: number;
  gameName: string;
  tagLine: string;
  showDropdown?: boolean;
}

export default defineComponent({
  props: {
    onClose: { type: Function, required: true },
    onCreate: { type: Function, required: false }
  },
  setup(props) {
    const options = [
      { value: 'NA', label: 'NA' },  { value: 'EUW', label: 'EUW' },
      { value: 'EUNE', label: 'EUNE' }, { value: 'KR', label: 'KR' },
      { value: 'JP', label: 'JP' },  { value: 'LAN', label: 'LAN' },
      { value: 'LAS', label: 'LAS' },{ value: 'OCE', label: 'OCE' },
      { value: 'RU', label: 'RU' },  { value: 'TR', label: 'TR' },
    ]

    const username = ref<string>('')
    const summoners = ref<SummonerField[]>([
      { id: 1, gameName: '', tagLine: 'EUNE', showDropdown: false }
    ])
    const nextId = ref(2)
    const busy = ref(false)
    const error = ref<string | null>(null)
    const success = ref<string | null>(null)

    const handleAddRow = () => {
      summoners.value.push({ id: nextId.value, gameName: '', tagLine: 'EUNE', showDropdown: false })
      nextId.value++
    }

    const handleBlur = (row: SummonerField) => {
      setTimeout(() => {
        row.showDropdown = false
      }, 200)
    }

    const selectOption = (row: SummonerField, value: string) => {
      row.tagLine = value
      row.showDropdown = false
    }

    const handleCreate = async () => {
      error.value = null
      success.value = null

      const name = username.value.trim()
      if (!name) { error.value = 'Username is required'; return }

      const accounts = summoners.value
        .map(s => ({ gameName: s.gameName.trim(), tagLine: s.tagLine }))
        .filter(s => s.gameName && s.tagLine)

      if (accounts.length === 0) {
        error.value = 'Add at least one game name and tagLine'
        return
      }

      try {
        busy.value = true
        const res = await createUser(name, accounts)
        success.value = 'User created successfully'
        // @ts-ignore
        props.onCreate?.({ username: name, accounts, response: res })
        setTimeout(() => props.onClose(), 700)
      } catch (e: any) {
        error.value = e?.message ?? 'Failed to create user'
      } finally {
        busy.value = false
      }
    }

    return { options, username, summoners, handleAddRow, handleBlur, selectOption, handleCreate, busy, error, success }
  }
})
</script>

<style scoped>
.popup-overlay {
  position: fixed; inset: 0; background-color: rgba(0, 0, 0, 0.8);
  display: flex; justify-content: center; align-items: center; z-index: 1000;
}
.popup-content {
  background: #1a1a1a; color: #fff; padding: 2rem; border-radius: 8px;
  max-width: 560px; width: 92%;
}
.popup-content h2 { margin-bottom: 1rem; }
.username-input-container { margin-bottom: 1rem; }
.username-input {
  width: 100%; padding: 0.5rem; font-size: 1rem;
  border: 1px solid #555; border-radius: 4px; background: #333; color: #fff;
}
.username-input::placeholder { color: #aaa; }

.username-fields { margin: 1rem 0; }
.username-row { margin-bottom: 1rem; padding: 1rem; border: 1px solid #444; border-radius: 4px; background: #2a2a2a; }

.search-form { display: flex; gap: 0.25rem; align-items: center; }
.search-form h1 { color: #fff; margin: 0 0.25rem; }

.search-input {
  background: #333; color: #fff; border: 1px solid #555; border-radius: 4px;
  padding: 0.5rem; font-size: 1rem;
  flex: 1; min-width: 0;
}

.combo-box {
  position: relative;
  display: flex;
  width: 120px;
}

.tagLine-input {
  background: #333; color: #fff; border: 1px solid #555; 
  border-right: none;
  border-radius: 4px 0 0 4px;
  padding: 0.5rem; font-size: 1rem;
  flex: 1;
  min-width: 0;
}

.dropdown-arrow {
  background: #333;
  color: #fff;
  border: 1px solid #555;
  border-radius: 0 4px 4px 0;
  padding: 0 0.5rem;
  cursor: pointer;
  font-size: 0.7rem;
}

.dropdown-arrow:hover {
  background: #444;
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #333;
  border: 1px solid #555;
  border-top: none;
  border-radius: 0 0 4px 4px;
  max-height: 200px;
  overflow-y: auto;
  z-index: 1000;
  margin-top: 1px;
}

.dropdown-item {
  padding: 0.5rem;
  cursor: pointer;
  color: #fff;
}

.dropdown-item:hover {
  background: #444;
}

.add-button {
  width: 40px; height: 40px; border-radius: 50%; background: #007bff; color: #fff;
  border: none; font-size: 1.5rem; cursor: pointer; margin: 0.5rem 0 1rem;
}
.add-button:hover { background: #0056b3; }

.action-buttons { display: flex; gap: 1rem; justify-content: center; }
.action-buttons button {
  padding: 0.5rem 1rem; font-size: 1rem; cursor: pointer; border: none; border-radius: 4px;
}
.action-buttons button:first-child { background: #28a745; color: #fff; }
.action-buttons button:first-child:hover { background: #218838; }
.action-buttons button:last-child { background: #dc3545; color: #fff; }
.action-buttons button:last-child:hover { background: #c82333; }

.error { color: #ff6b6b; margin-top: 0.5rem; }
.success { color: #28a745; margin-top: 0.5rem; }
</style>