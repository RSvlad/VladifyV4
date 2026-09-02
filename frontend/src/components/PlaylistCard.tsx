import type { Playlist } from '../playlists/types';

interface PlaylistCardProps {
  playlist: Playlist;
  onRefresh: (playlist: Playlist) => void;
  isLoading: boolean;
}

export function PlaylistCard({ playlist, onRefresh, isLoading }: PlaylistCardProps) {
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
      <button type="button" onClick={() => onRefresh(playlist)} disabled={isLoading}>
        {isLoading ? 'Освежавам…' : 'Освежи'}
      </button>
    </li>
  );
}
