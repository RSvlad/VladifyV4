import { useState } from 'react';
import type { Playlist, Track } from '../playlists/types';
import { TrackRow } from './TrackRow';

interface PlaylistCardProps {
  playlist: Playlist;
  tracks: Track[];
  onRefresh: (playlist: Playlist) => void;
  isRefreshing: boolean;
  onResolveTrack: (track: Track) => void;
  resolvingTrackId: string | null;
  onResolveAll: (tracks: Track[]) => void;
  isResolvingBatch: boolean;
}

export function PlaylistCard({
  playlist,
  tracks,
  onRefresh,
  isRefreshing,
  onResolveTrack,
  resolvingTrackId,
  onResolveAll,
  isResolvingBatch,
}: PlaylistCardProps) {
  const [showTracks, setShowTracks] = useState(false);
  const unresolvedTracks = tracks.filter((t) => !t.youTubeVideoId);

  return (
    <li className="playlist-card">
      <div className="playlist-card__header">
        <strong>{playlist.name}</strong>
        <span className="playlist-card__owner">{playlist.ownerName}</span>
      </div>
      <div className="playlist-card__meta">
        {playlist.trackIds.length} нумера
        {playlist.lastRefreshedAt && (
          <> · освежено {new Date(playlist.lastRefreshedAt).toLocaleString('sr-RS')}</>
        )}
      </div>
      <div className="playlist-card__actions">
        <button type="button" onClick={() => onRefresh(playlist)} disabled={isRefreshing}>
          {isRefreshing ? 'Освежавам…' : 'Освежи'}
        </button>
        <button type="button" onClick={() => setShowTracks((v) => !v)}>
          {showTracks ? 'Сакриј нумере' : 'Прикажи нумере'}
        </button>
        {unresolvedTracks.length > 0 && (
          <button
            type="button"
            onClick={() => onResolveAll(unresolvedTracks)}
            disabled={isResolvingBatch}
          >
            {isResolvingBatch ? 'Тражим…' : `Нађи све на YouTube-у (${unresolvedTracks.length})`}
          </button>
        )}
      </div>
      {showTracks && (
        <ul className="track-list">
          {tracks.map((track) => (
            <TrackRow
              key={track.spotifyId}
              track={track}
              onResolve={onResolveTrack}
              isLoading={resolvingTrackId === track.spotifyId}
            />
          ))}
        </ul>
      )}
    </li>
  );
}
