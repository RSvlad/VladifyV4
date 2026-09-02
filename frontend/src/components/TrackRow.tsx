import type { Track } from '../playlists/types';

interface TrackRowProps {
  track: Track;
  onResolve: (track: Track) => void;
  isLoading: boolean;
}

/**
 * One Track within a PlaylistCard's track list. Shows resolved/unresolved state
 * visually (Phase 1 UCD: a Track without a YouTube match must look distinct from
 * a resolved one) and offers a per-Track "Find on YouTube" action.
 */
export function TrackRow({ track, onResolve, isLoading }: TrackRowProps) {
  const isResolved = Boolean(track.youTubeVideoId);

  return (
    <li className={`track-row ${isResolved ? 'track-row--resolved' : 'track-row--unresolved'}`}>
      <span className="track-row__status" aria-hidden="true">
        {isResolved ? '●' : '○'}
      </span>
      <span className="track-row__title">
        {track.title} <span className="track-row__artists">— {track.artists.join(', ')}</span>
      </span>
      {!isResolved && (
        <button type="button" onClick={() => onResolve(track)} disabled={isLoading} className="track-row__resolve">
          {isLoading ? 'Тражим…' : 'Нађи на YouTube-у'}
        </button>
      )}
    </li>
  );
}
