/**
 * Unit test for src/App.vue
 *
 * What we verify:
 * 1️⃣ The component renders without throwing.
 * 2️⃣ The <router-view/> placeholder is present in the DOM.
 * 3️⃣ When a route is defined, the matched component is displayed inside the outlet.
 */

import { mount } from '@vue/test-utils'
import { describe, it, expect } from 'vitest'
import { createRouter, createMemoryHistory, RouterView } from 'vue-router'
import App from '../../src/App.vue'

// ------------------------------------------------------------------
// 1️⃣ Create a tiny router with two dummy routes (Home & Results)
// ------------------------------------------------------------------
const Home = { template: '<div class="home">Home page</div>' }
const Results = { template: '<div class="results">Results page</div>' }

const router = createRouter({
  history: createMemoryHistory(), // in‑memory, perfect for tests
  routes: [
    { path: '/', name: 'Home', component: Home },
    { path: '/results', name: 'Results', component: Results },
  ],
})

// ------------------------------------------------------------------
// 2️⃣ Helper to mount App with the router injected
// ------------------------------------------------------------------
async function mountApp(initialPath = '/') {
  // push the initial route before mounting so the router is ready
  await router.push(initialPath)
  await router.isReady()

  return mount(App, {
    global: {
      plugins: [router],
      // Stubbing <router-view/> is optional – we want the real outlet,
      // so we don’t stub it here.
    },
  })
}

// ------------------------------------------------------------------
// 3️⃣ The actual test suite
// ------------------------------------------------------------------
describe('App.vue (root component)', () => {
  it('renders the root div with id="app"', async () => {
    const wrapper = await mountApp()
    expect(wrapper.find('#app').exists()).toBe(true)
  })

  it('contains a <router-view/> placeholder', async () => {
    const wrapper = await mountApp()
    // Vue‑Test‑Utils treats <router-view/> as a real component,
    // so we can look for the rendered content of the active route.
    // By default the router starts at '/' → Home component.
    expect(wrapper.find('.home').exists()).toBe(true)
    expect(wrapper.find('.home').text()).toBe('Home page')
  })

  it('displays the Results component when navigating to /results', async () => {
    const wrapper = await mountApp('/results')
    // The router should have rendered the Results component inside the outlet.
    expect(wrapper.find('.results').exists()).toBe(true)
    expect(wrapper.find('.results').text()).toBe('Results page')
  })
})