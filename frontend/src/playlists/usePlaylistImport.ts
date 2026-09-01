import { useCallback, useState } from 'react';
import { importPlaylist, refreshPlaylist, PlaylistNotFoundError } from './api';
import { savePlaylist, saveTracks, getTracksByIds } from '../library/db';
import type { Playlist } from './types';

/**
 * Orchestrates the Import/Refresh use cases against the API, then persists the
 * result into the client-side Library. UI components consume this hook rather
 * than talking to api.ts or library/db.ts directly.
 */
export function usePlaylistImport() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const doImport = useCallback(async (spotifyPlaylistId: string): Promise<Playlist | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const { playlist, tracks } = await importPlaylist(spotifyPlaylistId);
      await saveTracks(tracks);
      await savePlaylist(playlist);
      return playlist;
    } catch (err) {
      // UCD: surface the server's user-facing message when we have one (e.g. playlist
      // not found), otherwise a generic fallback — never a raw error/stack trace.
      setError(err instanceof PlaylistNotFoundError ? err.message : "Something went wrong importing that playlist. Please try again.");
      return null;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const doRefresh = useCallback(async (playlist: Playlist): Promise<Playlist> => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await refreshPlaylist(playlist);
      // Per Glossary "Refresh": a silent failure returns the caller's own unchanged
      // state and an empty tracks list, so this path is a no-op write — safe either way.
      if (result.tracks.length > 0) {
        await saveTracks(result.tracks);
      }
      await savePlaylist(result.playlist);
      return result.playlist;
    } catch {
      // Network/server errors (not the silent-fail case, which never throws, per
      // ADR-006/007) get a visible signal — decided during Phase 2 (open-issues.md #8).
      // This is a different failure mode than "Spotify-side playlist unavailable",
      // which stays fully silent. Local data is kept untouched either way.
      setError("Couldn't reach the server to refresh this playlist. Please try again.");
      return playlist;
    } finally {
      setIsLoading(false);
    }
  }, []);

  return { doImport, doRefresh, isLoading, error };
}

export { getTracksByIds };
