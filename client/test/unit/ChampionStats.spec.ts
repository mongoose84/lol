// tests/unit/ChampionStats.spec.ts
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { mount, flushPromises } from '@vue/test-utils';
import ChampionStats from '../../src/views/ChampionStats.vue';
import useSummoner from '../../src/assets/useSummoner';

// ------------------------------------------------------------------
// Mock the composable so we can control its behaviour inside the component
// ------------------------------------------------------------------
vi.mock('../../src/assets/useSummoner');

describe('ChampionStats.vue', () => {
  // Helpers to reset the mock between tests
  const mockReturn = (overrides: Partial<ReturnType<typeof useSummoner>> = {}) => {
    (useSummoner as any).mockReturnValue({
      summoner: { value: null },
      loading: { value: false },
      error: { value: null },
      fetchSummoner: vi.fn(),
      ...overrides,
    });
  };

  beforeEach(() => {
    vi.resetAllMocks();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('renders the gameName and tagLine from props', async () => {
    mockReturn(); // default mock (nothing loading, no data)

    const wrapper = mount(ChampionStats, {
      props: {
        gameName: 'Doend',
        tagLine: '2749',
      },
    });

    // The heading should contain the concatenated Riot ID
    expect(wrapper.find('h2').text()).toContain('Doend#2749');
  });

  it('shows loading indicator while fetchSummoner is pending', async () => {
    // Simulate loading = true right after the component mounts
    mockReturn({
      loading: { value: true },
      fetchSummoner: vi.fn(),
    });

    const wrapper = mount(ChampionStats, {
      props: {
        gameName: 'Doend',
        tagLine: '2749',
      },
    });

    // The component should render the loading text
    expect(wrapper.text()).toContain('Loading…');
  });

  it('calls fetchSummoner on mount and when props change', async () => {
    const fetchMock = vi.fn().mockResolvedValue(undefined);
    mockReturn({
      fetchSummoner: fetchMock,
    });

    const wrapper = mount(ChampionStats, {
      props: {
        gameName: 'Doend',
        tagLine: '2749',
      },
    });

    // onMounted should have triggered a call
    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(fetchMock).toHaveBeenCalledWith('Doend', '2749');

    // Change the props – the watcher should fire another request
    await wrapper.setProps({ gameName: 'Other', tagLine: '1234' });
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(fetchMock).toHaveBeenCalledWith('Other', '1234');
  });
});