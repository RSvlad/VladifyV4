import { useCallback, useEffect, useState } from 'react';
import { usePlaylistImport } from './playlists/usePlaylistImport';
import { getAllPlaylists } from './library/db';
import type { Playlist } from './playlists/types';
import { ImportForm } from './components/ImportForm';
import { PlaylistCard } from './components/PlaylistCard';
import { ErrorToast } from './components/ErrorToast';
import './App.css';

export default function App() {
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [refreshingId, setRefreshingId] = useState<string | null>(null);
  const { doImport, doRefresh, isLoading, error, clearError } = usePlaylistImport();

  const loadLibrary = useCallback(async () => {
    setPlaylists(await getAllPlaylists());
  }, []);

  useEffect(() => {
    loadLibrary();
  }, [loadLibrary]);

  async function handleImport(spotifyPlaylistId: string) {
    const playlist = await doImport(spotifyPlaylistId);
    if (playlist) await loadLibrary();
  }

  async function handleRefresh(playlist: Playlist) {
    setRefreshingId(playlist.spotifyId);
    await doRefresh(playlist);
    await loadLibrary();
    setRefreshingId(null);
  }

  return (
    <main className="app">
      <h1>Vladify</h1>

      <ImportForm onImport={handleImport} isLoading={isLoading && refreshingId === null} />

      {playlists.length === 0 ? (
        <p className="empty-state">Библиотека је празна. Увези своју прву Spotify плејлисту изнад.</p>
      ) : (
        <ul className="playlist-list">
          {playlists.map((playlist) => (
            <PlaylistCard
              key={playlist.spotifyId}
              playlist={playlist}
              onRefresh={handleRefresh}
              isLoading={refreshingId === playlist.spotifyId}
            />
          ))}
        </ul>
      )}

      {error && <ErrorToast message={error} onDismiss={clearError} />}
    </main>
  );
}
