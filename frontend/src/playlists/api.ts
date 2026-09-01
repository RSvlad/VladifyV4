import type { Playlist, Track } from './types';

/**
 * Talks to the Vladify.Api Playlist endpoints. The server is stateless — every
 * response carries the full Playlist state, which the caller is responsible for
 * persisting into the client-side Library (see library/db.ts).
 */

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export class PlaylistNotFoundError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'PlaylistNotFoundError';
  }
}

export interface ImportPlaylistResponse {
  playlist: Playlist;
  tracks: Track[];
}

export async function importPlaylist(spotifyPlaylistId: string): Promise<ImportPlaylistResponse> {
  const response = await fetch(`${API_BASE_URL}/api/playlists/import`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ spotifyPlaylistId }),
  });

  if (response.status === 404) {
    const body = await response.json().catch(() => null);
    throw new PlaylistNotFoundError(
      body?.message ?? "Couldn't find that playlist on Spotify. Double-check the link and make sure it's public.",
    );
  }

  if (!response.ok) {
    throw new Error(`Import failed with status ${response.status}.`);
  }

  return response.json();
}

export interface RefreshPlaylistResponse {
  playlist: Playlist;
  tracks: Track[];
  tracksAdded: string[];
  tracksRemoved: string[];
}

/**
 * Per Glossary "Refresh": on failure the server returns 200 with the caller's own
 * unchanged state, so this function never throws for a failed-but-silent refresh —
 * the caller always gets something to write back to the Library.
 */
export async function refreshPlaylist(playlist: Playlist): Promise<RefreshPlaylistResponse> {
  const response = await fetch(`${API_BASE_URL}/api/playlists/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      spotifyId: playlist.spotifyId,
      name: playlist.name,
      ownerName: playlist.ownerName,
      trackIds: playlist.trackIds,
      importedAt: playlist.importedAt,
    }),
  });

  if (!response.ok) {
    throw new Error(`Refresh failed with status ${response.status}.`);
  }

  return response.json();
}
