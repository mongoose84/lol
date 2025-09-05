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
      provider: 'v8',               // default, works out‑of‑the‑box
      reporter: ['text', 'html'],   // text goes to console, html creates ./coverage
      reportsDirectory: './coverage', // <-- explicit folder (optional, default is ./coverage)
      all: true,                    // instrument *all* files, not only those imported in tests
      include: ['src/**/*.ts', 'src/**/*.vue'], // adjust to your source pattern
    },
  },
})