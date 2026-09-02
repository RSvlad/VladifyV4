import type { Track } from '../playlists/types';

/**
 * Talks to the Vladify.Api Track Resolution endpoints. Stateless like the Playlist
 * endpoints: the caller sends its own Track state and gets back the resolved (or
 * unresolved-with-reason) result to persist into the client-side Library.
 */

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export interface UnresolvedTrack {
  spotifyId: string;
  reason: string;
}

export interface ResolveTrackResponse {
  resolved: Track | null;
  unresolved: UnresolvedTrack | null;
}

interface ResolvedTrackDto {
  spotifyId: string;
  youTubeVideoId: string;
  youTubeTitle: string;
  youTubeChannelName: string;
  matchConfidence: number;
  resolvedAt: string;
}

function toTrackInput(track: Track) {
  return {
    spotifyId: track.spotifyId,
    title: track.title,
    artists: track.artists,
    album: track.album,
    durationMs: track.durationMs,
  };
}

function toTrack(dto: ResolvedTrackDto, source: Track): Track {
  return {
    ...source,
    youTubeVideoId: dto.youTubeVideoId,
    youTubeTitle: dto.youTubeTitle,
    youTubeChannelName: dto.youTubeChannelName,
    matchConfidence: dto.matchConfidence,
    resolvedAt: dto.resolvedAt,
  };
}

export async function resolveTrack(track: Track): Promise<ResolveTrackResponse> {
  const response = await fetch(`${API_BASE_URL}/api/tracks/resolve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ track: toTrackInput(track) }),
  });

  if (!response.ok) {
    throw new Error(`Track resolution failed with status ${response.status}.`);
  }

  const body = await response.json();
  return {
    resolved: body.resolved ? toTrack(body.resolved, track) : null,
    unresolved: body.unresolved,
  };
}

export interface ResolveTracksBatchResponse {
  resolved: Track[];
  failed: UnresolvedTrack[];
}

export async function resolveTracksBatch(tracks: Track[]): Promise<ResolveTracksBatchResponse> {
  const response = await fetch(`${API_BASE_URL}/api/tracks/resolve-batch`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ tracks: tracks.map(toTrackInput) }),
  });

  if (!response.ok) {
    throw new Error(`Batch track resolution failed with status ${response.status}.`);
  }

  const body = await response.json();
  const tracksById = new Map(tracks.map((t) => [t.spotifyId, t]));

  const resolved: Track[] = body.resolved.map((dto: ResolvedTrackDto) =>
    toTrack(dto, tracksById.get(dto.spotifyId)!),
  );

  return { resolved, failed: body.failed };
}
