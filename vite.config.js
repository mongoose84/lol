// vite.config.ts
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  test: {
    globals: true,               // enables `describe`, `it`, `expect` globally
    environment: 'happy-dom',    // DOM for mounting components
    setupFiles: ['./test/setup.ts'], // optional – see next step
    coverage: {
      reporter: ['text', 'html'],
      all: true,
    },
  },
})