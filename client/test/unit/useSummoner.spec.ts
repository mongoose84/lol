// tests/unit/useSummoner.spec.ts
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';
import useSummoner from '../../src/assets/useSummoner';

// ------------------------------------------------------------------
// Helper to unwrap the returned refs for easier assertions
// ------------------------------------------------------------------
function unwrap<T>(refObj: { value: T }): T {
  return refObj.value;
}

describe('useSummoner composable', () => {
  let mock: MockAdapter;

  beforeEach(() => {
    mock = new MockAdapter(axios);
  });

  afterEach(() => {
    mock.restore();
    vi.clearAllMocks();
  });

  it('sets loading, returns data on success', async () => {
    const fakeResponse = {
      puuid: 'a1b2c3d4-5678-90ab-cdef-1234567890ab',
      summonerId: '123456789',
      summonerLevel: 150,
      profileIconId: 1234,
      gameName: 'Doend',
      tagLine: '2749',
    };

    // Stub the proxy endpoint that the composable calls
    mock
      .onGet(
        /\/api\/by-riot-id\/Doend\/2749$/ // regex to match the exact URL
      )
      .reply(200, fakeResponse);

    const { summoner, loading, error, fetchSummoner } = useSummoner();

    // Initial state
    expect(unwrap(loading)).toBe(false);
    expect(unwrap(error)).toBeNull();
    expect(unwrap(summoner)).toBeNull();

    // Fire the request
    const promise = fetchSummoner('Doend', '2749');

    // Loading should be true immediately after the call
    expect(unwrap(loading)).toBe(true);

    await promise; // wait for the async request to finish

    // After resolution
    expect(unwrap(loading)).toBe(false);
    expect(unwrap(summoner)).toEqual(fakeResponse);
  });

  it('captures an error response', async () => {
    mock
      .onGet(/\/api\/by-riot-id\/Unknown\/0000$/)
      .reply(404, { error: 'Summoner not found' });

    const { summoner, loading, error, fetchSummoner } = useSummoner();

    const promise = fetchSummoner('Unknown', '0000');

    expect(unwrap(loading)).toBe(true);
    await promise;

    expect(unwrap(loading)).toBe(false);
    expect(unwrap(summoner)).toBeNull();
    expect(unwrap(error)).toBe('Summoner not found');
  });
});