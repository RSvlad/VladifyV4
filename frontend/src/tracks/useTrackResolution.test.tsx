import { act, renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { IDBFactory } from 'fake-indexeddb';
import { useTrackResolution } from './useTrackResolution';
import * as api from './api';
import { getTracksByIds, __resetDbForTests } from '../library/db';
import type { Track } from '../playlists/types';

/**
 * Phase 3 (Track Resolution) — frontend scenarios for the hook that wires the
 * Resolve/ResolveBatch use cases into the client-side Library. Same fake-indexeddb
 * setup as usePlaylistImport.test.tsx: fresh DB per test since library/db.ts caches
 * its handle at module scope.
 */

const track = (id: string): Track => ({
  spotifyId: id,
  title: `Title ${id}`,
  artists: ['Artist'],
  album: 'Album',
  durationMs: 200_000,
});

const resolved = (id: string): Track => ({
  ...track(id),
  youTubeVideoId: `yt-${id}`,
  youTubeTitle: `YT Title ${id}`,
  youTubeChannelName: 'Channel',
  matchConfidence: 0.8,
  resolvedAt: new Date().toISOString(),
});

beforeEach(() => {
  vi.restoreAllMocks();
  (globalThis as { indexedDB: IDBFactory }).indexedDB = new IDBFactory();
  __resetDbForTests();
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('useTrackResolution.doResolve', () => {
  it('persists the resolved track into the Library on success', async () => {
    const t = track('t1');
    const match = resolved('t1');
    vi.spyOn(api, 'resolveTrack').mockResolvedValue({ resolved: match, unresolved: null });

    const { result } = renderHook(() => useTrackResolution());

    let returned: Track | null = null;
    await act(async () => {
      returned = await result.current.doResolve(t);
    });

    expect(returned).toEqual(match);
    expect(result.current.error).toBeNull();

    await waitFor(async () => {
      const saved = await getTracksByIds(['t1']);
      expect(saved).toHaveLength(1);
      expect(saved[0].youTubeVideoId).toBe('yt-t1');
    });
  });

  it('sets an informational error and writes nothing when there is no confident match', async () => {
    const t = track('t1');
    vi.spyOn(api, 'resolveTrack').mockResolvedValue({
      resolved: null,
      unresolved: { spotifyId: 't1', reason: 'No confident match among the results found.' },
    });

    const { result } = renderHook(() => useTrackResolution());

    let returned: Track | null = null;
    await act(async () => {
      returned = await result.current.doResolve(t);
    });

    expect(returned).toBeNull();
    expect(result.current.error).toMatch(/no confident match/i);
    expect(await getTracksByIds(['t1'])).toEqual([]);
  });

  it('sets a generic error on network/server failure', async () => {
    const t = track('t1');
    vi.spyOn(api, 'resolveTrack').mockRejectedValue(new Error('network down'));

    const { result } = renderHook(() => useTrackResolution());

    await act(async () => {
      await result.current.doResolve(t);
    });

    expect(result.current.error).toMatch(/couldn't reach the server/i);
  });
});

describe('useTrackResolution.doResolveBatch', () => {
  it('reports an accurate summary and only persists the resolved tracks', async () => {
    const tracks = [track('a'), track('b'), track('c')];
    vi.spyOn(api, 'resolveTracksBatch').mockResolvedValue({
      resolved: [resolved('a'), resolved('c')],
      failed: [{ spotifyId: 'b', reason: 'No confident match among the results found.' }],
    });

    const { result } = renderHook(() => useTrackResolution());

    await act(async () => {
      await result.current.doResolveBatch(tracks);
    });

    expect(result.current.lastBatchSummary).toEqual({ resolvedCount: 2, failedCount: 1 });

    const savedA = await getTracksByIds(['a']);
    const savedB = await getTracksByIds(['b']);
    expect(savedA).toHaveLength(1);
    expect(savedB).toHaveLength(0);
  });
});

describe('useTrackResolution.clearError / clearBatchSummary', () => {
  it('reset their respective state independently', async () => {
    vi.spyOn(api, 'resolveTrack').mockRejectedValue(new Error('network down'));
    vi.spyOn(api, 'resolveTracksBatch').mockResolvedValue({ resolved: [], failed: [] });

    const { result } = renderHook(() => useTrackResolution());

    await act(async () => {
      await result.current.doResolveBatch([track('t1')]);
    });
    expect(result.current.lastBatchSummary).not.toBeNull();

    await act(async () => {
      await result.current.doResolve(track('t1'));
    });
    expect(result.current.error).not.toBeNull();
    // doResolve doesn't touch lastBatchSummary, so it should still be set from the batch call above.
    expect(result.current.lastBatchSummary).not.toBeNull();

    act(() => {
      result.current.clearError();
    });
    expect(result.current.error).toBeNull();
    expect(result.current.lastBatchSummary).not.toBeNull();

    act(() => {
      result.current.clearBatchSummary();
    });
    expect(result.current.lastBatchSummary).toBeNull();
  });
});
