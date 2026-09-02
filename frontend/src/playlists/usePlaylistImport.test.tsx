import { act, renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { IDBFactory } from 'fake-indexeddb';
import { usePlaylistImport } from './usePlaylistImport';
import * as api from './api';
import { PlaylistNotFoundError } from './api';
import { getAllPlaylists, getTracksByIds, __resetDbForTests } from '../library/db';
import type { Playlist, Track } from './types';

/**
 * Phase 3 (Problem 1: Spotify playlist import) — frontend scenarios for the hook
 * that wires the Import/Refresh use cases into the client-side Library.
 * fake-indexeddb (src/test/setup.ts) backs library/db.ts so these run against a
 * real IndexedDB implementation, not a mock of it. Each test gets a fresh
 * database instance (see beforeEach below) — library/db.ts caches its DB handle
 * at module scope, so without this, Playlists/Tracks written in one test would
 * leak into the next.
 */

const track = (id: string): Track => ({
  spotifyId: id,
  title: `Title ${id}`,
  artists: ['Artist'],
  album: 'Album',
  durationMs: 200_000,
});

const playlist = (overrides: Partial<Playlist> = {}): Playlist => ({
  spotifyId: 'p1',
  name: 'My Playlist',
  ownerName: 'owner',
  trackIds: ['t1', 't2'],
  importedAt: new Date().toISOString(),
  lastRefreshedAt: null,
  ...overrides,
});

beforeEach(() => {
  vi.restoreAllMocks();
  // Fresh IndexedDB per test — see __resetDbForTests' doc comment in library/db.ts.
  (globalThis as { indexedDB: IDBFactory }).indexedDB = new IDBFactory();
  __resetDbForTests();
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('usePlaylistImport.doImport', () => {
  it('persists the playlist and its tracks into the Library on success', async () => {
    const imported = playlist();
    const tracks = [track('t1'), track('t2')];
    vi.spyOn(api, 'importPlaylist').mockResolvedValue({ playlist: imported, tracks });

    const { result } = renderHook(() => usePlaylistImport());

    let returned: Playlist | null = null;
    await act(async () => {
      returned = await result.current.doImport('p1');
    });

    expect(returned).toEqual(imported);
    expect(result.current.error).toBeNull();

    await waitFor(async () => {
      expect(await getAllPlaylists()).toContainEqual(imported);
    });
    const savedTracks = await getTracksByIds(['t1', 't2']);
    expect(savedTracks).toHaveLength(2);
  });

  it('sets a visible error and writes nothing when the API reports not-found', async () => {
    vi.spyOn(api, 'importPlaylist').mockRejectedValue(
      new PlaylistNotFoundError("Couldn't find that playlist on Spotify. Double-check the link and make sure it's public."),
    );

    const { result } = renderHook(() => usePlaylistImport());

    let returned: Playlist | null = null;
    await act(async () => {
      returned = await result.current.doImport('missing');
    });

    expect(returned).toBeNull();
    expect(result.current.error).toMatch(/couldn't find that playlist/i);
    expect(await getAllPlaylists()).toEqual([]);
  });
});

describe('usePlaylistImport.doRefresh', () => {
  it('sets a visible error and keeps the local playlist unchanged on network failure', async () => {
    const existing = playlist();
    vi.spyOn(api, 'refreshPlaylist').mockRejectedValue(new Error('network down'));

    const { result } = renderHook(() => usePlaylistImport());

    let returned: Playlist | null = null;
    await act(async () => {
      returned = await result.current.doRefresh(existing);
    });

    expect(returned).toEqual(existing);
    expect(result.current.error).toMatch(/couldn't reach the server/i);
  });

  it('updates the Library with the added/removed tracks on success', async () => {
    const existing = playlist({ trackIds: ['t1', 't2'] });
    const refreshed = playlist({ trackIds: ['t2', 't3'] });
    vi.spyOn(api, 'refreshPlaylist').mockResolvedValue({
      playlist: refreshed,
      tracks: [track('t2'), track('t3')],
      tracksAdded: ['t3'],
      tracksRemoved: ['t1'],
    });

    const { result } = renderHook(() => usePlaylistImport());

    let returned: Playlist | null = null;
    await act(async () => {
      returned = await result.current.doRefresh(existing);
    });

    expect(returned).toEqual(refreshed);
    expect(result.current.error).toBeNull();

    const savedTracks = await getTracksByIds(['t2', 't3']);
    expect(savedTracks).toHaveLength(2);
  });
});

describe('usePlaylistImport.clearError', () => {
  it('resets the error state without requiring a reload', async () => {
    vi.spyOn(api, 'importPlaylist').mockRejectedValue(new PlaylistNotFoundError('not found'));

    const { result } = renderHook(() => usePlaylistImport());

    await act(async () => {
      await result.current.doImport('missing');
    });
    expect(result.current.error).not.toBeNull();

    act(() => {
      result.current.clearError();
    });

    expect(result.current.error).toBeNull();
  });
});
