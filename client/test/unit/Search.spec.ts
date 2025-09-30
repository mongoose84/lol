// tests/unit/Search.spec.ts
import { mount } from '@vue/test-utils'
import { describe, it, expect, vi } from 'vitest'
import Search from '../../src/views/Search.vue'

// ------------------------------------------------------------------
// Mock vue-router – we only need the `push` method that our component calls
// ------------------------------------------------------------------
const mockPush = vi.fn()
vi.mock('vue-router', () => ({
  // useRouter is what the component calls internally
  useRouter: () => ({
    push: mockPush,          // <-- our shared mock
  }),
}))

beforeEach(() => {
  mockPush.mockReset()
})

describe('Search.vue', () => {
  it('sets the default gameName to EUNE', () => {
    const wrapper = mount(Search)
    expect(wrapper.vm.tagLine).toBe('EUNE')
  })

  it('builds the correct query object and calls router.push on submit', async () => {
    const wrapper = mount(Search)

    // ----- fill the form -------------------------------------------------
    await wrapper.find('input.search-input').setValue('Doend')
    await wrapper.find('select.tagLine-select').setValue('EUW')

    // ----- submit ---------------------------------------------------------
    await wrapper.find('form').trigger('submit.prevent')

    // ----- assert ----------------------------------------------------------
    expect(mockPush).toHaveBeenCalledTimes(1)
    expect(mockPush).toHaveBeenCalledWith({
      name: 'ChampionStats',
      query: { gameName: 'Doend', tagLine: 'EUW' },
    })
  })

  it('does nothing when the champion field is empty', async () => {
    const wrapper = mount(Search)

    // Submit without typing a champion name
    await wrapper.find('form').trigger('submit.prevent')

    expect(mockPush).not.toHaveBeenCalled()
  })
})