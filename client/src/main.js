import { createApp } from 'vue'
import router from './router'
import App from './App.vue'
import './assets/main.css' // Tailwind CSS
createApp(App).use(router).mount('#app')
