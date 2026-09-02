import { useCallback, useEffect, useState } from 'react';
import { usePlaylistImport } from './playlists/usePlaylistImport';
import { useTrackResolution } from './tracks/useTrackResolution';
import { getAllPlaylists, getTracksByIds } from './library/db';
import type { Playlist, Track } from './playlists/types';
import { ImportForm } from './components/ImportForm';
import { PlaylistCard } from './components/PlaylistCard';
import { ErrorToast } from './components/ErrorToast';
import { BatchResolutionSummaryToast } from './components/BatchResolutionSummaryToast';
import './App.css';

export default function App() {
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [tracksByPlaylistId, setTracksByPlaylistId] = useState<Record<string, Track[]>>({});
  const [refreshingId, setRefreshingId] = useState<string | null>(null);
  const [resolvingTrackId, setResolvingTrackId] = useState<string | null>(null);
  const [resolvingBatchPlaylistId, setResolvingBatchPlaylistId] = useState<string | null>(null);

  const { doImport, doRefresh, isLoading: isImportLoading, error: importError, clearError: clearImportError } = usePlaylistImport();
  const {
    doResolve,
    doResolveBatch,
    error: resolveError,
    lastBatchSummary,
    clearError: clearResolveError,
    clearBatchSummary,
  } = useTrackResolution();

  const loadLibrary = useCallback(async () => {
    const loadedPlaylists = await getAllPlaylists();
    setPlaylists(loadedPlaylists);

    const entries = await Promise.all(
      loadedPlaylists.map(async (playlist) => [playlist.spotifyId, await getTracksByIds(playlist.trackIds)] as const),
    );
    setTracksByPlaylistId(Object.fromEntries(entries));
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

  async function handleResolveTrack(track: Track) {
    setResolvingTrackId(track.spotifyId);
    await doResolve(track);
    await loadLibrary();
    setResolvingTrackId(null);
  }

  async function handleResolveAll(playlistId: string, tracks: Track[]) {
    setResolvingBatchPlaylistId(playlistId);
    await doResolveBatch(tracks);
    await loadLibrary();
    setResolvingBatchPlaylistId(null);
  }

  const error = importError ?? resolveError;
  const clearError = importError ? clearImportError : clearResolveError;

  return (
    <main className="app">
      <h1>Vladify</h1>

      <ImportForm onImport={handleImport} isLoading={isImportLoading && refreshingId === null} />

      {playlists.length === 0 ? (
        <p className="empty-state">Библиотека је празна. Увези своју прву Spotify плејлисту изнад.</p>
      ) : (
        <ul className="playlist-list">
          {playlists.map((playlist) => (
            <PlaylistCard
              key={playlist.spotifyId}
              playlist={playlist}
              tracks={tracksByPlaylistId[playlist.spotifyId] ?? []}
              onRefresh={handleRefresh}
              isRefreshing={refreshingId === playlist.spotifyId}
              onResolveTrack={handleResolveTrack}
              resolvingTrackId={resolvingTrackId}
              onResolveAll={(tracks) => handleResolveAll(playlist.spotifyId, tracks)}
              isResolvingBatch={resolvingBatchPlaylistId === playlist.spotifyId}
            />
          ))}
        </ul>
      )}

      {lastBatchSummary && (
        <BatchResolutionSummaryToast
          resolvedCount={lastBatchSummary.resolvedCount}
          failedCount={lastBatchSummary.failedCount}
          onDismiss={clearBatchSummary}
        />
      )}

      {error && <ErrorToast message={error} onDismiss={clearError} />}
    </main>
  );
}
