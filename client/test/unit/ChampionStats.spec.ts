// tests/unit/ChampionStats.spec.ts
import { mount } from '@vue/test-utils'
import ChampionStats from '../../src/views/ChampionStats.vue'

describe('ChampionStats.vue', () => {
  it('renders gameName and tagLine from props', () => {
    const wrapper = mount(ChampionStats, {
      props: {
        gameName: 'Doend',
        tagLine: 'EUNE',
      },
    })

    // Adjust selectors to match your template (e.g., <h2>, <p>, etc.)
    expect(wrapper.text()).toContain('Doend')
    expect(wrapper.text()).toContain('EUNE')
  })

  
})