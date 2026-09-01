/**
 * Client-side mirror of the Domain model (Vladify.Domain.Playlists/Tracks).
 * Naming matches glossary.md — same ubiquitous language on both sides.
 */

export interface Track {
  spotifyId: string;
  title: string;
  artists: string[];
  album: string;
  durationMs: number;
}

export interface Playlist {
  spotifyId: string;
  name: string;
  ownerName: string;
  trackIds: string[];
  importedAt: string;
  lastRefreshedAt: string | null;
}
