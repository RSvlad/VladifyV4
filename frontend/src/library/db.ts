import { openDB, type DBSchema, type IDBPDatabase } from 'idb';
import type { Playlist, Track } from '../playlists/types';

/**
 * The client-side Library (glossary.md: "the user's client-side (IndexedDB)
 * collection of imported Playlists and Tracks. No server-side persistence of user
 * data."). Tracks are deduplicated by spotifyId and stored once, independent of
 * how many Playlists reference them — Playlist.trackIds holds only references.
 */

interface VladifyDb extends DBSchema {
  playlists: {
    key: string;
    value: Playlist;
  };
  tracks: {
    key: string;
    value: Track;
  };
}

const DB_NAME = 'vladify-library';
const DB_VERSION = 1;

let dbPromise: Promise<IDBPDatabase<VladifyDb>> | null = null;

function getDb(): Promise<IDBPDatabase<VladifyDb>> {
  dbPromise ??= openDB<VladifyDb>(DB_NAME, DB_VERSION, {
    upgrade(db) {
      db.createObjectStore('playlists', { keyPath: 'spotifyId' });
      db.createObjectStore('tracks', { keyPath: 'spotifyId' });
    },
  });

  return dbPromise;
}

/**
 * Test-only escape hatch: drops the cached DB handle so the next getDb() call
 * reopens against whatever `indexedDB` global is current. Needed because this
 * module caches its handle at module scope, which would otherwise leak state
 * across tests sharing one fake-indexeddb instance. Not used by app code.
 */
export function __resetDbForTests(): void {
  dbPromise = null;
}

export async function savePlaylist(playlist: Playlist): Promise<void> {
  const db = await getDb();
  await db.put('playlists', playlist);
}

export async function getPlaylist(spotifyId: string): Promise<Playlist | undefined> {
  const db = await getDb();
  return db.get('playlists', spotifyId);
}

export async function getAllPlaylists(): Promise<Playlist[]> {
  const db = await getDb();
  return db.getAll('playlists');
}

/** Upserts Tracks by spotifyId — dedup happens automatically via the keyPath. */
export async function saveTracks(tracks: Track[]): Promise<void> {
  const db = await getDb();
  const tx = db.transaction('tracks', 'readwrite');
  await Promise.all(tracks.map((track) => tx.store.put(track)));
  await tx.done;
}

export async function getTracksByIds(spotifyIds: string[]): Promise<Track[]> {
  const db = await getDb();
  const tracks = await Promise.all(spotifyIds.map((id) => db.get('tracks', id)));
  return tracks.filter((t): t is Track => t !== undefined);
}
